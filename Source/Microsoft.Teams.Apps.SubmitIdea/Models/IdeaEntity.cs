// <copyright file="IdeaEntity.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.SubmitIdea.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using Microsoft.Azure.Search;
    using Microsoft.WindowsAzure.Storage.Table;
    using Newtonsoft.Json;

    /// <summary>
    /// A class that represents team idea entity model which helps to create, insert, update and delete the idea.
    /// </summary>
    public class IdeaEntity : TableEntity
    {
        /// <summary>
        /// Gets or sets Azure Active Directory id of author who created the idea.
        /// </summary>
        [IsFilterable]
        public string CreatedByObjectId
        {
            get { return this.RowKey; }
            set { this.RowKey = value; }
        }

        /// <summary>
        /// Gets or sets unique identifier for each created idea.
        /// </summary>
        [Key]
        public string IdeaId
        {
            get { return this.PartitionKey; }
            set { this.PartitionKey = value; }
        }

        /// <summary>
        /// Gets or sets title of idea.
        /// </summary>
        [IsSearchable]
        [Required]
        [MaxLength(200)]
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets user entered post description value.
        /// </summary>
        [Required]
        [MaxLength(500)]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets user selected idea category value.
        /// </summary>
        [IsFilterable]
        [IsSearchable]
        [Required]
        public string Category { get; set; }

        /// <summary>
        /// Gets or sets user selected idea category id.
        /// </summary>
        [IsFilterable]
        [IsSearchable]
        public string CategoryId { get; set; }

        /// <summary>
        /// Gets or sets semicolon separated tags entered by user.
        /// </summary>
        [IsSearchable]
        [IsFilterable]
        public string Tags { get; set; }

        /// <summary>
        /// Gets or sets date time when entry is created.
        /// </summary>
        [IsSortable]
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// Gets or sets author name who created idea.
        /// </summary>
        [IsFilterable]
        public string CreatedByName { get; set; }

        /// <summary>
        /// Gets or sets date time when entry is updated.
        /// </summary>
        [IsSortable]
        public DateTime UpdatedDate { get; set; }

        /// <summary>
        /// Gets or sets user principle name of author who created the idea.
        /// </summary>
        [IsFilterable]
        [IsSearchable]
        public string CreatedByUserPrincipalName { get; set; }

        /// <summary>
        /// Gets or sets total number of likes received for a idea from users.
        /// </summary>
        [IsSortable]
        public int TotalVotes { get; set; }

        /// <summary>
        /// Gets or sets supporting document links for idea in json format.
        /// </summary>
        public string DocumentLinks { get; set; }

        /// <summary>
        /// Gets or sets name of user who has approved or rejected the idea.
        /// </summary>
        public string ApprovedOrRejectedByName { get; set; }

        /// <summary>
        /// Gets or sets Object identifier of user who has approved or rejected the idea.
        /// </summary>
        public string ApproverOrRejecterUserId { get; set; }

        /// <summary>
        /// Gets or sets status of idea i.e. Pending, Approved or Rejected.
        /// </summary>
        [IsFilterable]
        public int Status { get; set; }

        /// <summary>
        /// Gets or sets feedback comment if admin has rejected idea request.
        /// </summary>
        public string Feedback { get; set; }
    }
}
