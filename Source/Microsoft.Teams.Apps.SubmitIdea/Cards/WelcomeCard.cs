// <copyright file="WelcomeCard.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.SubmitIdea.Cards
{
    using System;
    using System.Collections.Generic;
    using AdaptiveCards;
    using Microsoft.Bot.Schema;
    using Microsoft.Bot.Schema.Teams;
    using Microsoft.Extensions.Localization;
    using Microsoft.Teams.Apps.SubmitIdea.Common;
    using Microsoft.Teams.Apps.SubmitIdea.Models;
    using Microsoft.Teams.Apps.SubmitIdea.Models.Card;
    using Newtonsoft.Json;

    /// <summary>
    /// Class that helps to return welcome card as attachment.
    /// </summary>
    public static class WelcomeCard
    {
        /// <summary>
        /// Represent welcome card icon width.
        /// </summary>
        private const uint WelcomeCardIconWidth = 56;

        /// <summary>
        /// Represent welcome card icon height.
        /// </summary>
        private const uint WelcomeCardIconHeight = 56;

        /// <summary>
        /// Get welcome card attachment to show on Microsoft Teams personal scope.
        /// </summary>
        /// <param name="applicationBasePath">Application base path to get the logo of the application.</param>
        /// <param name="localizer">The current cultures' string localizer.</param>
        /// <returns>User welcome card attachment.</returns>
        public static Attachment GetWelcomeCardAttachmentForPersonal(
            string applicationBasePath,
            IStringLocalizer<Strings> localizer)
        {
            AdaptiveCard card = new AdaptiveCard(new AdaptiveSchemaVersion(Constants.AdaptiveCardVersion))
            {
                Body = new List<AdaptiveElement>
                {
                    new AdaptiveColumnSet
                    {
                        Columns = new List<AdaptiveColumn>
                        {
                            new AdaptiveColumn
                            {
                                Width = AdaptiveColumnWidth.Auto,
                                Items = new List<AdaptiveElement>
                                {
                                    new AdaptiveImage
                                    {
                                        Url = new Uri($"{applicationBasePath}/Artifacts/applicationLogo.png"),
                                        AltText = localizer.GetString("AltTextForWelcomeCardImage"),
                                        HorizontalAlignment = AdaptiveHorizontalAlignment.Center,
                                        Size = AdaptiveImageSize.Large,
                                        PixelHeight = WelcomeCardIconHeight,
                                        PixelWidth = WelcomeCardIconWidth,
                                    },
                                },
                            },
                            new AdaptiveColumn
                            {
                                Width = AdaptiveColumnWidth.Stretch,
                                Items = new List<AdaptiveElement>
                                {
                                    new AdaptiveTextBlock
                                    {
                                        Text = localizer.GetString("WelcomeCardTitle"),
                                        Weight = AdaptiveTextWeight.Bolder,
                                        Size = AdaptiveTextSize.Large,
                                    },
                                    new AdaptiveTextBlock
                                    {
                                        Text = localizer.GetString("WelcomeCardContent"),
                                        Wrap = true,
                                        Spacing = AdaptiveSpacing.None,
                                    },
                                },
                            },
                        },
                    },
                    new AdaptiveTextBlock
                    {
                        HorizontalAlignment = AdaptiveHorizontalAlignment.Left,
                        Text = localizer.GetString("WelcomeSubHeaderText"),
                        Spacing = AdaptiveSpacing.Small,
                    },
                    new AdaptiveTextBlock
                    {
                        Text = localizer.GetString("SubmitAnIdeaBulletPoint"),
                        Wrap = true,
                        Spacing = AdaptiveSpacing.None,
                    },
                    new AdaptiveTextBlock
                    {
                        Text = localizer.GetString("ContentText"),
                        Spacing = AdaptiveSpacing.Small,
                    },
                },

                Actions = new List<AdaptiveAction>
                {
                    new AdaptiveSubmitAction
                    {
                        Title = localizer.GetString("SubmitAnIdeaButtonText"),
                        Data = new AdaptiveSubmitActionData
                        {
                            Msteams = new TaskModuleAction(Constants.SubmitAnIdeaAction, JsonConvert.SerializeObject(new BotCommand { Text = Constants.SubmitAnIdeaAction })),
                        },
                    },
                },
            };

            return new Attachment
            {
                ContentType = AdaptiveCard.ContentType,
                Content = card,
            };
        }

        /// <summary>
        /// This method will construct the user welcome card when bot is added in team scope.
        /// </summary>
        /// <param name="applicationBasePath">Application base URL.</param>
        /// <param name="localizer">The current cultures' string localizer.</param>
        /// <returns>Welcome card.</returns>
        public static Attachment GetWelcomeCardAttachmentForTeam(string applicationBasePath, IStringLocalizer<Strings> localizer)
        {
            AdaptiveCard card = new AdaptiveCard(new AdaptiveSchemaVersion(Constants.AdaptiveCardVersion))
            {
                Body = new List<AdaptiveElement>
                {
                    new AdaptiveColumnSet
                    {
                        Columns = new List<AdaptiveColumn>
                        {
                            new AdaptiveColumn
                            {
                                Width = AdaptiveColumnWidth.Auto,
                                Items = new List<AdaptiveElement>
                                {
                                    new AdaptiveImage
                                    {
                                        Url = new Uri($"{applicationBasePath}/Artifacts/applicationLogo.png"),
                                        AltText = localizer.GetString("AltTextForWelcomeCardImage"),
                                        HorizontalAlignment = AdaptiveHorizontalAlignment.Center,
                                        Size = AdaptiveImageSize.Large,
                                        PixelHeight = WelcomeCardIconHeight,
                                        PixelWidth = WelcomeCardIconWidth,
                                    },
                                },
                            },
                            new AdaptiveColumn
                            {
                                Width = AdaptiveColumnWidth.Stretch,
                                Items = new List<AdaptiveElement>
                                {
                                    new AdaptiveTextBlock
                                    {
                                        Text = localizer.GetString("WelcomeCardTitle"),
                                        Weight = AdaptiveTextWeight.Bolder,
                                        Size = AdaptiveTextSize.Large,
                                    },
                                    new AdaptiveTextBlock
                                    {
                                        Text = localizer.GetString("WelcomeTeamCardContent"),
                                        Wrap = true,
                                        Spacing = AdaptiveSpacing.None,
                                    },
                                },
                            },
                        },
                    },
                    new AdaptiveTextBlock
                    {
                        HorizontalAlignment = AdaptiveHorizontalAlignment.Left,
                        Text = localizer.GetString("WelcomeSubHeaderText"),
                        Spacing = AdaptiveSpacing.Small,
                    },
                    new AdaptiveTextBlock
                    {
                        Text = localizer.GetString("ConfigureDigestBulletText"),
                        Wrap = true,
                        Spacing = AdaptiveSpacing.None,
                    },
                    new AdaptiveTextBlock
                    {
                        Text = localizer.GetString("ContentText"),
                        Spacing = AdaptiveSpacing.Small,
                    },
                },

                Actions = new List<AdaptiveAction>
                {
                    new AdaptiveSubmitAction
                    {
                        Title = localizer.GetString("ConfigureDigestButtonText"),
                        Data = new AdaptiveSubmitActionData
                        {
                            Msteams = new TaskModuleAction(Constants.PreferenceSettings, JsonConvert.SerializeObject(new BotCommand { Text = Constants.PreferenceSettings })),
                        },
                    },
                },
            };

            return new Attachment
            {
                ContentType = AdaptiveCard.ContentType,
                Content = card,
            };
        }

        /// <summary>
        /// This method will construct the user welcome card when bot is added in curator team.
        /// </summary>
        /// <param name="applicationBasePath">Application base URL.</param>
        /// <param name="localizer">The current cultures' string localizer.</param>
        /// <returns>Welcome card.</returns>
        public static Attachment GetWelcomeCardAttachmentForCuratorTeam(string applicationBasePath, IStringLocalizer<Strings> localizer)
        {
            AdaptiveCard card = new AdaptiveCard(new AdaptiveSchemaVersion(Constants.AdaptiveCardVersion))
            {
                Body = new List<AdaptiveElement>
                {
                    new AdaptiveColumnSet
                    {
                        Columns = new List<AdaptiveColumn>
                        {
                            new AdaptiveColumn
                            {
                                Width = AdaptiveColumnWidth.Auto,
                                Items = new List<AdaptiveElement>
                                {
                                    new AdaptiveImage
                                    {
                                        Url = new Uri($"{applicationBasePath}/Artifacts/applicationLogo.png"),
                                        AltText = localizer.GetString("AltTextForWelcomeCardImage"),
                                        HorizontalAlignment = AdaptiveHorizontalAlignment.Center,
                                        Size = AdaptiveImageSize.Large,
                                        PixelHeight = WelcomeCardIconHeight,
                                        PixelWidth = WelcomeCardIconWidth,
                                    },
                                },
                            },
                            new AdaptiveColumn
                            {
                                Width = AdaptiveColumnWidth.Stretch,
                                Items = new List<AdaptiveElement>
                                {
                                    new AdaptiveTextBlock
                                    {
                                        Text = localizer.GetString("WelcomeCardTitle"),
                                        Weight = AdaptiveTextWeight.Bolder,
                                        Size = AdaptiveTextSize.Large,
                                    },
                                    new AdaptiveTextBlock
                                    {
                                        Text = localizer.GetString("CuratorWelcomeCardContent"),
                                        Wrap = true,
                                        Spacing = AdaptiveSpacing.None,
                                    },
                                },
                            },
                        },
                    },
                    new AdaptiveTextBlock
                    {
                        HorizontalAlignment = AdaptiveHorizontalAlignment.Left,
                        Text = localizer.GetString("WelcomeSubHeaderText"),
                        Spacing = AdaptiveSpacing.Small,
                    },
                    new AdaptiveTextBlock
                    {
                        Text = localizer.GetString("ConfigureDigestBulletText"),
                        Wrap = true,
                        Spacing = AdaptiveSpacing.None,
                    },
                    new AdaptiveTextBlock
                    {
                        Text = localizer.GetString("ContentText"),
                        Spacing = AdaptiveSpacing.Small,
                    },
                },

                Actions = new List<AdaptiveAction>
                {
                    new AdaptiveSubmitAction
                    {
                        Title = localizer.GetString("ConfigureDigestButtonText"),
                        Data = new AdaptiveSubmitActionData
                        {
                            Msteams = new TaskModuleAction(Constants.PreferenceSettings, JsonConvert.SerializeObject(new BotCommand { Text = Constants.PreferenceSettings })),
                        },
                    },
                },
            };

            return new Attachment
            {
                ContentType = AdaptiveCard.ContentType,
                Content = card,
            };
        }

        /// <summary>
        /// Get preference card as attachment.
        /// </summary>
        /// <param name="localizer">The current cultures' string localizer.</param>
        /// <returns>Set preference card attachment.</returns>
        public static Attachment GetPreferenceCard(IStringLocalizer<Strings> localizer)
        {
            AdaptiveCard card = new AdaptiveCard(new AdaptiveSchemaVersion(Constants.AdaptiveCardVersion))
            {
                Body = new List<AdaptiveElement>
                {
                    new AdaptiveColumnSet
                    {
                        Columns = new List<AdaptiveColumn>
                        {
                            new AdaptiveColumn
                            {
                                Items = new List<AdaptiveElement>
                                {
                                    new AdaptiveTextBlock
                                    {
                                        Spacing = AdaptiveSpacing.None,
                                        Text = localizer.GetString("DigestPreferenceCardHeaderText"),
                                        Wrap = true,
                                        IsSubtle = true,
                                    },
                                    new AdaptiveTextBlock
                                    {
                                        Spacing = AdaptiveSpacing.None,
                                        Text = localizer.GetString("DigestPreferenceCardContent"),
                                        Wrap = true,
                                        IsSubtle = true,
                                    },
                                },
                                Width = AdaptiveColumnWidth.Stretch,
                            },
                        },
                    },
                },
                Actions = new List<AdaptiveAction>
                {
                    new AdaptiveSubmitAction
                    {
                        Title = localizer.GetString("ConfigureDigestButtonText"),
                        Data = new AdaptiveSubmitActionData
                        {
                            Msteams = new TaskModuleAction(Constants.PreferenceSettings, JsonConvert.SerializeObject(new BotCommand { Text = Constants.PreferenceSettings })),
                        },
                    },
                },
            };
            Attachment adaptiveCardAttachment = new Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = card,
            };
            return adaptiveCardAttachment;
        }
    }
}