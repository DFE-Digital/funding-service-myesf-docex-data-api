using MapsterMapper;
using Microsoft.Azure.Cosmos.Serialization.HybridRow;
using Pds.Core.Common.Organisation.Enums;
using Pds.Core.Common.Organisation.Models;
using Pds.Core.Logging;
using Pds.DocumentExchange.Data.Services.DTOs;
using Pds.DocumentExchange.Data.Services.DTOs.User;
using Pds.DocumentExchange.Data.Services.Enums;
using Pds.DocumentExchange.Data.Services.Interfaces;
using Pds.DocumentExchange.Data.Services.Interfaces.Lookup;
using Pds.Services.Common.Helpers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Implementations
{
    /// <summary>
    /// A service to convert batch metadata objects collections to exchange documents.
    /// </summary>
    public class BatchToExchangeDocumentConverter : IBatchToExchangeDocumentConverter
    {
        private readonly IProductsLookup _productsLookup;
        private readonly IOrganisationsLookup _organisationsLookup;
        private readonly IFileMetadataUserEncryptor _fileMetadataUserEncryptor;
        private readonly IMapper _mapper;
        private readonly ILoggerAdapter<BatchToExchangeDocumentConverter> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="BatchToExchangeDocumentConverter"/> class.
        /// </summary>
        /// <param name="productsLookup">The products lookup.</param>
        /// <param name="organisationsLookup">The organisation API client.</param>
        /// <param name="fileMetadataUserEncryptor">The file metadata user encryptor.</param>
        /// <param name="mapper">The type mapper.</param>
        /// <param name="logger">The logger.</param>
        public BatchToExchangeDocumentConverter(
            IProductsLookup productsLookup,
            IOrganisationsLookup organisationsLookup,
            IFileMetadataUserEncryptor fileMetadataUserEncryptor,
            IMapper mapper,
            ILoggerAdapter<BatchToExchangeDocumentConverter> logger)
        {
            _productsLookup = productsLookup;
            _organisationsLookup = organisationsLookup;
            _fileMetadataUserEncryptor = fileMetadataUserEncryptor;
            _mapper = mapper;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<ExchangeDocument> ConvertToAgencyExchangeDocument(
           string parentBatchIdentifier,
           string batchIdentifier,
           FileMetadata fileMetadata,
           ExchangeDocumentDirection direction)
        {
            It.IsEmpty(parentBatchIdentifier)
                .AsGuard<ArgumentNullException>();

            It.IsEmpty(batchIdentifier)
                .AsGuard<ArgumentNullException>();

            It.IsNull(fileMetadata)
                .AsGuard<ArgumentNullException>();

            var eventsLookup = GetExchangeDocumentEventsLookup();

            var fileOrganisationIdentifier = GetFileOrganisationIdentifier(fileMetadata, direction);
            var organisation = await _organisationsLookup.Get(fileOrganisationIdentifier);

            return await CreateExchangeDocument(
                        batchIdentifier,
                        parentBatchIdentifier,
                        fileMetadata,
                        direction,
                        product => product.AgencyTeams.FirstOrDefault(),
                        eventsLookup,
                        organisation,
                        fileOrganisationIdentifier);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ExchangeDocument>> ConvertToAgencyExchangeDocuments(
            IEnumerable<BatchMetadata> batches,
            ExchangeDocumentDirection direction)
            => await ConvertToExchangeDocuments(batches, direction, product => product.AgencyTeams.FirstOrDefault());

        /// <inheritdoc/>
        public async Task<IEnumerable<ExchangeDocument>> ConvertToOrganisationExchangeDocuments(
            IEnumerable<BatchMetadata> batches,
            ExchangeDocumentDirection direction)
            => await ConvertToExchangeDocuments(batches, direction, product => string.Empty);

        private async Task<IEnumerable<ExchangeDocument>> ConvertToExchangeDocuments(
           IEnumerable<BatchMetadata> batches,
           ExchangeDocumentDirection direction,
           Func<Product, string> getAgencyTeam)
        {
            It.IsNull(batches)
                .AsGuard<ArgumentNullException>(nameof(batches));

            if (!batches.Any() || !batches.Any(batch => batch.Files.Any()))
            {
                return await Task.FromResult(Enumerable.Empty<ExchangeDocument>());
            }

            var eventsLookup = GetExchangeDocumentEventsLookup();
            var exchangeDocumentTasks = new List<Task<ExchangeDocument>>();
            var organisations = await _organisationsLookup.GetAllOrganisations();

            foreach (var batch in batches)
            {
                foreach (var file in batch.Files)
                {
                    var fileOrganisationIdentifier = GetFileOrganisationIdentifier(file, direction);
                    var organisation = organisations.ContainsKey(fileOrganisationIdentifier.Value)
                                                ? organisations[fileOrganisationIdentifier.Value]
                                                : new UnknownOrganisation(fileOrganisationIdentifier);

                    var task = CreateExchangeDocument(
                        batch.Id,
                        batch.ParentBatchIdentifier,
                        file,
                        direction,
                        getAgencyTeam,
                        eventsLookup,
                        organisation,
                        fileOrganisationIdentifier);

                    exchangeDocumentTasks.Add(task);
                }
            }

            var exchangeDocuments = await Task.WhenAll(exchangeDocumentTasks);
            return GroupExchangeDocumentsByVersion(exchangeDocuments);
        }

        private OrganisationIdentifier GetFileOrganisationIdentifier(
            FileMetadata file,
            ExchangeDocumentDirection direction)
        {
            // This has to be replaced with the new organisation identifier structure loaded from the file.
            int ukprn = direction == ExchangeDocumentDirection.PublishedByAgency
                ? file.ToUkprn
                : file.FromUkprn;

            return new OrganisationIdentifier
            {
                Type = OrganisationIdentifierType.Ukprn,
                Value = ukprn.ToString()
            };
        }

        private async Task<ExchangeDocument> CreateExchangeDocument(
           string batchIdentifier,
           string batchParentIdentifier,
           FileMetadata file,
           ExchangeDocumentDirection direction,
           Func<Product, string> getAgencyTeam,
           ConcurrentDictionary<FileAction, ExchangeDocumentEventType> eventsLookup,
           Organisation organisation,
           OrganisationIdentifier fileOrganisationIdentifier)
        {
            try
            {
                var productTask = _productsLookup.Get(file.ProductIdentifier);

                var product = await productTask;

                return new ExchangeDocument
                {
                    DocumentReference = new DocumentReference
                    {
                        BatchIdentifier = batchIdentifier,
                        ParentBatchIdentifier = batchParentIdentifier,
                        FileName = file.FileName
                    },
                    AgencyTeam = getAgencyTeam(product),
                    ExchangeDirection = direction,
                    EventHistory = CreateEvents(file.History, eventsLookup),
                    Product = product,
                    OrganisationInfo = new OrganisationInfo
                    {
                        OrganisationIdentifier = fileOrganisationIdentifier,
                        Name = organisation.Name
                    },
                    Organisation = organisation,
                    Year = GetAcademicYear(file.Metadata),
                    Version = file.Version,
                    PreviousVersions = Collection.Empty<ExchangeDocument>()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred in {nameof(CreateExchangeDocument)} for fileName: {file?.FileName}");
                throw;
            }
        }

        private IEnumerable<ExchangeDocument> GroupExchangeDocumentsByVersion(IEnumerable<ExchangeDocument> exchangeDocuments)
        {
            var documentsGroups = exchangeDocuments.GroupBy(document =>
            new
            {
                document.OrganisationInfo.OrganisationIdentifier.Type,
                document.OrganisationInfo.OrganisationIdentifier.Value,
                document.Product.Identifier,
                document.Year
            });

            return documentsGroups.Select(documentsGroup =>
            {
                var orderedExchangeDocuments = documentsGroup.OrderByDescending(doc => doc.Version);
                var latestVersionExchangeDocument = orderedExchangeDocuments.First();

                latestVersionExchangeDocument.PreviousVersions =
                    orderedExchangeDocuments.Skip(1)
                    .ToList();

                return latestVersionExchangeDocument;
            });
        }

        private int GetAcademicYear(Dictionary<string, string> metadata)
        {
            if (It.IsNull(metadata)
                || !metadata.TryGetValue("Year", out string parsedYear)
                || !int.TryParse(parsedYear, out int yearResult))
            {
                return -1;
            }

            return yearResult;
        }

        #region Events

        private IEnumerable<ExchangeDocumentEvent> CreateEvents(
            IEnumerable<FileMetadataHistory> fileHistory,
            ConcurrentDictionary<FileAction, ExchangeDocumentEventType> eventsLookup)
        {
            return fileHistory
                .Where(history => eventsLookup.ContainsKey(history.Action))
                .GroupBy(f => f.Action)
                .Select(historyByAction =>
                {
                    var history = historyByAction.OrderByDescending(h => h.ActionDateTimeUtc).First();

                    var decryptedFileMetadataUser = _fileMetadataUserEncryptor.Decrypt(history.User);
                    var mappedUser = _mapper.Map<FileMetadataUser, UserInfo>(decryptedFileMetadataUser);

                    return new ExchangeDocumentEvent
                    {
                        EventType = eventsLookup[history.Action],
                        EventDateTime = history.ActionDateTimeUtc,
                        UserInfo = mappedUser
                    };
                })
                .ToList();
        }

        private ConcurrentDictionary<FileAction, ExchangeDocumentEventType> GetExchangeDocumentEventsLookup()
        {
            return new ConcurrentDictionary<FileAction, ExchangeDocumentEventType>
            {
                [FileAction.ViewedByReceiver] = ExchangeDocumentEventType.DownloadedByReceiver,
                [FileAction.UploadedExternal] = ExchangeDocumentEventType.SentByOrganisation,
                [FileAction.Published] = ExchangeDocumentEventType.PublishedByAgency
            };
        }

        #endregion
    }
}