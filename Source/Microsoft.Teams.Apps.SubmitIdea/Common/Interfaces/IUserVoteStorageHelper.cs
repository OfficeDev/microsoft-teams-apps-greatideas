// <copyright file="IUserVoteStorageHelper.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.SubmitIdea.Common.Interfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Teams.Apps.SubmitIdea.Models;

    /// <summary>
    /// Interface for storage helper which helps in storing, updating or deleting user vote data in storage.
    /// </summary>
    public interface IUserVoteStorageHelper
    {
        /// <summary>
        /// Store user vote details to storage.
        /// </summary>
        /// <param name="userVoteEntity">User vote entity object.</param>
        /// <returns>A task that represents user vote entity data is added.</returns>
        Task<bool> AddUserVoteDetailsAsync(UserVoteEntity userVoteEntity);

        /// <summary>
        /// Delete user vote data from storage.
        /// </summary>
        /// <param name="postId">Represent a post id.</param>
        /// <param name="userId">Represent Azure Active Directory id of user.</param>
        /// <returns>A task that represents user vote data is deleted.</returns>
        Task<bool> DeleteUserVoteDetailsAsync(string postId, string userId);

        /// <summary>
        /// Get all user votes from storage.
        /// </summary>
        /// <param name="userId">Represent Azure Active Directory id of user.</param>
        /// <returns>List of user votes.</returns>
        Task<IEnumerable<UserVoteEntity>> GetVotesAsync(string userId);
    }
}
