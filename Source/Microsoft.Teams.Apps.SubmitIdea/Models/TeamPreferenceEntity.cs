// <copyright file="TeamPreferenceEntity.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.SubmitIdea.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using Microsoft.WindowsAzure.Storage.Table;

    /// <summary>
    /// A class that represents team preference entity model.
    /// </summary>
    public class TeamPreferenceEntity : TableEntity
    {
        /// <summary>
        /// Gets or sets unique value for each Team where preference has configured.
        /// </summary>
        public string TeamId
        {
            get
            {
                return this.PartitionKey;
            }

            set
            {
                this.PartitionKey = value;
                this.RowKey = value;
            }
        }

        /// <summary>
        /// Gets or sets user selected value for digest frequency like Monthly/Weekly.
        /// </summary>
        [Required]
        public string DigestFrequency { get; set; }

        /// <summary>
        /// Gets or sets semicolon separated tags selected by user.
        /// </summary>
        public string Tags { get; set; }

        /// <summary>
        /// Gets or sets semicolon separated tags selected by user.
        /// </summary>
        [Required]
        public string Categories { get; set; }

        /// <summary>
        /// Gets or sets date time when entry is created by user in UTC format.
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// Gets or sets date time when entry is updated by user in UTC format.
        /// </summary>
        public DateTime UpdatedDate { get; set; }

        /// <summary>
        /// Gets or sets user name of last user who updated the configured preference.
        /// </summary>
        public string UpdatedByName { get; set; }

        /// <summary>
        /// Gets or sets Azure Active Directory id of last user updated the configured preference.
        /// </summary>
        public string UpdatedByObjectId { get; set; }
    }
}
