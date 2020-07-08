// <copyright file="TeamIdeaController.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.SubmitIdea.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
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
    /// Controller to handle idea API operations from teams scope.
    /// </summary>
    [Route("api/teamidea")]
    [ApiController]
    [Authorize(PolicyNames.MustBeTeamMemberUserPolicy)]
    public class TeamIdeaController : BaseSubmitIdeaController
    {
        /// <summary>
        /// Logger implementation to send logs to the logger service.
        /// </summary>
        private readonly ILogger logger;

        /// <summary>
        /// Instance of team post storage helper to update post and get information of ideas.
        /// </summary>
        private readonly IIdeaStorageHelper ideaStorageHelper;

        /// <summary>
        /// Instance of Search service for working with storage.
        /// </summary>
        private readonly IIdeaSearchService ideaSearchService;

        /// <summary>
        /// Instance of team category storage provider for team's configured categories.
        /// </summary>
        private readonly ITeamCategoryStorageProvider teamCategoryStorageProvider;

        /// <summary>
        /// Instance of team category storage provider for team's configured categories.
        /// </summary>
        private readonly ICategoryStorageProvider categoryStorageProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="TeamIdeaController"/> class.
        /// </summary>
        /// <param name="logger">Logger implementation to send logs to the logger service.</param>
        /// <param name="telemetryClient">The Application Insights telemetry client.</param>
        /// <param name="ideaStorageHelper">Team post storage helper dependency injection.</param>
        /// <param name="ideaSearchService">The team post search service dependency injection.</param>
        /// <param name="teamCategoryStorageProvider">Team category storage provider dependency injection.</param>
        /// <param name="categoryStorageProvider">Category storage provider dependency injection.</param>
        public TeamIdeaController(
            ILogger<IdeaController> logger,
            TelemetryClient telemetryClient,
            IIdeaStorageHelper ideaStorageHelper,
            IIdeaSearchService ideaSearchService,
            ITeamCategoryStorageProvider teamCategoryStorageProvider,
            ICategoryStorageProvider categoryStorageProvider)
            : base(telemetryClient)
        {
            this.logger = logger;
            this.ideaStorageHelper = ideaStorageHelper;
            this.ideaSearchService = ideaSearchService;
            this.teamCategoryStorageProvider = teamCategoryStorageProvider;
            this.categoryStorageProvider = categoryStorageProvider;
        }

        /// <summary>
        /// Get list of posts for team's discover tab, as per the configured tags and title of posts.
        /// </summary>
        /// <param name="searchText">Search text represents the title of the posts.</param>
        /// <param name="teamId">Team id to get configured tags for a team.</param>
        /// <param name="pageCount">Page count for which post needs to be fetched.</param>
        /// <returns>List of posts as per the title and configured tags.</returns>
        [HttpGet("team-search-ideas")]
        public async Task<IActionResult> GetTeamIdeasSearchResultAsync(string searchText, string teamId, int pageCount)
        {
            this.RecordEvent("Team idea search result - HTTP Get call requested.");

            if (pageCount < 0)
            {
                this.logger.LogError($"{nameof(pageCount)} is found to be less than zero during {nameof(this.GetTeamIdeasSearchResultAsync)} call");
                return this.BadRequest($"Parameter {nameof(pageCount)} cannot be less than zero.");
            }

            var skipRecords = pageCount * Constants.LazyLoadPerPagePostCount;

            try
            {
                if (string.IsNullOrEmpty(teamId))
                {
                    this.logger.LogError("Error while fetching search ideas as per the title and configured tags from storage.");
                    return this.BadRequest(new { message = $"Parameter {nameof(teamId)} is either null or empty." });
                }

                // Get tags based on the team id for which tags has configured.
                var teamCategoryEntity = await this.teamCategoryStorageProvider.GetTeamCategoriesDataAsync(teamId);

                if (teamCategoryEntity == null || string.IsNullOrEmpty(teamCategoryEntity.Categories))
                {
                    this.logger.LogError($"No team category records found for team {teamId}.");
                    return this.Ok();
                }

                var categoriesQuery = string.IsNullOrEmpty(teamCategoryEntity.Categories) ? "*" : this.ideaStorageHelper.GetCategories(teamCategoryEntity.Categories);
                var filterQuery = $"search.ismatch('{categoriesQuery}', 'CategoryId')";
                var teamPosts = await this.ideaSearchService.GetTeamIdeasAsync(IdeaSearchScope.SearchTeamPostsForTitleText, searchText, userObjectId: null, count: Constants.LazyLoadPerPagePostCount, skip: skipRecords, filterQuery: filterQuery);
                this.RecordEvent("Team idea search result - HTTP Get call succeeded");

                return this.Ok(teamPosts);
            }
            catch (Exception ex)
            {
                this.RecordEvent("Team idea search result - HTTP Get call failed");
                this.logger.LogError(ex, "Error while making call to get ideas for search title text.");
                throw;
            }
        }

        /// <summary>
        /// Get unique author names from storage.
        /// </summary>
        /// <param name="teamId">Team id to get the configured categories for a team.</param>
        /// <returns>Returns unique user names.</returns>
        [HttpGet("authors-for-categories")]
        public async Task<IActionResult> GetAuthorNamesAsync(string teamId)
        {
            this.RecordEvent("Team post unique author names - HTTP Get call requested.");

            try
            {
                this.logger.LogInformation("Call to get unique author names.");
                if (string.IsNullOrEmpty(teamId))
                {
                    this.logger.LogError($"Parameter {nameof(teamId)} is found either null or empty.");
                    return this.BadRequest($"Parameter {nameof(teamId)} cannot be null or empty.");
                }

                // Get tags based on the team id for which tags has configured.
                var teamCategoryEntity = await this.teamCategoryStorageProvider.GetTeamCategoriesDataAsync(teamId);

                if (teamCategoryEntity == null || string.IsNullOrEmpty(teamCategoryEntity.Categories))
                {
                    return this.Ok(new List<string>());
                }

                var tagsQuery = this.ideaStorageHelper.GetCategories(teamCategoryEntity.Categories);
                var teamIdeas = await this.ideaSearchService.GetTeamIdeasAsync(IdeaSearchScope.FilterAsPerTeamTags, tagsQuery, null, null);
                var authorNames = this.ideaStorageHelper.GetAuthorNamesAsync(teamIdeas);
                this.RecordEvent("Team post unique author names - HTTP Get call succeeded");

                return this.Ok(authorNames);
            }
            catch (Exception ex)
            {
                this.RecordEvent("Team post unique author names - HTTP Get call failed.");
                this.logger.LogError(ex, "Error while making call to get unique user names.");
                throw;
            }
        }

        /// <summary>
        /// Get unique tags for teams categories from storage.
        /// </summary>
        /// <param name="teamId">Team id to get the configured categories for a team.</param>
        /// <returns>Returns unique tags for given team categories.</returns>
        [HttpGet("tags-for-categories")]
        public async Task<IActionResult> GetTeamCategoryTagsAsync(string teamId)
        {
            this.RecordEvent("Ideas unique tags - HTTP Get call requested.");

            try
            {
                this.logger.LogInformation("Call to get unique tags.");
                if (string.IsNullOrEmpty(teamId))
                {
                    this.logger.LogError("Error while fetching search ideas as per the tags and categories from storage.");
                    return this.BadRequest($"Parameter {nameof(teamId)} cannot be null or empty.");
                }

                // Get tags based on the team id for which tags has configured.
                var teamCategoryEntity = await this.teamCategoryStorageProvider.GetTeamCategoriesDataAsync(teamId);

                if (teamCategoryEntity == null || string.IsNullOrEmpty(teamCategoryEntity.Categories))
                {
                    return this.Ok(new List<string>());
                }

                var tagsQuery = this.ideaStorageHelper.GetCategories(teamCategoryEntity.Categories);
                var teamIdeas = await this.ideaSearchService.GetTeamIdeasAsync(
                    IdeaSearchScope.FilterAsPerTeamTags,
                    tagsQuery,
                    userObjectId: null);

                var tags = this.ideaStorageHelper.GetTeamTagsNamesAsync(teamIdeas);
                this.RecordEvent("Ideas unique tags - HTTP Get call succeeded");

                return this.Ok(tags);
            }
            catch (Exception ex)
            {
                this.RecordEvent("Ideas unique tags - HTTP Get call failed.");
                this.logger.LogError(ex, "Error while making call to get unique tags.");
                throw;
            }
        }

        /// <summary>
        /// Get list of teams configured unique categories.
        /// </summary>
        /// <param name="teamId">team identifier.</param>
        /// <returns>List of unique categories.</returns>
        [HttpGet("team-unique-categories")]
        public async Task<IActionResult> GetTeamsUniqueCategoriesAsync(string teamId)
        {
            try
            {
                this.logger.LogInformation("Call to get list of unique categories to show while configuring the preference.");

                if (string.IsNullOrEmpty(teamId))
                {
                    this.logger.LogError("Error while getting the list of unique categories from storage.");
                    return this.BadRequest(new { message = $"Parameter {nameof(teamId)} is either null or empty." });
                }

                var teamCategoryEntity = await this.teamCategoryStorageProvider.GetTeamCategoriesDataAsync(teamId);
                var categoryIds = teamCategoryEntity.Categories.Split(';').Where(categoryId => !string.IsNullOrWhiteSpace(categoryId)).Select(categoryId => categoryId.Trim());
                var categories = await this.categoryStorageProvider.GetCategoriesByIdsAsync(categoryIds);
                this.RecordEvent("Team idea unique category- HTTP get call succeeded");

                return this.Ok(categories);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error while making call to get unique categories.");
                throw;
            }
        }

        /// <summary>
        /// Get filtered team ideas for particular team as per the configured categories.
        /// </summary>
        /// <param name="teamId">Team id for which data will fetch.</param>
        /// <param name="pageCount">Page number to get search data.</param>
        /// <returns>Returns filtered list of team ideas as per the configured tags.</returns>
        [HttpGet("team-ideas")]
        public async Task<IActionResult> GetFilteredTeamPostsAsync(string teamId, int pageCount)
        {
            this.RecordEvent("Filtered team idea - HTTP Get call requested.");

            if (pageCount < 0)
            {
                this.logger.LogError($"{nameof(pageCount)} is found to be less than zero during {nameof(this.GetTeamIdeasSearchResultAsync)} call");
                return this.BadRequest($"Parameter {nameof(pageCount)} cannot be less than zero.");
            }

            var skipRecords = pageCount * Constants.LazyLoadPerPagePostCount;

            try
            {
                this.logger.LogInformation("Call to get filtered team idea details.");

                if (string.IsNullOrEmpty(teamId))
                {
                    this.logger.LogError($"Parameter {nameof(teamId)} is either null or empty.");
                    return this.BadRequest($"Parameter {nameof(teamId)} cannot be null or empty.");
                }

                // Get categories based on the team id for which categories has configured.
                var teamCategories = await this.teamCategoryStorageProvider.GetTeamCategoriesDataAsync(teamId);

                if (teamCategories == null || string.IsNullOrEmpty(teamCategories.Categories))
                {
                    return this.Ok(new List<IdeaEntity>());
                }

                // Prepare query based on the tags and get the data using search service.
                var categoriesQuery = this.ideaStorageHelper.GetCategories(teamCategories.Categories);

                var teamIdeas = await this.ideaSearchService.GetTeamIdeasAsync(
                    IdeaSearchScope.FilterAsPerTeamTags,
                    categoriesQuery,
                    userObjectId: null,
                    count: Constants.LazyLoadPerPagePostCount,
                    skip: skipRecords);

                this.RecordEvent("Filtered team idea - HTTP Get call succeeded");

                return this.Ok(teamIdeas);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error while making call to team post service.");
                this.RecordEvent("Filtered team idea - HTTP Get call failed");
                throw;
            }
        }
    }
}