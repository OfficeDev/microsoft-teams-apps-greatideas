// <copyright file="CategoryController.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.SubmitIdea.Controllers
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.ApplicationInsights;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Microsoft.Teams.Apps.SubmitIdea.Authentication;
    using Microsoft.Teams.Apps.SubmitIdea.Common.Interfaces;
    using Microsoft.Teams.Apps.SubmitIdea.Models;

    /// <summary>
    /// This endpoint is used to manage categories.
    /// </summary>
    [Route("api/category")]
    [ApiController]
    [Authorize]
    public class CategoryController : BaseSubmitIdeaController
    {
        /// <summary>
        /// Instance to send logs to the logger service.
        /// </summary>
        private readonly ILogger<CategoryController> logger;

        /// <summary>
        /// Provider for managing categories from the storage.
        /// </summary>
        private readonly ICategoryStorageProvider storageProvider;

        /// <summary>
        /// Instance of Search service for working with storage.
        /// </summary>
        private readonly IIdeaSearchService teamIdeaSearchService;

        /// <summary>
        /// Initializes a new instance of the <see cref="CategoryController"/> class.
        /// </summary>
        /// <param name="categoryStorageProvider">Category storage provider instance.</param>
        /// <param name="teamIdeaSearchService">Idea search service provider instance.</param>
        /// <param name="logger">Logger implementation to send logs to the logger service.</param>
        /// <param name="telemetryClient">The Application Insights telemetry client.</param>
        public CategoryController(
            ICategoryStorageProvider categoryStorageProvider,
            IIdeaSearchService teamIdeaSearchService,
            ILogger<CategoryController> logger,
            TelemetryClient telemetryClient)
            : base(telemetryClient)
        {
            this.logger = logger;
            this.storageProvider = categoryStorageProvider;
            this.teamIdeaSearchService = teamIdeaSearchService;
        }

        /// <summary>
        /// This method is used to get all categories.
        /// </summary>
        /// <returns>categories.</returns>
        [HttpGet("allcategories")]
        public async Task<IActionResult> GetCategoriesAsync()
        {
            this.RecordEvent("Categories - HTTP Get categories call accepted the request.");

            try
            {
                this.logger.LogInformation($"{nameof(this.GetCategoriesAsync)} is called to get all categories.");
                var categories = await this.storageProvider.GetCategoriesAsync();
                return this.Ok(categories);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"An exception occurred to get categories");
                this.RecordEvent("An exception occurred for categories - HTTP Get categories call.");
                throw;
            }
        }

        /// <summary>
        /// Post call to add or edit category record in storage.
        /// Only curators have permissions to perform POST action.
        /// </summary>
        /// <param name="categoryEntity">category entity to be added or updated.</param>
        /// <returns>category entity instance with new/updated records.</returns>
        [HttpPost]
        [Authorize(PolicyNames.MustBeCuratorTeamMemberUserPolicy)]
        public async Task<IActionResult> PostAsync([FromBody] CategoryEntity categoryEntity)
        {
            this.RecordEvent("Categories - HTTP Post call accepted the request.");

            try
            {
                var newCategoryEntity = new CategoryEntity
                {
                    CategoryId = Guid.NewGuid().ToString(),
                    CreatedOn = DateTime.UtcNow,
                    CreatedByUserId = this.UserAadId,
#pragma warning disable CA1062 // category entity is validated by model validations for null check and is responded with bad request status
                    CategoryName = categoryEntity.CategoryName,
#pragma warning restore CA1062 // category entity is validated by model validations for null check and is responded with bad request status
                    CategoryDescription = categoryEntity.CategoryDescription,
                };

                return this.Ok(await this.storageProvider.AddOrUpdateCategoryAsync(newCategoryEntity));
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error while adding/updating the category.");
                this.RecordEvent("HTTP POST call failed while adding/updating the category.");
                throw;
            }
        }

        /// <summary>
        /// Patch call to update category details in storage.
        /// </summary>
        /// <param name="categoryEntity">Holds category detail entity data.</param>
        /// <returns>Returns true for successful operation or else false.</returns>
        [HttpPatch]
        [Authorize(PolicyNames.MustBeCuratorTeamMemberUserPolicy)]
        public async Task<IActionResult> PatchAsync([FromBody] CategoryEntity categoryEntity)
        {
            this.RecordEvent("Categories - HTTP Patch categories call accepted the request.");

            try
            {
                // Validating category Id as it will be generated at server side in case of adding new category but cannot be null or empty in case of update.
#pragma warning disable CA1062 // category entity is validated by model validations for null check and is responded with bad request status
                var currentCategory = await this.storageProvider.GetCategoryDetailsAsync(categoryEntity.CategoryId);
#pragma warning restore CA1062 // category entity is validated by model validations for null check and is responded with bad request status
                if (currentCategory == null)
                {
                    this.logger.LogError($"Entity for {currentCategory.CategoryId} is not found to update.");
                    this.RecordEvent("Update category - HTTP Patch call failed");

                    return this.BadRequest($"An update cannot be performed for {currentCategory.CategoryId} because entity is not available.");
                }

                currentCategory.ModifiedByUserId = this.UserAadId;
                currentCategory.CategoryDescription = categoryEntity.CategoryDescription;
                currentCategory.CategoryName = categoryEntity.CategoryName;

                return this.Ok(await this.storageProvider.AddOrUpdateCategoryAsync(currentCategory));
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error while making call to team category service.");
                throw;
            }
        }

        /// <summary>
        /// Delete call to delete categories for provided category Ids.
        /// Only curators have permissions to perform delete action.
        /// </summary>
        /// <param name="categoryIds">Semicolon separated unique category ids that needs to be deleted.</param>
        /// <returns>Returns true for successful operation or else false.</returns>
        [HttpDelete("categories")]
        [Authorize(PolicyNames.MustBeCuratorTeamMemberUserPolicy)]
        public async Task<IActionResult> DeleteAsync(string categoryIds)
        {
            this.RecordEvent("Categories - HTTP Delete call accepted the request.");

            try
            {
                if (string.IsNullOrEmpty(categoryIds))
                {
                    this.logger.LogError($"Semicolon separated {nameof(categoryIds)} string is found to null or empty while {nameof(this.DeleteAsync)}");
                    return this.BadRequest($"{nameof(categoryIds)} cannot be null or empty.");
                }

                var categories = categoryIds.Split(",");
                var categoryEntities = await this.storageProvider.GetCategoriesByIdsAsync(categories);

                // check if the number of categories passed are all available for delete in storage
                // failure to get any mismatch will lead to bad request
                if (categories.Length != categoryEntities.Count())
                {
                    return this.BadRequest($"Either one or more {nameof(categoryIds)} are not available for delete operation.");
                }

                // get the list of categories which are in use by any approved/pending ideas
                var categoriesInUse = await this.teamIdeaSearchService.GetTeamIdeasAsync(IdeaSearchScope.CategoriesInUse, categoryIds, userObjectId: null);

                if (categoriesInUse.Any())
                {
                    this.logger.LogInformation($"Either one or more {nameof(categoryIds)} are tagged with active or pending ideas and cannot be deleted.");

                    // the false response is handled at client level
                    return this.Ok(false);
                }

                return this.Ok(await this.storageProvider.DeleteCategoriesAsync(categoryEntities));
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error while deleting categories.");
                this.RecordEvent("An exception occurred for Categories - HTTP Delete call.");
                throw;
            }
        }
    }
}
