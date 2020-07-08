// <copyright file="SettingsController.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.SubmitIdea.Controllers
{
    using System;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Microsoft.Teams.Apps.SubmitIdea.Models;

    /// <summary>
    /// This ASP controller is created to handle award requests and leverages TeamMemberUserPolicy for authorization.
    /// Dependency injection will provide the storage implementation and logger.
    /// Inherits <see cref="BaseSubmitIdeaController"/> to gather user claims for all incoming requests.
    /// The class provides endpoint to share required application settings.
    /// </summary>
    [Route("api/settings")]
    [ApiController]
    [Authorize]
    public class SettingsController : ControllerBase
    {
        /// <summary>
        /// Logger implementation to send logs to the logger service.
        /// </summary>
        private readonly ILogger<SettingsController> logger;

        /// <summary>
        /// Represents a set of key/value bot settings.
        /// </summary>
        private readonly IOptions<BotSettings> botSettings;

        /// <summary>
        /// Represents a set of key/value telemetry settings.
        /// </summary>
        private readonly IOptions<TelemetrySettings> telemetrySettings;

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsController"/> class.
        /// </summary>
        /// <param name="logger">Logger implementation to send logs to the logger service.</param>
        /// <param name="botSettings">Represents a set of key/value bot settings.</param>
        /// <param name="telemetrySettings">Represents a set of key/value telemetry settings.</param>
        public SettingsController(ILogger<SettingsController> logger, IOptions<BotSettings> botSettings, IOptions<TelemetrySettings> telemetrySettings)
        {
            this.logger = logger;
            this.botSettings = botSettings;
            this.telemetrySettings = telemetrySettings;
        }

        /// <summary>
        /// Get bot setting to client application.
        /// </summary>
        /// <returns>Bot id.</returns>
        [HttpGet("botsettings")]
        public IActionResult GetBotSettings()
        {
            try
            {
                return this.Ok(new
                {
                    botId = this.botSettings.Value.MicrosoftAppId,
                    instrumentationKey = this.telemetrySettings.Value.InstrumentationKey,
                });
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"Error while fetching bot setting.");
                throw;
            }
        }
    }
}