// <copyright file="ITeamCategoryStorageProvider.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.SubmitIdea.Common.Interfaces
{
    using System.Threading.Tasks;
    using Microsoft.Teams.Apps.SubmitIdea.Models;

    /// <summary>
    /// Interface for provider which helps in storing, updating or deleting idea categories storage.
    /// </summary>
    public interface ITeamCategoryStorageProvider
    {
        /// <summary>
        /// Stores or update team categories data in storage.
        /// </summary>
        /// <param name="teamCategoryEntity">Holds team preference detail entity data.</param>
        /// <returns>A task that represents team preference entity data is saved or updated.</returns>
        Task<bool> UpsertTeamCategoriesAsync(TeamCategoryEntity teamCategoryEntity);

        /// <summary>
        /// Get team categories data from storage.
        /// </summary>
        /// <param name="teamId">Team id for which need to fetch data.</param>
        /// <returns>A task that represents to hold team categories data.</returns>
        Task<TeamCategoryEntity> GetTeamCategoriesDataAsync(string teamId);
    }
}
