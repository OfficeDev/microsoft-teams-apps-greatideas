// <copyright file="AdaptiveSubmitActionData.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.SubmitIdea.Models.Card
{
    using Microsoft.Bot.Schema;
    using Newtonsoft.Json;

    /// <summary>
    /// Defines teams-specific behavior for an adaptive card submit action.
    /// </summary>
    public class AdaptiveSubmitActionData
    {
        /// <summary>
        /// Gets or sets the teams specific action.
        /// </summary>
        [JsonProperty("msteams")]
        public CardAction Msteams { get; set; }
    }
}
