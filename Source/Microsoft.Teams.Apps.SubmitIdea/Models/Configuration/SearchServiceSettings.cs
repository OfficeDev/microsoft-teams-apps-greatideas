// <copyright file="SearchServiceSettings.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.SubmitIdea.Models.Configuration
{
    /// <summary>
    /// A class that represents settings related to search service.
    /// </summary>
    public class SearchServiceSettings : StorageSettings
    {
        /// <summary>
        /// Gets or sets search service name.
        /// </summary>
        public string SearchServiceName { get; set; }

        /// <summary>
        /// Gets or sets search service query api key.
        /// </summary>
        public string SearchServiceQueryApiKey { get; set; }

        /// <summary>
        /// Gets or sets search service admin api key.
        /// </summary>
        public string SearchServiceAdminApiKey { get; set; }
    }
}
