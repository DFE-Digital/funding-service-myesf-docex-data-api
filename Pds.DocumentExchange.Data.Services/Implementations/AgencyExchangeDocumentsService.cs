using Pds.Core.Caching.Interfaces;
using Pds.Core.Caching.Models;
using Pds.Core.Logging;
using Pds.Core.Utils.Helpers;
using Pds.DocumentExchange.Data.Services.Constants;
using Pds.DocumentExchange.Data.Services.DTOs;
using Pds.DocumentExchange.Data.Services.Enums;
using Pds.DocumentExchange.Data.Services.Interfaces;
using Pds.DocumentExchange.Data.Services.Interfaces.Caching;
using Pds.DocumentExchange.Data.Services.Interfaces.Settings;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Implementations
{
    /// <summary>
    /// The agency exchange document service.
    /// </summary>
    public class AgencyExchangeDocumentsService : IAgencyExchangeDocumentsService
    {
        private readonly IConfigurationDataService _configurationService;
        private readonly IBatchesService _batchesService;
        private readonly IBatchToExchangeDocumentConverter _batchToExchangeDocumentConverter;
        private readonly ICacheManager _cacheManager;
        private readonly ILoggerAdapter<AgencyExchangeDocumentsService> _logger;

        private ICacheService Cache => _cacheManager.CacheService;

        private ICacheKeyBuilder CacheKeyBuilder => _cacheManager.CacheKeyBuilder;

        private CacheOptions DocumentListCachingOptions => _cacheManager.CacheOptionsProvider.DocumentList;

        /// <summary>
        /// Initializes a new instance of the <see cref="AgencyExchangeDocumentsService"/> class.
        /// </summary>
        /// <param name="batchesService">The batches service.</param>
        /// <param name="batchToExchangeDocumentConverter">The batch to exchange document converter.</param>
        /// <param name="cacheManager">The cache manager.</param>
        /// <param name="configuration">The configuration service.</param>
        /// <param name="logger">The logger.</param>
        public AgencyExchangeDocumentsService(
            IBatchesService batchesService,
            IBatchToExchangeDocumentConverter batchToExchangeDocumentConverter,
            ICacheManager cacheManager,
            IConfigurationDataService configuration,
            ILoggerAdapter<AgencyExchangeDocumentsService> logger)
        {
            _batchesService = batchesService;
            _batchToExchangeDocumentConverter = batchToExchangeDocumentConverter;
            _cacheManager = cacheManager;
            _logger = logger;
            _configurationService = configuration;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<DocumentWithViewedStatus>> GetReceivedFilesForAgencyWithSeenHistory(IEnumerable<string> teams)
        {
            _logger.LogInformation($"Getting ReceivedFilesForAgencyWithSeenHistory for teams ({string.Join(", ", teams)}) ");

            var allProducts = await _configurationService.GetProducts();

            var allowedProducts = allProducts.Where(p => p.AgencyTeams.Any(t => teams.Contains(t)));
            var result = await _batchesService.GetReceivedFilesForAgencyWithSeenHistory(allowedProducts);

            _logger.LogInformation($"Retrieved {result.Count()} files with seen history for teams ({string.Join(", ", teams)}) ");

            return result;
        }


        /// <inheritdoc/>
        public async Task<IEnumerable<ExchangeDocument>> GetExchangeDocuments(IEnumerable<string> teams, ExchangeDocumentDirection direction)
        {
            try
            {
                It.IsEmpty(teams)
                .AsGuard<ArgumentNullException>(nameof(teams));

                _logger.LogInformation($"Getting exchange documents for teams ({string.Join(", ", teams)}) " +
                    $"with direction {direction}");

                var exchangeDocuments = await GetAllTeamsExchangeDocuments(direction);

                var result = await GetExchangeDocumentsByTeams(teams, exchangeDocuments, direction);

                var documentList = result.ToList();

                _logger.LogInformation($"Retrieved exchange documents for teams ({string.Join(", ", teams)}) " +
                                       $"with direction {direction} Count: {documentList.Count}");

                return documentList;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred in {nameof(GetExchangeDocuments)} for teams: ({string.Join(", ", teams)} with direction: {direction}");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<ListResult<ExchangeDocument>> GetDeleteFiles(ExchangeDocumentDirection direction, int ukprn, string fileType, string year)
        {
            try
            {
                _logger.LogInformation($"Getting delete files for direction:{direction}, ukprn:{ukprn},fileType:{fileType}, year:{year}");

                var filesToDelete = await _batchesService.GetFileInfoForDelete(direction, ukprn, fileType, year);
                var result = await _batchToExchangeDocumentConverter.ConvertToOrganisationExchangeDocuments(filesToDelete, direction);

                return new ListResult<ExchangeDocument>
                {
                    Items = result.ToList(),
                    TotalItems = result.Count()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred in {nameof(GetDeleteFiles)} for direction:{direction}, ukprn:{ukprn},fileType:{fileType}, year:{year}");
                throw;
            }
        }

        private Task<IEnumerable<ExchangeDocument>> GetAllTeamsExchangeDocuments(ExchangeDocumentDirection direction)
         => Cache.Get(
             CacheKeyBuilder.BuildAgencyTeamExchangedDocumentsKey("AllTeams", direction),
             async () =>
             {
                 var allBatches = await GetBatches(direction);
                 _logger.LogInformation($"PFM ConvertToOrganisationExchangeDocuments {direction}");
                 var result = await _batchToExchangeDocumentConverter.ConvertToOrganisationExchangeDocuments(allBatches, direction);
                 _logger.LogInformation($"PFM ConvertToOrganisationExchangeDocuments {direction} result count:{result.Count()}");
                 return result;
             },
             DocumentListCachingOptions);

        private async Task<IEnumerable<ExchangeDocument>> GetExchangeDocumentsByTeams(
            IEnumerable<string> teams,
            IEnumerable<ExchangeDocument> allTeamsExchangeDocuments,
            ExchangeDocumentDirection direction)
        {
            var teamsExchangeDocumentsTasks = teams.Select(team => Cache.Get(
               CacheKeyBuilder.BuildAgencyTeamExchangedDocumentsKey(team, direction),
               () => GetExchangeDocumentsBySingleTeam(team, allTeamsExchangeDocuments),
               DocumentListCachingOptions));

            var teamsExchangeDocumentResults = await Task.WhenAll(teamsExchangeDocumentsTasks);
            return teamsExchangeDocumentResults.SelectMany(exchangeDocument => exchangeDocument);
        }

        private Task<IEnumerable<ExchangeDocument>> GetExchangeDocumentsBySingleTeam(
            string team,
            IEnumerable<ExchangeDocument> allTeamsExchangeDocuments)
        {
            var visibleExchangeDocuments = GetExchangeDocumentsAllowedToBeSeenByTeam(team, allTeamsExchangeDocuments);
            return Task.FromResult(visibleExchangeDocuments);
        }

        private async Task<IEnumerable<BatchMetadata>> GetBatches(ExchangeDocumentDirection direction)
        {
            _logger.LogInformation($"PFM Getting Batch details {direction}");

            var result = direction switch
            {
                ExchangeDocumentDirection.PublishedByAgency
                => await _batchesService.GetSentBatches(new[] { EsfaOrganisationInfo.Identifier }),

                ExchangeDocumentDirection.SentByOrganisation
                => await _batchesService.GetReceivedBatches(new[] { EsfaOrganisationInfo.Identifier }),

                _ => throw new NotImplementedException()
            };
            var batchData = result.ToList();
            _logger.LogInformation($"PFM Retrieved Batch details {direction} - Results: {batchData.Count}");

            return batchData;
        }

        private IEnumerable<ExchangeDocument> GetExchangeDocumentsAllowedToBeSeenByTeam(string team, IEnumerable<ExchangeDocument> exchangeDocuments)
            => exchangeDocuments
                .Where(document => document.Product.AgencyTeams.Contains(team, StringComparer.OrdinalIgnoreCase))
                .ToList();
    }
}