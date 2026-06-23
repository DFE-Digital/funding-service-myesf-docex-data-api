using Pds.Core.DistributedLocks.Interfaces;
using Pds.Core.Logging;
using Pds.Core.Utils;
using Pds.DocumentExchange.Data.Services.DTOs;
using Pds.DocumentExchange.Data.Services.DTOs.Configuration;
using Pds.DocumentExchange.Data.Services.Enums;
using Pds.DocumentExchange.Data.Services.Interfaces;
using Pds.DocumentExchange.Data.Services.Interfaces.Caching;
using Pds.DocumentExchange.Data.Services.Interfaces.Storage;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Implementations
{
    /// <summary>
    /// Service to process virus scan results.
    /// </summary>
    public class VirusScanResultProcessor : IVirusScanResultProcessor
    {
        private readonly IDirectoriesManager _directoriesManager;
        private readonly IServiceBusQueueManager _serviceQueueManager;
        private readonly ICosmosDbService _cosmosDbService;
        private readonly ISystemProvider _systemProvider;
        private readonly ICacheManager _cacheManager;
        private readonly IDistributedLocksService _distributedLocksService;
        private readonly ILoggerAdapter<VirusScanResultProcessor> _logger;

        private readonly string _agencySourceDirectory;
        private readonly string _agencyDestinationDirectory;
        private readonly string _organisationSourceDirectory;
        private readonly string _organisationDestinationDirectory;
        private readonly string _fileCopyDestinationDirectory;

        private readonly string _readyForEmailQueue;

        /// <summary>
        /// Initializes a new instance of the <see cref="VirusScanResultProcessor"/> class.
        /// </summary>
        /// <param name="cosmosDbService">The CosmosDb service.</param>
        /// <param name="directoriesManager">The directories manager.</param>
        /// <param name="serviceQueueManager">The service queue manager.</param>
        /// <param name="systemProvider">The system utilities.</param>
        /// <param name="cacheManager">The cache manager.</param>
        /// <param name="distributedLocksService">The distributed locks service.</param>
        /// <param name="configuration">The configuration.</param>
        /// <param name="logger">The logger.</param>
        public VirusScanResultProcessor(
            ICosmosDbService cosmosDbService,
            IDirectoriesManager directoriesManager,
            IServiceBusQueueManager serviceQueueManager,
            ISystemProvider systemProvider,
            ICacheManager cacheManager,
            IDistributedLocksService distributedLocksService,
            VirusScanResultProcessorConfiguration configuration,
            ILoggerAdapter<VirusScanResultProcessor> logger)
        {
            _cosmosDbService = cosmosDbService;
            _directoriesManager = directoriesManager;
            _serviceQueueManager = serviceQueueManager;
            _systemProvider = systemProvider;
            _cacheManager = cacheManager;
            _distributedLocksService = distributedLocksService;
            _logger = logger;

            _agencySourceDirectory = configuration.AgencySourceDirectoryKey;
            _agencyDestinationDirectory = configuration.AgencyDestinationDirectoryKey;
            _organisationSourceDirectory = configuration.OrganisationSourceDirectoryKey;
            _organisationDestinationDirectory = configuration.OrganisationDestinationDirectoryKey;
            _fileCopyDestinationDirectory = configuration.FileCopyDestinationDirectoryKey;

            _readyForEmailQueue = configuration.ReadyForEmailQueue;
        }

        /// <inheritdoc/>
        public async Task<FileMetadata> ProcessAgencyFileResultOk(DocumentReference documentReference)
            => await ProcessFileResultOk(
                documentReference,
                await _directoriesManager.GetDirectory(_agencySourceDirectory),
                await _directoriesManager.GetDirectory(_agencyDestinationDirectory),
                PushMessageIfAgencyBatchCompleted);

        /// <inheritdoc/>
        public async Task<FileMetadata> ProcessOrganisationFileResultOk(DocumentReference documentReference)
            => await ProcessFileResultOk(
                documentReference,
                await _directoriesManager.GetDirectory(_organisationSourceDirectory),
                await _directoriesManager.GetDirectory(_organisationDestinationDirectory),
                PushMessageIfOrganisationBatchCompleted);

        /// <inheritdoc/>
        public async Task<FileMetadata> ProcessAgencyFileResultVirusFound(DocumentReference documentReference)
        {
            return await ProcessFileResultVirusFound(
                documentReference,
                await _directoriesManager.GetDirectory(_agencySourceDirectory),
                PushMessageIfAgencyBatchCompleted);
        }

        /// <inheritdoc/>
        public async Task<FileMetadata> ProcessOrganisationFileResultVirusFound(DocumentReference documentReference)
        {
            return await ProcessFileResultVirusFound(
                documentReference,
                await _directoriesManager.GetDirectory(_organisationSourceDirectory),
                PushMessageIfOrganisationBatchCompleted);
        }

        private async Task<FileMetadata> ProcessFileResultOk(
            DocumentReference documentReference,
            IDirectory sourceDirectory,
            IDirectory destinationDirectory,
            Func<string, Task> pushMessageToQueueIfBatchCompleted)
        {
            var fileHistory = new List<FileMetadataHistory>();

            var fileInfo = new FileMetadata
            {
                FileName = documentReference.FileName,
                Processed = true,
                VirusScanSuccessful = true,
                History = fileHistory
            };

            try
            {
                fileHistory.Add(CreateMetadataHistory(FileAction.FileScanGood));
                fileHistory.Add(CreateMetadataHistory(FileAction.Copying));

                var copyDestinationDirectory = await _directoriesManager.GetDirectory(_fileCopyDestinationDirectory);
                var copiedFileName = await sourceDirectory.Copy(copyDestinationDirectory, documentReference.FileName);

                fileHistory.Add(CreateMetadataHistory(FileAction.Moving));
                await sourceDirectory.Move(destinationDirectory, documentReference.FileName);

                fileInfo.Version = await UpdateFileMetadata(documentReference.BatchIdentifier, fileInfo, copiedFileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error ocurred when processing the virus scan result for file {documentReference.FileName}");
                fileHistory.Add(CreateMetadataHistory(FileAction.Error));
            }
            finally
            {
                await _cosmosDbService.AddHistoryToFileMetadata(documentReference.BatchIdentifier, fileInfo);
                await pushMessageToQueueIfBatchCompleted(documentReference.ParentBatchIdentifier);
            }

            return fileInfo;
        }

        private async Task<FileMetadata> ProcessFileResultVirusFound(
           DocumentReference documentReference,
           IDirectory sourceDirectory,
           Func<string, Task> pushMessageToQueueIfBatchCompleted)
        {
            var fileHistory = new List<FileMetadataHistory>();

            var fileInfo = new FileMetadata
            {
                FileName = documentReference.FileName,
                Processed = true,
                VirusScanSuccessful = false,
                History = fileHistory
            };

            try
            {
                fileHistory.Add(CreateMetadataHistory(FileAction.FileScanBad));
                fileHistory.Add(CreateMetadataHistory(FileAction.Deleting));

                await sourceDirectory.Delete(documentReference.FileName);

                fileInfo.Version = await UpdateFileMetadata(documentReference.BatchIdentifier, fileInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error ocurred when processing the virus scan result for file {documentReference.FileName}");
                fileHistory.Add(CreateMetadataHistory(FileAction.Error));
            }
            finally
            {
                await _cosmosDbService.AddHistoryToFileMetadata(documentReference.BatchIdentifier, fileInfo);
                await pushMessageToQueueIfBatchCompleted(documentReference.ParentBatchIdentifier);
            }

            return fileInfo;
        }

        private FileMetadataHistory CreateMetadataHistory(FileAction fileAction)
            => new FileMetadataHistory
            {
                ActionDateTimeUtc = _systemProvider.DateTime.UtcNow(),
                Action = fileAction,
                User = null
            };

        private Task<int> UpdateFileMetadata(string batchIdentifier, FileMetadata fileInfo, string copiedFileName = null)
        {
            var fieldsToUpdate = new List<string> { "Processed", "Okay" };

            var originalFileName = fileInfo.FileName;

            if (!string.IsNullOrEmpty(copiedFileName))
            {
                var copiedFileNameUpdated = !originalFileName.Equals(copiedFileName, StringComparison.InvariantCultureIgnoreCase);
                if (copiedFileNameUpdated)
                {
                    fileInfo.FileName = copiedFileName;
                    fieldsToUpdate.Add("Filename");
                }
            }

            return _cosmosDbService.UpdateDocumentMetadataAndVersion(fileInfo, batchIdentifier, fieldsToUpdate, originalFileName);
        }

        private async Task PushMessageIfAgencyBatchCompleted(string parentBatchIdentifier)
            => await PushMessageIfBatchCompleted(parentBatchIdentifier, ExchangeDocumentDirection.PublishedByAgency);

        private async Task PushMessageIfOrganisationBatchCompleted(string parentBatchIdentifier)
            => await PushMessageIfBatchCompleted(parentBatchIdentifier, ExchangeDocumentDirection.SentByOrganisation);

        private async Task PushMessageIfBatchCompleted(string parentBatchIdentifier, ExchangeDocumentDirection documentDirection)
        {
            var unprocessedFiles = await _cosmosDbService.GetCountOfUnprocessedFiles(parentBatchIdentifier);

            if (unprocessedFiles == 0)
            {
                var resourceKey = _cacheManager.CacheKeyBuilder.BuildEmailResourceLockKey(parentBatchIdentifier);
                var lockValue = Guid.NewGuid().ToString();

                if (await _distributedLocksService.TryAcquireLock(resourceKey, lockValue, TimeSpan.FromDays(1)))
                {
                    _logger.LogInformation($"Lock {resourceKey} acquired with value {lockValue}.");

                    var newMessage = new ScannedBatchQueueMessage
                    {
                        ParentBatchIdentifier = parentBatchIdentifier,
                        DocumentDirection = documentDirection
                    };

                    var queue = _serviceQueueManager.GetQueue(_readyForEmailQueue);
                    await queue.PushMessage(newMessage);
                }
            }
        }
    }
}