// <copyright file="IUserVoteStorageProvider.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.SubmitIdea.Common.Interfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Teams.Apps.SubmitIdea.Models;

    /// <summary>
    /// Interface for provider which helps in storing, updating or deleting user votes for posts in storage.
    /// </summary>
    public interface IUserVoteStorageProvider
    {
        /// <summary>
        /// Stores or update user votes data in storage.
        /// </summary>
        /// <param name="voteEntity">Holds user vote entity data.</param>
        /// <returns>A task that represents user vote entity data is saved or updated.</returns>
        Task<bool> UpsertUserVoteAsync(UserVoteEntity voteEntity);

        /// <summary>
        /// Delete user vote data from storage.
        /// </summary>
        /// <param name="ideaId">Represent idea id.</param>
        /// <param name="userId">Represent Azure Active Directory id of user.</param>
        /// <returns>A task that represents user vote data is deleted.</returns>
        Task<bool> DeleteEntityAsync(string ideaId, string userId);

        /// <summary>
        /// Get all user votes from storage.
        /// </summary>
        /// <param name="userId">Represent Azure Active Directory id of user.</param>
        /// <returns>A task that represents a collection of user votes.</returns>
        Task<List<UserVoteEntity>> GetVotesAsync(string userId);

        /// <summary>
        /// Get user vote for post.
        /// </summary>
        /// <param name="userId">Represent Azure Active Directory id of user.</param>
        /// <param name="ideaId">Idea Id for which user has voted.</param>
        /// <returns>A task that represents a collection of user votes.</returns>
        Task<UserVoteEntity> GetUserVoteForPostAsync(string userId, string ideaId);
    }
}
