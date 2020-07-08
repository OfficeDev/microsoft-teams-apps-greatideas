// <copyright file="UserVoteStorageProvider.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.SubmitIdea.Common.Providers
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Microsoft.Teams.Apps.SubmitIdea.Common.Interfaces;
    using Microsoft.Teams.Apps.SubmitIdea.Models;
    using Microsoft.Teams.Apps.SubmitIdea.Models.Configuration;
    using Microsoft.WindowsAzure.Storage.Table;

    /// <summary>
    /// Implements storage provider which helps to create, get, update or delete user vote data in Microsoft Azure Table storage.
    /// </summary>
    public class UserVoteStorageProvider : BaseStorageProvider, IUserVoteStorageProvider
    {
        /// <summary>
        /// Represents user vote entity name.
        /// </summary>
        private const string UserVoteEntityName = "UserVoteEntity";

        /// <summary>
        /// Initializes a new instance of the <see cref="UserVoteStorageProvider"/> class.
        /// Handles Microsoft Azure Table storage read write operations.
        /// </summary>
        /// <param name="options">A set of key/value application configuration properties for Microsoft Azure Table storage.</param>
        /// <param name="logger">Logger implementation to trace log in Azure Application Insights service.</param>
        public UserVoteStorageProvider(
            IOptions<StorageSettings> options,
            ILogger<BaseStorageProvider> logger)
            : base(options?.Value.ConnectionString, UserVoteEntityName, logger)
        {
            options = options ?? throw new ArgumentNullException(nameof(options));
        }

        /// <summary>
        /// Get all user votes from Microsoft Azure Table storage.
        /// </summary>
        /// <param name="userId">Represent Azure Active Directory id of user.</param>
        /// <returns>A task that represents a collection of user votes.</returns>
        public async Task<List<UserVoteEntity>> GetVotesAsync(string userId)
        {
            await this.EnsureInitializedAsync();

            if (string.IsNullOrWhiteSpace(userId))
            {
                return null;
            }

            string partitionKeyCondition = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, userId);

            List<UserVoteEntity> userVotes = new List<UserVoteEntity>();
            TableContinuationToken continuationToken = null;
            TableQuery<UserVoteEntity> query = new TableQuery<UserVoteEntity>().Where(partitionKeyCondition);

            do
            {
                var queryResult = await this.CloudTable.ExecuteQuerySegmentedAsync(query, null);
                if (queryResult?.Results != null)
                {
                    userVotes.AddRange(queryResult.Results);
                    continuationToken = queryResult.ContinuationToken;
                }
            }
            while (continuationToken != null);

            return userVotes;
        }

        /// <summary>
        /// Get user vote for post.
        /// </summary>
        /// <param name="userId">Represent Azure Active Directory id of user.</param>
        /// <param name="ideaId">Idea Id for which user has voted.</param>
        /// <returns>A task that represents a collection of user votes.</returns>
        public async Task<UserVoteEntity> GetUserVoteForPostAsync(string userId, string ideaId)
        {
            await this.EnsureInitializedAsync();
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(ideaId))
            {
                return null;
            }

            var retrieveOperation = TableOperation.Retrieve<UserVoteEntity>(userId, ideaId);
            var queryResult = await this.CloudTable.ExecuteAsync(retrieveOperation);

            if (queryResult?.Result != null)
            {
                return (UserVoteEntity)queryResult.Result;
            }

            return null;
        }

        /// <summary>
        /// Stores or update user votes data in Microsoft Azure Table storage.
        /// </summary>
        /// <param name="voteEntity">Holds user vote entity data.</param>
        /// <returns>A boolean that represents user vote entity is successfully saved/updated or not.</returns>
        public async Task<bool> UpsertUserVoteAsync(UserVoteEntity voteEntity)
        {
            var result = await this.StoreOrUpdateEntityAsync(voteEntity);
            return result.HttpStatusCode == (int)HttpStatusCode.NoContent;
        }

        /// <summary>
        /// Delete user vote data from storage.
        /// </summary>
        /// <param name="ideaId">Represents idea id.</param>
        /// <param name="userId">Represent Azure Active Directory id of user.</param>
        /// <returns>A boolean that represents user vote data is successfully deleted or not.</returns>
        public async Task<bool> DeleteEntityAsync(string ideaId, string userId)
        {
            var userVoteEntity = await this.GetUserVoteForPostAsync(userId, ideaId);
            if (userVoteEntity == null)
            {
                return false;
            }

            TableOperation deleteOperation = TableOperation.Delete(userVoteEntity);
            var result = await this.CloudTable.ExecuteAsync(deleteOperation);

            return result.HttpStatusCode == (int)HttpStatusCode.NoContent;
        }

        /// <summary>
        /// Stores or update user votes data in storage.
        /// </summary>
        /// <param name="voteEntity">Holds user vote entity data.</param>
        /// <returns>A task that represents user vote entity data is saved or updated.</returns>
        private async Task<TableResult> StoreOrUpdateEntityAsync(UserVoteEntity voteEntity)
        {
            await this.EnsureInitializedAsync();
            voteEntity = voteEntity ?? throw new ArgumentNullException(nameof(voteEntity));

            if (string.IsNullOrWhiteSpace(voteEntity.UserId) || string.IsNullOrWhiteSpace(voteEntity.IdeaId))
            {
                return null;
            }

            TableOperation addOrUpdateOperation = TableOperation.InsertOrReplace(voteEntity);

            return await this.CloudTable.ExecuteAsync(addOrUpdateOperation);
        }
    }
}
