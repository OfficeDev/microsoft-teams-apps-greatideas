// <copyright file="IdeaController.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.SubmitIdea.Controllers
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.ApplicationInsights;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Microsoft.Teams.Apps.SubmitIdea.Authentication;
    using Microsoft.Teams.Apps.SubmitIdea.Common;
    using Microsoft.Teams.Apps.SubmitIdea.Common.Interfaces;
    using Microsoft.Teams.Apps.SubmitIdea.Models;

    /// <summary>
    /// Controller to handle idea API operations from personal scope.
    /// </summary>
    [ApiController]
    [Route("api/idea")]
    [Authorize]
    public class IdeaController : BaseSubmitIdeaController
    {
        /// <summary>
        /// Logger implementation to send logs to the logger service.
        /// </summary>
        private readonly ILogger logger;

        /// <summary>
        /// Instance of team post storage helper to update idea and get information of ideas.
        /// </summary>
        private readonly IIdeaStorageHelper ideaStorageHelper;

        /// <summary>
        /// Instance of team post storage provider to update ideas and get information of ideas.
        /// </summary>
        private readonly IIdeaStorageProvider ideaStorageProvider;

        /// <summary>
        /// Instance of Search service for working with storage.
        /// </summary>
        private readonly IIdeaSearchService ideaSearchService;

        /// <summary>
        /// Instance of team category storage provider for team's configured categories.
        /// </summary>
        private readonly ICategoryStorageProvider categoryStorageProvider;

        /// <summary>
        /// Instance of team category storage provider for team's configured categories.
        /// </summary>
        private readonly ITeamCategoryStorageProvider teamCategoryStorageProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="IdeaController"/> class.
        /// </summary>
        /// <param name="logger">Logger implementation to send logs to the logger service.</param>
        /// <param name="telemetryClient">The Application Insights telemetry client.</param>
        /// <param name="ideaStorageHelper">Team post storage helper dependency injection.</param>
        /// <param name="ideaStorageProvider">Team idea storage provider dependency injection.</param>
        /// <param name="ideaSearchService">The team post search service dependency injection.</param>
        /// <param name="categoryStorageProvider">Category storage provider dependency injection.</param>
        /// <param name="teamCategoryStorageProvider">Team category storage provider dependency injection.</param>
        public IdeaController(
            ILogger<IdeaController> logger,
            TelemetryClient telemetryClient,
            IIdeaStorageHelper ideaStorageHelper,
            IIdeaStorageProvider ideaStorageProvider,
            IIdeaSearchService ideaSearchService,
            ICategoryStorageProvider categoryStorageProvider,
            ITeamCategoryStorageProvider teamCategoryStorageProvider)
            : base(telemetryClient)
        {
            this.logger = logger;
            this.ideaStorageHelper = ideaStorageHelper;
            this.ideaSearchService = ideaSearchService;
            this.ideaStorageProvider = ideaStorageProvider;
            this.categoryStorageProvider = categoryStorageProvider;
            this.teamCategoryStorageProvider = teamCategoryStorageProvider;
        }

        /// <summary>
        /// Get call to retrieve list of idea.
        /// </summary>
        /// <param name="pageCount">Page number to get search data from Azure Search service.</param>
        /// <returns>List of ideas.</returns>
        [HttpGet]
        public async Task<IActionResult> GetAsync(int pageCount)
        {
            this.RecordEvent("Ideas - HTTP Get call for all ideas accepted the request.");

            if (pageCount < 0)
            {
                this.logger.LogError($"{nameof(pageCount)} is found to be less than zero during {nameof(this.GetAsync)} call");
                return this.BadRequest($"Parameter {nameof(pageCount)} cannot be less than zero.");
            }

            var skipRecords = pageCount * Constants.LazyLoadPerPagePostCount;

            try
            {
                var teamPosts = await this.ideaSearchService
                    .GetTeamIdeasAsync(
                    IdeaSearchScope.AllItems,
                    searchQuery: null,
                    userObjectId: null,
                    count: Constants.LazyLoadPerPagePostCount,
                    skip: skipRecords);

                return this.Ok(teamPosts);
            }
            catch (Exception ex)
            {
                this.RecordEvent("An exception occurred in Ideas - HTTP Get call.");
                this.logger.LogError(ex, "An exception occurred while making call to idea GET service.");
                throw;
            }
        }

        /// <summary>
        /// Get call to retrieve team idea entity.
        /// </summary>
        /// <param name="userId">User id to fetch the idea details.</param>
        /// <param name="ideaId">Unique id of idea to get fetch data from Azure storage.</param>
        /// <returns>List of team ideas.</returns>
        [HttpGet("idea")]
        public async Task<IActionResult> GetIdeaAsync(string userId, string ideaId)
        {
            this.RecordEvent("Ideas - HTTP Get idea to get specific idea call accepted the request.");

            try
            {
                if (string.IsNullOrEmpty(ideaId))
                {
                    this.logger.LogError($"{nameof(ideaId)} is found to be null or empty to get specific idea.");
                    return this.BadRequest($"Parameter {nameof(ideaId)} cannot be null or empty.");
                }

                if (string.IsNullOrEmpty(userId))
                {
                    this.logger.LogError($"{nameof(userId)} is found to be null or empty");
                    return this.BadRequest($"Parameter {nameof(userId)} cannot be null or empty.");
                }

                var teamPosts = await this.ideaStorageProvider.GetIdeaEntityAsync(userId, ideaId);
                this.RecordEvent("idea - HTTP Get call succeeded");

                return this.Ok(teamPosts);
            }
            catch (Exception ex)
            {
                this.RecordEvent("An exception occurred for Idea - HTTP Get call for specific idea call.");
                this.logger.LogError(ex, "Error while making call to idea GET service.");
                throw;
            }
        }

        /// <summary>
        /// Post call to store idea details in storage.
        /// </summary>
        /// <param name="ideaEntity">Holds team idea detail entity data.</param>
        /// <returns>Returns idea entity instance on successful operation else returns false.</returns>
        [HttpPost]
        public async Task<IActionResult> PostAsync([FromBody] IdeaEntity ideaEntity)
        {
            this.RecordEvent("Ideas - HTTP Post idea call accepted the request.");

            try
            {
                var updatedTeamPostEntity = new IdeaEntity
                {
                    IdeaId = Guid.NewGuid().ToString(),
                    CreatedByObjectId = this.UserAadId,
                    CreatedByName = this.UserName,
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow,
                    TotalVotes = 0,
                    Status = (int)IdeaStatus.Pending,
#pragma warning disable CA1062 // team idea entity is validated by model validations for null check and is responded with bad request status
                    Title = ideaEntity.Title,
                    Description = ideaEntity.Description,
                    Category = ideaEntity.Category,
                    CategoryId = ideaEntity.CategoryId,
                    DocumentLinks = ideaEntity.DocumentLinks,
                    Tags = ideaEntity.Tags,
                    CreatedByUserPrincipalName = this.UserPrincipalName,
#pragma warning restore CA1062 // team idea entity is validated by model validations for null check and is responded with bad request status
                };

                var result = await this.ideaStorageProvider.UpsertIdeaAsync(updatedTeamPostEntity);

                if (result)
                {
                    this.RecordEvent("Idea - HTTP Post call succeeded");
                    await this.ideaSearchService.RunIndexerOnDemandAsync();

                    return this.Ok(updatedTeamPostEntity);
                }

                return this.Ok(result);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error while making call to idea service.");
                this.RecordEvent("An exception occurred for Idea - HTTP Post call.");
                throw;
            }
        }

        /// <summary>
        /// Patch call to update idea approve/reject status in storage.
        /// The API method is only accessible to curator team members.
        /// </summary>
        /// <param name="ideaEntity">Holds team idea detail entity data.</param>
        /// <returns>Returns true for successful operation.</returns>
        [HttpPatch]
        [Authorize(PolicyNames.MustBeCuratorTeamMemberUserPolicy)]
        public async Task<IActionResult> PatchAsync([FromBody] IdeaEntity ideaEntity)
        {
            this.RecordEvent("Ideas - HTTP Patch idea call accepted the request.");

            try
            {
#pragma warning disable CA1062 // idea entity is validated by model validations for null check and is responded with bad request status
                if (string.IsNullOrEmpty(ideaEntity.IdeaId))
#pragma warning restore CA1062 // idea entity is validated by model validations for null check and is responded with bad request status
                {
                    this.logger.LogError($"{nameof(ideaEntity.IdeaId)} is found to be null or empty to update idea.");
                    return this.BadRequest($"Parameter {nameof(ideaEntity.IdeaId)} cannot be null or empty.");
                }

                // Validating Idea Id as it will be generated at server side in case of adding new post but cannot be null or empty in case of update.
                var entity = await this.ideaStorageProvider.GetIdeaEntityAsync(ideaEntity.CreatedByObjectId, ideaEntity.IdeaId);
                if (entity == null)
                {
                    this.logger.LogError($"Entity for {ideaEntity.IdeaId} is not found to update.");
                    this.RecordEvent("Update idea - HTTP Patch call failed");

                    return this.BadRequest($"An update cannot be performed for {ideaEntity.IdeaId} because entity is not available.");
                }

                entity.ApprovedOrRejectedByName = this.UserName;
                entity.ApproverOrRejecterUserId = this.UserAadId;
                entity.Status = ideaEntity.Status;
                entity.Feedback = ideaEntity.Feedback;
                entity.Category = ideaEntity.Category;
                entity.CategoryId = ideaEntity.CategoryId;
                entity.UpdatedDate = DateTime.UtcNow;
                var result = await this.ideaStorageProvider.UpsertIdeaAsync(entity);

                if (result)
                {
                    this.RecordEvent("Team idea - HTTP Patch call succeeded");
                    await this.ideaSearchService.RunIndexerOnDemandAsync();
                }

                return this.Ok(result);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error while making call to idea post service.");
                throw;
            }
        }

        /// <summary>
        /// Get unique user names of authors for all ideas.
        /// </summary>
        /// <returns>Returns unique user names.</returns>
        [HttpGet("unique-user-names")]
        public async Task<IActionResult> GetUniqueUserNamesAsync()
        {
            this.RecordEvent("Ideas - HTTP Get unique user names call accepted the request.");

            try
            {
                // Search query will be null if there is no search criteria used. userObjectId will be used when we want to get posts created by respective user.
                var teamPosts = await this.ideaSearchService.GetTeamIdeasAsync(
                    IdeaSearchScope.UniqueUserNames,
                    searchQuery: null,
                    userObjectId: null);

                var authorNames = this.ideaStorageHelper.GetAuthorNamesAsync(teamPosts);

                this.RecordEvent("Team idea unique user names - HTTP Get call succeeded");

                return this.Ok(authorNames);
            }
            catch (Exception ex)
            {
                this.RecordEvent("An exception occurred to get idea unique user names - HTTP Get call failed");
                this.logger.LogError(ex, "Error while making call to get unique user names.");
                throw;
            }
        }

        /// <summary>
        /// Get list of ideas as per the title text.
        /// </summary>
        /// <param name="searchText">Search text represents the title field to find and get team posts.</param>
        /// <param name="pageCount">Page number to get search data from Azure Search service.</param>
        /// <returns>List of filtered team posts as per the search text for title.</returns>
        [HttpGet("search-ideas")]
        public async Task<IActionResult> GetSearchResultForIdeasAsync(string searchText, int pageCount)
        {
            this.RecordEvent("Ideas - HTTP Get search results call accepted the request.");

            if (pageCount < 0)
            {
                this.logger.LogError($"{pageCount} is less than zero");
                return this.BadRequest($"{nameof(pageCount)} cannot be less than zero.");
            }

            var skipRecords = pageCount * Constants.LazyLoadPerPagePostCount;

            try
            {
                var teamPosts = await this.ideaSearchService.GetTeamIdeasAsync(
                    IdeaSearchScope.SearchTeamPostsForTitleText,
                    searchText,
                    userObjectId: null,
                    skip: skipRecords,
                    count: Constants.LazyLoadPerPagePostCount);

                this.RecordEvent("Team idea title search - HTTP Get call succeeded");

                return this.Ok(teamPosts);
            }
            catch (Exception ex)
            {
                this.RecordEvent("Ideas - HTTP Get search user names call failed.");
                this.logger.LogError(ex, "Error while making call to get idea for search title text.");
                throw;
            }
        }

        /// <summary>
        /// Get list of unique categories to show while configuring the preference.
        /// </summary>
        /// <param name="searchText">Search text represents the text to get unique categories.</param>
        /// <returns>List of unique categories.</returns>
        [HttpGet("unique-categories")]
        public async Task<IActionResult> GetUniqueCategoriesAsync(string searchText)
        {
            this.RecordEvent("Ideas - HTTP Get unique categories call accepted the request.");

            try
            {
                if (string.IsNullOrEmpty(searchText))
                {
                    this.logger.LogError($"Parameter {nameof(searchText)} is found null or empty while trying to get unique categories.");
                    return this.BadRequest($"Parameter {nameof(searchText)} cannot be null or empty.");
                }

                var teamPosts = await this.ideaSearchService.GetTeamIdeasAsync(
                    IdeaSearchScope.Categories,
                    searchText,
                    userObjectId: null);

                var uniqueCategoryIds = this.ideaStorageHelper.GetCategoryIds(teamPosts);
                var categories = await this.categoryStorageProvider.GetCategoriesByIdsAsync(uniqueCategoryIds);
                this.RecordEvent("Team idea unique category- HTTP get call succeeded");

                return this.Ok(categories);
            }
            catch (Exception ex)
            {
                this.RecordEvent("Ideas - HTTP Get unique categories call failed.");
                this.logger.LogError(ex, "Error while making call to get unique tags.");
                throw;
            }
        }

        /// <summary>
        /// Get ideas as per the applied filters from storage.
        /// </summary>
        /// <param name="categories">Semicolon separated types of posts like blog post or Other.</param>
        /// /// <param name="sharedByNames">Semicolon separated User names to filter the posts.</param>
        /// /// <param name="tags">Semicolon separated tags to match the post tags for which data will fetch.</param>
        /// /// <param name="sortBy">Represents sorting type like: Popularity or Newest.</param>
        /// <param name="teamId">Team id to get configured tags for a team.</param>
        /// <param name="pageCount">Page count for which post needs to be fetched.</param>
        /// <returns>Returns filtered list of ideas as per the selected filters.</returns>
        [HttpGet("applied-filtered-team-ideas")]
        public async Task<IActionResult> GetAppliedFiltersTeamIdeasAsync(string categories, string sharedByNames, string tags, string sortBy, string teamId, int pageCount)
        {
            this.RecordEvent("Ideas - HTTP Get filter ideas call accepted the request.");

            if (pageCount < 0)
            {
                this.logger.LogError($"{pageCount} is less than zero");
                return this.BadRequest($"{nameof(pageCount)} cannot be less than zero.");
            }

            var skipRecords = pageCount * Constants.LazyLoadPerPagePostCount;

            var teamCategoryEntity = new TeamCategoryEntity();

            try
            {
                // Team id will be empty when called from personal scope tab.
                if (!string.IsNullOrEmpty(teamId))
                {
                    teamCategoryEntity = await this.teamCategoryStorageProvider.GetTeamCategoriesDataAsync(teamId);
                }

                var tagsQuery = string.IsNullOrEmpty(tags) ? "*" : this.ideaStorageHelper.GetTags(tags);
                categories = string.IsNullOrEmpty(categories) ? teamCategoryEntity.Categories : categories;
                var filterQuery = this.ideaStorageHelper.GetFilterSearchQuery(categories, sharedByNames);
                var teamPosts = await this.ideaSearchService.GetTeamIdeasAsync(
                    IdeaSearchScope.FilterTeamPosts,
                    tagsQuery,
                    userObjectId: null,
                    sortBy: sortBy,
                    filterQuery: filterQuery,
                    count: Constants.LazyLoadPerPagePostCount,
                    skip: skipRecords);

                this.RecordEvent("Team idea applied filters - HTTP Get call succeeded");

                return this.Ok(teamPosts);
            }
            catch (Exception ex)
            {
                this.RecordEvent("Team idea applied filters - HTTP Get call failed.");
                this.logger.LogError(ex, "Error while making call to get ideas as per the applied filters service.");
                throw;
            }
        }

        /// <summary>
        /// Get list of unique tags to show while configuring the preference.
        /// </summary>
        /// <param name="searchText">Search text represents the text to find and get unique tags.</param>
        /// <returns>List of unique tags.</returns>
        [HttpGet("unique-tags")]
        public async Task<IActionResult> GetUniqueTagsAsync(string searchText)
        {
            this.RecordEvent("Idea tags - HTTP Get call requested.");

            try
            {
                this.logger.LogInformation("Call to get list of unique tags to show while configuring the preference.");

                if (string.IsNullOrEmpty(searchText))
                {
                    this.logger.LogError($"Parameter {nameof(searchText)} is found to be null or empty.");
                    return this.BadRequest($"Parameter {nameof(searchText)} cannot be null or empty.");
                }

                var teamPosts = await this.ideaSearchService.GetTeamIdeasAsync(
                    IdeaSearchScope.TeamPreferenceTags,
                    searchText,
                    userObjectId: null);

                var uniqueTags = this.ideaStorageHelper.GetUniqueTags(teamPosts, searchText);
                this.RecordEvent("Team preferences tags - HTTP Get call succeeded");

                return this.Ok(uniqueTags);
            }
            catch (Exception ex)
            {
                this.RecordEvent("Team preferences tags - HTTP Get call failed");
                this.logger.LogError(ex, "Error while making call to get unique tags.");
                throw;
            }
        }
    }
}