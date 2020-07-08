// <copyright file="TeamPreferenceStorageProvider.cs" company="Microsoft">
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
    /// Implements storage provider which helps to create, get or update team preferences data in Microsoft Azure Table storage.
    /// </summary>
    public class TeamPreferenceStorageProvider : BaseStorageProvider, ITeamPreferenceStorageProvider
    {
        /// <summary>
        /// Represents team preference entity name.
        /// </summary>
        private const string TeamPreferenceEntityName = "TeamPreferenceEntity";

        /// <summary>
        /// Initializes a new instance of the <see cref="TeamPreferenceStorageProvider"/> class.
        /// Handles Microsoft Azure Table storage read write operations.
        /// </summary>
        /// <param name="options">A set of key/value application configuration properties for Microsoft Azure Table storage.</param>
        /// <param name="logger">Logger implementation to send logs to the logger service.</param>
        public TeamPreferenceStorageProvider(
            IOptions<StorageSettings> options,
            ILogger<BaseStorageProvider> logger)
            : base(options?.Value.ConnectionString, TeamPreferenceEntityName, logger)
        {
            options = options ?? throw new ArgumentNullException(nameof(options));
        }

        /// <summary>
        /// Get team preference data from Microsoft Azure Table storage.
        /// </summary>
        /// <param name="teamId">Team Id for which need to fetch data.</param>
        /// <returns>A task that represents an object to hold team preference data.</returns>
        public async Task<TeamPreferenceEntity> GetTeamPreferenceDataAsync(string teamId)
        {
            await this.EnsureInitializedAsync();
            teamId = teamId ?? throw new ArgumentNullException(nameof(teamId));

            var operation = TableOperation.Retrieve<TeamPreferenceEntity>(teamId, teamId);
            var teamPreference = await this.CloudTable.ExecuteAsync(operation);

            return teamPreference.Result as TeamPreferenceEntity;
        }

        /// <summary>
        /// Get team preferences data from Microsoft Azure Table storage.
        /// </summary>
        /// <param name="digestFrequency">Digest frequency text for notification like Monthly/Weekly.</param>
        /// <returns>A task that represent collection to hold team preferences data.</returns>
        public async Task<IEnumerable<TeamPreferenceEntity>> GetTeamPreferencesAsync(DigestFrequency digestFrequency)
        {
            await this.EnsureInitializedAsync();
            var digestFrequencyCondition = TableQuery.GenerateFilterCondition(nameof(TeamPreferenceEntity.DigestFrequency), QueryComparisons.Equal, digestFrequency.ToString());

            TableQuery<TeamPreferenceEntity> query = new TableQuery<TeamPreferenceEntity>().Where(digestFrequencyCondition);
            TableContinuationToken continuationToken = null;
            var teamPreferenceCollection = new List<TeamPreferenceEntity>();

            do
            {
                var queryResult = await this.CloudTable.ExecuteQuerySegmentedAsync(query, continuationToken);

                if (queryResult?.Results != null)
                {
                    teamPreferenceCollection.AddRange(queryResult.Results);
                    continuationToken = queryResult.ContinuationToken;
                }
            }
            while (continuationToken != null);

            return teamPreferenceCollection;
        }

        /// <summary>
        /// Stores or update team preference data in Microsoft Azure Table storage.
        /// </summary>
        /// <param name="teamPreferenceEntity">Represents team preference entity object.</param>
        /// <returns>A boolean that represents team preference entity is successfully saved/updated or not.</returns>
        public async Task<bool> UpsertTeamPreferenceAsync(TeamPreferenceEntity teamPreferenceEntity)
        {
            var result = await this.StoreOrUpdateEntityAsync(teamPreferenceEntity);
            return result.HttpStatusCode == (int)HttpStatusCode.NoContent;
        }

        /// <summary>
        /// Stores or update team preference data in Microsoft Azure Table storage.
        /// </summary>
        /// <param name="teamPreferenceEntity">Holds team preference detail entity data.</param>
        /// <returns>A task that represents team preference entity data is saved or updated.</returns>
        private async Task<TableResult> StoreOrUpdateEntityAsync(TeamPreferenceEntity teamPreferenceEntity)
        {
            await this.EnsureInitializedAsync();
            teamPreferenceEntity = teamPreferenceEntity ?? throw new ArgumentNullException(nameof(teamPreferenceEntity));

            if (string.IsNullOrWhiteSpace(teamPreferenceEntity.DigestFrequency) || string.IsNullOrWhiteSpace(teamPreferenceEntity.Categories))
            {
                return null;
            }

            TableOperation addOrUpdateOperation = TableOperation.InsertOrReplace(teamPreferenceEntity);

            return await this.CloudTable.ExecuteAsync(addOrUpdateOperation);
        }
    }
}
