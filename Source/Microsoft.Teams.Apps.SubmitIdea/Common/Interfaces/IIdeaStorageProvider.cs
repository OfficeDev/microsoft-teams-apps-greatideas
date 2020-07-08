// <copyright file="IIdeaStorageProvider.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.SubmitIdea.Common.Interfaces
{
    using System.Threading.Tasks;
    using Microsoft.Teams.Apps.SubmitIdea.Models;

    /// <summary>
    /// Interface for provider which helps in retrieving, storing, updating and deleting idea details in storage.
    /// </summary>
    public interface IIdeaStorageProvider
    {
        /// <summary>
        /// Stores or update idea details data in storage.
        /// </summary>
        /// <param name="ideaEntity">Holds idea detail entity data.</param>
        /// <returns>A boolean that represents idea entity data is successfully saved/updated or not.</returns>
        Task<bool> UpsertIdeaAsync(IdeaEntity ideaEntity);

        /// <summary>
        /// Get idea data from storage.
        /// </summary>
        /// <param name="createdByUserId">Azure Active Directory id of author who created the idea.</param>
        /// <param name="ideaId">Idea id to fetch the idea details.</param>
        /// <returns>A task that represent a object to hold idea details.</returns>
        Task<IdeaEntity> GetIdeaEntityAsync(string createdByUserId, string ideaId);

        /// <summary>
        /// Get team idea data from storage.
        /// </summary>
        /// <param name="ideaId">Idea id to fetch the idea details.</param>
        /// <returns>A task that represent a object to hold idea details.</returns>
        Task<IdeaEntity> GetIdeaEntityAsync(string ideaId);

        /// <summary>
        /// Get team idea data from storage.
        /// </summary>
        /// <param name="createdByUserId">Azure Active Directory id of author who created the idea.</param>
        /// <param name="ideaId">Idea id to fetch the idea details.</param>
        /// <returns>A task that represent a object to hold idea details.</returns>
        Task<IdeaEntity> GetPostAsync(string createdByUserId, string ideaId);
    }
}
