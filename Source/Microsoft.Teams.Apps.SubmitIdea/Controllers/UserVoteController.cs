// <copyright file="UserVoteController.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.SubmitIdea.Controllers
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.ApplicationInsights;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Microsoft.Teams.Apps.SubmitIdea.Common.Interfaces;
    using Microsoft.Teams.Apps.SubmitIdea.Models;
    using Microsoft.WindowsAzure.Storage;
    using Polly;
    using Polly.Contrib.WaitAndRetry;
    using Polly.Retry;

    /// <summary>
    /// Controller to handle user vote operations.
    /// </summary>
    [ApiController]
    [Route("api/uservote")]
    [Authorize]
    public class UserVoteController : BaseSubmitIdeaController
    {
        /// <summary>
        /// Retry policy with jitter.
        /// </summary>
        /// <remarks>
        /// Reference: https://github.com/Polly-Contrib/Polly.Contrib.WaitAndRetry#new-jitter-recommendation.
        /// </remarks>
        private readonly AsyncRetryPolicy retryPolicy;

        /// <summary>
        /// Logger implementation to send logs to the logger service.
        /// </summary>
        private readonly ILogger logger;

        /// <summary>
        /// Instance of user vote storage provider to add and delete user vote.
        /// </summary>
        private readonly IUserVoteStorageProvider userVoteStorageProvider;

        /// <summary>
        /// Instance of team post storage provider.
        /// </summary>
        private readonly IIdeaStorageProvider teamIdeaStorageProvider;

        /// <summary>
        /// Instance of Search service for working with storage.
        /// </summary>
        private readonly IIdeaSearchService teamIdeaSearchService;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserVoteController"/> class.
        /// </summary>
        /// <param name="logger">Logger implementation to send logs to the logger service.</param>
        /// <param name="telemetryClient">The Application Insights telemetry client.</param>
        /// <param name="userVoteStorageProvider">Instance of user vote storage provider to add and delete user vote.</param>
        /// <param name="teamIdeaStorageProvider">Instance of team post storage provider to update post and get information of posts.</param>
        /// <param name="teamIdeaSearchService">The team post search service dependency injection.</param>
        public UserVoteController(
            ILogger<UserVoteController> logger,
            TelemetryClient telemetryClient,
            IUserVoteStorageProvider userVoteStorageProvider,
            IIdeaStorageProvider teamIdeaStorageProvider,
            IIdeaSearchService teamIdeaSearchService)
            : base(telemetryClient)
        {
            this.logger = logger;
            this.userVoteStorageProvider = userVoteStorageProvider;
            this.teamIdeaStorageProvider = teamIdeaStorageProvider;
            this.teamIdeaSearchService = teamIdeaSearchService;
            this.retryPolicy = Policy.Handle<StorageException>(ex => ex.RequestInformation.HttpStatusCode == StatusCodes.Status412PreconditionFailed)
               .WaitAndRetryAsync(Backoff.ConstantBackoff(TimeSpan.FromMilliseconds(250), 25));
        }

        /// <summary>
        /// Get call to retrieve list of votes for user.
        /// </summary>
        /// <returns>List of team posts.</returns>
        [HttpGet("votes")]
        public async Task<IActionResult> GetVotesAsync()
        {
            try
            {
                this.logger.LogInformation("call to retrieve list of votes for user.");

                var userVotes = await this.userVoteStorageProvider.GetVotesAsync(this.UserAadId);
                this.RecordEvent("User votes - HTTP Get call succeeded.");

                return this.Ok(userVotes);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error while making call to team post service.");
                throw;
            }
        }

        /// <summary>
        /// Stores user vote for a idea.
        /// </summary>
        /// <param name="postCreatedByUserId">AAD user Id of user who created idea.</param>
        /// <param name="postId">Id of the post to delete vote.</param>
        /// <returns>Returns true for successful operation.</returns>
        [HttpPost("vote")]
        public async Task<IActionResult> AddVoteAsync(string postCreatedByUserId, string postId)
        {
            this.logger.LogInformation("call to add user vote.");

            if (string.IsNullOrEmpty(postCreatedByUserId))
            {
                this.logger.LogError($"Error while deleting vote. Parameter {nameof(postCreatedByUserId)} is either null or empty.");
                return this.BadRequest(new { message = $"Parameter {nameof(postCreatedByUserId)} is either null or empty." });
            }

            if (string.IsNullOrEmpty(postId))
            {
                this.logger.LogError($"Error while deleting vote. {nameof(postId)} is either null or empty.");
                return this.BadRequest(new { message = $"{nameof(postId)} is either null or empty." });
            }

            bool isUserVoteSavedSuccessful = false;
            bool isPostSavedSuccessful = false;

            // Note: the implementation here uses Azure table storage for handling votes
            // in posts and user vote tables. Table storage are not transactional and there
            // can be instances where the vote count might be off.The table operations are
            // wrapped with retry policies in case of conflict or failures to minimize the risks.
            try
            {
#pragma warning disable CA1062 // post details are validated by model validations for null check and is responded with bad request status
                var userVoteForPost = await this.userVoteStorageProvider.GetUserVoteForPostAsync(this.UserAadId, postId);
#pragma warning restore CA1062 // post details are validated by model validations for null check and is responded with bad request status

                if (userVoteForPost == null)
                {
                    UserVoteEntity userVote = new UserVoteEntity
                    {
                        UserId = this.UserAadId,
                        IdeaId = postId,
                    };

                    await this.retryPolicy.ExecuteAsync(async () =>
                    {
                        isUserVoteSavedSuccessful = await this.AddUserVoteAsync(userVote);
                    });

                    if (!isUserVoteSavedSuccessful)
                    {
                        this.logger.LogError($"User vote is not updated successfully for post {postId} by {this.UserAadId} ");
                        return this.StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while saving user vote.");
                    }

                    // Retry if storage operation conflict occurs during updating user vote count.
                    await this.retryPolicy.ExecuteAsync(async () =>
                    {
                        isPostSavedSuccessful = await this.UpdateTotalCountAsync(postCreatedByUserId, postId, isUpvote: true);
                    });
                }
                else
                {
                    return this.BadRequest(new { message = $"Already voted for {nameof(postId)}" });
                }
            }
#pragma warning disable CA1031 // catching generic exception to trace error in telemetry and return false value to client
            catch (Exception ex)
#pragma warning restore CA1031 // catching generic exception to trace error in telemetry and return false value to client
            {
                this.logger.LogError(ex, "Exception occurred while updating user vote.");
            }
            finally
            {
                if (isPostSavedSuccessful)
                {
                    // run Azure search service to refresh the index for getting latest vote count
                    await this.teamIdeaSearchService.RunIndexerOnDemandAsync();
                }
                else
                {
                    // revert user vote entry if the post total count didn't saved successfully
                    this.logger.LogError($"Post vote count is not updated successfully for post {postId} by {this.UserAadId} ");

                    // exception handling is implemented in method and no additional check is required
                    var isUserVoteDeletedSuccessful = await this.userVoteStorageProvider.DeleteEntityAsync(postId, this.UserAadId);
                    if (isUserVoteDeletedSuccessful)
                    {
                        this.logger.LogInformation("Vote revoked from storage");
                    }
                    else
                    {
                        this.logger.LogError("Vote cannot be revoked from storage");
                    }
                }
            }

            return this.Ok(isPostSavedSuccessful);
        }

        /// <summary>
        /// Deletes user vote for an idea.
        /// </summary>
        /// <param name="postCreatedByUserId">AAD user Id of user who created idea.</param>
        /// <param name="postId">Id of the post to delete vote.</param>
        /// <remarks> Note: the implementation here uses Azure table storage for handling votes
        /// in posts and user vote tables. Table storage are not transactional and there
        /// can be instances where the vote count might be off. The table operations are
        /// wrapped with retry policies in case of conflict or failures to minimize the risks.</remarks>
        /// <returns>Returns true for successful operation.</returns>
        [HttpDelete]
        public async Task<IActionResult> DeleteVoteAsync(string postCreatedByUserId, string postId)
        {
            this.logger.LogInformation("call to delete user vote.");

            if (string.IsNullOrEmpty(postCreatedByUserId))
            {
                this.logger.LogError($"Error while deleting vote. Parameter {nameof(postCreatedByUserId)} is either null or empty.");
                return this.BadRequest(new { message = $"Parameter {nameof(postCreatedByUserId)} is either null or empty." });
            }

            if (string.IsNullOrEmpty(postId))
            {
                this.logger.LogError($"Error while deleting vote. {nameof(postId)} is either null or empty.");
                return this.BadRequest(new { message = $"{nameof(postId)} is either null or empty." });
            }

            bool isPostSavedSuccessful = false;
            bool isUserVoteDeletedSuccessful = false;

            // Note: the implementation here uses Azure table storage for handling votes
            // in posts and user vote tables.Table storage are not transactional and there
            // can be instances where the vote count might be off.The table operations are
            // wrapped with retry policies in case of conflict or failures to minimize the risks.
            try
            {
                isUserVoteDeletedSuccessful = await this.userVoteStorageProvider.DeleteEntityAsync(postId, this.UserAadId);

                if (!isUserVoteDeletedSuccessful)
                {
                    this.logger.LogError($"Vote is not updated successfully for post {postId} by {postCreatedByUserId} ");
                    return this.StatusCode(StatusCodes.Status500InternalServerError, "Vote is not updated successfully.");
                }

                // Retry if storage operation conflict occurs while updating post count.
                await this.retryPolicy.ExecuteAsync(async () =>
                {
                    isPostSavedSuccessful = await this.UpdateTotalCountAsync(postCreatedByUserId, postId, isUpvote: false);
                });
            }
#pragma warning disable CA1031 // catching generic exception to trace error in telemetry and return false value to client
            catch (Exception ex)
#pragma warning restore CA1031 // catching generic exception to trace error in telemetry and return false value to client
            {
                this.logger.LogError(ex, "Exception occurred while deleting the user vote count.");
            }
            finally
            {
                // if user vote is not saved successfully
                // revert back the total post count
                if (isPostSavedSuccessful)
                {
                    // run Azure search service to refresh the index for getting latest vote count
                    await this.teamIdeaSearchService.RunIndexerOnDemandAsync();
                }
                else
                {
                    UserVoteEntity userVote = new UserVoteEntity
                    {
                        UserId = this.UserAadId,
                        IdeaId = postId,
                    };

                    // add the user vote back to storage
                    await this.retryPolicy.ExecuteAsync(async () =>
                    {
                        await this.AddUserVoteAsync(userVote);
                    });
                }
            }

            return this.Ok(isPostSavedSuccessful);
        }

        /// <summary>
        /// Add user vote in store with retry attempts
        /// </summary>
        /// <param name="userVote">User vote instance with user and post id</param>
        /// <returns>True if operation executed successfully else false</returns>
        private async Task<bool> AddUserVoteAsync(UserVoteEntity userVote)
        {
            bool isUserVoteSavedSuccessful = false;
            try
            {
                // Update operation will throw exception if the column has already been updated
                // or if there is a transient error (handled by an Azure storage internally)
                isUserVoteSavedSuccessful = await this.userVoteStorageProvider.UpsertUserVoteAsync(userVote);
            }
            catch (StorageException ex)
            {
                if (ex.RequestInformation.HttpStatusCode == StatusCodes.Status412PreconditionFailed)
                {
                    this.logger.LogInformation("Optimistic concurrency violation – entity has changed since it was retrieved.");
                    throw;
                }
            }
#pragma warning disable CA1031 // catching generic exception to trace log error in telemetry and continue the execution
            catch (Exception ex)
#pragma warning restore CA1031 // catching generic exception to trace log error in telemetry and continue the execution
            {
                // log exception details to telemetry
                // but do not attempt to retry in order to avoid multiple vote count decrement
                this.logger.LogError(ex, "Exception occurred while reading post details.");
            }

            return isUserVoteSavedSuccessful;
        }

        /// <summary>
        /// Increment or decrement the total vote counts of post
        /// </summary>
        /// <param name="postCreatedByUserId">Post owner user object id</param>
        /// <param name="postId">Post unique id</param>
        /// <param name="isUpvote">Set true to increase total count by 1 else false</param>
        /// <returns>True if operation executed successfully else false</returns>
        private async Task<bool> UpdateTotalCountAsync(string postCreatedByUserId, string postId, bool isUpvote = false)
        {
            bool isPostSavedSuccessful = false;
            try
            {
                var postEntity = await this.teamIdeaStorageProvider.GetPostAsync(postCreatedByUserId, postId);

                postEntity.TotalVotes = isUpvote ? postEntity.TotalVotes + 1 : postEntity.TotalVotes - 1;

                if (postEntity.TotalVotes >= 0)
                {
                    isPostSavedSuccessful = await this.teamIdeaStorageProvider.UpsertIdeaAsync(postEntity);
                }
            }
            catch (StorageException ex)
            {
                if (ex.RequestInformation.HttpStatusCode == StatusCodes.Status412PreconditionFailed)
                {
                    this.logger.LogInformation("Optimistic concurrency violation – entity has changed since it was retrieved.");
                    throw;
                }
            }
#pragma warning disable CA1031 // catching generic exception to trace log error in telemetry and continue the execution
            catch (Exception ex)
#pragma warning restore CA1031 // catching generic exception to trace log error in telemetry and continue the execution
            {
                // log exception details to telemetry
                // but do not attempt to retry in order to avoid multiple vote count increment
                this.logger.LogError(ex, "Exception occurred while reading post details.");
            }

            return isPostSavedSuccessful;
        }
    }
}