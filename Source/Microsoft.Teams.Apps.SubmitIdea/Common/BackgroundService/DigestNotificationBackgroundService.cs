// <copyright file="DigestNotificationBackgroundService.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.SubmitIdea.Common.BackgroundService
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.ApplicationInsights.DataContracts;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Microsoft.Teams.Apps.SubmitIdea.Common.Interfaces;
    using Microsoft.Teams.Apps.SubmitIdea.Models;

    /// <summary>
    /// This class inherits IHostedService and implements the methods related to background tasks for sending Weekly/Monthly notifications.
    /// </summary>
    public class DigestNotificationBackgroundService : BackgroundService
    {
        /// <summary>
        /// Logger implementation to send logs to the logger service..
        /// </summary>
        private readonly ILogger<DigestNotificationBackgroundService> logger;

        /// <summary>
        /// Instance of notification helper which helps in sending notifications.
        /// </summary>
        private readonly IDigestNotificationHelper digestNotificationHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="DigestNotificationBackgroundService"/> class.
        /// BackgroundService class that inherits IHostedService and implements the methods related to sending notification tasks.
        /// </summary>
        /// <param name="logger">Logger implementation to send logs to the logger service.</param>
        /// <param name="notificationHelper">Helper to send notification in channels.</param>
        public DigestNotificationBackgroundService(
            ILogger<DigestNotificationBackgroundService> logger,
            IDigestNotificationHelper notificationHelper)
        {
            this.logger = logger;
            this.digestNotificationHelper = notificationHelper;
        }

        /// <summary>
        ///  This method is called when the Microsoft.Extensions.Hosting.IHostedService starts.
        ///  The implementation should return a task that represents the lifetime of the long
        ///  running operation(s) being performed.
        ///  Digest preference notification will be sent to team based on digest frequency (weekly/monthly) for selected idea categories.
        /// </summary>
        /// <param name="stoppingToken">Triggered when Microsoft.Extensions.Hosting.IHostedService.StopAsync(System.Threading.CancellationToken) is called.</param>
        /// <returns>A System.Threading.Tasks.Task that represents the long running operations.</returns>
        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var currentDateTime = DateTimeOffset.UtcNow;
                    this.logger.LogInformation($"Notification Hosted Service is running at: {currentDateTime}.");

                    if (currentDateTime.DayOfWeek == DayOfWeek.Monday)
                    {
                        this.logger.LogInformation($"Monday of the month: {currentDateTime} and sending the notification.");
                        await this.SendNotificationWeeklyAsync(currentDateTime);
                    }

                    // Send digest notification if it's the 1st day of the Month.
                    if (currentDateTime.Day == 1)
                    {
                        this.logger.LogInformation($"First day of the month: {currentDateTime} and sending the notification.");
                        await this.SendNotificationMonthlyAsync(currentDateTime);
                    }
                }
#pragma warning disable CA1031 // Catching general exception for any errors occurred during background service execution.
                catch (Exception ex)
#pragma warning restore CA1031 // Catching general exception for any errors occurred during background service execution.
                {
                    this.logger.LogError(ex, $"Error while running the background service to send digest notification): {ex.Message} at: {DateTimeOffset.UtcNow}", SeverityLevel.Error);
                }

                await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
            }
        }

        /// <summary>
        /// Method invokes send notification task which gets posts data as per configured preference and send the notification.
        /// </summary>
        /// <returns>A task that sends notification in different channels.</returns>
        private async Task SendNotificationWeeklyAsync(DateTimeOffset dateTimeOffset)
        {
            try
            {
                DateTime fromDate = dateTimeOffset.AddDays(-7).Date;
                DateTime toDate = dateTimeOffset.Date;

                this.logger.LogInformation("Notification task queued for sending weekly notification.");
                await this.digestNotificationHelper.SendNotificationInChannelAsync(fromDate, toDate, DigestFrequency.Weekly);
            }
#pragma warning disable CA1031 // Catching general exception for any errors occurred during background service execution.
            catch (Exception ex)
#pragma warning restore CA1031 // Catching general exception for any errors occurred during background service execution.
            {
                this.logger.LogError(ex, $"Error while sending the Weekly notification at {dateTimeOffset}.");
            }
        }

        /// <summary>
        /// Method invokes send notification task which gets posts data as per configured preference and send the notification.
        /// </summary>
        /// <returns>A task that sends notification in different channels.</returns>
        private async Task SendNotificationMonthlyAsync(DateTimeOffset dateTimeOffset)
        {
            try
            {
                DateTime fromDate = dateTimeOffset.AddMonths(-1).Date;
                DateTime toDate = dateTimeOffset.Date;

                this.logger.LogInformation("Notification task queued for sending monthly notification.");
                await this.digestNotificationHelper.SendNotificationInChannelAsync(fromDate, toDate, DigestFrequency.Monthly);
            }
#pragma warning disable CA1031 // Catching general exception for any errors occurred during background service execution.
            catch (Exception ex)
#pragma warning restore CA1031 // Catching general exception for any errors occurred during background service execution.
            {
                this.logger.LogError(ex, $"Error while sending the Monthly notification at {dateTimeOffset}.");
            }
        }
    }
}
