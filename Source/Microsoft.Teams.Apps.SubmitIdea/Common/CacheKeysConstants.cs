// <copyright file="CacheKeysConstants.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.SubmitIdea.Common
{
    /// <summary>
    /// Constants to list keys used by cache layers in application.
    /// </summary>
    public static class CacheKeysConstants
    {
        /// <summary>
        /// Cache key for curators.
        /// </summary>
        public const string Curator = "_Curator";

        /// <summary>
        /// Cache key for Team members.
        /// </summary>
        public const string TeamMember = "_Tm";
    }
}
