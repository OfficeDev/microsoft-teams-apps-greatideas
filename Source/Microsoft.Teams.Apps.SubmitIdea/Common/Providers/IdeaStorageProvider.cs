// <copyright file="IdeaStorageProvider.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.SubmitIdea.Common.Providers
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Microsoft.Teams.Apps.SubmitIdea.Common.Interfaces;
    using Microsoft.Teams.Apps.SubmitIdea.Models;
    using Microsoft.Teams.Apps.SubmitIdea.Models.Configuration;
    using Microsoft.WindowsAzure.Storage.Table;

    /// <summary>
    /// Implements storage provider which helps to create, get, update or delete team idea data in Microsoft Azure Table storage.
    /// </summary>
    public class IdeaStorageProvider : BaseStorageProvider, IIdeaStorageProvider
    {
        /// <summary>
        /// Sets the max length of the title.
        /// </summary>
        private const int IdeaTitleMaxLength = 200;

        /// <summary>
        /// Sets the max length of the idea description.
        /// </summary>
        private const int IdeaDescriptionMaxLength = 500;

        /// <summary>
        /// Initializes a new instance of the <see cref="IdeaStorageProvider"/> class.
        /// Handles Microsoft Azure Table storage read write operations.
        /// </summary>
        /// <param name="options">A set of key/value application configuration properties for Microsoft Azure Table storage.</param>
        /// <param name="logger">Logger implementation to send logs to the logger service.</param>
        public IdeaStorageProvider(
            IOptions<StorageSettings> options,
            ILogger<BaseStorageProvider> logger)
            : base(options?.Value.ConnectionString, Constants.IdeaEntityTableName, logger)
        {
            options = options ?? throw new ArgumentNullException(nameof(options));
        }

        /// <summary>
        /// Stores or update team idea details data in Microsoft Azure Table storage.
        /// </summary>
        /// <param name="ideaEntity">Holds team idea detail entity data.</param>
        /// <returns>A boolean that represents team idea entity data is successfully saved/updated or not.</returns>
        public async Task<bool> UpsertIdeaAsync(IdeaEntity ideaEntity)
        {
            var result = await this.StoreOrUpdateEntityAsync(ideaEntity);

            return result.HttpStatusCode == (int)HttpStatusCode.NoContent;
        }

        /// <summary>
        /// Get team idea data from Microsoft Azure Table storage.
        /// </summary>
        /// <param name="createdByUserId">Azure Active Directory id of author who created the idea.</param>
        /// <param name="ideaId">Idea id to fetch the idea details.</param>
        /// <returns>A task that represent a object to hold idea details.</returns>
        public async Task<IdeaEntity> GetIdeaEntityAsync(string createdByUserId, string ideaId)
        {
            // When there is no team post created by user and Messaging Extension is open, table initialization is required here before creating search index or data source or indexer.
            await this.EnsureInitializedAsync();

            if (string.IsNullOrEmpty(ideaId) || string.IsNullOrEmpty(createdByUserId))
            {
                return null;
            }

            var operation = TableOperation.Retrieve<IdeaEntity>(ideaId, createdByUserId);
            var data = await this.CloudTable.ExecuteAsync(operation);

            return data.Result as IdeaEntity;
        }

        /// <summary>
        /// Get team idea data from Microsoft Azure Table storage.
        /// </summary>
        /// <param name="ideaId">Idea id to fetch the idea details.</param>
        /// <returns>A task that represent a object to hold idea details.</returns>
        public async Task<IdeaEntity> GetIdeaEntityAsync(string ideaId)
        {
            // When there is no team post created by user and Messaging Extension is open, table initialization is required here before creating search index or data source or indexer.
            await this.EnsureInitializedAsync();

            if (string.IsNullOrEmpty(ideaId))
            {
                return null;
            }

            string partitionKeyCondition = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, ideaId);
            TableQuery<IdeaEntity> query = new TableQuery<IdeaEntity>().Where(partitionKeyCondition);
            var queryResult = await this.CloudTable.ExecuteQuerySegmentedAsync(query, null);

            return queryResult?.FirstOrDefault();
        }

        /// <summary>
        /// Get idea details.
        /// </summary>
        /// <param name="createdByUserId">Azure Active Directory id of author who created the idea.</param>
        /// <param name="ideaId">Post id to fetch the post details.</param>
        /// <returns>A task that represent a object to hold idea details.</returns>
        public async Task<IdeaEntity> GetPostAsync(string createdByUserId, string ideaId)
        {
            // When there is no post created by user and Messaging Extension is open, table initialization is required here before creating search index or data source or indexer.
            await this.EnsureInitializedAsync();

            if (string.IsNullOrEmpty(ideaId) || string.IsNullOrEmpty(createdByUserId))
            {
                return null;
            }

            var operation = TableOperation.Retrieve<IdeaEntity>(ideaId, createdByUserId);
            var data = await this.CloudTable.ExecuteAsync(operation);

            return data.Result as IdeaEntity;
        }

        /// <summary>
        /// Stores or update team idea details data in Microsoft Azure Table storage.
        /// </summary>
        /// <param name="entity">Holds team idea detail entity data.</param>
        /// <returns>A task that represents idea post entity data is saved or updated.</returns>
        private async Task<TableResult> StoreOrUpdateEntityAsync(IdeaEntity entity)
        {
            await this.EnsureInitializedAsync();
            entity = entity ?? throw new ArgumentNullException(nameof(entity));

            if (string.IsNullOrWhiteSpace(entity.Title)
                || string.IsNullOrWhiteSpace(entity.Description)
                || string.IsNullOrWhiteSpace(entity.Category)
                || entity?.Title.Length > IdeaTitleMaxLength
                || entity?.Description.Length > IdeaDescriptionMaxLength)
            {
                return null;
            }

            TableOperation addOrUpdateOperation = TableOperation.InsertOrReplace(entity);

            return await this.CloudTable.ExecuteAsync(addOrUpdateOperation);
        }
    }
}
