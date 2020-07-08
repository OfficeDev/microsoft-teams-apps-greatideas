// <copyright file="UserConversationState.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.SubmitIdea.Models
{
    /// <summary>
    /// A class that represents user conversation state model.
    /// </summary>
    public class UserConversationState
    {
        /// <summary>
        /// Gets or sets a value indicating whether the welcome card is sent to user or not.
        /// </summary>
        /// <remark>Value is null when bot is installed for first time.</remark>
        public bool IsWelcomeCardSent { get; set; }
    }
}
