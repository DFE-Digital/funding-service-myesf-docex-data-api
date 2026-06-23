using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pds.Core.DistributedLocks.Interfaces;
using Pds.Core.Logging;
using Pds.Core.Utils;
using Pds.DocumentExchange.Data.Services.DTOs;
using Pds.DocumentExchange.Data.Services.DTOs.Configuration;
using Pds.DocumentExchange.Data.Services.Enums;
using Pds.DocumentExchange.Data.Services.Implementations;
using Pds.DocumentExchange.Data.Services.Interfaces;
using Pds.DocumentExchange.Data.Services.Interfaces.Caching;
using Pds.DocumentExchange.Data.Services.Interfaces.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Tests.Unit.VirusScan
{
    [TestClass]
    public class VirusScanResultProcessorTests
    {
        private readonly Mock<ICosmosDbService> _mockCosmosDbService = new Mock<ICosmosDbService>(MockBehavior.Strict);
        private readonly Mock<IDirectoriesManager> _mockDirectoriesManager = new Mock<IDirectoriesManager>(MockBehavior.Strict);
        private readonly Mock<IServiceBusQueueManager> _mockServiceQueueManager = new Mock<IServiceBusQueueManager>(MockBehavior.Strict);
        private readonly Mock<ISystemProvider> _mockSystemProvider = new Mock<ISystemProvider>(MockBehavior.Strict);
        private readonly Mock<ICacheManager> _mockCacheManager = new Mock<ICacheManager>(MockBehavior.Strict);
        private readonly Mock<IDistributedLocksService> _mockDistributedLocksService = new Mock<IDistributedLocksService>(MockBehavior.Strict);
        private readonly Mock<ILoggerAdapter<VirusScanResultProcessor>> _mockLogger = new Mock<ILoggerAdapter<VirusScanResultProcessor>>();

        private readonly Mock<IDirectory> _agencySourceMockDirectory = new Mock<IDirectory>(MockBehavior.Strict);
        private readonly Mock<IDirectory> _agencyDestinationMockDirectory = new Mock<IDirectory>(MockBehavior.Strict);
        private readonly Mock<IDirectory> _organisationSourceMockDirectory = new Mock<IDirectory>(MockBehavior.Strict);
        private readonly Mock<IDirectory> _organisationDestinationMockDirectory = new Mock<IDirectory>(MockBehavior.Strict);
        private readonly Mock<IDirectory> _fileCopyDestinationMockDirectory = new Mock<IDirectory>(MockBehavior.Strict);

        private readonly Mock<IJsonMessageServiceBusQueue> _readyForEmailMockQueue = new Mock<IJsonMessageServiceBusQueue>(MockBehavior.Strict);

        private readonly VirusScanResultProcessor _virusScanResultProcessor;

        public VirusScanResultProcessorTests()
        {
            var configuration = new VirusScanResultProcessorConfiguration
            {
                AgencySourceDirectoryKey = "agency-source-dir-key",
                AgencyDestinationDirectoryKey = "agency-destination-dir-key",
                OrganisationSourceDirectoryKey = "organisation-source-dir-key",
                OrganisationDestinationDirectoryKey = "organisation-destination-dir-key",
                FileCopyDestinationDirectoryKey = "file-copy-destination-dir-key",
                ReadyForEmailQueue = "ready-for-email-queue"
            };

            _mockDirectoriesManager
                .Setup(manager => manager.GetDirectory(configuration.AgencySourceDirectoryKey))
                .ReturnsAsync(_agencySourceMockDirectory.Object);

            _mockDirectoriesManager
                .Setup(manager => manager.GetDirectory(configuration.AgencyDestinationDirectoryKey))
                .ReturnsAsync(_agencyDestinationMockDirectory.Object);

            _mockDirectoriesManager
                 .Setup(manager => manager.GetDirectory(configuration.OrganisationSourceDirectoryKey))
                 .ReturnsAsync(_organisationSourceMockDirectory.Object);

            _mockDirectoriesManager
                .Setup(manager => manager.GetDirectory(configuration.OrganisationDestinationDirectoryKey))
                .ReturnsAsync(_organisationDestinationMockDirectory.Object);

            _mockDirectoriesManager
                .Setup(manager => manager.GetDirectory(configuration.FileCopyDestinationDirectoryKey))
                .ReturnsAsync(_fileCopyDestinationMockDirectory.Object);

            _mockServiceQueueManager
                .Setup(manager => manager.GetQueue(configuration.ReadyForEmailQueue))
                .Returns(_readyForEmailMockQueue.Object);

            _virusScanResultProcessor = new VirusScanResultProcessor(
                _mockCosmosDbService.Object,
                _mockDirectoriesManager.Object,
                _mockServiceQueueManager.Object,
                _mockSystemProvider.Object,
                _mockCacheManager.Object,
                _mockDistributedLocksService.Object,
                configuration,
                _mockLogger.Object);
        }

        #region Agency file result ok

        [TestMethod, TestCategory("Unit")]
        public async Task ProcessAgencyFileResultOk_WhenBatchCompletedAndLockAcquired()
        {
            // Arrange
            var ukprn = 12345678;
            var productType = 10090;
            var year = 201819;

            var batchIdentifier = "batch-identifier";
            var parentBatchIdentifier = "parent-batch-identifier";

            var documentReference = CreateDocumentReference(ukprn, productType, year, batchIdentifier, parentBatchIdentifier);

            var utcNow = new DateTime(2020, 1, 1);

            var expectedFileInfo = new FileMetadata
            {
                FileName = documentReference.FileName,
                Processed = true,
                VirusScanSuccessful = true,
                Version = 2,
                History = CreateMetadataHistoryHistoryScanOk(utcNow)
            };

            SetupSystemProviderDateTimeUtcNow(utcNow);
            SetupAgencySourceDirectoryFileCopyAndMove(documentReference.FileName, documentReference.FileName);
            SetupCosmosDbUpdateFileMetadata(batchIdentifier, expectedFileInfo, documentReference.FileName);
            SetupCosmosDbGetCountOfUnprocessedFiles(parentBatchIdentifier, 0);
            SetupDistributedLock(parentBatchIdentifier, true);
            SetupQueuePushMessage(parentBatchIdentifier, ExchangeDocumentDirection.PublishedByAgency);

            // Act
            var result = await _virusScanResultProcessor.ProcessAgencyFileResultOk(documentReference);

            // Assert
            result.Should().BeEquivalentTo(expectedFileInfo);
        }

        [TestMethod, TestCategory("Unit")]
        public async Task ProcessAgencyFileResultOk_WhenBatchCompletedAndLockNotAcquired()
        {
            // Arrange
            var ukprn = 12345678;
            var productType = 10090;
            var year = 201819;

            var batchIdentifier = "batch-identifier";
            var parentBatchIdentifier = "parent-batch-identifier";

            var documentReference = CreateDocumentReference(ukprn, productType, year, batchIdentifier, parentBatchIdentifier);

            var utcNow = new DateTime(2020, 1, 1);

            var expectedFileInfo = new FileMetadata
            {
                FileName = documentReference.FileName,
                Processed = true,
                VirusScanSuccessful = true,
                Version = 2,
                History = CreateMetadataHistoryHistoryScanOk(utcNow)
            };

            SetupSystemProviderDateTimeUtcNow(utcNow);
            SetupAgencySourceDirectoryFileCopyAndMove(documentReference.FileName, documentReference.FileName);
            SetupCosmosDbUpdateFileMetadata(batchIdentifier, expectedFileInfo, documentReference.FileName);
            SetupCosmosDbGetCountOfUnprocessedFiles(parentBatchIdentifier, 0);
            SetupDistributedLock(parentBatchIdentifier, false);

            // Act
            var result = await _virusScanResultProcessor.ProcessAgencyFileResultOk(documentReference);

            // Assert
            result.Should().BeEquivalentTo(expectedFileInfo);

            _readyForEmailMockQueue
              .Verify(queue => queue.PushMessage(It.IsAny<ScannedBatchQueueMessage>()), Times.Never);
        }

        [TestMethod, TestCategory("Unit")]
        public async Task ProcessAgencyFileResultOk_WhenBatchIncomplete()
        {
            // Arrange
            var ukprn = 12345678;
            var productType = 10090;
            var year = 201819;

            var batchIdentifier = "batch-identifier";
            var parentBatchIdentifier = "parent-batch-identifier";

            var documentReference = CreateDocumentReference(ukprn, productType, year, batchIdentifier, parentBatchIdentifier);

            var utcNow = new DateTime(2020, 1, 1);

            var expectedFileInfo = new FileMetadata
            {
                FileName = documentReference.FileName,
                Processed = true,
                VirusScanSuccessful = true,
                Version = 2,
                History = CreateMetadataHistoryHistoryScanOk(utcNow)
            };

            SetupSystemProviderDateTimeUtcNow(utcNow);
            SetupAgencySourceDirectoryFileCopyAndMove(documentReference.FileName, documentReference.FileName);
            SetupCosmosDbUpdateFileMetadata(batchIdentifier, expectedFileInfo, documentReference.FileName);
            SetupCosmosDbGetCountOfUnprocessedFiles(parentBatchIdentifier, 1);
            SetupDistributedLock(parentBatchIdentifier, true);

            // Act
            var result = await _virusScanResultProcessor.ProcessAgencyFileResultOk(documentReference);

            // Assert
            result.Should().BeEquivalentTo(expectedFileInfo);

            _readyForEmailMockQueue
               .Verify(queue => queue.PushMessage(It.IsAny<ScannedBatchQueueMessage>()), Times.Never);
        }

        [TestMethod, TestCategory("Unit")]
        public async Task ProcessAgencyFileResultOk_WhenBatchCompletedAndFileExistsAndLockAcquired()
        {
            // Arrange
            var ukprn = 12345678;
            var productType = 10090;
            var year = 201819;

            var batchIdentifier = "batch-identifier";
            var parentBatchIdentifier = "parent-batch-identifier";

            var documentReference = CreateDocumentReference(ukprn, productType, year, batchIdentifier, parentBatchIdentifier);
            var copiedFileName = $"{ukprn}_{productType}_{year}-2.pdf";

            var utcNow = new DateTime(2020, 1, 1);

            var expectedFileInfo = new FileMetadata
            {
                FileName = copiedFileName,
                Processed = true,
                VirusScanSuccessful = true,
                Version = 2,
                History = CreateMetadataHistoryHistoryScanOk(utcNow)
            };

            SetupSystemProviderDateTimeUtcNow(utcNow);
            SetupAgencySourceDirectoryFileCopyAndMove(documentReference.FileName, copiedFileName);
            SetupCosmosDbUpdateFileMetadata(batchIdentifier, expectedFileInfo, documentReference.FileName);
            SetupCosmosDbGetCountOfUnprocessedFiles(parentBatchIdentifier, 0);
            SetupDistributedLock(parentBatchIdentifier, true);
            SetupQueuePushMessage(parentBatchIdentifier, ExchangeDocumentDirection.PublishedByAgency);

            // Act
            var result = await _virusScanResultProcessor.ProcessAgencyFileResultOk(documentReference);

            // Assert
            result.Should().BeEquivalentTo(expectedFileInfo);
        }

        [TestMethod, TestCategory("Unit")]
        public async Task ProcessAgencyFileResultOk_WhenBatchCompletedAndFileExistsAndLockNotAcquired()
        {
            // Arrange
            var ukprn = 12345678;
            var productType = 10090;
            var year = 201819;

            var batchIdentifier = "batch-identifier";
            var parentBatchIdentifier = "parent-batch-identifier";

            var documentReference = CreateDocumentReference(ukprn, productType, year, batchIdentifier, parentBatchIdentifier);
            var copiedFileName = $"{ukprn}_{productType}_{year}-2.pdf";

            var utcNow = new DateTime(2020, 1, 1);

            var expectedFileInfo = new FileMetadata
            {
                FileName = copiedFileName,
                Processed = true,
                VirusScanSuccessful = true,
                Version = 2,
                History = CreateMetadataHistoryHistoryScanOk(utcNow)
            };

            SetupSystemProviderDateTimeUtcNow(utcNow);
            SetupAgencySourceDirectoryFileCopyAndMove(documentReference.FileName, copiedFileName);
            SetupCosmosDbUpdateFileMetadata(batchIdentifier, expectedFileInfo, documentReference.FileName);
            SetupCosmosDbGetCountOfUnprocessedFiles(parentBatchIdentifier, 0);
            SetupDistributedLock(parentBatchIdentifier, false);

            // Act
            var result = await _virusScanResultProcessor.ProcessAgencyFileResultOk(documentReference);

            // Assert
            result.Should().BeEquivalentTo(expectedFileInfo);

            _readyForEmailMockQueue
              .Verify(queue => queue.PushMessage(It.IsAny<ScannedBatchQueueMessage>()), Times.Never);
        }

        [TestMethod, TestCategory("Unit")]
        public async Task ProcessAgencyFileResultOk_WhenBatchIncompleteAndFileExists()
        {
            // Arrange
            var ukprn = 12345678;
            var productType = 10090;
            var year = 201819;

            var batchIdentifier = "batch-identifier";
            var parentBatchIdentifier = "parent-batch-identifier";

            var documentReference = CreateDocumentReference(ukprn, productType, year, batchIdentifier, parentBatchIdentifier);
            var copiedFileName = $"{ukprn}_{productType}_{year}-2.pdf";

            var utcNow = new DateTime(2020, 1, 1);

            var expectedFileInfo = new FileMetadata
            {
                FileName = copiedFileName,
                Processed = true,
                VirusScanSuccessful = true,
                Version = 2,
                History = CreateMetadataHistoryHistoryScanOk(utcNow)
            };

            SetupSystemProviderDateTimeUtcNow(utcNow);
            SetupAgencySourceDirectoryFileCopyAndMove(documentReference.FileName, copiedFileName);
            SetupCosmosDbUpdateFileMetadata(batchIdentifier, expectedFileInfo, documentReference.FileName);
            SetupCosmosDbGetCountOfUnprocessedFiles(parentBatchIdentifier, 1);
            SetupDistributedLock(parentBatchIdentifier, true);

            // Act
            var result = await _virusScanResultProcessor.ProcessAgencyFileResultOk(documentReference);

            // Assert
            result.Should().BeEquivalentTo(expectedFileInfo);

            _readyForEmailMockQueue
              .Verify(queue => queue.PushMessage(It.IsAny<ScannedBatchQueueMessage>()), Times.Never);
        }
        #endregion


        #region Agency file result virus found

        [TestMethod, TestCategory("Unit")]
        public async Task ProcessAgencyFileResultVirusFound_WhenBatchCompletedAndLockAcquired()
        {
            // Arrange
            var ukprn = 12345678;
            var productType = 10090;
            var year = 201819;

            var batchIdentifier = "batch-identifier";
            var parentBatchIdentifier = "parent-batch-identifier";

            var documentReference = CreateDocumentReference(ukprn, productType, year, batchIdentifier, parentBatchIdentifier);

            var utcNow = new DateTime(2020, 1, 1);

            var expectedFileInfo = new FileMetadata
            {
                FileName = documentReference.FileName,
                Processed = true,
                VirusScanSuccessful = false,
                Version = 2,
                History = CreateMetadataHistoryHistoryScanVirusFound(utcNow)
            };

            SetupSystemProviderDateTimeUtcNow(utcNow);
            SetupAgencySourceDirectoryDelete(documentReference.FileName);
            SetupCosmosDbUpdateFileMetadata(batchIdentifier, expectedFileInfo, documentReference.FileName);
            SetupCosmosDbGetCountOfUnprocessedFiles(parentBatchIdentifier, 0);
            SetupDistributedLock(parentBatchIdentifier, true);
            SetupQueuePushMessage(parentBatchIdentifier, ExchangeDocumentDirection.PublishedByAgency);

            // Act
            var result = await _virusScanResultProcessor.ProcessAgencyFileResultVirusFound(documentReference);

            // Assert
            result.Should().BeEquivalentTo(expectedFileInfo);
        }

        [TestMethod, TestCategory("Unit")]
        public async Task ProcessAgencyFileResultVirusFound_WhenBatchCompletedAndLockNotAcquired()
        {
            // Arrange
            var ukprn = 12345678;
            var productType = 10090;
            var year = 201819;

            var batchIdentifier = "batch-identifier";
            var parentBatchIdentifier = "parent-batch-identifier";

            var documentReference = CreateDocumentReference(ukprn, productType, year, batchIdentifier, parentBatchIdentifier);

            var utcNow = new DateTime(2020, 1, 1);

            var expectedFileInfo = new FileMetadata
            {
                FileName = documentReference.FileName,
                Processed = true,
                VirusScanSuccessful = false,
                Version = 2,
                History = CreateMetadataHistoryHistoryScanVirusFound(utcNow)
            };

            SetupSystemProviderDateTimeUtcNow(utcNow);
            SetupAgencySourceDirectoryDelete(documentReference.FileName);
            SetupCosmosDbUpdateFileMetadata(batchIdentifier, expectedFileInfo, documentReference.FileName);
            SetupCosmosDbGetCountOfUnprocessedFiles(parentBatchIdentifier, 0);
            SetupDistributedLock(parentBatchIdentifier, false);

            // Act
            var result = await _virusScanResultProcessor.ProcessAgencyFileResultVirusFound(documentReference);

            // Assert
            result.Should().BeEquivalentTo(expectedFileInfo);

            _readyForEmailMockQueue
              .Verify(queue => queue.PushMessage(It.IsAny<ScannedBatchQueueMessage>()), Times.Never);
        }

        [TestMethod, TestCategory("Unit")]
        public async Task ProcessAgencyFileResultVirusFound_WhenBatchIncomplete()
        {
            // Arrange
            var ukprn = 12345678;
            var productType = 10090;
            var year = 201819;

            var batchIdentifier = "batch-identifier";
            var parentBatchIdentifier = "parent-batch-identifier";

            var documentReference = CreateDocumentReference(ukprn, productType, year, batchIdentifier, parentBatchIdentifier);

            var utcNow = new DateTime(2020, 1, 1);

            var expectedFileInfo = new FileMetadata
            {
                FileName = documentReference.FileName,
                Processed = true,
                VirusScanSuccessful = false,
                Version = 2,
                History = CreateMetadataHistoryHistoryScanVirusFound(utcNow)
            };

            SetupSystemProviderDateTimeUtcNow(utcNow);
            SetupAgencySourceDirectoryDelete(documentReference.FileName);
            SetupCosmosDbUpdateFileMetadata(batchIdentifier, expectedFileInfo, documentReference.FileName);
            SetupCosmosDbGetCountOfUnprocessedFiles(parentBatchIdentifier, 1);
            SetupDistributedLock(parentBatchIdentifier, true);
            SetupQueuePushMessage(parentBatchIdentifier, ExchangeDocumentDirection.PublishedByAgency);

            // Act
            var result = await _virusScanResultProcessor.ProcessAgencyFileResultVirusFound(documentReference);

            // Assert
            result.Should().BeEquivalentTo(expectedFileInfo);

            _readyForEmailMockQueue
              .Verify(queue => queue.PushMessage(It.IsAny<ScannedBatchQueueMessage>()), Times.Never);
        }
        #endregion


        #region Organisation file result ok

        [TestMethod, TestCategory("Unit")]
        public async Task ProcessOrganisationFileResultOk_WhenBatchCompletedAndLockAcquired()
        {
            // Arrange
            var ukprn = 12345678;
            var productType = 10090;
            var year = 201819;

            var batchIdentifier = "batch-identifier";
            var parentBatchIdentifier = "parent-batch-identifier";

            var documentReference = CreateDocumentReference(ukprn, productType, year, batchIdentifier, parentBatchIdentifier);

            var utcNow = new DateTime(2020, 1, 1);

            var expectedFileInfo = new FileMetadata
            {
                FileName = documentReference.FileName,
                Processed = true,
                VirusScanSuccessful = true,
                Version = 2,
                History = CreateMetadataHistoryHistoryScanOk(utcNow)
            };

            SetupSystemProviderDateTimeUtcNow(utcNow);
            SetupOrganisationSourceDirectoryFileCopyAndMove(documentReference.FileName, documentReference.FileName);
            SetupCosmosDbUpdateFileMetadata(batchIdentifier, expectedFileInfo, documentReference.FileName);
            SetupCosmosDbGetCountOfUnprocessedFiles(parentBatchIdentifier, 0);
            SetupDistributedLock(parentBatchIdentifier, true);
            SetupQueuePushMessage(parentBatchIdentifier, ExchangeDocumentDirection.SentByOrganisation);

            // Act
            var result = await _virusScanResultProcessor.ProcessOrganisationFileResultOk(documentReference);

            // Assert
            result.Should().BeEquivalentTo(expectedFileInfo);
        }

        [TestMethod, TestCategory("Unit")]
        public async Task ProcessOrganisationFileResultOk_WhenBatchCompletedAndLockNotAcquired()
        {
            // Arrange
            var ukprn = 12345678;
            var productType = 10090;
            var year = 201819;

            var batchIdentifier = "batch-identifier";
            var parentBatchIdentifier = "parent-batch-identifier";

            var documentReference = CreateDocumentReference(ukprn, productType, year, batchIdentifier, parentBatchIdentifier);

            var utcNow = new DateTime(2020, 1, 1);

            var expectedFileInfo = new FileMetadata
            {
                FileName = documentReference.FileName,
                Processed = true,
                VirusScanSuccessful = true,
                Version = 2,
                History = CreateMetadataHistoryHistoryScanOk(utcNow)
            };

            SetupSystemProviderDateTimeUtcNow(utcNow);
            SetupOrganisationSourceDirectoryFileCopyAndMove(documentReference.FileName, documentReference.FileName);
            SetupCosmosDbUpdateFileMetadata(batchIdentifier, expectedFileInfo, documentReference.FileName);
            SetupCosmosDbGetCountOfUnprocessedFiles(parentBatchIdentifier, 0);
            SetupDistributedLock(parentBatchIdentifier, false);

            // Act
            var result = await _virusScanResultProcessor.ProcessOrganisationFileResultOk(documentReference);

            // Assert
            result.Should().BeEquivalentTo(expectedFileInfo);

            _readyForEmailMockQueue
              .Verify(queue => queue.PushMessage(It.IsAny<ScannedBatchQueueMessage>()), Times.Never);
        }

        [TestMethod, TestCategory("Unit")]
        public async Task ProcessOrganisationFileResultOk_WhenBatchIncomplete()
        {
            // Arrange
            var ukprn = 12345678;
            var productType = 10090;
            var year = 201819;

            var batchIdentifier = "batch-identifier";
            var parentBatchIdentifier = "parent-batch-identifier";

            var documentReference = CreateDocumentReference(ukprn, productType, year, batchIdentifier, parentBatchIdentifier);

            var utcNow = new DateTime(2020, 1, 1);

            var expectedFileInfo = new FileMetadata
            {
                FileName = documentReference.FileName,
                Processed = true,
                VirusScanSuccessful = true,
                Version = 2,
                History = CreateMetadataHistoryHistoryScanOk(utcNow)
            };

            SetupSystemProviderDateTimeUtcNow(utcNow);
            SetupOrganisationSourceDirectoryFileCopyAndMove(documentReference.FileName, documentReference.FileName);
            SetupCosmosDbUpdateFileMetadata(batchIdentifier, expectedFileInfo, documentReference.FileName);
            SetupCosmosDbGetCountOfUnprocessedFiles(parentBatchIdentifier, 1);
            SetupDistributedLock(parentBatchIdentifier, true);
            SetupQueuePushMessage(parentBatchIdentifier, ExchangeDocumentDirection.SentByOrganisation);

            // Act
            var result = await _virusScanResultProcessor.ProcessOrganisationFileResultOk(documentReference);

            // Assert
            result.Should().BeEquivalentTo(expectedFileInfo);

            _readyForEmailMockQueue
              .Verify(queue => queue.PushMessage(It.IsAny<ScannedBatchQueueMessage>()), Times.Never);
        }

        [TestMethod, TestCategory("Unit")]
        public async Task ProcessOrganisationFileResultOk_WhenBatchCompletedAndFileExistsAndLockAcquired()
        {
            var ukprn = 12345678;
            var productType = 10090;
            var year = 201819;

            var batchIdentifier = "batch-identifier";
            var parentBatchIdentifier = "parent-batch-identifier";

            var documentReference = CreateDocumentReference(ukprn, productType, year, batchIdentifier, parentBatchIdentifier);
            var copiedFileName = $"{ukprn}_{productType}_{year}-2.pdf";

            var utcNow = new DateTime(2020, 1, 1);

            var expectedFileInfo = new FileMetadata
            {
                FileName = copiedFileName,
                Processed = true,
                VirusScanSuccessful = true,
                Version = 2,
                History = CreateMetadataHistoryHistoryScanOk(utcNow)
            };

            SetupSystemProviderDateTimeUtcNow(utcNow);
            SetupOrganisationSourceDirectoryFileCopyAndMove(documentReference.FileName, copiedFileName);
            SetupCosmosDbUpdateFileMetadata(batchIdentifier, expectedFileInfo, documentReference.FileName);
            SetupCosmosDbGetCountOfUnprocessedFiles(parentBatchIdentifier, 0);
            SetupDistributedLock(parentBatchIdentifier, true);
            SetupQueuePushMessage(parentBatchIdentifier, ExchangeDocumentDirection.SentByOrganisation);

            // Act
            var result = await _virusScanResultProcessor.ProcessOrganisationFileResultOk(documentReference);

            // Assert
            result.Should().BeEquivalentTo(expectedFileInfo);
        }

        [TestMethod, TestCategory("Unit")]
        public async Task ProcessOrganisationFileResultOk_WhenBatchCompletedAndFileExistsAndLockNotAcquired()
        {
            var ukprn = 12345678;
            var productType = 10090;
            var year = 201819;

            var batchIdentifier = "batch-identifier";
            var parentBatchIdentifier = "parent-batch-identifier";

            var documentReference = CreateDocumentReference(ukprn, productType, year, batchIdentifier, parentBatchIdentifier);
            var copiedFileName = $"{ukprn}_{productType}_{year}-2.pdf";

            var utcNow = new DateTime(2020, 1, 1);

            var expectedFileInfo = new FileMetadata
            {
                FileName = copiedFileName,
                Processed = true,
                VirusScanSuccessful = true,
                Version = 2,
                History = CreateMetadataHistoryHistoryScanOk(utcNow)
            };

            SetupSystemProviderDateTimeUtcNow(utcNow);
            SetupOrganisationSourceDirectoryFileCopyAndMove(documentReference.FileName, copiedFileName);
            SetupCosmosDbUpdateFileMetadata(batchIdentifier, expectedFileInfo, documentReference.FileName);
            SetupCosmosDbGetCountOfUnprocessedFiles(parentBatchIdentifier, 0);
            SetupDistributedLock(parentBatchIdentifier, false);

            // Act
            var result = await _virusScanResultProcessor.ProcessOrganisationFileResultOk(documentReference);

            // Assert
            result.Should().BeEquivalentTo(expectedFileInfo);

            _readyForEmailMockQueue
              .Verify(queue => queue.PushMessage(It.IsAny<ScannedBatchQueueMessage>()), Times.Never);
        }

        [TestMethod, TestCategory("Unit")]
        public async Task ProcessOrganisationFileResultOk_WhenBatchIncompleteAndFileExists()
        {
            var ukprn = 12345678;
            var productType = 10090;
            var year = 201819;

            var batchIdentifier = "batch-identifier";
            var parentBatchIdentifier = "parent-batch-identifier";

            var documentReference = CreateDocumentReference(ukprn, productType, year, batchIdentifier, parentBatchIdentifier);
            var copiedFileName = $"{ukprn}_{productType}_{year}-2.pdf";

            var utcNow = new DateTime(2020, 1, 1);

            var expectedFileInfo = new FileMetadata
            {
                FileName = copiedFileName,
                Processed = true,
                VirusScanSuccessful = true,
                Version = 2,
                History = CreateMetadataHistoryHistoryScanOk(utcNow)
            };

            SetupSystemProviderDateTimeUtcNow(utcNow);
            SetupOrganisationSourceDirectoryFileCopyAndMove(documentReference.FileName, copiedFileName);
            SetupCosmosDbUpdateFileMetadata(batchIdentifier, expectedFileInfo, documentReference.FileName);
            SetupCosmosDbGetCountOfUnprocessedFiles(parentBatchIdentifier, 1);
            SetupDistributedLock(parentBatchIdentifier, true);
            SetupQueuePushMessage(parentBatchIdentifier, ExchangeDocumentDirection.SentByOrganisation);

            // Act
            var result = await _virusScanResultProcessor.ProcessOrganisationFileResultOk(documentReference);

            // Assert
            result.Should().BeEquivalentTo(expectedFileInfo);

            _readyForEmailMockQueue
              .Verify(queue => queue.PushMessage(It.IsAny<ScannedBatchQueueMessage>()), Times.Never);
        }
        #endregion


        #region Organisation file result virus found

        [TestMethod, TestCategory("Unit")]
        public async Task ProcessOrganisationFileResultVirusFound_WhenBatchCompletedAndLockAcquired()
        {
            var ukprn = 12345678;
            var productType = 10090;
            var year = 201819;

            var batchIdentifier = "batch-identifier";
            var parentBatchIdentifier = "parent-batch-identifier";

            var documentReference = CreateDocumentReference(ukprn, productType, year, batchIdentifier, parentBatchIdentifier);

            var utcNow = new DateTime(2020, 1, 1);

            var expectedFileInfo = new FileMetadata
            {
                FileName = documentReference.FileName,
                Processed = true,
                VirusScanSuccessful = false,
                Version = 2,
                History = CreateMetadataHistoryHistoryScanVirusFound(utcNow)
            };

            SetupSystemProviderDateTimeUtcNow(utcNow);
            SetupOrganisationSourceDirectoryDelete(documentReference.FileName);
            SetupCosmosDbUpdateFileMetadata(batchIdentifier, expectedFileInfo, documentReference.FileName);
            SetupCosmosDbGetCountOfUnprocessedFiles(parentBatchIdentifier, 0);
            SetupDistributedLock(parentBatchIdentifier, true);
            SetupQueuePushMessage(parentBatchIdentifier, ExchangeDocumentDirection.SentByOrganisation);

            // Act
            var result = await _virusScanResultProcessor.ProcessOrganisationFileResultVirusFound(documentReference);

            // Assert
            result.Should().BeEquivalentTo(expectedFileInfo);
        }

        [TestMethod, TestCategory("Unit")]
        public async Task ProcessOrganisationFileResultVirusFound_WhenBatchCompletedAndLockNotAcquired()
        {
            var ukprn = 12345678;
            var productType = 10090;
            var year = 201819;

            var batchIdentifier = "batch-identifier";
            var parentBatchIdentifier = "parent-batch-identifier";

            var documentReference = CreateDocumentReference(ukprn, productType, year, batchIdentifier, parentBatchIdentifier);

            var utcNow = new DateTime(2020, 1, 1);

            var expectedFileInfo = new FileMetadata
            {
                FileName = documentReference.FileName,
                Processed = true,
                VirusScanSuccessful = false,
                Version = 2,
                History = CreateMetadataHistoryHistoryScanVirusFound(utcNow)
            };

            SetupSystemProviderDateTimeUtcNow(utcNow);
            SetupOrganisationSourceDirectoryDelete(documentReference.FileName);
            SetupCosmosDbUpdateFileMetadata(batchIdentifier, expectedFileInfo, documentReference.FileName);
            SetupCosmosDbGetCountOfUnprocessedFiles(parentBatchIdentifier, 0);
            SetupDistributedLock(parentBatchIdentifier, false);

            // Act
            var result = await _virusScanResultProcessor.ProcessOrganisationFileResultVirusFound(documentReference);

            // Assert
            result.Should().BeEquivalentTo(expectedFileInfo);

            _readyForEmailMockQueue
              .Verify(queue => queue.PushMessage(It.IsAny<ScannedBatchQueueMessage>()), Times.Never);
        }

        [TestMethod, TestCategory("Unit")]
        public async Task ProcessOrganisationFileResultVirusFound_WhenBatchIncomplete()
        {
            var ukprn = 12345678;
            var productType = 10090;
            var year = 201819;

            var batchIdentifier = "batch-identifier";
            var parentBatchIdentifier = "parent-batch-identifier";

            var documentReference = CreateDocumentReference(ukprn, productType, year, batchIdentifier, parentBatchIdentifier);

            var utcNow = new DateTime(2020, 1, 1);

            var expectedFileInfo = new FileMetadata
            {
                FileName = documentReference.FileName,
                Processed = true,
                VirusScanSuccessful = false,
                Version = 2,
                History = CreateMetadataHistoryHistoryScanVirusFound(utcNow)
            };

            SetupSystemProviderDateTimeUtcNow(utcNow);
            SetupOrganisationSourceDirectoryDelete(documentReference.FileName);
            SetupCosmosDbUpdateFileMetadata(batchIdentifier, expectedFileInfo, documentReference.FileName);
            SetupCosmosDbGetCountOfUnprocessedFiles(parentBatchIdentifier, 1);
            SetupDistributedLock(parentBatchIdentifier, true);
            SetupQueuePushMessage(parentBatchIdentifier, ExchangeDocumentDirection.SentByOrganisation);

            // Act
            var result = await _virusScanResultProcessor.ProcessOrganisationFileResultVirusFound(documentReference);

            // Assert
            result.Should().BeEquivalentTo(expectedFileInfo);

            _readyForEmailMockQueue
              .Verify(queue => queue.PushMessage(It.IsAny<ScannedBatchQueueMessage>()), Times.Never);
        }
        #endregion

        private void SetupSystemProviderDateTimeUtcNow(DateTime utcNow)
        {
            _mockSystemProvider
                .Setup(systemProvider => systemProvider.DateTime.UtcNow())
                .Returns(utcNow);
        }

        private void SetupAgencySourceDirectoryFileCopyAndMove(string originalFileName, string copiedFileName)
        {
            _agencySourceMockDirectory
                .Setup(dir => dir.Copy(_fileCopyDestinationMockDirectory.Object, originalFileName))
                .ReturnsAsync(copiedFileName);

            _agencySourceMockDirectory
                .Setup(dir => dir.Move(_agencyDestinationMockDirectory.Object, originalFileName))
                .ReturnsAsync(originalFileName);
        }

        private void SetupOrganisationSourceDirectoryFileCopyAndMove(string originalFileName, string copiedFileName)
        {
            _organisationSourceMockDirectory
                .Setup(dir => dir.Copy(_fileCopyDestinationMockDirectory.Object, originalFileName))
                .ReturnsAsync(copiedFileName);

            _organisationSourceMockDirectory
                .Setup(dir => dir.Move(_organisationDestinationMockDirectory.Object, originalFileName))
                .ReturnsAsync(originalFileName);
        }

        private void SetupAgencySourceDirectoryDelete(string originalFileName)
        {
            _agencySourceMockDirectory
                .Setup(dir => dir.Delete(originalFileName))
                .Returns(Task.CompletedTask);
        }

        private void SetupOrganisationSourceDirectoryDelete(string originalFileName)
        {
            _organisationSourceMockDirectory
                .Setup(dir => dir.Delete(originalFileName))
                .Returns(Task.CompletedTask);
        }

        private void SetupCosmosDbUpdateFileMetadata(string batchIdentifier, FileMetadata expectedFileMetadata, string originalFileName)
        {
            Expression<Func<FileMetadata, bool>> fileMetadataMatch =
                fileMetadata =>
                fileMetadata.FileName == expectedFileMetadata.FileName
                && fileMetadata.Processed == expectedFileMetadata.Processed
                && fileMetadata.VirusScanSuccessful == expectedFileMetadata.VirusScanSuccessful
                && fileMetadata.History.All(history =>
                    expectedFileMetadata.History.Any(expectedHistory =>
                    history.ActionDateTimeUtc == expectedHistory.ActionDateTimeUtc
                    && history.Action == expectedHistory.Action
                    && history.User == expectedHistory.User));

            var fieldsToUpdate = new[] { "Processed", "Okay", "Filename" };

            _mockCosmosDbService
               .Setup(cosmosDb => cosmosDb.UpdateDocumentMetadataAndVersion(
                   It.Is(fileMetadataMatch),
                   batchIdentifier,
                   It.Is<IEnumerable<string>>(fields => fields.All(field => fieldsToUpdate.Contains(field))),
                   originalFileName))
               .ReturnsAsync(expectedFileMetadata.Version);

            _mockCosmosDbService
                .Setup(cosmosDb => cosmosDb.AddHistoryToFileMetadata(batchIdentifier, It.Is(fileMetadataMatch)))
                .ReturnsAsync("1 files added history to");
        }

        private void SetupCosmosDbGetCountOfUnprocessedFiles(string parentBatchIdentifier, int unprocessedFileCount)
        {
            _mockCosmosDbService
               .Setup(cosmosDb => cosmosDb.GetCountOfUnprocessedFiles(parentBatchIdentifier))
               .ReturnsAsync(unprocessedFileCount);
        }

        private void SetupDistributedLock(string parentBatchIdentifier, bool lockAcquired)
        {
            var resourceKey = "the-email-resource-key";

            _mockCacheManager
                .Setup(cacheManager => cacheManager.CacheKeyBuilder.BuildEmailResourceLockKey(parentBatchIdentifier))
                .Returns(resourceKey);

            _mockDistributedLocksService
                .Setup(distributedLock => distributedLock.TryAcquireLock(resourceKey, It.IsAny<string>(), It.IsAny<TimeSpan>()))
                .ReturnsAsync(lockAcquired);
        }

        private void SetupQueuePushMessage(string parentBatchIdentifier, ExchangeDocumentDirection direction)
        {
            _readyForEmailMockQueue
               .Setup(queue => queue.PushMessage(It.Is<ScannedBatchQueueMessage>(
                   message => message.ParentBatchIdentifier == parentBatchIdentifier && message.DocumentDirection == direction)))
               .Returns(Task.CompletedTask);
        }

        private DocumentReference CreateDocumentReference(int ukprn, int productType, int year, string batchIdentifier, string parentBatchIdentifier)
        {
            var originalFileName = $"{ukprn}_{productType}_{year}.pdf";

            return new DocumentReference
            {
                FileName = originalFileName,
                BatchIdentifier = batchIdentifier,
                ParentBatchIdentifier = parentBatchIdentifier
            };
        }

        private FileMetadataHistory[] CreateMetadataHistoryHistoryScanOk(DateTime creationDate)
            => new FileMetadataHistory[]
            {
                new FileMetadataHistory { ActionDateTimeUtc = creationDate, Action = FileAction.FileScanGood, User = null },
                new FileMetadataHistory { ActionDateTimeUtc = creationDate, Action = FileAction.Copying, User = null },
                new FileMetadataHistory { ActionDateTimeUtc = creationDate, Action = FileAction.Moving, User = null }
            };

        private FileMetadataHistory[] CreateMetadataHistoryHistoryScanVirusFound(DateTime creationDate)
            => new FileMetadataHistory[]
            {
                new FileMetadataHistory { ActionDateTimeUtc = creationDate, Action = FileAction.FileScanBad, User = null },
                new FileMetadataHistory { ActionDateTimeUtc = creationDate, Action = FileAction.Deleting, User = null }
            };
    }
}