// <copyright file="IdeaSearchService.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.SubmitIdea.Common.SearchServices
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using Microsoft.ApplicationInsights.DataContracts;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Azure.Search;
    using Microsoft.Azure.Search.Models;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Microsoft.Rest.Azure;
    using Microsoft.Teams.Apps.SubmitIdea.Common.Interfaces;
    using Microsoft.Teams.Apps.SubmitIdea.Helpers.Extensions;
    using Microsoft.Teams.Apps.SubmitIdea.Models;
    using Microsoft.Teams.Apps.SubmitIdea.Models.Configuration;
    using Polly;
    using Polly.Contrib.WaitAndRetry;
    using Polly.Retry;

    /// <summary>
    /// Team idea Search service which helps in creating index, indexer and data source if it doesn't exist
    /// for indexing source which will be used for search by Messaging Extension.
    /// </summary>
    public class IdeaSearchService : IIdeaSearchService, IDisposable
    {
        /// <summary>
        /// Azure Search service indexer name for ideas.
        /// </summary>
        private const string IdeaIndexerName = "team-idea-indexer";

        /// <summary>
        /// Azure Search service data source name for ideas.
        /// </summary>
        private const string IdeaDataSourceName = "team-idea-storage";

        /// <summary>
        /// Represents the sorting type as popularity means to sort the data based on number of votes.
        /// </summary>
        private const string SortByPopular = "Popularity";

        /// <summary>
        /// Azure Search service maximum search result count for idea entity.
        /// </summary>
        private const int ApiSearchResultCount = 1500;

        /// <summary>
        /// Used to initialize task.
        /// </summary>
        private readonly Lazy<Task> initializeTask;

        /// <summary>
        /// Instance of Azure Search service client.
        /// </summary>
        private readonly SearchServiceClient searchServiceClient;

        /// <summary>
        /// Instance of Azure Search index client.
        /// </summary>
        private readonly SearchIndexClient searchIndexClient;

        /// <summary>
        /// Instance of team post storage helper to update idea and get information of ideas.
        /// </summary>
        private readonly IIdeaStorageProvider teamIdeaStorageProvider;

        /// <summary>
        /// Logger implementation to send logs to the logger service..
        /// </summary>
        private readonly ILogger<IdeaSearchService> logger;

        /// <summary>
        /// Represents a set of key/value application configuration properties.
        /// </summary>
        private readonly SearchServiceSettings options;

        /// <summary>
        /// Retry policy with jitter.
        /// </summary>
        /// <remarks>
        /// Reference: https://github.com/Polly-Contrib/Polly.Contrib.WaitAndRetry#new-jitter-recommendation.
        /// </remarks>
        private readonly AsyncRetryPolicy retryPolicy;

        /// <summary>
        /// Flag: Has Dispose already been called?
        /// </summary>
        private bool disposed = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="IdeaSearchService"/> class.
        /// </summary>
        /// <param name="optionsAccessor">A set of key/value application configuration properties.</param>
        /// <param name="teamIdeaStorageProvider">Team idea storage provider dependency injection.</param>
        /// <param name="logger">Logger implementation to send logs to the logger service.</param>
        /// <param name="searchServiceClient">Search service client dependency injection.</param>
        /// <param name="searchIndexClient">Search index client dependency injection.</param>
        public IdeaSearchService(
            IOptions<SearchServiceSettings> optionsAccessor,
            IIdeaStorageProvider teamIdeaStorageProvider,
            ILogger<IdeaSearchService> logger,
            SearchServiceClient searchServiceClient,
            SearchIndexClient searchIndexClient)
        {
            optionsAccessor = optionsAccessor ?? throw new ArgumentNullException(nameof(optionsAccessor));
            this.options = optionsAccessor.Value;
            this.initializeTask = new Lazy<Task>(() => this.InitializeAsync());
            this.teamIdeaStorageProvider = teamIdeaStorageProvider;
            this.logger = logger;
            this.searchServiceClient = searchServiceClient;
            this.searchIndexClient = searchIndexClient;
            this.retryPolicy = Policy.Handle<CloudException>(
                ex => (int)ex.Response.StatusCode == StatusCodes.Status409Conflict ||
                (int)ex.Response.StatusCode == StatusCodes.Status429TooManyRequests)
                .WaitAndRetryAsync(Backoff.LinearBackoff(TimeSpan.FromMilliseconds(2000), 2));
        }

        /// <summary>
        /// Provides idea search results based on query details provided by the user.
        /// </summary>
        /// <param name="searchScope">Scope of the search.</param>
        /// <param name="searchQuery">Query which the user had typed in Messaging Extension search field.</param>
        /// <param name="userObjectId">Azure Active Directory object id of the user.</param>
        /// <param name="count">Number of search results to return.</param>
        /// <param name="skip">Number of search results to skip.</param>
        /// <param name="sortBy">Represents sorting type like: Popularity or Newest.</param>
        /// <param name="filterQuery">Filter bar based query.</param>
        /// <returns>List of search results.</returns>
        public async Task<IEnumerable<IdeaEntity>> GetTeamIdeasAsync(
            IdeaSearchScope searchScope,
            string searchQuery,
            string userObjectId,
            int? count = null,
            int? skip = null,
            string sortBy = null,
            string filterQuery = null)
        {
            await this.EnsureInitializedAsync();
            var searchParameters = this.InitializeSearchParameters(searchScope, userObjectId, count, skip, sortBy, filterQuery);

            SearchContinuationToken continuationToken = null;
            var ideas = new List<IdeaEntity>();

            if (searchScope == IdeaSearchScope.SearchTeamPostsForTitleText && !string.IsNullOrWhiteSpace(searchQuery))
            {
                searchQuery = searchQuery.EscapeCharactersInQuery();
            }

            var ideaSearchResult = await this.searchIndexClient.Documents.SearchAsync<IdeaEntity>(searchQuery, searchParameters);

            if (ideaSearchResult?.Results != null)
            {
                ideas.AddRange(ideaSearchResult.Results.Select(p => p.Document));
                continuationToken = ideaSearchResult.ContinuationToken;
            }

            if (continuationToken == null)
            {
                return ideas;
            }

            do
            {
                var searchResult = await this.searchIndexClient.Documents.ContinueSearchAsync<IdeaEntity>(continuationToken);

                if (searchResult?.Results != null)
                {
                    ideas.AddRange(searchResult.Results.Select(p => p.Document));
                    continuationToken = searchResult.ContinuationToken;
                }
            }
            while (continuationToken != null);

            return ideas;
        }

        /// <summary>
        /// Creates Index, Data Source and Indexer for search service.
        /// </summary>
        /// <returns>A task that represents the work queued to execute.</returns>
        public async Task CreateSearchServiceIndexAsync()
        {
            await this.CreateSearchIndexAsync();
            await this.CreateDataSourceAsync();
            await this.CreateIndexerAsync();
        }

        /// <summary>
        /// Run the indexer on demand.
        /// </summary>
        /// <returns>A task that represents the work queued to execute</returns>
        public async Task RunIndexerOnDemandAsync()
        {
            // Retry once after 1 second if conflict occurs during indexer run.
            // If conflict occurs again means another index run is in progress and it will index data for which first failure occurred.
            // Hence ignore second conflict and continue.
            var requestId = Guid.NewGuid().ToString();

            try
            {
                await this.retryPolicy.ExecuteAsync(async () =>
                {
                    try
                    {
                        this.logger.LogInformation($"On-demand indexer run request #{requestId} - start");
                        await this.searchServiceClient.Indexers.RunAsync(IdeaIndexerName);
                        this.logger.LogInformation($"On-demand indexer run request #{requestId} - complete");
                    }
                    catch (CloudException ex)
                    {
                        this.logger.LogError(ex, $"Failed to run on-demand indexer run for request #{requestId}: {ex.Message}");
                        throw;
                    }
                });
            }
            catch (CloudException ex)
            {
                this.logger.LogError(ex, $"Failed to run on-demand indexer for retry. Request #{requestId}: {ex.Message}");
            }
        }

        /// <summary>
        /// Dispose search service instance.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Protected implementation of Dispose pattern.
        /// </summary>
        /// <param name="disposing">True if already disposed else false.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (this.disposed)
            {
                return;
            }

            if (disposing)
            {
                this.searchServiceClient.Dispose();
                this.searchIndexClient.Dispose();
            }

            this.disposed = true;
        }

        /// <summary>
        /// Create index, indexer and data source if doesn't exist.
        /// </summary>
        /// <returns>A task that represents the work queued to execute.</returns>
        private async Task InitializeAsync()
        {
            try
            {
                // When there are no team post created by user and messaging extension is accessed,
                // storage data source initialization is required here before creating search index or data source or indexer.
                await this.teamIdeaStorageProvider.GetIdeaEntityAsync(string.Empty);
                await this.CreateSearchServiceIndexAsync();
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"Failed to initialize Azure Search Service: {ex.Message}", SeverityLevel.Error);
                throw;
            }
        }

        /// <summary>
        /// Create index in Azure Search service if it doesn't exist.
        /// </summary>
        /// <returns><see cref="Task"/> That represents index is created if it is not created.</returns>
        private async Task CreateSearchIndexAsync()
        {
            // Recreate only if there is a change in the storage schema.
            // Manually need to drop and create index whenever there is storage schema design change.
            if (await this.searchServiceClient.Indexes.ExistsAsync(Constants.TeamIdeaIndexName))
            {
                return;
            }

            var index = new Index()
            {
                Name = Constants.TeamIdeaIndexName,
                Fields = FieldBuilder.BuildForType<IdeaEntity>(),
            };
            await this.searchServiceClient.Indexes.CreateAsync(index);
        }

        /// <summary>
        /// Create data source if it doesn't exist in Azure Search service.
        /// </summary>
        /// <returns><see cref="Task"/> That represents data source is added to Azure Search service.</returns>
        private async Task CreateDataSourceAsync()
        {
            // Recreate only if there is a change in the storage schema.
            // Manually need to drop and create DataSources whenever there is storage schema design change.
            if (await this.searchServiceClient.DataSources.ExistsAsync(IdeaDataSourceName))
            {
                return;
            }

            var dataSource = DataSource.AzureTableStorage(
                IdeaDataSourceName,
                this.options.ConnectionString,
                Constants.IdeaEntityTableName,
                query: null,
                new SoftDeleteColumnDeletionDetectionPolicy("IsRemoved", true));

            await this.searchServiceClient.DataSources.CreateAsync(dataSource);
        }

        /// <summary>
        /// Create indexer if it doesn't exist in Azure Search service.
        /// </summary>
        /// <returns><see cref="Task"/> That represents indexer is created if not available in Azure Search service.</returns>
        private async Task CreateIndexerAsync()
        {
            // Recreate only if there is a change in the storage schema.
            // Manually need to drop and create Indexers whenever there is storage schema design change.
            if (await this.searchServiceClient.Indexers.ExistsAsync(IdeaIndexerName))
            {
                return;
            }

            var indexer = new Indexer()
            {
                Name = IdeaIndexerName,
                DataSourceName = IdeaDataSourceName,
                TargetIndexName = Constants.TeamIdeaIndexName,
            };

            await this.searchServiceClient.Indexers.CreateAsync(indexer);
            await this.searchServiceClient.Indexers.RunAsync(IdeaIndexerName);
        }

        /// <summary>
        /// Initialization of InitializeAsync method which will help in indexing.
        /// </summary>
        /// <returns>Represents an asynchronous operation.</returns>
        private Task EnsureInitializedAsync()
        {
            return this.initializeTask.Value;
        }

        /// <summary>
        /// Initialization of search service parameters which will help in searching the documents.
        /// </summary>
        /// <param name="searchScope">Scope of the search.</param>
        /// <param name="userObjectId">Azure Active Directory object id of the user.</param>
        /// <param name="count">Number of search results to return.</param>
        /// <param name="skip">Number of search results to skip.</param>
        /// <param name="sortBy">Represents sorting type like: Popularity or Newest.</param>
        /// <param name="filterQuery">Filter bar based query.</param>
        /// <returns>Represents an search parameter object.</returns>
        private SearchParameters InitializeSearchParameters(
            IdeaSearchScope searchScope,
            string userObjectId,
            int? count = null,
            int? skip = null,
            string sortBy = null,
            string filterQuery = null)
        {
            SearchParameters searchParameters = new SearchParameters()
            {
                Top = count ?? ApiSearchResultCount,
                Skip = skip ?? 0,
                IncludeTotalResultCount = false,
                Select = new[]
                {
                    nameof(IdeaEntity.IdeaId),
                    nameof(IdeaEntity.CategoryId),
                    nameof(IdeaEntity.Category),
                    nameof(IdeaEntity.Title),
                    nameof(IdeaEntity.Description),
                    nameof(IdeaEntity.Tags),
                    nameof(IdeaEntity.CreatedDate),
                    nameof(IdeaEntity.CreatedByName),
                    nameof(IdeaEntity.UpdatedDate),
                    nameof(IdeaEntity.CreatedByObjectId),
                    nameof(IdeaEntity.TotalVotes),
                    nameof(IdeaEntity.Status),
                    nameof(IdeaEntity.CreatedByUserPrincipalName),
                },
                SearchFields = new[] { nameof(IdeaEntity.Title) },
                Filter = string.IsNullOrEmpty(filterQuery) ? null : $"({filterQuery})",
            };

            switch (searchScope)
            {
                case IdeaSearchScope.AllItems:
                    searchParameters.OrderBy = new[] { $"{nameof(IdeaEntity.UpdatedDate)} desc" };

                    break;

                case IdeaSearchScope.PostedByMe:
                    searchParameters.Filter = $"{nameof(IdeaEntity.CreatedByObjectId)} eq '{userObjectId}' ";
                    searchParameters.OrderBy = new[] { $"{nameof(IdeaEntity.UpdatedDate)} desc" };
                    break;

                case IdeaSearchScope.Popular:
                    searchParameters.OrderBy = new[] { $"{nameof(IdeaEntity.TotalVotes)} desc" };
                    break;

                case IdeaSearchScope.TeamPreferenceTags:
                    searchParameters.SearchFields = new[] { nameof(IdeaEntity.Tags) };
                    searchParameters.Top = 5000;
                    searchParameters.Select = new[] { nameof(IdeaEntity.Tags) };
                    break;

                case IdeaSearchScope.Categories:
                    searchParameters.SearchFields = new[] { nameof(IdeaEntity.CategoryId) };
                    searchParameters.Top = 5000;
                    searchParameters.Select = new[] { nameof(IdeaEntity.CategoryId) };
                    break;

                case IdeaSearchScope.CategoriesInUse:
                    searchParameters.SearchFields = new[] { nameof(IdeaEntity.CategoryId) };
                    searchParameters.Filter = $"{nameof(IdeaEntity.Status)} eq {(int)IdeaStatus.Pending} or {nameof(IdeaEntity.Status)} eq {(int)IdeaStatus.Approved}";
                    searchParameters.Select = new[] { nameof(IdeaEntity.Category) };
                    break;

                case IdeaSearchScope.FilterAsPerTeamTags:
                    searchParameters.OrderBy = new[] { $"{nameof(IdeaEntity.UpdatedDate)} desc" };
                    searchParameters.SearchFields = new[] { nameof(IdeaEntity.CategoryId) };
                    break;

                case IdeaSearchScope.FilterPostsAsPerDateRange:
                    searchParameters.OrderBy = new[] { $"{nameof(IdeaEntity.UpdatedDate)} desc" };
                    searchParameters.Top = 200;
                    break;

                case IdeaSearchScope.UniqueUserNames:
                    searchParameters.OrderBy = new[] { $"{nameof(IdeaEntity.UpdatedDate)} desc" };
                    searchParameters.Select = new[] { nameof(IdeaEntity.CreatedByName) };
                    break;

                case IdeaSearchScope.SearchTeamPostsForTitleText:
                    searchParameters.OrderBy = new[] { $"{nameof(IdeaEntity.UpdatedDate)} desc" };
                    searchParameters.QueryType = QueryType.Full;
                    searchParameters.SearchFields = new[] { nameof(IdeaEntity.Title) };
                    break;

                case IdeaSearchScope.Pending:
                    searchParameters.Filter = $"{nameof(IdeaEntity.Status)} eq {(int)IdeaStatus.Pending} ";
                    searchParameters.OrderBy = new[] { $"{nameof(IdeaEntity.UpdatedDate)} desc" };
                    break;

                case IdeaSearchScope.Approved:
                    searchParameters.Filter = $"{nameof(IdeaEntity.Status)} eq {(int)IdeaStatus.Approved} ";
                    searchParameters.OrderBy = new[] { $"{nameof(IdeaEntity.UpdatedDate)} desc" };
                    break;

                case IdeaSearchScope.FilterTeamPosts:

                    if (!string.IsNullOrEmpty(sortBy))
                    {
                        searchParameters.OrderBy = sortBy == SortByPopular ? new[] { $"{nameof(IdeaEntity.TotalVotes)} desc" } : new[] { $"{nameof(IdeaEntity.UpdatedDate)} desc" };
                    }

                    searchParameters.SearchFields = new[] { nameof(IdeaEntity.Tags) };
                    break;
            }

            return searchParameters;
        }

        /// <summary>
        /// Escaping unsafe and reserved characters from Azure Search Service search query.
        /// Special characters that requires escaping includes
        /// + - &amp; | ! ( ) { } [ ] ^ " ~ * ? : \ /
        /// Refer https://docs.microsoft.com/en-us/azure/search/query-lucene-syntax#escaping-special-characters to know more.
        /// </summary>
        /// <param name="query">Query which the user had typed in search field.</param>
        /// <returns>Returns string escaping unsafe and reserved characters.</returns>
        private string EscapeCharactersInQuery(string query)
        {
            string pattern = @"([_|\\@&\?\*\+!-:~'\^/(){}<>#&\[\]])";
            string substitution = "\\$&";
            query = Regex.Replace(query, pattern, substitution);

            return query;
        }
    }
}
