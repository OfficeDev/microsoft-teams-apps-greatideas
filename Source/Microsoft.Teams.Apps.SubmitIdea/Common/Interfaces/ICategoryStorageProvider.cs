// <copyright file="ICategoryStorageProvider.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.SubmitIdea.Common.Interfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Teams.Apps.SubmitIdea.Models;

    /// <summary>
    /// Interface for manage category storage operations, which helps add/edit/delete categories.
    /// </summary>
    public interface ICategoryStorageProvider
    {
        /// <summary>
        /// Add or update category in the storage.
        /// </summary>
        /// <param name="categoryEntity">Represents the category entity that needs to be stored or updated.</param>
        /// <returns>Category entity that is added or updated.</returns>
        Task<CategoryEntity> AddOrUpdateCategoryAsync(CategoryEntity categoryEntity);

        /// <summary>
        /// This method is used to delete categories for provided category Ids.
        /// </summary>
        /// <param name="categoryEntities">list of category entities that needs to be deleted.</param>
        /// <returns>boolean result.</returns>
        Task<bool> DeleteCategoriesAsync(IEnumerable<CategoryEntity> categoryEntities);

        /// <summary>
        /// This method is used to get all categories.
        /// </summary>
        /// <returns>list of all category entities.</returns>
        Task<IEnumerable<CategoryEntity>> GetCategoriesAsync();

        /// <summary>
        /// This method is used to get category details by id.
        /// </summary>
        /// <param name="categoryIds">Semicolon separated unique category ids that needs to be deleted.</param>
        /// <returns>list of all category entities.</returns>
        Task<IEnumerable<CategoryEntity>> GetCategoriesByIdsAsync(IEnumerable<string> categoryIds);

        /// <summary>
        /// This method is used to fetch category details for a given category Id.
        /// </summary>
        /// <param name="categoryId">Category Id.</param>
        /// <returns>Category details.</returns>
        Task<CategoryEntity> GetCategoryDetailsAsync(string categoryId);
    }
}