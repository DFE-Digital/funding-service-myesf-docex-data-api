using Pds.Core.Logging;
using Pds.DocumentExchange.Data.Services.DTOs;
using Pds.DocumentExchange.Data.Services.DTOs.Configuration;
using Pds.DocumentExchange.Data.Services.Enums;
using Pds.DocumentExchange.Data.Services.Exceptions;
using Pds.DocumentExchange.Data.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Implementations.VirusScan
{
    /// <summary>
    /// Service to send files to be scanned by an antivirus.
    /// </summary>
    public class VirusScanProcessor : IVirusScanProcessor
    {
        private readonly ICosmosDbService _cosmosDbService;
        private readonly IVirusScanner _virusScanner;
        private readonly IServiceBusQueueManager _queueManager;
        private readonly IPagingService _pagingService;
        private readonly ILoggerAdapter<VirusScanProcessor> _logger;

        private readonly int _maxScanBatchSize;
        private readonly string _successfulScanQueueName;
        private readonly string _virusFoundScanQueueName;

        /// <summary>
        /// Initializes a new instance of the <see cref="VirusScanProcessor"/> class.
        /// </summary>
        /// <param name="cosmosDbService">The cosmosDb service.</param>
        /// <param name="virusScanner">The antivirus.</param>
        /// <param name="queueManager">The queueManager.</param>
        /// <param name="pagingService">Paging service.</param>
        /// <param name="configuration">The configuration.</param>
        /// <param name="logger">The logger.</param>
        public VirusScanProcessor(
            ICosmosDbService cosmosDbService,
            IVirusScanner virusScanner,
            IServiceBusQueueManager queueManager,
            IPagingService pagingService,
            VirusScanProcessorConfiguration configuration,
            ILoggerAdapter<VirusScanProcessor> logger)
        {
            _cosmosDbService = cosmosDbService;
            _virusScanner = virusScanner;
            _queueManager = queueManager;
            _pagingService = pagingService;
            _logger = logger;

            _maxScanBatchSize = configuration.MaxBatchSize;
            _successfulScanQueueName = configuration.SuccessfulScanQueueName;
            _virusFoundScanQueueName = configuration.VirusFoundScanQueueName;
        }

        /// <inheritdoc/>
        public async Task RunVirusScanOrganisation(string batchIdentifier)
            => await RunVirusScan(batchIdentifier, ExchangeDocumentDirection.SentByOrganisation);

        /// <inheritdoc/>
        public async Task RunVirusScanAgency(string batchIdentifier)
            => await RunVirusScan(batchIdentifier, ExchangeDocumentDirection.PublishedByAgency);

        private async Task RunVirusScan(string batchIdentifier, ExchangeDocumentDirection documentDirection)
        {
            var scanSummary = Enum
               .GetValues(typeof(FileSafety))
               .Cast<FileSafety>()
               .ToDictionary(fs => fs, fs => 0);

            await foreach (var currentScannedFileInfo in ScanBatch(batchIdentifier, documentDirection))
            {
                try
                {
                    scanSummary[currentScannedFileInfo.ScanResult]++;
                    var selectedQueue = SelectQueue(currentScannedFileInfo.ScanResult);
                    if (selectedQueue != null)
                    {
                        var message = CreateQueueMessage(currentScannedFileInfo);
                        await selectedQueue.PushMessage(message);

                        _logger.LogInformation($"Scanned file {currentScannedFileInfo.FileInfo.FileName} sent to queue {selectedQueue.QueueName}");
                    }
                    else if (currentScannedFileInfo.ScanResult == FileSafety.AlreadyScanned)
                    {
                        // Nothing to do.
                    }
                    else
                    {
                        _logger.LogInformation($"Unhandled virus scan result ({currentScannedFileInfo.ScanResult}) for file {currentScannedFileInfo.FileInfo.FileName}");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"An error ocurred when processing the file {currentScannedFileInfo.FileInfo.FileName}");
                }
            }

            if (scanSummary[FileSafety.InternalError] > 0)
            {
                _logger.LogError($"Finished virus scans for parent batch {batchIdentifier}. " +
                  $"File Safety Internal Error: {scanSummary[FileSafety.InternalError]}");

                throw new FileSafetyInternalException($"File safety internal error occured on the batch {batchIdentifier}.");
            }
            else
            {
                _logger.LogInformation(
                  $"Finished virus scans for parent batch {batchIdentifier}. " +
                  $"Results: {string.Join(", ", scanSummary.Select(s => $"{s.Key}: {s.Value}"))}");
            }
        }

        private async IAsyncEnumerable<ScannedFileInfo> ScanBatch(string batchIdentifier, ExchangeDocumentDirection documentDirection)
        {
            var batches = await _cosmosDbService.GetBatchesByParent(batchIdentifier);

            foreach (var currentBatch in batches)
            {
                var pagedFiles = _pagingService.Paginate(currentBatch.Files, _maxScanBatchSize);
                foreach (var currentFilesGroup in pagedFiles)
                {
                    await foreach (var item in _virusScanner.ScanFiles(
                        currentFilesGroup,
                        currentBatch.Id,
                        currentBatch.ParentBatchIdentifier,
                        documentDirection))
                    {
                        yield return item;
                    }
                }
            }
        }

        private IJsonMessageServiceBusQueue SelectQueue(FileSafety fileSafety)
        {
            switch (fileSafety)
            {
                case FileSafety.Okay:
                    return _queueManager.GetQueue(_successfulScanQueueName);

                case FileSafety.Virus:
                    return _queueManager.GetQueue(_virusFoundScanQueueName);

                default:
                    return null;
            }
        }

        private ScannedFileQueueMessage CreateQueueMessage(ScannedFileInfo scannedFileInfo)
        {
            return new ScannedFileQueueMessage
            {
                BatchIdentifier = scannedFileInfo.BatchIdentifier,
                ParentBatchIdentifier = scannedFileInfo.ParentBatchIdentifier,
                FileName = scannedFileInfo.FileInfo.FileName,
                DocumentDirection = scannedFileInfo.DocumentDirection
            };
        }
    }
}