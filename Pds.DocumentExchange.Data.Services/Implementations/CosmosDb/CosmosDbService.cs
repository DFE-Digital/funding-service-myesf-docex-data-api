using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Pds.Core.Common.Organisation.Models;
using Pds.Core.Logging;
using Pds.DocumentExchange.Data.Core.Common;
using Pds.DocumentExchange.Data.Repository.Interfaces;
using Pds.DocumentExchange.Data.Services.DTOs;
using Pds.DocumentExchange.Data.Services.DTOs.Notification;
using Pds.DocumentExchange.Data.Services.DTOs.SupportTools;
using Pds.DocumentExchange.Data.Services.Enums;
using Pds.DocumentExchange.Data.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Implementations.CosmosDb
{
    /// <summary>The shared Cosmos Db Service.</summary>
    public partial class CosmosDbService : CosmosDbServiceBase, ICosmosDbService
    {
        /// <summary>Initializes a new instance of the <see cref="CosmosDbService" /> class.</summary>
        /// <param name="documentsRepository">The cosmos database documents repository.</param>
        /// <param name="fileRepository">The file repository.</param>
        /// <param name="logger">The logging service.</param>
        public CosmosDbService(
            IDocumentsRepository documentsRepository,
            IFileRepository fileRepository,
            ILoggerAdapter<CosmosDbServiceBase> logger)
            : base(documentsRepository, fileRepository, logger)
        {
        }

        /// <inheritdoc />
        public async Task<string> AddHistoryToFileMetadata(string batchIdentifier, FileMetadata fileInfo)
        {
            var fileHistoryRequest = new
            {
                BatchId = batchIdentifier,
                Files = new FileMetadata[] { fileInfo }
            };

            var serialisedFileHistoryRequest = JsonConvert.SerializeObject(fileHistoryRequest);
            return await RunStoredProcedureAsync<string>(CosmosDbStoredProcedureNames.AddHistoryToFileMetadata, serialisedFileHistoryRequest);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<BatchMetadata>> GetBatchesByParent(string parentBatchIdentifier)
        {
            IEnumerable<IEnumerable<BatchMetadata>> results = await RunStoredProcedureWithContinuationTokenAsync<IEnumerable<BatchMetadata>>(CosmosDbStoredProcedureNames.GetBatchesByParent, parentBatchIdentifier);
            List<BatchMetadata> collatedResults = new List<BatchMetadata>();
            foreach (var result in results)
            {
                collatedResults.AddRange(result);
            }

            return collatedResults;
        }

        /// <inheritdoc/>
        public async Task<TValue> GetConfiguration<TValue>(ConfigurationSection section)
        {
            return await RunStoredProcedureAsync<TValue>(CosmosDbStoredProcedureNames.GetConfiguration, section.ToString());
        }

        /// <inheritdoc/>
        public async Task<TValue> UpdateConfiguration<TValue>(ConfigurationSection section, TValue newValue)
        {
            var newValueJson = JsonConvert.SerializeObject(newValue);
            return await RunStoredProcedureAsync<TValue>(CosmosDbStoredProcedureNames.UpdateConfiguration, section.ToString(), newValueJson);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<TValue>> GetListConfiguration<TValue>(ConfigurationSection section)
        {
            return await RunStoredProcedureAsync<IEnumerable<TValue>>(CosmosDbStoredProcedureNames.GetListConfiguration, section.ToString());
        }

        /// <inheritdoc/>
        public async Task<TListItems> AddOrUpdateListConfiguration<TListItems, TIdentifier>(ConfigurationSection section, TIdentifier oldIdentifier, TListItems newValue)
        {
            var newValueJson = JsonConvert.SerializeObject(newValue, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });

            return await RunStoredProcedureAsync<TListItems>(CosmosDbStoredProcedureNames.AddOrUpdateListConfiguration, section.ToString(), oldIdentifier, newValueJson);
        }

        /// <inheritdoc />
        public async Task<int> GetCountOfUnprocessedFiles(string parentBatchIdentifier)
        {
            return await RunStoredProcedureAsync<int>(CosmosDbStoredProcedureNames.GetCountOfUnprocessedFiles, parentBatchIdentifier);
        }

        /// <inheritdoc />
        public async Task<int> GetCurrentVersionNumber(OrganisationIdentifier fromIdentifier, OrganisationIdentifier toIdentifier, int year, int fileType)
        {
            return await RunStoredProcedureAsync<int>(CosmosDbStoredProcedureNames.GetCurrentVersionNumber, fromIdentifier.Value, toIdentifier.Value, year, fileType);
        }

        /// <inheritdoc />
        public async Task<KeyValuePair<string, Dictionary<string, string>>> GetReportOfDocumentsViewed(DateTime from, DateTime to)
        {
            return await RunStoredProcedureAsync<KeyValuePair<string, Dictionary<string, string>>>(CosmosDbStoredProcedureNames.GetReportOfDocumentsViewed, from, to);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<ClickSummary>> GetReportOfDocumentsViewedSummary(DateTime from, DateTime to)
        {
            return await RunStoredProcedureAsync<IEnumerable<ClickSummary>>(CosmosDbStoredProcedureNames.GetReportOfDocumentsViewedSummary, from, to);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<ClickDetail>> GetReportOfFileScanBad(DateTime from, DateTime to)
        {
            return await RunStoredProcedureAsync<IEnumerable<ClickDetail>>(CosmosDbStoredProcedureNames.GetReportOfFileScanBad, from, to);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<ErrorDetail>> GetReportOfInitialUploadErrors(DateTime @from, DateTime to)
        {
            return await RunStoredProcedureAsync<IEnumerable<ErrorDetail>>(CosmosDbStoredProcedureNames.GetReportOfInitialUploadErrors, from, to);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<ClickDetail>> GetReportOfFilesSentFromAgency(DateTime from, DateTime to)
        {
            return await RunStoredProcedureAsync<IEnumerable<ClickDetail>>(CosmosDbStoredProcedureNames.GetReportOfFilesSentFromAgency, from, to);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<MIReport>> GetMIReport(DateTime from, DateTime to)
        {
            var results = await RunStoredProcedureWithContinuationTokenAsync<IEnumerable<MIReport>>(
                CosmosDbStoredProcedureNames.GetMIReport,
                [from.ToString("yyyy-MM-ddTHH:mm:ss"), to.ToString("yyyy-MM-ddTHH:mm:ss")]);

            List<MIReport> collatedResults = new();
            foreach (var result in results)
            {
                collatedResults.AddRange(result);
            }

            return collatedResults;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<BatchMetadata>> MetadataOfDocumentsReceivedByOrganisations(
            IEnumerable<OrganisationIdentifier> identifiers)
        {
            var ukprns = identifiers.Select(id => Convert.ToInt32(id.Value)).ToArray();
            return await RunQueryAsync<BatchMetadata>(nameof(MetadataOfDocumentsReceivedByOrganisations), ("@ukprns", ukprns));
        }

        /// <inheritdoc />
        public async Task<IEnumerable<BatchMetadata>> MetadataOfDocumentsSentByOrganisations(
            IEnumerable<OrganisationIdentifier> identifiers)
        {
            var ukprns = identifiers.Select(id => Convert.ToInt32(id.Value)).ToArray();
            return await RunQueryAsync<BatchMetadata>(nameof(MetadataOfDocumentsSentByOrganisations), ("@ukprns", ukprns));
        }

        /// <inheritdoc />
        public async Task<bool> TryAndSetAnExclusiveEmailLockIdOnTheBatches(string parentBatchIdentifier, string lockIdentifier)
        {
            return await RunStoredProcedureAsync<bool>(CosmosDbStoredProcedureNames.TryAndSetAnExclusiveEmailLockIdOnTheBatches, parentBatchIdentifier, lockIdentifier);
        }

        /// <inheritdoc />
        public async Task UpdateDocumentMetadata(FileMetadata metadata, string batchIdentifier, IEnumerable<string> fieldsToUpdate, string existingFileName = null)
        {
            var serialisedUpdateObject = CreateSerialisedUpdateObject(metadata, batchIdentifier, fieldsToUpdate, existingFileName);
            await RunStoredProcedureAsync<string>(CosmosDbStoredProcedureNames.UpdateDocumentMetadata, serialisedUpdateObject);
        }

        /// <inheritdoc />
        public Task<int> UpdateDocumentMetadataAndVersion(FileMetadata metadata, string batchIdentifier, IEnumerable<string> fieldsToUpdate, string existingFileName = null)
        {
            var serialisedUpdateObject = CreateSerialisedUpdateObject(metadata, batchIdentifier, fieldsToUpdate, existingFileName);
            return RunStoredProcedureAsync<int>(CosmosDbStoredProcedureNames.UpdateDocumentMetadataAndVersion, serialisedUpdateObject);
        }

        /// <inheritdoc />
        public async Task AddBatch(BatchMetadata batchMetadata)
        {
            if (batchMetadata.Files == null || !batchMetadata.Files.Any())
            {
                throw new FormatException("The batch needs to have at least one file.");
            }

            await CreateDocument(batchMetadata);
        }

        /// <summary>Ensures all stored procedures up to date asynchronous.</summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task EnsureAllStoredProceduresUpToDate()
        {
            await EnsureAllStoredProceduresAreUpToDate();
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<DocumentWithViewedStatus>> ReceivedFilesForAgencyWithSeenHistory(IEnumerable<Product> allowedProducts)
        {
            var productIds = allowedProducts.Select(i => i.Identifier.ToString()).ToArray();
            return await RunQueryAsync<DocumentWithViewedStatus>(nameof(ReceivedFilesForAgencyWithSeenHistory), ("@allowedProducts", productIds));
        }

        /// <inheritdoc/>
        public async Task AddBatchNotificationSummary(BatchNotificationSummary batchNotificationSummary)
        {
            await CreateDocument(batchNotificationSummary);
        }

        /// <inheritdoc/>
        public async Task<FileMetadata> GetFile(DocumentReference documentReference)
        {
            var files = await RunQueryAsync<FileMetadata>(
                CosmosDbStoredProcedureNames.GetFile,
                ("@parentBatchId", documentReference.ParentBatchIdentifier),
                ("@batchId", documentReference.BatchIdentifier),
                ("@fileName", documentReference.FileName));

            var file = files.SingleOrDefault();
            return file;
        }

        /// <inheritdoc/>
        public Task<ListResult<PublishedBatch>> GetPublishedBatches(int pageNumber, int pageSize)
            => RunStoredProcedureAsync<ListResult<PublishedBatch>>(CosmosDbStoredProcedureNames.GetPublishedBatches, pageNumber, pageSize);

        /// <inheritdoc/>
        public Task<IEnumerable<NotificationRecipient>> GetNotificationRecipientsByParentBatch(Guid parentBatchId)
            => RunStoredProcedureAsync<IEnumerable<NotificationRecipient>>(CosmosDbStoredProcedureNames.GetNotificationRecipientsByParentBatch, parentBatchId);


        /// <inheritdoc/>
        public Task<IEnumerable<BatchMetadata>> GetAgencyDeleteFilesInfo(int ukprn, string fileType, string year)
            => RunQueryAsync<BatchMetadata>(
                CosmosDbStoredProcedureNames.GetAgencyDeleteFilesInfo,
                ("@ukprn", ukprn),
                ("@fileType", fileType),
                ("@year", year));

        /// <inheritdoc/>
        public Task<IEnumerable<BatchMetadata>> GetOrganisationDeleteFilesInfo(int ukprn, string fileType)
           => RunQueryAsync<BatchMetadata>(
                CosmosDbStoredProcedureNames.GetOrganisationDeleteFilesInfo,
                ("@ukprn", ukprn),
                ("@fileType", fileType));

        /// <inheritdoc/>
        public Task AddSpiRefreshLog(SpiRefreshLog log)
            => CreateDocument(log);

        /// <inheritdoc/>
        public Task<IEnumerable<SpiRefreshLog>> GetSpiRefreshLogs()
            => RunQueryAsync<SpiRefreshLog>(nameof(GetSpiRefreshLogs));

        private string CreateSerialisedUpdateObject(FileMetadata metadata, string batchIdentifier, IEnumerable<string> fieldsToUpdate, string existingFileName = null)
        {
            var updateObject = new
            {
                BatchId = batchIdentifier,
                Metadata = metadata,
                FieldsToUpdate = fieldsToUpdate,
                ExistingFileName = existingFileName ?? metadata.FileName
            };

            return JsonConvert.SerializeObject(updateObject);
        }
    }
}