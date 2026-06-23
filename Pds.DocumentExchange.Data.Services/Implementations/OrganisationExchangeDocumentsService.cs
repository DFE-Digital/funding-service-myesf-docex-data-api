using Pds.Core.Caching.Interfaces;
using Pds.Core.Caching.Models;
using Pds.Core.Common.Organisation.Models;
using Pds.Core.Logging;
using Pds.Core.Utils.Helpers;
using Pds.DocumentExchange.Data.Services.DTOs;
using Pds.DocumentExchange.Data.Services.Enums;
using Pds.DocumentExchange.Data.Services.Interfaces;
using Pds.DocumentExchange.Data.Services.Interfaces.Caching;
using Pds.DocumentExchange.Data.Services.Interfaces.Lookup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Implementations
{
    /// <summary>
    /// The organisation exchange documents service.
    /// </summary>
    public class OrganisationExchangeDocumentsService : IOrganisationExchangeDocumentsService
    {
        private readonly IBatchesService _batchesService;
        private readonly IBatchToExchangeDocumentConverter _batchToExchangeDocumentConverter;
        private readonly ICacheManager _cacheManager;
        private readonly IOrganisationsLookup _organisationsLookup;
        private readonly ILoggerAdapter<OrganisationExchangeDocumentsService> _logger;

        private ICacheService Cache => _cacheManager.CacheService;

        private ICacheKeyBuilder CacheKeyBuilder => _cacheManager.CacheKeyBuilder;

        private CacheOptions DocumentListCachingOptions => _cacheManager.CacheOptionsProvider.DocumentList;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrganisationExchangeDocumentsService"/> class.
        /// </summary>
        /// <param name="batchesService">The batches service.</param>
        /// <param name="batchToExchangeDocumentConverter">The batch to exchange document converter.</param>
        /// <param name="cacheManager">The cache manager.</param>
        /// <param name="organisationsLookup">The organisations lookup.</param>
        /// <param name="logger">The logger.</param>
        public OrganisationExchangeDocumentsService(
            IBatchesService batchesService,
            IBatchToExchangeDocumentConverter batchToExchangeDocumentConverter,
            ICacheManager cacheManager,
            IOrganisationsLookup organisationsLookup,
            ILoggerAdapter<OrganisationExchangeDocumentsService> logger)
        {
            _batchesService = batchesService;
            _batchToExchangeDocumentConverter = batchToExchangeDocumentConverter;
            _cacheManager = cacheManager;
            _organisationsLookup = organisationsLookup;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ExchangeDocument>> GetExchangeDocuments(OrganisationIdentifier organisationIdentifier, ExchangeDocumentDirection direction)
        {
            (It.IsNull(organisationIdentifier) || It.IsEmpty(organisationIdentifier.Value))
                .AsGuard<ArgumentNullException>();

            _logger.LogInformation($"Getting exchange documents for organisation ({organisationIdentifier.Value}) " +
                $"with direction {direction}");

            return await Cache.Get(
                CacheKeyBuilder.BuildOrganisationExchangedDocumentsKey(organisationIdentifier, direction),
                async () =>
                {
                    var allBatches = await GetBatches(organisationIdentifier, direction);
                    var result = await _batchToExchangeDocumentConverter.ConvertToOrganisationExchangeDocuments(allBatches, direction);

                    return result.ToList();
                },
                DocumentListCachingOptions);
        }

        private async Task<IEnumerable<BatchMetadata>> GetBatches(OrganisationIdentifier organisationIdentifier, ExchangeDocumentDirection direction)
        {
            var organisationIdentifiers = await _organisationsLookup.GetSelfAndChildUkprns(organisationIdentifier);

            return direction switch
            {
                ExchangeDocumentDirection.PublishedByAgency
                    => await _batchesService.GetReceivedBatches(organisationIdentifiers),

                ExchangeDocumentDirection.SentByOrganisation
                    => await _batchesService.GetSentBatches(organisationIdentifiers),

                _ => throw new NotImplementedException()
            };
        }
    }
}
