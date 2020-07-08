// <copyright file="AzureActiveDirectorySettings.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.SubmitIdea.Models.Configuration
{
    /// <summary>
    /// A class which helps to provide Azure Active Directory settings for application.
    /// </summary>
    public class AzureActiveDirectorySettings
    {
        /// <summary>
        /// Gets or sets tenant id of application.
        /// </summary>
        public string TenantId { get; set; }

        /// <summary>
        /// Gets or sets client id of application.
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// Gets or sets application id URI.
        /// </summary>
        public string ApplicationIdURI { get; set; }

        /// <summary>
        /// Gets or sets valid issuer URL.
        /// </summary>
        public string ValidIssuers { get; set; }

        /// <summary>
        /// Gets or sets Azure Active Directory instance.
        /// </summary>
        public string Instance { get; set; }
    }
}
