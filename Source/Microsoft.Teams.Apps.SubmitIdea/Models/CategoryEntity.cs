// <copyright file="CategoryEntity.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.SubmitIdea.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using Microsoft.WindowsAzure.Storage.Table;

    /// <summary>
    /// Category Entity.
    /// </summary>
    public class CategoryEntity : TableEntity
    {
        /// <summary>
        /// Constant partition key value.
        /// </summary>
        public const string CategoryPartitionKey = "Category";

        /// <summary>
        /// Initializes a new instance of the <see cref="CategoryEntity"/> class.
        /// </summary>
        public CategoryEntity()
        {
            this.PartitionKey = CategoryPartitionKey;
        }

        /// <summary>
        /// Gets or sets category id.
        /// </summary>
        public string CategoryId
        {
            get { return this.RowKey; }
            set { this.RowKey = value; }
        }

        /// <summary>
        /// Gets or sets category name.
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string CategoryName { get; set; }

        /// <summary>
        /// Gets or sets category description.
        /// </summary>
        [Required]
        [MaxLength(300)]
        public string CategoryDescription { get; set; }

        /// <summary>
        /// Gets or sets created by user Id.
        /// </summary>
        public string CreatedByUserId { get; set; }

        /// <summary>
        /// Gets or sets modified by user Id.
        /// </summary>
        public string ModifiedByUserId { get; set; }

        /// <summary>
        /// Gets or sets created on date time.
        /// </summary>
        public DateTime CreatedOn { get; set; }
    }
}
