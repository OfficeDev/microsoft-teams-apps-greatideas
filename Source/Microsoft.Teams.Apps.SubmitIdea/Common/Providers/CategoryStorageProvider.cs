// <copyright file="CategoryStorageProvider.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.SubmitIdea.Common.Providers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Microsoft.Teams.Apps.SubmitIdea.Common.Interfaces;
    using Microsoft.Teams.Apps.SubmitIdea.Models;
    using Microsoft.Teams.Apps.SubmitIdea.Models.Configuration;
    using Microsoft.WindowsAzure.Storage.Table;

    /// <summary>
    ///  Implements storage provider which helps to add, edit, delete idea category data in Microsoft Azure Table storage
    /// </summary>
    public class CategoryStorageProvider : BaseStorageProvider, ICategoryStorageProvider
    {
        /// <summary>
        /// Represents idea category entity name.
        /// </summary>
        private const string CategoryTable = "CategoryEntity";

        /// <summary>
        /// Sets the batch size of table operation.
        /// </summary>
        private const int CategoryTableOperationBatchLimit = 10;

        /// <summary>
        /// Initializes a new instance of the <see cref="CategoryStorageProvider"/> class.
        /// </summary>
        /// <param name="options">A set of key/value application configuration properties for storage.</param>
        /// <param name="logger">Logger implementation to send logs to the logger service.</param>
        public CategoryStorageProvider(
            IOptions<StorageSettings> options,
            ILogger<CategoryStorageProvider> logger)
            : base(options?.Value.ConnectionString, CategoryTable, logger)
        {
        }

        /// <summary>
        /// This method is used to get all categories.
        /// </summary>
        /// <returns>list of all category.</returns>
        public async Task<IEnumerable<CategoryEntity>> GetCategoriesAsync()
        {
            await this.EnsureInitializedAsync();
            string filter = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, CategoryEntity.CategoryPartitionKey);
            var query = new TableQuery<CategoryEntity>().Where(filter);
            TableContinuationToken continuationToken = null;
            var categories = new List<CategoryEntity>();

            do
            {
                var queryResult = await this.CloudTable.ExecuteQuerySegmentedAsync(query, continuationToken);
                categories.AddRange(queryResult?.Results);
                continuationToken = queryResult?.ContinuationToken;
            }
            while (continuationToken != null);

            return categories.OrderByDescending(category => category.Timestamp);
        }

        /// <summary>
        /// This method is used to fetch category details for a given category Id.
        /// </summary>
        /// <param name="categoryId">Category Id.</param>
        /// <returns>Category details.</returns>
        public async Task<CategoryEntity> GetCategoryDetailsAsync(string categoryId)
        {
            await this.EnsureInitializedAsync();

            if (string.IsNullOrWhiteSpace(categoryId))
            {
                return null;
            }

            var operation = TableOperation.Retrieve<CategoryEntity>(CategoryEntity.CategoryPartitionKey, categoryId);
            var category = await this.CloudTable.ExecuteAsync(operation);
            return category.Result as CategoryEntity;
        }

        /// <summary>
        /// This method is used to get category details by ids.
        /// </summary>
        /// <param name="categoryIds">List of idea category ids.</param>
        /// <returns>list of all category.</returns>
        public async Task<IEnumerable<CategoryEntity>> GetCategoriesByIdsAsync(IEnumerable<string> categoryIds)
        {
            await this.EnsureInitializedAsync();

            categoryIds = categoryIds ?? throw new ArgumentNullException(nameof(categoryIds));

            // The max supported categories are 10 and can be executed
            // If the categories are expected to increase further, then
            // it is recommended to execute it in batches.
            string categoriesCondition = this.CreateCategoriesFilter(categoryIds);

            TableQuery<CategoryEntity> query = new TableQuery<CategoryEntity>().Where(categoriesCondition);
            TableContinuationToken continuationToken = null;
            var categoryCollection = new List<CategoryEntity>();
            do
            {
                var queryResult = await this.CloudTable.ExecuteQuerySegmentedAsync(query, continuationToken);
                if (queryResult?.Results != null)
                {
                    categoryCollection.AddRange(queryResult.Results);
                    continuationToken = queryResult.ContinuationToken;
                }
            }
            while (continuationToken != null);

            return categoryCollection;
        }

        /// <summary>
        /// Add or update category in table storage.
        /// </summary>
        /// <param name="categoryEntity">represents the category entity that needs to be stored or updated.</param>
        /// <returns>category entity that is added or updated.</returns>
        public async Task<CategoryEntity> AddOrUpdateCategoryAsync(CategoryEntity categoryEntity)
        {
            await this.EnsureInitializedAsync();

            categoryEntity = categoryEntity ?? throw new ArgumentNullException(nameof(categoryEntity));

            if (string.IsNullOrWhiteSpace(categoryEntity.CategoryName) || string.IsNullOrWhiteSpace(categoryEntity.CategoryDescription))
            {
                return null;
            }

            TableOperation addOrUpdateOperation = TableOperation.InsertOrReplace(categoryEntity);
            var result = await this.CloudTable.ExecuteAsync(addOrUpdateOperation);
            return result.Result as CategoryEntity;
        }

        /// <summary>
        /// This method is used to delete categories for provided category Ids.
        /// </summary>
        /// <param name="categoryEntities">List of category entities.</param>
        /// <returns>boolean result.</returns>
        public async Task<bool> DeleteCategoriesAsync(IEnumerable<CategoryEntity> categoryEntities)
        {
            await this.EnsureInitializedAsync();

            categoryEntities = categoryEntities ?? throw new ArgumentNullException(nameof(categoryEntities));

            TableBatchOperation tableOperation;
            int batchCount = (int)Math.Ceiling((double)categoryEntities.Count() / CategoryTableOperationBatchLimit);

            for (int batchIndex = 0; batchIndex < batchCount; batchIndex++)
            {
                tableOperation = new TableBatchOperation();
                var categoryEntitiesBatch = categoryEntities
                    .Skip(batchIndex * CategoryTableOperationBatchLimit)
                    .Take(CategoryTableOperationBatchLimit);

                foreach (var category in categoryEntitiesBatch)
                {
                    tableOperation.Delete(category);
                }

                if (tableOperation.Count > 0)
                {
                    await this.CloudTable.ExecuteBatchAsync(tableOperation);
                }
            }

            return true;
        }

        /// <summary>
        /// Get combined filter condition for user private ideas data.
        /// </summary>
        /// <param name="categoryIds">List of user private idea id.</param>
        /// <returns>Returns combined filter for user private ideas.</returns>
        private string CreateCategoriesFilter(IEnumerable<string> categoryIds)
        {
            var categoryIdConditions = new List<string>();
            StringBuilder combinedCaregoryIdsFilter = new StringBuilder();

            categoryIds = categoryIds.Where(s => !string.IsNullOrWhiteSpace(s)).Distinct();

            foreach (var categoryId in categoryIds)
            {
                categoryIdConditions.Add("(" + TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, categoryId) + ")");
            }

            if (categoryIdConditions.Count >= 2)
            {
                var categories = categoryIdConditions.Take(categoryIdConditions.Count - 1).ToList();

                categories.ForEach(postCondition =>
                {
                    combinedCaregoryIdsFilter.Append($"{postCondition} {"or"} ");
                });

                combinedCaregoryIdsFilter.Append($"{categoryIdConditions.Last()}");

                return combinedCaregoryIdsFilter.ToString();
            }
            else
            {
                return TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, categoryIds.FirstOrDefault());
            }
        }
    }
}
