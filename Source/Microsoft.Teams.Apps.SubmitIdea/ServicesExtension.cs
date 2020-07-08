// <copyright file="ServicesExtension.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.SubmitIdea
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Web;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Localization;
    using Microsoft.Azure.Search;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Azure;
    using Microsoft.Bot.Builder.BotFramework;
    using Microsoft.Bot.Builder.Integration.AspNet.Core;
    using Microsoft.Bot.Connector.Authentication;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Teams.Apps.SubmitIdea.Bot;
    using Microsoft.Teams.Apps.SubmitIdea.Common;
    using Microsoft.Teams.Apps.SubmitIdea.Common.BackgroundService;
    using Microsoft.Teams.Apps.SubmitIdea.Common.Interfaces;
    using Microsoft.Teams.Apps.SubmitIdea.Common.Providers;
    using Microsoft.Teams.Apps.SubmitIdea.Common.SearchServices;
    using Microsoft.Teams.Apps.SubmitIdea.Helpers;
    using Microsoft.Teams.Apps.SubmitIdea.Models;
    using Microsoft.Teams.Apps.SubmitIdea.Models.Configuration;

    /// <summary>
    /// Class which helps to extend ServiceCollection.
    /// </summary>
    public static class ServicesExtension
    {
        /// <summary>
        /// Adds application configuration settings to specified IServiceCollection.
        /// </summary>
        /// <param name="services">Collection of services.</param>
        /// <param name="configuration">Application configuration properties.</param>
        public static void AddConfigurationSettings(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<BotSettings>(options =>
            {
                options.SecurityKey = configuration.GetValue<string>("App:SecurityKey");
                options.AppBaseUri = configuration.GetValue<string>("App:AppBaseUri");
                options.TenantId = configuration.GetValue<string>("App:TenantId");
                options.CacheDurationInMinutes = configuration.GetValue<int>("Cache:DurationInMinutes");
                options.MedianFirstRetryDelay = configuration.GetValue<double>("RetryPolicy:medianFirstRetryDelay");
                options.RetryCount = configuration.GetValue<int>("RetryPolicy:retryCount");
                options.MicrosoftAppId = configuration.GetValue<string>("MicrosoftAppId");
                options.MicrosoftAppPassword = configuration.GetValue<string>("MicrosoftAppPassword");
                options.CuratorTeamId = ParseTeamIdFromDeepLink(configuration.GetValue<string>("App:CuratorTeamLink"));
            });

            services.Configure<AzureActiveDirectorySettings>(options =>
            {
                options.TenantId = configuration.GetValue<string>("AzureAd:TenantId");
                options.ClientId = configuration.GetValue<string>("AzureAd:ClientId");
                options.ApplicationIdURI = configuration.GetValue<string>("AzureAd:ApplicationIdURI");
                options.ValidIssuers = configuration.GetValue<string>("AzureAd:ValidIssuers");
                options.Instance = configuration.GetValue<string>("AzureAd:Instance");
            });

            services.Configure<TelemetrySettings>(options =>
            {
                options.InstrumentationKey = configuration.GetValue<string>("ApplicationInsights:InstrumentationKey");
            });

            services.Configure<StorageSettings>(options =>
            {
                options.ConnectionString = configuration.GetValue<string>("Storage:ConnectionString");
            });

            services.Configure<SearchServiceSettings>(searchServiceSettings =>
            {
                searchServiceSettings.SearchServiceName = configuration.GetValue<string>("SearchService:SearchServiceName");
                searchServiceSettings.SearchServiceQueryApiKey = configuration.GetValue<string>("SearchService:SearchServiceQueryApiKey");
                searchServiceSettings.SearchServiceAdminApiKey = configuration.GetValue<string>("SearchService:SearchServiceAdminApiKey");
                searchServiceSettings.ConnectionString = configuration.GetValue<string>("Storage:ConnectionString");
            });
        }

        /// <summary>
        /// Adds helpers to specified IServiceCollection.
        /// </summary>
        /// <param name="services">Collection of services.</param>
        /// <param name="configuration">Application configuration properties.</param>
        public static void AddHelpers(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddApplicationInsightsTelemetry(configuration.GetValue<string>("ApplicationInsights:InstrumentationKey"));

            services.AddSingleton<IIdeaStorageProvider, IdeaStorageProvider>();
            services.AddSingleton<ITeamPreferenceStorageProvider, TeamPreferenceStorageProvider>();
            services.AddSingleton<IUserVoteStorageProvider, UserVoteStorageProvider>();
            services.AddSingleton<ICategoryStorageProvider, CategoryStorageProvider>();
            services.AddSingleton<ITeamStorageProvider, TeamStorageProvider>();
            services.AddSingleton<ITeamCategoryStorageProvider, TeamCategoryStorageProvider>();

            services.AddSingleton<IIdeaSearchService, IdeaSearchService>();

            services.AddSingleton<IMessagingExtensionHelper, MessagingExtensionHelper>();
            services.AddSingleton<IIdeaStorageHelper, IdeaStorageHelper>();
            services.AddSingleton<ITeamPreferenceStorageHelper, TeamPreferenceStorageHelper>();
            services.AddSingleton<IUserVoteStorageHelper, UserVoteStorageHelper>();
#pragma warning disable CA2000 // This is singleton which has lifetime same as the app
            services.AddSingleton(new SearchServiceClient(configuration.GetValue<string>("SearchService:SearchServiceName"), new SearchCredentials(configuration.GetValue<string>("SearchService:SearchServiceAdminApiKey"))));
            services.AddSingleton(new SearchIndexClient(configuration.GetValue<string>("SearchService:SearchServiceName"), Constants.TeamIdeaIndexName, new SearchCredentials(configuration.GetValue<string>("SearchService:SearchServiceQueryApiKey"))));
#pragma warning restore CA2000 // This is singleton which has lifetime same as the app
            services.AddHostedService<DigestNotificationBackgroundService>();
            services.AddSingleton<IDigestNotificationHelper, DigestNotificationHelper>();
            services.AddSingleton<ITeamsInfoHelper, TeamsInfoHelper>();
        }

        /// <summary>
        /// Adds credential providers for authentication.
        /// </summary>
        /// <param name="services">Collection of services.</param>
        /// <param name="configuration">Application configuration properties.</param>
        public static void AddCredentialProviders(this IServiceCollection services, IConfiguration configuration)
        {
            services
                .AddSingleton<ICredentialProvider, ConfigurationCredentialProvider>();
            services.AddSingleton(new MicrosoftAppCredentials(configuration.GetValue<string>("MicrosoftAppId"), configuration.GetValue<string>("MicrosoftAppPassword")));
        }

        /// <summary>
        /// Adds user state and conversation state to specified IServiceCollection.
        /// </summary>
        /// <param name="services">Collection of services.</param>
        /// <param name="configuration">Application configuration properties.</param>
        public static void AddBotStates(this IServiceCollection services, IConfiguration configuration)
        {
            // Create the User state. (Used in this bot's Dialog implementation.)
            services.AddSingleton<UserState>();

            // Create the Conversation state. (Used by the Dialog system itself.)
            services.AddSingleton<ConversationState>();

            // For conversation state.
            services.AddSingleton<IStorage>(new AzureBlobStorage(configuration.GetValue<string>("Storage:ConnectionString"), "bot-state"));
        }

        /// <summary>
        /// Adds bot framework adapter to specified IServiceCollection.
        /// </summary>
        /// <param name="services">Collection of services.</param>
        public static void AddBotFrameworkAdapter(this IServiceCollection services)
        {
            // Create the Bot Framework Adapter with error handling enabled.
            services.AddSingleton<IBotFrameworkHttpAdapter, SubmitIdeaAdapterWithErrorHandler>();

            services.AddTransient<IBot, SubmitIdeaActivityHandler>();

            // Create the Middleware that will be added to the middleware pipeline in the AdapterWithErrorHandler.
            services.AddSingleton<SubmitIdeaActivityMiddleware>();
            services.AddTransient(serviceProvider => (BotFrameworkAdapter)serviceProvider.GetRequiredService<IBotFrameworkHttpAdapter>());
        }

        /// <summary>
        /// Adds localization.
        /// </summary>
        /// <param name="services">Collection of services.</param>
        /// <param name="configuration">Application configuration properties.</param>
        public static void AddLocalization(this IServiceCollection services, IConfiguration configuration)
        {
            // Add i18n.
            services.AddLocalization(options => options.ResourcesPath = "Resources");

            services.Configure<RequestLocalizationOptions>(options =>
            {
                var defaultCulture = CultureInfo.GetCultureInfo(configuration.GetValue<string>("i18n:DefaultCulture"));
                var supportedCultures = configuration.GetValue<string>("i18n:SupportedCultures").Split(',')
                    .Select(culture => CultureInfo.GetCultureInfo(culture))
                    .ToList();

                options.DefaultRequestCulture = new RequestCulture(defaultCulture);
                options.SupportedCultures = supportedCultures;
                options.SupportedUICultures = supportedCultures;

                options.RequestCultureProviders = new List<IRequestCultureProvider>
                {
                    new SubmitIdeaLocalizationCultureProvider(),
                };
            });
        }

        /// <summary>
        /// Based on deep link URL received find team id and set it.
        /// </summary>
        /// <param name="teamIdDeepLink">Deep link to get the team id.</param>
        /// <returns>A team id from the deep link URL.</returns>
        private static string ParseTeamIdFromDeepLink(string teamIdDeepLink)
        {
            // team id regex match
            // for a pattern like https://teams.microsoft.com/l/team/19%3a64c719819fb1412db8a28fd4a30b581a%40thread.tacv2/conversations?groupId=53b4782c-7c98-4449-993a-441870d10af9&tenantId=72f988bf-86f1-41af-91ab-2d7cd011db47
            // regex checks for 19%3a64c719819fb1412db8a28fd4a30b581a%40thread.tacv2
            var match = Regex.Match(teamIdDeepLink, @"teams.microsoft.com/l/team/(\S+)/");
            if (!match.Success)
            {
                throw new ArgumentException($"Invalid team found.");
            }

            return HttpUtility.UrlDecode(match.Groups[1].Value);
        }
    }
}