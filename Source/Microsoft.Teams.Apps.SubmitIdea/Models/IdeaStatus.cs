// <copyright file="IdeaStatus.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.SubmitIdea.Models
{
    /// <summary>
    /// Enum that represents idea status
    /// </summary>
    public enum IdeaStatus
    {
        /// <summary>
        /// Represents pending status
        /// </summary>
        Pending,

        /// <summary>
        /// Represents approved status
        /// </summary>
        Approved,

        /// <summary>
        /// Represents rejected status
        /// </summary>
        Rejected,
    }
}
