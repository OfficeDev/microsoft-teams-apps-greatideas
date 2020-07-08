// <copyright file="IdeaStorageHelper.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.SubmitIdea.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Extensions.Logging;
    using Microsoft.Teams.Apps.SubmitIdea.Common.Interfaces;
    using Microsoft.Teams.Apps.SubmitIdea.Helpers.Extensions;
    using Microsoft.Teams.Apps.SubmitIdea.Models;

    /// <summary>
    /// Implements team idea storage helper which helps to construct the model, create search query for team idea.
    /// </summary>
    public class IdeaStorageHelper : IIdeaStorageHelper
    {
        /// <summary>
        /// Logger implementation to send logs to the logger service.
        /// </summary>
        private readonly ILogger<IdeaStorageHelper> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="IdeaStorageHelper"/> class.
        /// </summary>
        /// <param name="logger">Logger implementation to send logs to the logger service.</param>
        public IdeaStorageHelper(
            ILogger<IdeaStorageHelper> logger)
        {
            this.logger = logger;
        }

        /// <summary>
        /// Create team idea model data.
        /// </summary>
        /// <param name="teamIdeaEntity">Team idea detail.</param>
        /// <param name="userId">User Azure active directory id.</param>
        /// <param name="userName">Author who created the idea.</param>
        /// <returns>A task that represents team idea entity data.</returns>
        public IdeaEntity CreateTeamIdeaModel(IdeaEntity teamIdeaEntity, string userId, string userName)
        {
            try
            {
                teamIdeaEntity = teamIdeaEntity ?? throw new ArgumentNullException(nameof(teamIdeaEntity));

                teamIdeaEntity.IdeaId = Guid.NewGuid().ToString();
                teamIdeaEntity.CreatedByObjectId = userId;
                teamIdeaEntity.CreatedByName = userName;
                teamIdeaEntity.CreatedDate = DateTime.UtcNow;
                teamIdeaEntity.UpdatedDate = DateTime.UtcNow;

                return teamIdeaEntity;
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Exception occurred while creating the idea model data.");
                throw;
            }
        }

        /// <summary>
        /// Create updated team idea model data for storage.
        /// </summary>
        /// <param name="ideaEntity">Team idea detail.</param>
        /// <returns>A task that represents idea entity updated data.</returns>
        public IdeaEntity CreateUpdatedTeamIdeaModel(IdeaEntity ideaEntity)
        {
            try
            {
                ideaEntity = ideaEntity ?? throw new ArgumentNullException(nameof(ideaEntity));

                ideaEntity.UpdatedDate = DateTime.UtcNow;

                return ideaEntity;
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Exception occurred while getting the team idea model data");
                throw;
            }
        }

        /// <summary>
        /// Get filtered team idea as per the configured tags.
        /// </summary>
        /// <param name="teamIdeas">Team idea entities.</param>
        /// <param name="searchText">Search text for tags.</param>
        /// <returns>Represents team ideas.</returns>
        public IEnumerable<IdeaEntity> GetFilteredTeamIdeasAsPerTags(IEnumerable<IdeaEntity> teamIdeas, string searchText)
        {
            try
            {
                teamIdeas = teamIdeas ?? throw new ArgumentNullException(nameof(teamIdeas));
                searchText = searchText ?? throw new ArgumentNullException(nameof(searchText));
                var filteredTeamIdeas = new List<IdeaEntity>();

                foreach (var teamIdea in teamIdeas)
                {
                    foreach (var tag in searchText.Split(";"))
                    {
                        if (Array.Exists(teamIdea.Tags?.Split(";"), tagText => tagText.Equals(tag.Trim(), StringComparison.InvariantCultureIgnoreCase)))
                        {
                            filteredTeamIdeas.Add(teamIdea);
                            break;
                        }
                    }
                }

                return filteredTeamIdeas;
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Exception occurred while preparing the team preference entities list.");
                throw;
            }
        }

        /// <summary>
        /// Get tags to fetch ideas as per the configured tags.
        /// </summary>
        /// <param name="tags">Tags of a configured idea.</param>
        /// <returns>Represents tags to fetch team posts.</returns>
        public string GetTags(string tags)
        {
            try
            {
                tags = tags ?? throw new ArgumentNullException(nameof(tags));
                var postTags = tags.Split(';').Where(postType => !string.IsNullOrWhiteSpace(postType)).ToList();

                return string.Join(" ", postTags);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Exception occurred while preparing the query for tags to get team idea as per the configured tags.");
                throw;
            }
        }

        /// <summary>
        /// Get tags to fetch team ideas as per the configured categories.
        /// </summary>
        /// <param name="categories">Categories of a configured team ideas.</param>
        /// <returns>Represents categories to fetch team ideas.</returns>
        public string GetCategories(string categories)
        {
            try
            {
                categories = categories ?? throw new ArgumentNullException(nameof(categories));
                var ideaCategories = categories.Split(';').Where(category => !string.IsNullOrWhiteSpace(category)).Distinct().ToList();

                return string.Join(" ", ideaCategories);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Exception occurred while preparing the query for categories to get team ideas as per the configured categories.");
                throw;
            }
        }

        /// <summary>
        /// Get filtered team posts as per the date range from storage.
        /// </summary>
        /// <param name="teamIdeas">Team ideas data.</param>
        /// <param name="fromDate">Start date from which data should fetch.</param>
        /// <param name="toDate">End date till when data should fetch.</param>
        /// <returns>A task that represent collection to hold team posts data.</returns>
        public IEnumerable<IdeaEntity> GetTeamIdeasInDateRangeAsync(IEnumerable<IdeaEntity> teamIdeas, DateTime fromDate, DateTime toDate)
        {
            return teamIdeas.Where(post => post.UpdatedDate >= fromDate && post.UpdatedDate <= toDate);
        }

        /// <summary>
        /// Get filtered user names from team ideas data.
        /// </summary>
        /// <param name="teamIdeas">Represents a collection of team ideas.</param>
        /// <returns>Represents team posts.</returns>
        public IEnumerable<string> GetAuthorNamesAsync(IEnumerable<IdeaEntity> teamIdeas)
        {
            try
            {
                teamIdeas = teamIdeas ?? throw new ArgumentNullException(nameof(teamIdeas));

                return teamIdeas.Select(idea => idea.CreatedByName).Distinct().OrderBy(createdByName => createdByName);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Exception occurred while preparing the unique user names list.");
                throw;
            }
        }

        /// <summary>
        /// Get filtered tag names from team ideas data.
        /// </summary>
        /// <param name="teamIdeas">Represents a collection of team ideas.</param>
        /// <returns>Represents collection of tag names.</returns>
        public IEnumerable<string> GetTeamTagsNamesAsync(IEnumerable<IdeaEntity> teamIdeas)
        {
            try
            {
                teamIdeas = teamIdeas ?? throw new ArgumentNullException(nameof(teamIdeas));

                var tagsCollection = new List<string>();
                foreach (var teamIdea in teamIdeas)
                {
                    var tagsData = teamIdea.Tags.Split(';').Distinct();
                    tagsCollection.AddRange(tagsData);
                }

                return tagsCollection.Distinct();
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Exception occurred while preparing the unique user names list.");
                throw;
            }
        }

        /// <summary>
        /// Get filtered category ids from team ideas data.
        /// </summary>
        /// <param name="teamIdeas">Represents a collection of team ideas.</param>
        /// <returns>Represents list of category ids.</returns>
        public IEnumerable<string> GetCategoryIds(IEnumerable<IdeaEntity> teamIdeas)
        {
            try
            {
                teamIdeas = teamIdeas ?? throw new ArgumentNullException(nameof(teamIdeas));

                return teamIdeas.Select(idea => idea.CategoryId).Distinct().OrderBy(category => category);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Exception occurred while preparing the unique category id list.");
                throw;
            }
        }

        /// <summary>
        /// Get combined query to fetch team ideas as per the selected filter.
        /// </summary>
        /// <param name="postTypes">Post type like: Blog post or Other.</param>
        /// <param name="sharedByNames">User names selected in filter.</param>
        /// <returns>Represents user names query to filter team ideas.</returns>
        public string GetFilterSearchQuery(string postTypes, string sharedByNames)
        {
            try
            {
                var typesQuery = this.GetIdeaCategoriesQuery(postTypes);
                var sharedByNamesQuery = this.GetSharedByNamesQuery(sharedByNames);
                string combinedQuery = string.Empty;

                if (string.IsNullOrEmpty(typesQuery) && string.IsNullOrEmpty(sharedByNamesQuery))
                {
                    return null;
                }

                if (!string.IsNullOrEmpty(typesQuery) && !string.IsNullOrEmpty(sharedByNamesQuery))
                {
                    return $"({typesQuery}) and ({sharedByNamesQuery})";
                }

                if (!string.IsNullOrEmpty(typesQuery))
                {
                    return $"({typesQuery})";
                }

                if (!string.IsNullOrEmpty(sharedByNamesQuery))
                {
                    return $"({sharedByNamesQuery})";
                }

                return null;
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Exception occurred while preparing the query to get filter bar search result for team ideas.");
                throw;
            }
        }

        /// <summary>
        /// Get idea category query to fetch team ideas as per the selected filter.
        /// </summary>
        /// <param name="categories">Team's configured categories.</param>
        /// <returns>Represents post type query to filter team ideas.</returns>
        public string GetIdeaCategoriesQuery(string categories)
        {
            try
            {
                if (string.IsNullOrEmpty(categories))
                {
                    return null;
                }

                var ideaCategories = categories.Split(";")
                    .Where(ideaCategory => !string.IsNullOrWhiteSpace(ideaCategory))
                    .Select(ideaCategory => $"CategoryId eq '{ideaCategory}'");

                return string.Join(" or ", ideaCategories);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Exception occurred while preparing the query for ideas types to get ideas as per the selected types.");
                throw;
            }
        }

        /// <summary>
        /// Get posts unique tags.
        /// </summary>
        /// <param name="teamIdeas">Team idea entities.</param>
        /// <param name="searchText">Search text for tags.</param>
        /// <returns>Represents team tags.</returns>
        public IEnumerable<string> GetUniqueTags(IEnumerable<IdeaEntity> teamIdeas, string searchText)
        {
            try
            {
                teamIdeas = teamIdeas ?? throw new ArgumentNullException(nameof(teamIdeas));
                var tags = new List<string>();

                if (searchText == "*")
                {
                    foreach (var teamPost in teamIdeas)
                    {
                        tags.AddRange(teamPost.Tags?.Split(";"));
                    }
                }
                else
                {
                    foreach (var teamPost in teamIdeas)
                    {
                        tags.AddRange(teamPost.Tags?.Split(";").Where(tag => tag.Contains(searchText, StringComparison.InvariantCultureIgnoreCase)));
                    }
                }

                return tags.Distinct().OrderBy(tag => tag);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Exception occurred while preparing the team preference entity model data");
                throw;
            }
        }

        /// <summary>
        /// Get user names query to fetch team ideas as per the selected filter.
        /// </summary>
        /// <param name="sharedByNames">User names selected in filter.</param>
        /// <returns>Represents user names query to filter team ideas.</returns>
        private string GetSharedByNamesQuery(string sharedByNames)
        {
            try
            {
                if (string.IsNullOrEmpty(sharedByNames))
                {
                    return null;
                }

                if (string.IsNullOrEmpty(sharedByNames))
                {
                    return null;
                }

                var names = sharedByNames.Split(";")
                    .Where(name => !string.IsNullOrWhiteSpace(name))
                    .Select(name => $"CreatedByName eq '{name.EscapeCharactersInQuery()}'");

                return string.Join(" or ", names);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Exception occurred while preparing the query for shared by names to get team ideas as per the selected names.");
                throw;
            }
        }
    }
}