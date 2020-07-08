// <copyright file="MustBeCuratorTeamMemberHandler.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.SubmitIdea.Authentication
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Options;
    using Microsoft.Teams.Apps.SubmitIdea.Common;
    using Microsoft.Teams.Apps.SubmitIdea.Common.Interfaces;
    using Microsoft.Teams.Apps.SubmitIdea.Models;

    /// <summary>
    /// This class is an authorization handler, which handles the authorization requirement.
    /// </summary>
    public class MustBeCuratorTeamMemberHandler : AuthorizationHandler<MustBeCuratorTeamMemberRequirement>
    {
        /// <summary>
        /// A set of key/value configuration of bot settings.
        /// </summary>
        private readonly IOptions<BotSettings> botSettings;

        /// <summary>
        /// Provider to fetch team details from bot adapter.
        /// </summary>
        private readonly ITeamsInfoHelper teamsInfoHelper;

        /// <summary>
        /// Cache for storing authorization result.
        /// </summary>
        private readonly IMemoryCache memoryCache;

        /// <summary>
        /// Initializes a new instance of the <see cref="MustBeCuratorTeamMemberHandler"/> class.
        /// </summary>
        /// <param name="botSettings">Represents a set of key/value bot settings.</param>
        /// <param name="teamsInfoHelper">Provider to fetch team details from bot adapter.</param>
        /// <param name="memoryCache">MemoryCache instance for caching authorization result.</param>
        public MustBeCuratorTeamMemberHandler(IOptions<BotSettings> botSettings, ITeamsInfoHelper teamsInfoHelper, IMemoryCache memoryCache)
        {
            botSettings = botSettings ?? throw new ArgumentNullException(nameof(botSettings));

            this.botSettings = botSettings;
            this.botSettings.Value.CacheDurationInMinutes = this.botSettings.Value.CacheDurationInMinutes > 0
                ? this.botSettings.Value.CacheDurationInMinutes : 60;

            this.teamsInfoHelper = teamsInfoHelper;
            this.memoryCache = memoryCache;
        }

        /// <summary>
        /// This method handles the authorization requirement.
        /// </summary>
        /// <param name="context">AuthorizationHandlerContext instance.</param>
        /// <param name="requirement">IAuthorizationRequirement instance.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        protected async override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            MustBeCuratorTeamMemberRequirement requirement)
        {
            context = context ?? throw new ArgumentNullException(nameof(context));

            var oidClaimType = "http://schemas.microsoft.com/identity/claims/objectidentifier";

            var oidClaim = context.User.Claims.FirstOrDefault(p => oidClaimType.Equals(p.Type, StringComparison.OrdinalIgnoreCase));

            if (await this.ValidateUserAsync(this.botSettings.Value.CuratorTeamId, oidClaim?.Value))
            {
                context.Succeed(requirement);
            }
        }

        /// <summary>
        /// Check if a user is a member of a curator team.
        /// </summary>
        /// <param name="teamId">The team id of that the uses to check if the user is a member of curator team. </param>
        /// <param name="userAadObjectId">The user's Azure Active Directory object id.</param>
        /// <returns>The flag indicates that the user is a part of certain team or not.</returns>
        private async Task<bool> ValidateUserAsync(string teamId, string userAadObjectId)
        {
            bool isCacheEntryExists = this.memoryCache.TryGetValue(this.GetCacheKey(userAadObjectId), out bool isUserValidMember);
            if (!isCacheEntryExists)
            {
                var teamMember = await this.teamsInfoHelper.GetTeamMemberAsync(teamId, userAadObjectId);
                isUserValidMember = teamMember != null;

                this.memoryCache.Set(this.GetCacheKey(userAadObjectId), isUserValidMember, TimeSpan.FromMinutes(this.botSettings.Value.CacheDurationInMinutes));
            }

            return isUserValidMember;
        }

        private string GetCacheKey(string userAadObjectId)
        {
            return CacheKeysConstants.Curator + this.botSettings.Value.CuratorTeamId + userAadObjectId;
        }
    }
}
