// <copyright file="StorageSettings.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.SubmitIdea.Models.Configuration
{
    /// <summary>
    /// A class which helps to provide storage settings.
    /// </summary>
    public class StorageSettings : BotSettings
    {
        /// <summary>
        /// Gets or sets storage connection string.
        /// </summary>
        public string ConnectionString { get; set; }
    }
}
