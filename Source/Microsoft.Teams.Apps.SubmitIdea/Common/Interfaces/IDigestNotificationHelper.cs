// <copyright file="IDigestNotificationHelper.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.SubmitIdea.Common.Interfaces
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Teams.Apps.SubmitIdea.Models;

    /// <summary>
    /// Interface for notification helper, which helps in sending list card notification on Monthly/Weekly basis as per the configured preference in different channels.
    /// </summary>
    public interface IDigestNotificationHelper
    {
        /// <summary>
        /// Send notification in channels on Weekly or Monthly basis as per the configured preference in different channels.
        /// Fetch data based on the date range and send it accordingly.
        /// </summary>
        /// <param name="startDate">Start date from which data should fetch.</param>
        /// <param name="endDate">End date till when data should fetch.</param>
        /// <param name="digestFrequency">Digest frequency enum for notification like Monthly/Weekly.</param>
        /// <returns>A task that sends notification in channel.</returns>
        Task SendNotificationInChannelAsync(DateTime startDate, DateTime endDate, DigestFrequency digestFrequency);
    }
}
