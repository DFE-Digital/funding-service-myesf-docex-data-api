using Pds.Core.Common.Organisation.Models;
using Pds.Core.Logging;
using Pds.Core.Utils.Helpers;
using Pds.DocumentExchange.Data.Services.DTOs;
using Pds.DocumentExchange.Data.Services.Enums;
using Pds.DocumentExchange.Data.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Implementations
{
    /// <summary>
    /// The batches service.
    /// </summary>
    public class BatchesService : IBatchesService
    {
        private readonly ICosmosDbService _cosmosDbService;
        private readonly ILoggerAdapter<BatchesService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="BatchesService"/> class.
        /// </summary>
        /// <param name="cosmosDbService">The Cosmos DB service.</param>
        /// <param name="logger">The logger.</param>
        public BatchesService(
            ICosmosDbService cosmosDbService,
            ILoggerAdapter<BatchesService> logger)
        {
            _cosmosDbService = cosmosDbService;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<DocumentWithViewedStatus>> GetReceivedFilesForAgencyWithSeenHistory(IEnumerable<Product> allowedProducts)
            => await _cosmosDbService.ReceivedFilesForAgencyWithSeenHistory(allowedProducts);

        /// <inheritdoc/>
        public async Task<IEnumerable<BatchMetadata>> GetFileInfoForDelete(ExchangeDocumentDirection direction, int ukprn, string fileType, string year)
        {
            var deleteFiles = direction == ExchangeDocumentDirection.PublishedByAgency
        ? await _cosmosDbService.GetAgencyDeleteFilesInfo(ukprn, fileType, year)
        : await _cosmosDbService.GetOrganisationDeleteFilesInfo(ukprn, fileType);

            return deleteFiles;
        }


        /// <inheritdoc/>
        public Task<IEnumerable<BatchMetadata>> GetReceivedBatches(IEnumerable<OrganisationIdentifier> organisationIdentifiers)
            => GetBatches(organisationIdentifiers, _cosmosDbService.MetadataOfDocumentsReceivedByOrganisations);

        /// <inheritdoc/>
        public Task<IEnumerable<BatchMetadata>> GetSentBatches(IEnumerable<OrganisationIdentifier> organisationIdentifiers)
            => GetBatches(organisationIdentifiers, _cosmosDbService.MetadataOfDocumentsSentByOrganisations);

        private async Task<IEnumerable<BatchMetadata>> GetBatches(
            IEnumerable<OrganisationIdentifier> organisationIdentifiers,
            Func<IEnumerable<OrganisationIdentifier>, Task<IEnumerable<BatchMetadata>>> getBatchesFunc)
        {
            (It.IsEmpty(organisationIdentifiers) || organisationIdentifiers.Any(id => It.IsNull(id) || It.IsEmpty(id.Value)))
                .AsGuard<ArgumentNullException>(nameof(organisationIdentifiers));

            _logger.LogInformation("Getting batches for " +
                $"({string.Join(", ", organisationIdentifiers.Select(id => id.Value))}) " +
                $"using {nameof(getBatchesFunc)}.");

            var batches = await getBatchesFunc(organisationIdentifiers);

            var result = batches.ToList();

            _logger.LogInformation("Retrieved data for " +
                                   $"({string.Join(", ", organisationIdentifiers.Select(id => id.Value))}) " +
                                   $"using {nameof(getBatchesFunc)}. Result count: ${result.Count}");

            return result;
        }
    }
}