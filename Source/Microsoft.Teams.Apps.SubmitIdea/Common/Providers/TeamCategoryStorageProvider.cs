// <copyright file="TeamCategoryStorageProvider.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.SubmitIdea.Common.Providers
{
    using System;
    using System.Net;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Microsoft.Teams.Apps.SubmitIdea.Common.Interfaces;
    using Microsoft.Teams.Apps.SubmitIdea.Models;
    using Microsoft.Teams.Apps.SubmitIdea.Models.Configuration;
    using Microsoft.WindowsAzure.Storage.Table;

    /// <summary>
    /// Implements storage provider which helps to create, get, update or delete team tags data in storage.
    /// </summary>
    public class TeamCategoryStorageProvider : BaseStorageProvider, ITeamCategoryStorageProvider
    {
        /// <summary>
        /// Represents team tag entity name.
        /// </summary>
        private const string TeamCategoryEntityName = "TeamCategoryEntity";

        /// <summary>
        /// Initializes a new instance of the <see cref="TeamCategoryStorageProvider"/> class.
        /// Handles storage read write operations.
        /// </summary>
        /// <param name="options">A set of key/value application configuration properties for storage.</param>
        /// <param name="logger">Logger implementation to send logs to the logger service.</param>
        public TeamCategoryStorageProvider(
            IOptions<StorageSettings> options,
            ILogger<BaseStorageProvider> logger)
            : base(options?.Value.ConnectionString, TeamCategoryEntityName, logger)
        {
            options = options ?? throw new ArgumentNullException(nameof(options));
        }

        /// <summary>
        /// Get idea categories for team configuration.
        /// </summary>
        /// <param name="teamId">Team id for which need to fetch data.</param>
        /// <returns>A task that represents an object to hold team tags data.</returns>
        public async Task<TeamCategoryEntity> GetTeamCategoriesDataAsync(string teamId)
        {
            await this.EnsureInitializedAsync();
            teamId = teamId ?? throw new ArgumentNullException(nameof(teamId));

            var operation = TableOperation.Retrieve<TeamCategoryEntity>(teamId, teamId);
            var teamCategoryEntity = await this.CloudTable.ExecuteAsync(operation);

            return teamCategoryEntity.Result as TeamCategoryEntity;
        }

        /// <summary>
        /// Stores or update team tags data in storage.
        /// </summary>
        /// <param name="teamCategoryEntity">Represents team tag entity object.</param>
        /// <returns>A boolean that represents team tags entity is successfully saved/updated or not.</returns>
        public async Task<bool> UpsertTeamCategoriesAsync(TeamCategoryEntity teamCategoryEntity)
        {
            var result = await this.StoreOrUpdateEntityAsync(teamCategoryEntity);

            return result.HttpStatusCode == (int)HttpStatusCode.NoContent;
        }

        /// <summary>
        /// Stores or update team tags data in storage.
        /// </summary>
        /// <param name="teamCategoryEntity">Represents team tag entity object.</param>
        /// <returns>A task that represents team tags entity data is saved or updated.</returns>
        private async Task<TableResult> StoreOrUpdateEntityAsync(TeamCategoryEntity teamCategoryEntity)
        {
            await this.EnsureInitializedAsync();
            teamCategoryEntity = teamCategoryEntity ?? throw new ArgumentNullException(nameof(teamCategoryEntity));

            if (string.IsNullOrWhiteSpace(teamCategoryEntity.Categories) || string.IsNullOrWhiteSpace(teamCategoryEntity.ChannelId))
            {
                return null;
            }

            TableOperation addOrUpdateOperation = TableOperation.InsertOrReplace(teamCategoryEntity);

            return await this.CloudTable.ExecuteAsync(addOrUpdateOperation);
        }
    }
}
