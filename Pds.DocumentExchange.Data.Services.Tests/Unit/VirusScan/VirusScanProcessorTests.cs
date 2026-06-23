using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pds.Core.Logging;
using Pds.DocumentExchange.Data.Services.DTOs;
using Pds.DocumentExchange.Data.Services.DTOs.Configuration;
using Pds.DocumentExchange.Data.Services.Enums;
using Pds.DocumentExchange.Data.Services.Exceptions;
using Pds.DocumentExchange.Data.Services.Implementations.VirusScan;
using Pds.DocumentExchange.Data.Services.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Tests.Unit.VirusScan
{
    [TestClass]
    public class VirusScanProcessorTests
    {
        private readonly Mock<ICosmosDbService> _cosmosDbService = new Mock<ICosmosDbService>(MockBehavior.Strict);
        private readonly Mock<IVirusScanner> _virusScanner = new Mock<IVirusScanner>(MockBehavior.Strict);
        private readonly Mock<IServiceBusQueueManager> _queueManager = new Mock<IServiceBusQueueManager>(MockBehavior.Strict);
        private readonly Mock<IPagingService> _pagingService = new Mock<IPagingService>(MockBehavior.Strict);
        private readonly Mock<ILoggerAdapter<VirusScanProcessor>> _logger = new Mock<ILoggerAdapter<VirusScanProcessor>>();

        private readonly VirusScanProcessor _virusScanProcessor;

        private readonly VirusScanProcessorConfiguration _config = new VirusScanProcessorConfiguration
        {
            MaxBatchSize = 5,
            SuccessfulScanQueueName = "successful-queue",
            VirusFoundScanQueueName = "virus-queue"
        };

        public VirusScanProcessorTests()
        {
            _virusScanProcessor = new VirusScanProcessor(
                _cosmosDbService.Object,
                _virusScanner.Object,
                _queueManager.Object,
                _pagingService.Object,
                _config,
                _logger.Object);
        }

        #region RunVirusScanAgency

        [TestMethod, TestCategory("Unit")]
        public async Task RunVirusScanAgency_WhenFileHasNoVirus_PushesMessageToSuccessQueue()
        {
            // Arrange
            var batchIdentifier = "batch-id";

            var fileMetadata = new FileMetadata
            {
                FileName = "file-name.pdf",
                ProductIdentifier = "1000"
            };

            var batchMetadata = new BatchMetadata
            {
                Id = batchIdentifier,
                ParentBatchIdentifier = batchIdentifier,
                Files = new[] { fileMetadata }
            };

            var batchMetadataList = new[] { batchMetadata };

            var pagedExchangeDocuments = new List<IEnumerable<FileMetadata>>
            {
                batchMetadata.Files
            };

            var scannedFileInfo = new ScannedFileInfo
            {
                BatchIdentifier = batchMetadata.Id,
                ParentBatchIdentifier = batchMetadata.ParentBatchIdentifier,
                DocumentDirection = ExchangeDocumentDirection.PublishedByAgency,
                FileInfo = fileMetadata,
                ScanResult = FileSafety.Okay
            };

            var scannedValuesAsync = ToAsyncEnumerable(new[] { scannedFileInfo });

            _cosmosDbService
                .Setup(db => db.GetBatchesByParent(batchIdentifier))
                .ReturnsAsync(batchMetadataList);

            _pagingService
                .Setup(pagingService => pagingService.Paginate(batchMetadata.Files, _config.MaxBatchSize))
                .Returns(pagedExchangeDocuments);

            _virusScanner
                .Setup(virusScanner => virusScanner.ScanFiles(
                    batchMetadata.Files,
                    batchMetadata.Id,
                    batchMetadata.ParentBatchIdentifier,
                    ExchangeDocumentDirection.PublishedByAgency))
                .Returns(scannedValuesAsync);

            _queueManager
                .Setup(queueManager => queueManager.GetQueue(_config.SuccessfulScanQueueName))
                .Returns(Mock.Of<IJsonMessageServiceBusQueue>());

            // Act
            await _virusScanProcessor.RunVirusScanAgency(batchIdentifier);

            // Assert
            _queueManager
                .Verify(queueManager => queueManager.GetQueue(_config.SuccessfulScanQueueName), Times.Once);
        }

        [TestMethod, TestCategory("Unit")]
        [ExpectedException(typeof(FileSafetyInternalException), "File safety internal error occured on the batch batch-id.")]
        public async Task RunVirusScanAgency_WhenFileHasInternalError_ThrowsException()
        {
            // Arrange
            var batchIdentifier = "batch-id";

            var fileMetadata = new FileMetadata
            {
                FileName = "file-name.pdf",
                ProductIdentifier = "1000"
            };

            var batchMetadata = new BatchMetadata
            {
                Id = batchIdentifier,
                ParentBatchIdentifier = batchIdentifier,
                Files = new[] { fileMetadata }
            };

            var batchMetadataList = new[] { batchMetadata };

            var pagedExchangeDocuments = new List<IEnumerable<FileMetadata>>
            {
                batchMetadata.Files
            };

            var scannedFileInfo = new ScannedFileInfo
            {
                BatchIdentifier = batchMetadata.Id,
                ParentBatchIdentifier = batchMetadata.ParentBatchIdentifier,
                DocumentDirection = ExchangeDocumentDirection.PublishedByAgency,
                FileInfo = fileMetadata,
                ScanResult = FileSafety.InternalError
            };

            var scannedValuesAsync = ToAsyncEnumerable(new[] { scannedFileInfo });

            _cosmosDbService
                .Setup(db => db.GetBatchesByParent(batchIdentifier))
                .ReturnsAsync(batchMetadataList);

            _pagingService
                .Setup(pagingService => pagingService.Paginate(batchMetadata.Files, _config.MaxBatchSize))
                .Returns(pagedExchangeDocuments);

            _virusScanner
                .Setup(virusScanner => virusScanner.ScanFiles(
                    batchMetadata.Files,
                    batchMetadata.Id,
                    batchMetadata.ParentBatchIdentifier,
                    ExchangeDocumentDirection.PublishedByAgency))
                .Returns(scannedValuesAsync);

            // Act
            await _virusScanProcessor.RunVirusScanAgency(batchIdentifier);
        }

        [TestMethod, TestCategory("Unit")]
        public async Task RunVirusScanAgency_WhenFileHasVirus_PushesMessageToVirusQueue()
        {
            // Arrange
            var batchIdentifier = "batch-id";

            var fileMetadata = new FileMetadata
            {
                FileName = "file-name.pdf",
                ProductIdentifier = "1000"
            };

            var batchMetadata = new BatchMetadata
            {
                Id = batchIdentifier,
                ParentBatchIdentifier = batchIdentifier,
                Files = new[] { fileMetadata }
            };

            var batchMetadataList = new[] { batchMetadata };

            var pagedExchangeDocuments = new List<IEnumerable<FileMetadata>>
            {
                batchMetadata.Files
            };

            var scannedFileInfo = new ScannedFileInfo
            {
                BatchIdentifier = batchMetadata.Id,
                ParentBatchIdentifier = batchMetadata.ParentBatchIdentifier,
                DocumentDirection = ExchangeDocumentDirection.PublishedByAgency,
                FileInfo = fileMetadata,
                ScanResult = FileSafety.Virus
            };

            var scannedValuesAsync = ToAsyncEnumerable(new[] { scannedFileInfo });

            _cosmosDbService
                .Setup(db => db.GetBatchesByParent(batchIdentifier))
                .ReturnsAsync(batchMetadataList);

            _pagingService
                .Setup(pagingService => pagingService.Paginate(batchMetadata.Files, _config.MaxBatchSize))
                .Returns(pagedExchangeDocuments);

            _virusScanner
                .Setup(virusScanner => virusScanner.ScanFiles(
                    batchMetadata.Files,
                    batchMetadata.Id,
                    batchMetadata.ParentBatchIdentifier,
                    ExchangeDocumentDirection.PublishedByAgency))
                .Returns(scannedValuesAsync);

            _queueManager
                .Setup(queueManager => queueManager.GetQueue(_config.VirusFoundScanQueueName))
                .Returns(Mock.Of<IJsonMessageServiceBusQueue>());

            // Act
            await _virusScanProcessor.RunVirusScanAgency(batchIdentifier);

            // Assert
            _queueManager
                .Verify(queueManager => queueManager.GetQueue(_config.VirusFoundScanQueueName), Times.Once);
        }

        #endregion


        #region RunVirusScanOrganisation

        [TestMethod, TestCategory("Unit")]
        public async Task RunVirusScanOrganisation_WhenFileHasNoVirus_PushesMessageToSuccessQueue()
        {
            // Arrange
            var batchIdentifier = "batch-id";

            var fileMetadata = new FileMetadata
            {
                FileName = "file-name.pdf",
                ProductIdentifier = "1000"
            };

            var batchMetadata = new BatchMetadata
            {
                Id = batchIdentifier,
                ParentBatchIdentifier = batchIdentifier,
                Files = new[] { fileMetadata }
            };

            var batchMetadataList = new[] { batchMetadata };

            var pagedExchangeDocuments = new List<IEnumerable<FileMetadata>>
            {
                batchMetadata.Files
            };

            var scannedFileInfo = new ScannedFileInfo
            {
                BatchIdentifier = batchMetadata.Id,
                ParentBatchIdentifier = batchMetadata.ParentBatchIdentifier,
                DocumentDirection = ExchangeDocumentDirection.SentByOrganisation,
                FileInfo = fileMetadata,
                ScanResult = FileSafety.Okay
            };

            var scannedValuesAsync = ToAsyncEnumerable(new[] { scannedFileInfo });

            _cosmosDbService
                .Setup(db => db.GetBatchesByParent(batchIdentifier))
                .ReturnsAsync(batchMetadataList);

            _pagingService
                .Setup(pagingService => pagingService.Paginate(batchMetadata.Files, _config.MaxBatchSize))
                .Returns(pagedExchangeDocuments);

            _virusScanner
                .Setup(virusScanner => virusScanner.ScanFiles(
                    batchMetadata.Files,
                    batchMetadata.Id,
                    batchMetadata.ParentBatchIdentifier,
                    ExchangeDocumentDirection.SentByOrganisation))
                .Returns(scannedValuesAsync);

            _queueManager
                .Setup(queueManager => queueManager.GetQueue(_config.SuccessfulScanQueueName))
                .Returns(Mock.Of<IJsonMessageServiceBusQueue>());

            // Act
            await _virusScanProcessor.RunVirusScanOrganisation(batchIdentifier);

            // Assert
            _queueManager
                .Verify(queueManager => queueManager.GetQueue(_config.SuccessfulScanQueueName), Times.Once);
        }

        [TestMethod, TestCategory("Unit")]
        [ExpectedException(typeof(FileSafetyInternalException), "File safety internal error occured on the batch batch-id.")]
        public async Task RunVirusScanOrganisation_WhenFileHasInternalError_ThrowsException()
        {
            // Arrange
            var batchIdentifier = "batch-id";

            var fileMetadata = new FileMetadata
            {
                FileName = "file-name.pdf",
                ProductIdentifier = "1000"
            };

            var batchMetadata = new BatchMetadata
            {
                Id = batchIdentifier,
                ParentBatchIdentifier = batchIdentifier,
                Files = new[] { fileMetadata }
            };

            var batchMetadataList = new[] { batchMetadata };

            var pagedExchangeDocuments = new List<IEnumerable<FileMetadata>>
            {
                batchMetadata.Files
            };

            var scannedFileInfo = new ScannedFileInfo
            {
                BatchIdentifier = batchMetadata.Id,
                ParentBatchIdentifier = batchMetadata.ParentBatchIdentifier,
                DocumentDirection = ExchangeDocumentDirection.SentByOrganisation,
                FileInfo = fileMetadata,
                ScanResult = FileSafety.InternalError
            };

            var scannedValuesAsync = ToAsyncEnumerable(new[] { scannedFileInfo });

            _cosmosDbService
                .Setup(db => db.GetBatchesByParent(batchIdentifier))
                .ReturnsAsync(batchMetadataList);

            _pagingService
                .Setup(pagingService => pagingService.Paginate(batchMetadata.Files, _config.MaxBatchSize))
                .Returns(pagedExchangeDocuments);

            _virusScanner
                .Setup(virusScanner => virusScanner.ScanFiles(
                    batchMetadata.Files,
                    batchMetadata.Id,
                    batchMetadata.ParentBatchIdentifier,
                    ExchangeDocumentDirection.SentByOrganisation))
                .Returns(scannedValuesAsync);

            // Act
            await _virusScanProcessor.RunVirusScanOrganisation(batchIdentifier);
        }

        [TestMethod, TestCategory("Unit")]
        public async Task RunVirusScanOrganisation_WhenFileHasVirus_PushesMessageToVirusQueue()
        {
            // Arrange
            var batchIdentifier = "batch-id";

            var fileMetadata = new FileMetadata
            {
                FileName = "file-name.pdf",
                ProductIdentifier = "1000"
            };

            var batchMetadata = new BatchMetadata
            {
                Id = batchIdentifier,
                ParentBatchIdentifier = batchIdentifier,
                Files = new[] { fileMetadata }
            };

            var batchMetadataList = new[] { batchMetadata };

            var pagedExchangeDocuments = new List<IEnumerable<FileMetadata>>
            {
                batchMetadata.Files
            };

            var scannedFileInfo = new ScannedFileInfo
            {
                BatchIdentifier = batchMetadata.Id,
                ParentBatchIdentifier = batchMetadata.ParentBatchIdentifier,
                DocumentDirection = ExchangeDocumentDirection.SentByOrganisation,
                FileInfo = fileMetadata,
                ScanResult = FileSafety.Virus
            };

            var scannedValuesAsync = ToAsyncEnumerable(new[] { scannedFileInfo });

            _cosmosDbService
                .Setup(db => db.GetBatchesByParent(batchIdentifier))
                .ReturnsAsync(batchMetadataList);

            _pagingService
                .Setup(pagingService => pagingService.Paginate(batchMetadata.Files, _config.MaxBatchSize))
                .Returns(pagedExchangeDocuments);

            _virusScanner
                .Setup(virusScanner => virusScanner.ScanFiles(
                    batchMetadata.Files,
                    batchMetadata.Id,
                    batchMetadata.ParentBatchIdentifier,
                    ExchangeDocumentDirection.SentByOrganisation))
                .Returns(scannedValuesAsync);

            _queueManager
                .Setup(queueManager => queueManager.GetQueue(_config.VirusFoundScanQueueName))
                .Returns(Mock.Of<IJsonMessageServiceBusQueue>());

            // Act
            await _virusScanProcessor.RunVirusScanOrganisation(batchIdentifier);

            // Assert
            _queueManager
                .Verify(queueManager => queueManager.GetQueue(_config.VirusFoundScanQueueName), Times.Once);
        }

        #endregion

        private async IAsyncEnumerable<T> ToAsyncEnumerable<T>(IEnumerable<T> values)
        {
            foreach (var currentValue in values)
            {
                yield return currentValue;
            }

            await Task.CompletedTask;
        }
    }
}