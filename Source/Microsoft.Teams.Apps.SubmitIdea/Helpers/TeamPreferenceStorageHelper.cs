// <copyright file="TeamPreferenceStorageHelper.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.SubmitIdea.Helpers
{
    using System;
    using Microsoft.Extensions.Logging;
    using Microsoft.Teams.Apps.SubmitIdea.Common.Interfaces;
    using Microsoft.Teams.Apps.SubmitIdea.Models;

    /// <summary>
    /// Implements team preference storage helper which helps to construct the model, get unique tags for team preference.
    /// </summary>
    public class TeamPreferenceStorageHelper : ITeamPreferenceStorageHelper
    {
        /// <summary>
        /// Logger implementation to send logs to the logger service.
        /// </summary>
        private readonly ILogger<TeamPreferenceStorageHelper> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="TeamPreferenceStorageHelper"/> class.
        /// </summary>
        /// <param name="logger">Logger implementation to send logs to the logger service.</param>
        public TeamPreferenceStorageHelper(
            ILogger<TeamPreferenceStorageHelper> logger)
        {
            this.logger = logger;
        }

        /// <summary>
        /// Create team preference model data.
        /// </summary>
        /// <param name="entity">Represents team preference entity object.</param>
        /// <returns>Represents team preference entity model.</returns>
        public TeamPreferenceEntity CreateTeamPreferenceModel(TeamPreferenceEntity entity)
        {
            try
            {
                entity = entity ?? throw new ArgumentNullException(nameof(entity));
                entity.TeamId = entity.TeamId;
                entity.CreatedDate = DateTime.UtcNow;
                entity.UpdatedDate = DateTime.UtcNow;

                return entity;
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Exception occurred while preparing the team preference entity model data");
                throw;
            }
        }
    }
}
