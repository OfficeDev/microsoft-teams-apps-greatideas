// <copyright file="TeamsInfoHelper.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.SubmitIdea.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Integration.AspNet.Core;
    using Microsoft.Bot.Builder.Teams;
    using Microsoft.Bot.Connector.Authentication;
    using Microsoft.Bot.Schema;
    using Microsoft.Bot.Schema.Teams;
    using Microsoft.Extensions.Logging;
    using Microsoft.Teams.Apps.SubmitIdea.Common;
    using Microsoft.Teams.Apps.SubmitIdea.Common.Interfaces;

    /// <summary>
    /// Class that handles the helper methods to fetch team channel information.
    /// </summary>
    public class TeamsInfoHelper : ITeamsInfoHelper
    {
        /// <summary>
        /// Bot adapter.
        /// </summary>
        private readonly IBotFrameworkHttpAdapter botAdapter;

        /// <summary>
        /// Provider to fetch team details from the Storage.
        /// </summary>
        private readonly ITeamStorageProvider teamStorageProvider;

        /// <summary>
        /// Microsoft application credentials.
        /// </summary>
        private readonly MicrosoftAppCredentials microsoftAppCredentials;

        /// <summary>
        /// Instance to send logs to the logger service.
        /// </summary>
        private readonly ILogger<TeamsInfoHelper> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="TeamsInfoHelper"/> class.
        /// </summary>
        /// <param name="botAdapter">Bot adapter.</param>
        /// <param name="teamStorageProvider">Provider to fetch team details from Azure Table Storage.</param>
        /// <param name="microsoftAppCredentials">Microsoft application credentials.</param>
        /// <param name="logger">Logger implementation to send logs to the logger service.</param>
        public TeamsInfoHelper(
            IBotFrameworkHttpAdapter botAdapter,
            ITeamStorageProvider teamStorageProvider,
            MicrosoftAppCredentials microsoftAppCredentials,
            ILogger<TeamsInfoHelper> logger)
        {
            this.botAdapter = botAdapter;
            this.teamStorageProvider = teamStorageProvider;
            this.microsoftAppCredentials = microsoftAppCredentials;
            this.logger = logger;
        }

        /// <summary>
        /// To fetch team channel information for specified team.
        /// </summary>
        /// <param name="teamId">Team id.</param>
        /// <returns>Team channel information.</returns>
        public async Task<IEnumerable<TeamsChannelAccount>> GetTeamMembersAsync(string teamId)
        {
            IEnumerable<TeamsChannelAccount> teamsChannelAccounts = new List<TeamsChannelAccount>();

            var teamDetails = await this.teamStorageProvider.GetTeamDetailAsync(teamId);
            string serviceUrl = teamDetails.ServiceUrl;
            var conversationReference = new ConversationReference
            {
                ChannelId = Constants.TeamsBotFrameworkChannelId,
                ServiceUrl = serviceUrl,
            };
            await ((BotFrameworkAdapter)this.botAdapter).ContinueConversationAsync(
                this.microsoftAppCredentials.MicrosoftAppId,
                conversationReference,
                async (context, token) =>
                {
                    teamsChannelAccounts = await TeamsInfo.GetTeamMembersAsync(context, teamId, CancellationToken.None);
                }, default);

            return teamsChannelAccounts;
        }

        /// <summary>
        /// To fetch team member information for specified team.
        /// Return null if the member is not found in team id or either of the information is incorrect.
        /// Caller should handle null value to throw unauthorized if required
        /// </summary>
        /// <param name="teamId">Team id.</param>
        /// <param name="userId">User object id.</param>
        /// <returns>Returns team member information.</returns>
        public async Task<TeamsChannelAccount> GetTeamMemberAsync(string teamId, string userId)
        {
            TeamsChannelAccount teamMember = new TeamsChannelAccount();

            try
            {
                var teamDetails = await this.teamStorageProvider.GetTeamDetailAsync(teamId);
                string serviceUrl = teamDetails.ServiceUrl;

                var conversationReference = new ConversationReference
                {
                    ChannelId = Constants.TeamsBotFrameworkChannelId,
                    ServiceUrl = serviceUrl,
                };
                await ((BotFrameworkAdapter)this.botAdapter).ContinueConversationAsync(
                    this.microsoftAppCredentials.MicrosoftAppId,
                    conversationReference,
                    async (context, token) =>
                    {
                        teamMember = await TeamsInfo.GetTeamMemberAsync(context, userId, teamId, CancellationToken.None);
                    }, default);
            }
#pragma warning disable CA1031 // Catching general exceptions to log exception details in telemetry client.
            catch (Exception ex)
#pragma warning restore CA1031 // Catching general exceptions to log exception details in telemetry client.
            {
                this.logger.LogError(ex, $"Error occurred while fetching team member for team: {teamId} - user object id: {userId} ");

                // Return null if the member is not found in team id or either of the information is incorrect.
                // Caller should handle null value to throw unauthorized if required.
                return null;
            }

            return teamMember;
        }
    }
}
