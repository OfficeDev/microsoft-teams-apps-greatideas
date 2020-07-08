// <copyright file="DigestNotificationListCard.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.SubmitIdea.Cards
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Bot.Schema;
    using Microsoft.Teams.Apps.SubmitIdea.Models;
    using Microsoft.Teams.Apps.SubmitIdea.Models.Card;

    /// <summary>
    /// Class that helps to create notification list card for channel.
    /// </summary>
    public static class DigestNotificationListCard
    {
        /// <summary>
        /// Get list card for channel notification.
        /// </summary>
        /// <param name="applicationBasePath">Application base path to get the list card icon.</param>
        /// <param name="teamIdeaEntities">Team idea entities.</param>
        /// <param name="cardTitle">Notification list card title.</param>
        /// <returns>An attachment card for channel notification.</returns>
        public static Attachment GetNotificationListCard(
            string applicationBasePath,
            IEnumerable<IdeaEntity> teamIdeaEntities,
            string cardTitle)
        {
            teamIdeaEntities = teamIdeaEntities ?? throw new ArgumentNullException(nameof(teamIdeaEntities));

            ListCard card = new ListCard
            {
                Title = cardTitle,
                Items = new List<ListItem>(),
            };

            foreach (var teamIdeaEntity in teamIdeaEntities)
            {
                card.Items.Add(new ListItem
                {
                    Type = "resultItem",
                    Title = teamIdeaEntity.Title,
                    Subtitle = $"{teamIdeaEntity.CreatedByName} | {teamIdeaEntity.TotalVotes}",
                    Icon = $"{applicationBasePath}/Artifacts/blogIcon.png",
                });
            }

            var attachment = new Attachment()
            {
                ContentType = "application/vnd.microsoft.teams.card.list",
                Content = card,
            };

            return attachment;
        }
    }
}
