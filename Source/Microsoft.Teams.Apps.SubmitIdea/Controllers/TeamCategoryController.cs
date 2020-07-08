// <copyright file="TeamCategoryController.cs" company="Microsoft">
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
    using Microsoft.Teams.Apps.SubmitIdea.Models;

    /// <summary>
    /// Controller to handle team categories API operations.
    /// </summary>
    [Route("api/teamcategory")]
    [ApiController]
    [Authorize(PolicyNames.MustBeTeamMemberUserPolicy)]
    public class TeamCategoryController : BaseSubmitIdeaController
    {
        /// <summary>
        /// Logger implementation to send logs to the logger service.
        /// </summary>
        private readonly ILogger logger;

        /// <summary>
        /// Instance of team category storage provider for team categories.
        /// </summary>
        private readonly ITeamCategoryStorageProvider teamCategoryStorageProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="TeamCategoryController"/> class.
        /// </summary>
        /// <param name="logger">Logger implementation to send logs to the logger service.</param>
        /// <param name="telemetryClient">The Application Insights telemetry client.</param>
        /// <param name="teamCategoryStorageProvider">Team category storage provider dependency injection.</param>
        public TeamCategoryController(
            ILogger<TeamCategoryController> logger,
            TelemetryClient telemetryClient,
            ITeamCategoryStorageProvider teamCategoryStorageProvider)
            : base(telemetryClient)
        {
            this.logger = logger;
            this.teamCategoryStorageProvider = teamCategoryStorageProvider;
        }

        /// <summary>
        /// Get call to retrieve team categories data.
        /// </summary>
        /// <param name="teamId">Team Id - unique value for each Team where categories has configured.</param>
        /// <returns>Represents Team category entity model.</returns>
        [HttpGet]
        public async Task<IActionResult> GetAsync(string teamId)
        {
            this.RecordEvent("Team categories - HTTP Get call requested.");

            try
            {
                this.logger.LogInformation("Call to retrieve team categories data.");
                if (string.IsNullOrEmpty(teamId))
                {
                    this.logger.LogError($"Parameter {nameof(teamId)} is either null or empty.");
                    return this.BadRequest($"Parameter {nameof(teamId)} cannot be null or empty.");
                }

                var teamPreference = await this.teamCategoryStorageProvider.GetTeamCategoriesDataAsync(teamId);
                this.RecordEvent("Team categories - HTTP Get call succeeded");

                return this.Ok(teamPreference);
            }
            catch (Exception ex)
            {
                this.RecordEvent("Team categories - HTTP Get call failed.");
                this.logger.LogError(ex, "Error while making call to team categories service.");
                throw;
            }
        }

        /// <summary>
        /// Post call to store team category details in storage.
        /// </summary>
        /// <param name="teamCategoryEntity">Holds team category detail entity data.</param>
        /// <returns>Returns true for successful operation.</returns>
        [HttpPost]
        public async Task<IActionResult> PostAsync([FromBody] TeamCategoryEntity teamCategoryEntity)
        {
            this.RecordEvent("Team categories - HTTP POST call requested.");

            try
            {
                this.logger.LogInformation("Call to add team category details.");

#pragma warning disable CA1062 // teamCategoryEntity is validated by model validations for null check and is responded with bad request status
                if (string.IsNullOrEmpty(teamCategoryEntity.TeamId))
#pragma warning restore CA1062 // teamCategoryEntity is validated by model validations for null check and is responded with bad request status
                {
                    this.logger.LogError($"Parameter {nameof(teamCategoryEntity.TeamId)} is found null or empty.");
                    return this.BadRequest(new { message = $"Parameter {nameof(teamCategoryEntity.TeamId)} is either null or empty." });
                }

                var postEntity = new TeamCategoryEntity()
                {
                    ChannelId = teamCategoryEntity.TeamId,
                    CreatedByName = this.UserName,
                    CreatedByObjectId = this.UserAadId,
                    CreatedDate = DateTime.UtcNow,
                    Categories = teamCategoryEntity.Categories,
                    TeamId = teamCategoryEntity.TeamId,
                };

                var result = await this.teamCategoryStorageProvider.UpsertTeamCategoriesAsync(postEntity);
                this.RecordEvent("Team categories - HTTP POST call succeeded");

                return this.Ok(result);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error while making call to team category service.");
                this.RecordEvent("Team categories - HTTP POST call failed");
                throw;
            }
        }

        /// <summary>
        /// Patch call to store team category preference details in storage.
        /// </summary>
        /// <param name="teamCategoryEntity">Holds team category detail entity data.</param>
        /// <returns>Returns true for successful operation.</returns>
        [HttpPatch]
        public async Task<IActionResult> PatchAsync([FromBody] TeamCategoryEntity teamCategoryEntity)
        {
            this.RecordEvent("Team categories - HTTP PATCH call requested.");

            try
            {
                this.logger.LogInformation("Call to update team category details.");

#pragma warning disable CA1062 // teamCategoryEntity is validated by model validations for null check and is responded with bad request status
                if (string.IsNullOrEmpty(teamCategoryEntity.TeamId))
#pragma warning restore CA1062 // teamCategoryEntity is validated by model validations for null check and is responded with bad request status
                {
                    this.logger.LogError($"Parameter {nameof(teamCategoryEntity.TeamId)} is found null or empty.");
                    return this.BadRequest(new { message = $"Parameter {nameof(teamCategoryEntity.TeamId)} is either null or empty." });
                }

                var teamCategoryPreference = await this.teamCategoryStorageProvider.GetTeamCategoriesDataAsync(teamCategoryEntity.TeamId);
                if (teamCategoryPreference == null)
                {
                    this.logger.LogError($"Entity for {teamCategoryEntity.TeamId} is not found to update.");
                    this.RecordEvent("Update category - HTTP Patch call failed");

                    return this.BadRequest($"An update cannot be performed for {teamCategoryEntity.TeamId} because entity is not available.");
                }

                teamCategoryPreference.Categories = teamCategoryEntity.Categories;
                teamCategoryPreference.UpdatedByObjectId = this.UserAadId;
                teamCategoryPreference.UpdatedDate = DateTime.UtcNow;

                var result = await this.teamCategoryStorageProvider.UpsertTeamCategoriesAsync(teamCategoryPreference);
                this.RecordEvent("Team categories - HTTP PATCH call succeeded");

                return this.Ok(result);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error while making call to team category service.");
                this.RecordEvent("Team categories - HTTP PATCH call failed");
                throw;
            }
        }

        /// <summary>
        /// Get list of configured categories for a team to show on filter bar dropdown list.
        /// </summary>
        /// <param name="teamId">Team id to get the configured categories for a team.</param>
        /// <returns>List of configured categories.</returns>
        [HttpGet("configured-categories")]
        public async Task<IActionResult> GetConfiguredTagsAsync(string teamId)
        {
            this.RecordEvent("Team categories - HTTP Get call requested.");

            try
            {
                this.logger.LogInformation("Call to get list of configured categories for a team.");
                if (string.IsNullOrEmpty(teamId))
                {
                    this.logger.LogError("Error while creating or updating team category details in storage.");
                    return this.BadRequest(new { message = $"Parameter {nameof(teamId)} is either null or empty." });
                }

                var teamTagDetail = await this.teamCategoryStorageProvider.GetTeamCategoriesDataAsync(teamId);
                this.RecordEvent("Team categories - HTTP Get call succeeded");

                return this.Ok(teamTagDetail?.Categories?.Split(";"));
            }
            catch (Exception ex)
            {
                this.RecordEvent("Team categories - HTTP Get call failed");
                this.logger.LogError(ex, "Error while making call to get configured categories.");
                throw;
            }
        }
    }
}