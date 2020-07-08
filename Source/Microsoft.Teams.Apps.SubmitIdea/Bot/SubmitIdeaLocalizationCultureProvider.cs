// <copyright file="SubmitIdeaLocalizationCultureProvider.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.SubmitIdea.Bot
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Localization;
    using Microsoft.Bot.Schema;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// This class is responsible for implementing the <see cref="IRequestCultureProvider"/> for Bot Activities
    /// received from BotFramework.
    /// </summary>
    internal sealed class SubmitIdeaLocalizationCultureProvider : IRequestCultureProvider
    {
        /// <summary>
        /// Get the culture of current request.
        /// </summary>
        /// <param name="httpContext">The current HTTP request.</param>
        /// <returns>A Task resolving to the culture info if found, null otherwise.</returns>
        public async Task<ProviderCultureResult> DetermineProviderCultureResult(HttpContext httpContext)
        {
            if (httpContext?.Request?.Body?.CanRead != true)
            {
                return null;
            }

            var isBotFrameworkUserAgent =
                httpContext.Request.Headers["User-Agent"]
                .Any(userAgent => userAgent.Contains("Microsoft-BotFramework", StringComparison.OrdinalIgnoreCase));

            try
            {
                if (isBotFrameworkUserAgent)
                {
                    // Wrap the request stream so that we can rewind it back to the start for regular request processing.
                    httpContext.Request.EnableBuffering();

                    // Read the request body, parse out the activity object, and set the parsed culture information.
                    var streamReader = new StreamReader(httpContext.Request.Body, Encoding.UTF8, true, 1024, leaveOpen: true);
                    using (var jsonReader = new JsonTextReader(streamReader))
                    {
                        var obj = await JObject.LoadAsync(jsonReader);
                        var activity = obj.ToObject<Activity>();

                        var result = new ProviderCultureResult(activity.Locale);
                        httpContext.Request.Body.Seek(0, SeekOrigin.Begin);
                        return result;
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }

            return null;
        }
    }
}