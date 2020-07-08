// <copyright file="Preference.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.SubmitIdea.Models
{
    /// <summary>
    /// Class which holds submitted data of preference.
    /// </summary>
    public class Preference
    {
        /// <summary>
        /// Gets or sets team preference entity model.
        /// </summary>
        public TeamPreferenceEntity ConfigureDetails { get; set; }

        /// <summary>
        /// Gets or sets Command to show submit or cancel event on Task Module.
        /// </summary>
        public string Command { get; set; }
    }
}
