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
    using global::Azure.Data.Tables;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Microsoft.Teams.Apps.SubmitIdea.Common.Interfaces;
    using Microsoft.Teams.Apps.SubmitIdea.Models;
    using Microsoft.Teams.Apps.SubmitIdea.Models.Configuration;

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
            var filter =
                TableClient.CreateQueryFilter<CategoryEntity>(
                    e => e.PartitionKey == CategoryEntity.CategoryPartitionKey);

            var entities = await this.Table.QueryAsync<CategoryEntity>(filter).ToListAsync();
            return entities.OrderByDescending(category => category.Timestamp);
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

            var response =
                await this.Table.GetEntityAsync<CategoryEntity>(CategoryEntity.CategoryPartitionKey, categoryId);
            return response.Value;
        }

        /// <summary>
        /// This method is used to get category details by ids.
        /// </summary>
        /// <param name="format"> Add I formatter</param>
        /// <param name="categoryIds">List of idea category ids.</param>
        /// <returns>list of all category.</returns>
        public async Task<IEnumerable<CategoryEntity>> GetCategoriesByIdsAsync(IFormatProvider format, IEnumerable<string> categoryIds)
        {
            await this.EnsureInitializedAsync();

            categoryIds = categoryIds ?? throw new ArgumentNullException(nameof(categoryIds));

            // The max supported categories are 10 and can be executed
            // If the categories are expected to increase further, then
            // it is recommended to execute it in batches.
            var categoriesCondition = this.CreateCategoriesFilter(format, categoryIds);

            return await this.Table.QueryAsync<CategoryEntity>(categoriesCondition).ToListAsync();
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

            var result = await this.Table.UpsertEntityAsync(categoryEntity);
            if (result.IsError)
            {
                throw new ApplicationException("Unable to update the entity");
            }

            return categoryEntity;
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

            foreach (var category in categoryEntities)
            {
                await this.Table.DeleteEntityAsync(CategoryEntity.CategoryPartitionKey, category.RowKey);
            }

            return true;
        }

        /// <summary>
        /// Get combined filter condition for user private ideas data.
        /// </summary>
        /// <param name="format">format provider</param>
        /// <param name="categoryIds">List of user private idea id.</param>
        /// <returns>Returns combined filter for user private ideas.</returns>
        private string CreateCategoriesFilter(IFormatProvider format, IEnumerable<string> categoryIds)
        {
            var combinedCategoryIdsFilter = new StringBuilder();

            categoryIds = categoryIds.Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToArray();

            var categoryIdConditions = categoryIds
                .Select(categoryId => $"({TableClient.CreateQueryFilter<CategoryEntity>(e => e.RowKey == categoryId)})")
                .ToList();

            if (categoryIdConditions.Count >= 2)
            {
                var categories = categoryIdConditions.Take(categoryIdConditions.Count - 1).ToList();
                categories.ForEach(postCondition =>
                {
                    combinedCategoryIdsFilter.Append(format, $"{postCondition} {"or"} ");
                });

                combinedCategoryIdsFilter.Append(format, $"{categoryIdConditions.Last()}");

                return combinedCategoryIdsFilter.ToString();
            }
            else
            {
                return TableClient.CreateQueryFilter<CategoryEntity>(e => e.RowKey == categoryIds.FirstOrDefault());
            }
        }
    }
}
