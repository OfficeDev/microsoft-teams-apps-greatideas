// <copyright file="ListCard.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.SubmitIdea.Models.Card
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// A class that represents list card model.
    /// </summary>
    public class ListCard
    {
        /// <summary>
        /// Gets or sets title of list card.
        /// </summary>
        [JsonProperty("title")]
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets items of list card.
        /// </summary>
        [JsonProperty("items")]
#pragma warning disable CA2227 // Getting error to make collection property as read only but needs to assign values.
        public List<ListItem> Items { get; set; }
    }
}
