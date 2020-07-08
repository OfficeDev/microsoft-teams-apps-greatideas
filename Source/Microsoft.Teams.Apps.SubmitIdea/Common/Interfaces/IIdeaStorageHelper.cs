// <copyright file="IIdeaStorageHelper.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.SubmitIdea.Common.Interfaces
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Teams.Apps.SubmitIdea.Models;

    /// <summary>
    /// Interface for storage helper which helps in preparing model data for ideas.
    /// </summary>
    public interface IIdeaStorageHelper
    {
        /// <summary>
        /// Create idea details model.
        /// </summary>
        /// <param name="teamIdeaEntity">Team idea object.</param>
        /// <param name="userId">Azure Active directory id of user.</param>
        /// <param name="userName">Author who created the idea.</param>
        /// <returns>A task that represents team idea entity data.</returns>
        IdeaEntity CreateTeamIdeaModel(IdeaEntity teamIdeaEntity, string userId, string userName);

        /// <summary>
        /// Create updated idea model to save in storage.
        /// </summary>
        /// <param name="ideaEntity">Team idea detail.</param>
        /// <returns>A task that represents idea entity updated data.</returns>
        IdeaEntity CreateUpdatedTeamIdeaModel(IdeaEntity ideaEntity);

        /// <summary>
        /// Get filtered ideas as per the configured tags.
        /// </summary>
        /// <param name="teamIdeas">Team idea entities.</param>
        /// <param name="searchText">Search text for tags.</param>
        /// <returns>Represents team ideas.</returns>
        IEnumerable<IdeaEntity> GetFilteredTeamIdeasAsPerTags(IEnumerable<IdeaEntity> teamIdeas, string searchText);

        /// <summary>
        /// Get tags query to fetch ideas as per the configured tags.
        /// </summary>
        /// <param name="tags">Tags of a configured idea.</param>
        /// <returns>Represents tags query to fetch ideas.</returns>
        string GetTags(string tags);

        /// <summary>
        /// Get filtered ideas as per the date range from storage.
        /// </summary>
        /// <param name="teamIdeas">List of idea entities.</param>
        /// <param name="fromDate">Start date from which data should fetch.</param>
        /// <param name="toDate">End date till when data should fetch.</param>
        /// <returns>A task that represent collection to hold ideas data.</returns>
        IEnumerable<IdeaEntity> GetTeamIdeasInDateRangeAsync(IEnumerable<IdeaEntity> teamIdeas, DateTime fromDate, DateTime toDate);

        /// <summary>
        /// Get filtered unique user names.
        /// </summary>
        /// <param name="teamIdeas">Team idea entities.</param>
        /// <returns>Represents team ideas.</returns>
        IEnumerable<string> GetAuthorNamesAsync(IEnumerable<IdeaEntity> teamIdeas);

        /// <summary>
        /// Get combined query to fetch ideas as per the selected filter.
        /// </summary>
        /// <param name="postTypes">Post type like: Blog post or Other.</param>
        /// <param name="sharedByNames">User names selected in filter.</param>
        /// <returns>Represents user names query to filter team posts.</returns>
        string GetFilterSearchQuery(string postTypes, string sharedByNames);

        /// <summary>
        /// Get filtered category Ids from team ideas data.
        /// </summary>
        /// <param name="teamIdeas">Represents a collection of team ideas.</param>
        /// <returns>Represents team posts.</returns>
        IEnumerable<string> GetCategoryIds(IEnumerable<IdeaEntity> teamIdeas);

        /// <summary>
        /// Get tags to fetch ideas as per the configured categories.
        /// </summary>
        /// <param name="categories">Categories of a configured team ideas.</param>
        /// <returns>Represents categories to fetch ideas.</returns>
        string GetCategories(string categories);

        /// <summary>
        /// Get filtered tag names from ideas data.
        /// </summary>
        /// <param name="teamIdeas">Represents a collection of ideas.</param>
        /// <returns>Represents collection of tag names.</returns>
        IEnumerable<string> GetTeamTagsNamesAsync(IEnumerable<IdeaEntity> teamIdeas);

        /// <summary>
        /// Get idea category query to fetch ideas as per the selected filter.
        /// </summary>
        /// <param name="categories">Team's configured categories.</param>
        /// <returns>Represents post type query to filter ideas.</returns>
        string GetIdeaCategoriesQuery(string categories);

        /// <summary>
        /// Get posts unique tags.
        /// </summary>
        /// <param name="teamIdeas">Team post entities.</param>
        /// <param name="searchText">Input tag as search text.</param>
        /// <returns>Represents team tags.</returns>
        IEnumerable<string> GetUniqueTags(IEnumerable<IdeaEntity> teamIdeas, string searchText);
    }
}
