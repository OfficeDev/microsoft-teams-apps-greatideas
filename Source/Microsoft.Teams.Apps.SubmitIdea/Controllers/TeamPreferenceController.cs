// <copyright file="TeamPreferenceController.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.SubmitIdea.Controllers
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.ApplicationInsights;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Microsoft.Teams.Apps.SubmitIdea.Authentication;
    using Microsoft.Teams.Apps.SubmitIdea.Common.Interfaces;

    /// <summary>
    /// Controller to handle team preference API operations.
    /// </summary>
    [Route("api/teampreference")]
    [ApiController]
    [Authorize]
    public class TeamPreferenceController : BaseSubmitIdeaController
    {
        /// <summary>
        /// Logger implementation to send logs to the logger service.
        /// </summary>
        private readonly ILogger logger;

        /// <summary>
        /// Instance of team preference storage helper.
        /// </summary>
        private readonly ITeamPreferenceStorageHelper teamPreferenceStorageHelper;

        /// <summary>
        /// Instance of team preference storage provider for team preferences.
        /// </summary>
        private readonly ITeamPreferenceStorageProvider teamPreferenceStorageProvider;

        /// <summary>
        /// Instance of Search service for working with storage.
        /// </summary>
        private readonly IIdeaSearchService teamIdeaSearchService;

        /// <summary>
        /// Initializes a new instance of the <see cref="TeamPreferenceController"/> class.
        /// </summary>
        /// <param name="logger">Logger implementation to send logs to the logger service.</param>
        /// <param name="telemetryClient">The Application Insights telemetry client.</param>
        /// <param name="teamPreferenceStorageHelper">Team preference storage helper dependency injection.</param>
        /// <param name="teamPreferenceStorageProvider">Team preference storage provider dependency injection.</param>
        /// <param name="teamIdeaSearchService">The team post search service dependency injection.</param>
        public TeamPreferenceController(
            ILogger<TeamPreferenceController> logger,
            TelemetryClient telemetryClient,
            ITeamPreferenceStorageHelper teamPreferenceStorageHelper,
            ITeamPreferenceStorageProvider teamPreferenceStorageProvider,
            IIdeaSearchService teamIdeaSearchService)
            : base(telemetryClient)
        {
            this.logger = logger;
            this.teamPreferenceStorageHelper = teamPreferenceStorageHelper;
            this.teamPreferenceStorageProvider = teamPreferenceStorageProvider;
            this.teamIdeaSearchService = teamIdeaSearchService;
        }

        /// <summary>
        /// Get call to retrieve team preference data.
        /// </summary>
        /// <param name="teamId">Team id - unique value for each Team where preference has configured.</param>
        /// <returns>Represents Team preference entity model.</returns>
        [HttpGet]
        [Authorize(PolicyNames.MustBeTeamMemberUserPolicy)]
        public async Task<IActionResult> GetAsync(string teamId)
        {
            this.RecordEvent("Team preferences - HTTP Get call requested.");

            try
            {
                this.logger.LogInformation("Call to retrieve list of team preference.");

                if (string.IsNullOrEmpty(teamId))
                {
                    this.logger.LogError($"Parameter {nameof(teamId)} is found to be null or empty.");
                    return this.BadRequest(new { message = $"Parameter {nameof(teamId)} cannot be null or empty." });
                }

                var teamPreference = await this.teamPreferenceStorageProvider.GetTeamPreferenceDataAsync(teamId);
                this.RecordEvent("Team preferences - HTTP Get call succeeded");

                return this.Ok(teamPreference);
            }
            catch (Exception ex)
            {
                this.RecordEvent("Team preferences - HTTP Get call failed");
                this.logger.LogError(ex, "Error while making call to team preference service.");
                throw;
            }
        }
    }
}