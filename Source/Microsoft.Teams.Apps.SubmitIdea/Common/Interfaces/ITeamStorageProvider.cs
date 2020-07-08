// <copyright file="ITeamStorageProvider.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.SubmitIdea.Common.Interfaces
{
    using System.Threading.Tasks;
    using Microsoft.Teams.Apps.SubmitIdea.Models;

    /// <summary>
    /// Interface for team storage provider.
    /// </summary>
    public interface ITeamStorageProvider
    {
        /// <summary>
        /// Store or update team details in the storage.
        /// </summary>
        /// <param name="teamEntity">Represents team entity used for storage and retrieval.</param>
        /// <returns><see cref="Task"/> Returns the status whether team entity is stored or not.</returns>
        Task<bool> StoreOrUpdateTeamDetailAsync(TeamEntity teamEntity);

        /// <summary>
        /// Get already saved team entity from the storage.
        /// </summary>
        /// <param name="teamId">Team Id.</param>
        /// <returns><see cref="Task"/>Returns team entity.</returns>
        Task<TeamEntity> GetTeamDetailAsync(string teamId);

        /// <summary>
        /// This method delete the team detail record from the storage.
        /// </summary>
        /// <param name="teamEntity">Team configuration entity.</param>
        /// <returns>A <see cref="Task"/> of type bool where true represents entity record is successfully deleted from the storage while false indicates failure in deleting data.</returns>
        Task<bool> DeleteTeamDetailAsync(TeamEntity teamEntity);
    }
}
