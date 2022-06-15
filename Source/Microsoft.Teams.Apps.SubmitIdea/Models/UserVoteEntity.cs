// <copyright file="UserVoteEntity.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.SubmitIdea.Models
{
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// A class that represents user like/vote model.
    /// </summary>
    public class UserVoteEntity : ATableEntity
    {
        /// <summary>
        /// Gets or sets unique Azure Active Directory id of user.
        /// </summary>
        public string UserId
        {
            get => this.PartitionKey;
            set => this.PartitionKey = value;
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
