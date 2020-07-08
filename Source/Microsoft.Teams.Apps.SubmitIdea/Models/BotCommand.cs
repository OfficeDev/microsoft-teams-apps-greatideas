// <copyright file="BotCommand.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.SubmitIdea.Models
{
    /// <summary>
    /// A class that represents properties to be parsed from activity value.
    /// </summary>
    public class BotCommand
    {
        /// <summary>
        /// Gets or sets bot command text.
        /// </summary>
        public string Text { get; set; }
    }
}