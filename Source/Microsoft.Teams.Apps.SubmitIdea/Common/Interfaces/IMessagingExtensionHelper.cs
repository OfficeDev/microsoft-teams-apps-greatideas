// <copyright file="IMessagingExtensionHelper.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.SubmitIdea.Common.Interfaces
{
    using System.Threading.Tasks;
    using Microsoft.Bot.Schema.Teams;

    /// <summary>
    /// Interface that handles the search activities for Messaging Extension.
    /// </summary>
    public interface IMessagingExtensionHelper
    {
        /// <summary>
        /// Get the results from Azure Search service and populate the result (card + preview).
        /// </summary>
        /// <param name="query">Query which the user had typed in Messaging Extension search field.</param>
        /// <param name="commandId">Command id to determine which tab in Messaging Extension has been invoked.</param>
        /// <param name="userObjectId">Azure Active Directory id of the user.</param>
        /// <param name="count">Number of search results to return.</param>
        /// <param name="skip">Number of search results to skip.</param>
        /// <returns><see cref="Task"/>Returns Messaging Extension result object, which will be used for providing the card.</returns>
        Task<MessagingExtensionResult> GetTeamPostSearchResultAsync(
            string query,
            string commandId,
            string userObjectId,
            int? count,
            int? skip);

        /// <summary>
        /// Get the value of the searchText parameter in the Messaging Extension query.
        /// </summary>
        /// <param name="query">Contains Messaging Extension query keywords.</param>
        /// <returns>A value of the searchText parameter.</returns>
        string GetSearchResult(MessagingExtensionQuery query);
    }
}
