// <copyright file="UserVoteEntity.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.SubmitIdea.Models
{
    using System.ComponentModel.DataAnnotations;
    using Microsoft.WindowsAzure.Storage.Table;

    /// <summary>
    /// A class that represents user like/vote model.
    /// </summary>
    public class UserVoteEntity : TableEntity
    {
        /// <summary>
        /// Gets or sets unique Azure Active Directory id of user.
        /// </summary>
        public string UserId
        {
            get { return this.PartitionKey; }
            set { this.PartitionKey = value; }
        }

        /// <summary>
        /// Gets or sets unique identifier for each created idea.
        /// </summary>
        [Key]
        public string IdeaId
        {
            get { return this.RowKey; }
            set { this.RowKey = value; }
        }
    }
}
