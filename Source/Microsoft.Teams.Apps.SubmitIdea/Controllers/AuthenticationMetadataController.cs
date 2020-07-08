// <copyright file="AuthenticationMetadataController.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.SubmitIdea.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Options;
    using Microsoft.Teams.Apps.SubmitIdea.Models.Configuration;

    /// <summary>
    /// Controller for sign in authentication data.
    /// </summary>
    [Route("api/authenticationMetadata")]
    public class AuthenticationMetadataController : ControllerBase
    {
        /// <summary>
        /// Represents a set of key/value application configuration properties for bot.
        /// </summary>
        private readonly IOptions<AzureActiveDirectorySettings> options;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticationMetadataController"/> class.
        /// </summary>
        /// <param name="options">A set of key/value application configuration properties for bot.</param>
        public AuthenticationMetadataController(IOptions<AzureActiveDirectorySettings> options)
        {
            this.options = options ?? throw new ArgumentNullException(nameof(options));
        }

        /// <summary>
        /// Get authentication consent Url.
        /// </summary>
        /// <param name="windowLocationOriginDomain">Window location origin domain.</param>
        /// <param name="loginHint">User Principal Name value.</param>
        /// <returns>Conset Url.</returns>
        [HttpGet("consentUrl")]
        public string GetConsentUrl(
            [FromQuery]string windowLocationOriginDomain,
            [FromQuery]string loginHint)
        {
            var consentUrlComponentDictionary = new Dictionary<string, string>
            {
                ["redirect_uri"] = $"https://{windowLocationOriginDomain}/signin-simple-end",
                ["client_id"] = this.options.Value.ClientId,
                ["response_type"] = "id_token",
                ["response_mode"] = "fragment",
                ["scope"] = "https://graph.microsoft.com/User.Read openid profile",
                ["nonce"] = Guid.NewGuid().ToString(),
                ["state"] = Guid.NewGuid().ToString(),
                ["login_hint"] = loginHint,
            };

            var consentUrlComponentList = consentUrlComponentDictionary
                .Select(p => $"{p.Key}={HttpUtility.UrlEncode(p.Value)}")
                .ToList();

            var consentUrlPrefix = $"https://login.microsoftonline.com/{this.options.Value.TenantId}/oauth2/v2.0/authorize?";
            var consentUrlString = consentUrlPrefix + string.Join('&', consentUrlComponentList);

            return consentUrlString;
        }
    }
}
