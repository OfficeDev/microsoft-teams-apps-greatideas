// <copyright file="SubmitIdeaActivityHandler.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.SubmitIdea.Bot
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.ApplicationInsights;
    using Microsoft.ApplicationInsights.DataContracts;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Teams;
    using Microsoft.Bot.Schema;
    using Microsoft.Bot.Schema.Teams;
    using Microsoft.Extensions.Localization;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Microsoft.Teams.Apps.SubmitIdea.Cards;
    using Microsoft.Teams.Apps.SubmitIdea.Common;
    using Microsoft.Teams.Apps.SubmitIdea.Common.Interfaces;
    using Microsoft.Teams.Apps.SubmitIdea.Helpers;
    using Microsoft.Teams.Apps.SubmitIdea.Models;
    using Newtonsoft.Json;

    /// <summary>
    /// This class is responsible for reacting to incoming events from Microsoft Teams sent from BotFramework.
    /// </summary>
    public sealed class SubmitIdeaActivityHandler : TeamsActivityHandler
    {
        /// <summary>
        /// Sets the height of the error message task module.
        /// </summary>
        private const int ErrorMessageTaskModuleHeight = 460;

        /// <summary>
        /// Sets the width of the error message task module.
        /// </summary>
        private const int ErrorMessageTaskModuleWidth = 600;

        /// <summary>
        /// State management object for maintaining user conversation state.
        /// </summary>
        private readonly BotState userState;

        /// <summary>
        /// A set of key/value application configuration properties for bot settings.
        /// </summary>
        private readonly IOptions<BotSettings> botOptions;

        /// <summary>
        /// Instance to send logs to the logger service.
        /// </summary>
        private readonly ILogger<SubmitIdeaActivityHandler> logger;

        /// <summary>
        /// The current cultures' string localizer.
        /// </summary>
        private readonly IStringLocalizer<Strings> localizer;

        /// <summary>
        /// Instance of Application Insights Telemetry client.
        /// </summary>
        private readonly TelemetryClient telemetryClient;

        /// <summary>
        /// Messaging Extension search helper to fetch idea details from storage.
        /// </summary>
        private readonly IMessagingExtensionHelper messagingExtensionHelper;

        /// <summary>
        /// Instance of team preference storage helper.
        /// </summary>
        private readonly ITeamPreferenceStorageHelper teamPreferenceStorageHelper;

        /// <summary>
        /// Instance of team preference storage provider to add/update digest preferences for team.
        /// </summary>
        private readonly ITeamPreferenceStorageProvider teamPreferenceStorageProvider;

        /// <summary>
        /// Provider for fetching information about team details from storage.
        /// </summary>
        private readonly ITeamStorageProvider teamStorageProvider;

        /// <summary>
        /// Represents unique id of a  curator Team.
        /// </summary>
        private readonly string curatorTeamId;

        /// <summary>
        /// Initializes a new instance of the <see cref="SubmitIdeaActivityHandler"/> class.
        /// </summary>
        /// <param name="logger">Instance to send logs to the logger service.</param>
        /// <param name="localizer">The current cultures' string localizer.</param>
        /// <param name="telemetryClient">The Application Insights telemetry client.</param>
        /// <param name="messagingExtensionHelper">Messaging Extension helper dependency injection.</param>
        /// <param name="userState">State management object for maintaining user conversation state.</param>
        /// <param name="teamPreferenceStorageHelper">Team preference storage helper dependency injection.</param>
        /// <param name="teamPreferenceStorageProvider">Team preference storage provider dependency injection.</param>
        /// <param name="botOptions">A set of key/value application configuration properties for activity handler.</param>
        /// <param name="teamStorageProvider">Provider for fetching information about team details from storage.</param>
        public SubmitIdeaActivityHandler(
            ILogger<SubmitIdeaActivityHandler> logger,
            IStringLocalizer<Strings> localizer,
            TelemetryClient telemetryClient,
            IMessagingExtensionHelper messagingExtensionHelper,
            UserState userState,
            ITeamPreferenceStorageHelper teamPreferenceStorageHelper,
            ITeamPreferenceStorageProvider teamPreferenceStorageProvider,
            IOptions<BotSettings> botOptions,
            ITeamStorageProvider teamStorageProvider)
        {
            this.logger = logger;
            this.localizer = localizer;
            this.telemetryClient = telemetryClient;
            this.messagingExtensionHelper = messagingExtensionHelper;
            this.userState = userState;
            this.teamPreferenceStorageHelper = teamPreferenceStorageHelper;
            this.teamPreferenceStorageProvider = teamPreferenceStorageProvider;
            this.botOptions = botOptions ?? throw new ArgumentNullException(nameof(botOptions));
            this.curatorTeamId = botOptions.Value.CuratorTeamId;
            this.teamStorageProvider = teamStorageProvider;
        }

        /// <summary>
        /// Handles an incoming activity.
        /// </summary>
        /// <param name="turnContext">Context object containing information cached for a single turn of conversation with a user.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>
        /// Reference link: https://docs.microsoft.com/en-us/dotnet/api/microsoft.bot.builder.activityhandler.onturnasync?view=botbuilder-dotnet-stable.
        /// </remarks>
        public override Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default)
        {
            turnContext = turnContext ?? throw new ArgumentNullException(nameof(turnContext));
            this.RecordEvent(nameof(this.OnTurnAsync), turnContext);

            return base.OnTurnAsync(turnContext, cancellationToken);
        }

        /// <summary>
        /// Invoked when members other than this bot (like a user) are removed from the conversation.
        /// </summary>
        /// <param name="turnContext">Context object containing information cached for a single turn of conversation with a user.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        protected override async Task OnConversationUpdateActivityAsync(ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            turnContext = turnContext ?? throw new ArgumentNullException(nameof(turnContext));
            this.RecordEvent(nameof(this.OnConversationUpdateActivityAsync), turnContext);

            var activity = turnContext.Activity;
            this.logger.LogInformation($"conversationType: {activity.Conversation.ConversationType}, membersAdded: {activity.MembersAdded?.Count}, membersRemoved: {activity.MembersRemoved?.Count}");

            if (activity.Conversation.ConversationType == Constants.PersonalConversationType)
            {
                if (activity.MembersAdded != null && activity.MembersAdded.Any(member => member.Id != activity.Recipient.Id))
                {
                    await this.HandleMemberAddedInPersonalScopeAsync(turnContext);
                }
            }
            else if (activity.Conversation.ConversationType == Constants.ChannelConversationType)
            {
                if (activity.MembersAdded != null && activity.MembersAdded.Any(member => member.Id == activity.Recipient.Id))
                {
                    await this.HandleMemberAddedInTeamAsync(turnContext);
                }
                else if (activity.MembersRemoved != null && activity.MembersRemoved.Any(member => member.Id == activity.Recipient.Id))
                {
                    await this.HandleMemberRemovedInTeamScopeAsync(turnContext);
                }
            }
        }

        /// <summary>
        /// Invoked when the user opens the Messaging Extension or searching any content in it.
        /// </summary>
        /// <param name="turnContext">Context object containing information cached for a single turn of conversation with a user.</param>
        /// <param name="query">Contains Messaging Extension query keywords.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
        /// <returns>Messaging extension response object to fill compose extension section.</returns>
        /// <remarks>
        /// https://docs.microsoft.com/en-us/dotnet/api/microsoft.bot.builder.teams.teamsactivityhandler.onteamsmessagingextensionqueryasync?view=botbuilder-dotnet-stable.
        /// </remarks>
        protected override async Task<MessagingExtensionResponse> OnTeamsMessagingExtensionQueryAsync(
            ITurnContext<IInvokeActivity> turnContext,
            MessagingExtensionQuery query,
            CancellationToken cancellationToken)
        {
            try
            {
                turnContext = turnContext ?? throw new ArgumentNullException(nameof(turnContext));
                this.RecordEvent(nameof(this.OnTeamsMessagingExtensionQueryAsync), turnContext);

                var activity = turnContext.Activity;

                var messagingExtensionQuery = JsonConvert.DeserializeObject<MessagingExtensionQuery>(activity.Value.ToString());
                var searchQuery = this.messagingExtensionHelper.GetSearchResult(messagingExtensionQuery);

                return new MessagingExtensionResponse
                {
                    ComposeExtension = await this.messagingExtensionHelper.GetTeamPostSearchResultAsync(searchQuery, messagingExtensionQuery.CommandId, activity.From.AadObjectId, messagingExtensionQuery.QueryOptions.Count, messagingExtensionQuery.QueryOptions.Skip),
                };
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"Failed to handle the Messaging Extension command {turnContext.Activity.Name}: {ex.Message}", SeverityLevel.Error);
                throw;
            }
        }

        /// <summary>
        /// Invoked when task module fetch event is received from the bot.
        /// </summary>
        /// <param name="turnContext">Context object containing information cached for a single turn of conversation with a user.</param>
        /// <param name="taskModuleRequest">Task module invoke request value payload.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        protected override async Task<TaskModuleResponse> OnTeamsTaskModuleFetchAsync(ITurnContext<IInvokeActivity> turnContext, TaskModuleRequest taskModuleRequest, CancellationToken cancellationToken)
        {
            try
            {
                turnContext = turnContext ?? throw new ArgumentNullException(nameof(turnContext));
                taskModuleRequest = taskModuleRequest ?? throw new ArgumentNullException(nameof(taskModuleRequest));
                this.RecordEvent(nameof(this.OnTeamsTaskModuleFetchAsync), turnContext);

                var activity = turnContext.Activity;
                if (taskModuleRequest.Data == null)
                {
                    this.telemetryClient.TrackTrace("Request data obtained on task module fetch action is null.");
                    await turnContext.SendActivityAsync(this.localizer.GetString("WelcomeCardContent"));

                    return null;
                }

                var postedValues = JsonConvert.DeserializeObject<BotCommand>(taskModuleRequest.Data.ToString());
                var command = postedValues.Text;

                switch (command.ToUpperInvariant())
                {
                    case Constants.SubmitAnIdeaAction:
                        return CardHelper.GetSubmitIdeaTaskModuleResponse(this.botOptions.Value.AppBaseUri, this.localizer);

                    case Constants.PreferenceSettings:
                        return CardHelper.GetDigestPreferenceTaskModuleResponse(this.botOptions.Value.AppBaseUri, this.localizer);

                    default:
                        this.logger.LogInformation($"Invalid command for task module fetch activity.Command is : {command} ");
                        await turnContext.SendActivityAsync(this.localizer.GetString("UnsupportedBotPersonalCommandText"));

                        return null;
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error while fetching task module received by the bot.");
                throw;
            }
        }

        /// <summary>
        /// Invoked when a message activity is received from the bot.
        /// </summary>
        /// <param name="turnContext">Context object containing information cached for a single turn of conversation with a user.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            try
            {
                turnContext = turnContext ?? throw new ArgumentNullException(nameof(turnContext));
                var message = turnContext.Activity;

                message = message ?? throw new NullReferenceException(nameof(message));

                if (message.Conversation.ConversationType == Constants.ChannelConversationType)
                {
                    var command = message.RemoveRecipientMention().Trim();

                    switch (command.ToUpperInvariant())
                    {
                        case Constants.PreferenceSettings: // Preference command to get the card to setup the category preference of a team.
                            await turnContext.SendActivityAsync(MessageFactory.Attachment(WelcomeCard.GetPreferenceCard(localizer: this.localizer)), cancellationToken);
                            break;

                        default:
                            this.logger.LogInformation($"Received a command {command.ToUpperInvariant()} which is not supported.");
                            await turnContext.SendActivityAsync(MessageFactory.Text(this.localizer.GetString("UnsupportedBotCommandText")));
                            break;
                    }
                }
                else
                {
                    this.logger.LogInformation($"Received a command which is not supported.");
                    await turnContext.SendActivityAsync(MessageFactory.Text(this.localizer.GetString("UnsupportedBotPersonalCommandText")));
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error while message activity is received from the bot.");
                throw;
            }
        }

        /// <summary>
        /// When OnTurn method receives a submit invoke activity on bot turn, it calls this method.
        /// </summary>
        /// <param name="turnContext">Provides context for a turn of a bot.</param>
        /// <param name="taskModuleRequest">Task module invoke request value payload.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents a task module response.</returns>
        protected override async Task<TaskModuleResponse> OnTeamsTaskModuleSubmitAsync(ITurnContext<IInvokeActivity> turnContext, TaskModuleRequest taskModuleRequest, CancellationToken cancellationToken)
        {
            try
            {
                if (turnContext == null || taskModuleRequest == null)
                {
                    return new TaskModuleResponse
                    {
                        Task = new TaskModuleContinueResponse
                        {
                            Type = "continue",
                            Value = new TaskModuleTaskInfo()
                            {
                                Url = $"{this.botOptions.Value.AppBaseUri}/error",
                                Height = ErrorMessageTaskModuleHeight,
                                Width = ErrorMessageTaskModuleWidth,
                                Title = this.localizer.GetString("ApplicationName"),
                            },
                        },
                    };
                }

                var preferenceData = JsonConvert.DeserializeObject<Preference>(taskModuleRequest.Data?.ToString());

                if (preferenceData == null)
                {
                    this.logger.LogInformation($"Request data obtained on task module submit action is null.");
                    await turnContext.SendActivityAsync(this.localizer.GetString("ErrorMessage"));

                    return null;
                }

                if (preferenceData.Command == Constants.PreferenceSubmit)
                {
                    if (preferenceData.ConfigureDetails != null)
                    {
                        var currentTeamPreferenceDetail = this.teamPreferenceStorageHelper.CreateTeamPreferenceModel(preferenceData.ConfigureDetails);
                        TeamPreferenceEntity teamPreferenceDetail;

                        if (currentTeamPreferenceDetail == null)
                        {
                            teamPreferenceDetail = new TeamPreferenceEntity
                            {
                                CreatedDate = DateTime.UtcNow,
                                DigestFrequency = preferenceData.ConfigureDetails.DigestFrequency,
                                Categories = preferenceData.ConfigureDetails.Categories,
                                TeamId = preferenceData.ConfigureDetails.TeamId,
                                UpdatedByName = turnContext.Activity.From.Name,
                                UpdatedByObjectId = turnContext.Activity.From.AadObjectId,
                            };
                        }
                        else
                        {
                            currentTeamPreferenceDetail.DigestFrequency = preferenceData.ConfigureDetails.DigestFrequency;
                            currentTeamPreferenceDetail.Categories = preferenceData.ConfigureDetails.Categories;
                            currentTeamPreferenceDetail.TeamId = preferenceData.ConfigureDetails.TeamId;
                            currentTeamPreferenceDetail.UpdatedByName = turnContext.Activity.From.Name;
                            currentTeamPreferenceDetail.UpdatedByObjectId = turnContext.Activity.From.AadObjectId;
                            teamPreferenceDetail = currentTeamPreferenceDetail;
                        }

                        await this.teamPreferenceStorageProvider.UpsertTeamPreferenceAsync(teamPreferenceDetail);
                    }
                    else
                    {
                        this.logger.LogInformation("Preference details received from task module is null.");
                        return new TaskModuleResponse
                        {
                            Task = new TaskModuleContinueResponse
                            {
                                Type = "continue",
                                Value = new TaskModuleTaskInfo()
                                {
                                    Url = $"{this.botOptions.Value.AppBaseUri}/error",
                                    Height = ErrorMessageTaskModuleHeight,
                                    Width = ErrorMessageTaskModuleWidth,
                                    Title = this.localizer.GetString("ApplicationName"),
                                },
                            },
                        };
                    }
                }

                return null;
            }
#pragma warning disable CA1031 // Catching general exception for any errors occurred during saving data to storage.
            catch (Exception ex)
#pragma warning restore CA1031 // Catching general exception for any errors occurred during saving data to storage.
            {
                this.logger.LogError(ex, "Error in submit action of task module.");
                return new TaskModuleResponse
                {
                    Task = new TaskModuleContinueResponse
                    {
                        Type = "continue",
                        Value = new TaskModuleTaskInfo()
                        {
                            Url = $"{this.botOptions.Value.AppBaseUri}/error",
                            Height = ErrorMessageTaskModuleHeight,
                            Width = ErrorMessageTaskModuleWidth,
                            Title = this.localizer.GetString("ApplicationName"),
                        },
                    },
                };
            }
        }

        /// <summary>
        /// Records event data to Application Insights telemetry client
        /// </summary>
        /// <param name="eventName">Name of the event.</param>
        /// <param name="turnContext">Provides context for a turn in a bot.</param>
        private void RecordEvent(string eventName, ITurnContext turnContext)
        {
            var teamsChannelData = turnContext.Activity.GetChannelData<TeamsChannelData>();

            this.telemetryClient.TrackEvent(eventName, new Dictionary<string, string>
            {
                { "userId", turnContext.Activity.From.AadObjectId },
                { "tenantId", turnContext.Activity.Conversation.TenantId },
                { "teamId", teamsChannelData?.Team?.Id },
                { "channelId", teamsChannelData?.Channel?.Id },
            });
        }

        /// <summary>
        /// Send welcome card to personal chat.
        /// </summary>
        /// <param name="turnContext">Provides context for a turn in a bot.</param>
        /// <returns>A task that represents a response.</returns>
        private async Task HandleMemberAddedInPersonalScopeAsync(ITurnContext<IConversationUpdateActivity> turnContext)
        {
            this.logger.LogInformation($"Bot added in personal {turnContext.Activity.Conversation.Id}");

            var userStateAccessors = this.userState.CreateProperty<UserConversationState>(nameof(UserConversationState));
            var userConversationState = await userStateAccessors.GetAsync(turnContext, () => new UserConversationState());

            if (userConversationState.IsWelcomeCardSent)
            {
                return;
            }

            var userWelcomeCardAttachment = WelcomeCard.GetWelcomeCardAttachmentForPersonal(
                this.botOptions.Value.AppBaseUri,
                localizer: this.localizer);

            await turnContext.SendActivityAsync(MessageFactory.Attachment(userWelcomeCardAttachment));
            userConversationState.IsWelcomeCardSent = true;
            await userStateAccessors.SetAsync(turnContext, userConversationState);
        }

        /// <summary>
        /// Send team welcome card to Team channel.
        /// </summary>
        /// <param name="turnContext">Provides context for a turn in a bot.</param>
        /// <returns>A task that represents a response.</returns>
        private async Task HandleMemberAddedInTeamAsync(ITurnContext<IConversationUpdateActivity> turnContext)
        {
            this.logger.LogInformation($"Bot added in team {turnContext.Activity.Conversation.Id}");
            var channelData = turnContext.Activity.GetChannelData<TeamsChannelData>();
            Attachment welcomeCardAttachment;
            if (channelData.Team.Id == this.curatorTeamId)
            {
                welcomeCardAttachment = WelcomeCard.GetWelcomeCardAttachmentForCuratorTeam(this.botOptions.Value.AppBaseUri, this.localizer);
            }
            else
            {
                welcomeCardAttachment = WelcomeCard.GetWelcomeCardAttachmentForTeam(this.botOptions.Value.AppBaseUri, this.localizer);
            }

            // Storing team information to storage
            var teamsDetails = turnContext.Activity.TeamsGetTeamInfo();
            TeamEntity teamEntity = new TeamEntity
            {
                TeamId = teamsDetails.Id,
                BotInstalledOn = DateTime.UtcNow,
                ServiceUrl = turnContext.Activity.ServiceUrl,
            };

            bool operationStatus = await this.teamStorageProvider.StoreOrUpdateTeamDetailAsync(teamEntity);
            if (!operationStatus)
            {
                this.logger.LogWarning($"Unable to store bot Installation detail in storage.");
            }

            await turnContext.SendActivityAsync(MessageFactory.Attachment(welcomeCardAttachment));
        }

        /// <summary>
        /// Deleting team information from storage when bot is uninstalled from a team.
        /// </summary>
        /// <param name="turnContext">Provides context for a turn in a bot.</param>
        /// <returns>A task that represents a response.</returns>
        private async Task HandleMemberRemovedInTeamScopeAsync(ITurnContext<IConversationUpdateActivity> turnContext)
        {
            this.logger.LogInformation($"Bot removed from team {turnContext.Activity.Conversation.Id}");
            var teamsChannelData = turnContext.Activity.GetChannelData<TeamsChannelData>();
            var teamId = teamsChannelData.Team.Id;

            // Deleting team information from storage when bot is uninstalled from a team.
            this.logger.LogInformation($"Bot removed {turnContext.Activity.Conversation.Id}");
            var teamEntity = await this.teamStorageProvider.GetTeamDetailAsync(teamId);

            if (teamEntity == null)
            {
                this.logger.LogWarning($"No team is found for team id {teamId} to delete team details");
                return;
            }

            bool operationStatus = await this.teamStorageProvider.DeleteTeamDetailAsync(teamEntity);
            if (!operationStatus)
            {
                this.logger.LogWarning("Unable to remove team details from storage.");
            }
        }
    }
}