// <copyright file="UserVoteStorageHelper.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.SubmitIdea.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Microsoft.Teams.Apps.SubmitIdea.Common.Interfaces;
    using Microsoft.Teams.Apps.SubmitIdea.Models;

    /// <summary>
    /// Implements user vote storage helper which is responsible for storing, updating or deleting user vote data in storage.
    /// </summary>
    public class UserVoteStorageHelper : IUserVoteStorageHelper
    {
        /// <summary>
        /// Logger implementation to send logs to the logger service.
        /// </summary>
        private readonly ILogger<UserVoteStorageHelper> logger;

        /// <summary>
        /// Storage provider for working with user vote data in storage.
        /// </summary>
        private readonly IUserVoteStorageProvider userVoteStorageProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserVoteStorageHelper"/> class.
        /// </summary>
        /// <param name="logger">Logger implementation to send logs to the logger service.</param>
        /// <param name="userVoteStorageProvider">User vote storage provider dependency injection.</param>
        public UserVoteStorageHelper(
            ILogger<UserVoteStorageHelper> logger,
            IUserVoteStorageProvider userVoteStorageProvider)
        {
            this.logger = logger;
            this.userVoteStorageProvider = userVoteStorageProvider;
        }

        /// <summary>
        /// Store user vote details to storage.
        /// </summary>
        /// <param name="userVoteEntity">Represents user vote entity object.</param>
        /// <returns>A task that represents user vote entity data is added.</returns>
        public async Task<bool> AddUserVoteDetailsAsync(UserVoteEntity userVoteEntity)
        {
            try
            {
                userVoteEntity = userVoteEntity ?? throw new ArgumentNullException(nameof(userVoteEntity));

                if (userVoteEntity == null)
                {
                    return false;
                }

                return await this.userVoteStorageProvider.UpsertUserVoteAsync(userVoteEntity);
            }
            catch (Exception ex)
            {
                this.logger.LogError("Exception occurred while adding the user vote.", ex);
                throw;
            }
        }

        /// <summary>
        /// Delete user vote details to storage.
        /// </summary>
        /// <param name="postId">Represent a post id.</param>
        /// <param name="userId">Represent Azure Active Directory id of user.</param>
        /// <returns>A task that represents user vote entity data is deleted.</returns>
        public async Task<bool> DeleteUserVoteDetailsAsync(string postId, string userId)
        {
            try
            {
                if (string.IsNullOrEmpty(postId))
                {
                    return false;
                }

                return await this.userVoteStorageProvider.DeleteEntityAsync(postId, userId);
            }
            catch (Exception ex)
            {
                this.logger.LogError("Exception occurred while deleting the user vote.", ex);
                throw;
            }
        }

        /// <summary>
        /// Get all user votes from storage.
        /// </summary>
        /// <param name="userId">Represent Azure Active Directory id of user.</param>
        /// <returns>List of user votes.</returns>
        public async Task<IEnumerable<UserVoteEntity>> GetVotesAsync(string userId)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    return null;
                }

                return await this.userVoteStorageProvider.GetVotesAsync(userId);
            }
            catch (Exception ex)
            {
                this.logger.LogError("Exception occurred while getting the user votes.", ex);
                throw;
            }
        }
    }
}
