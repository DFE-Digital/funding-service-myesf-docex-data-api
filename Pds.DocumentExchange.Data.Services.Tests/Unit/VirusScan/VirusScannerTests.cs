using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using Pds.Core.Logging;
using Pds.Core.Utils;
using Pds.DocumentExchange.Data.Services.DTOs;
using Pds.DocumentExchange.Data.Services.DTOs.Configuration;
using Pds.DocumentExchange.Data.Services.Enums;
using Pds.DocumentExchange.Data.Services.Implementations.VirusScan;
using Pds.DocumentExchange.Data.Services.Interfaces;
using Pds.DocumentExchange.Data.Services.Interfaces.Storage;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Tests.Unit.VirusScan
{
    [TestClass]
    public class VirusScannerTests
    {
        [TestMethod, TestCategory("Unit")]
        public async Task FileNotFoundTest()
        {
            // Arrange
            var directoriesManager = new Mock<IDirectoriesManager>();
            var organisationDirectory = new Mock<IDirectory>();
            var antivirus = new Mock<IAntivirus>();
            var cosmosDb = new Mock<ICosmosDbService>();
            var systemProvider = new Mock<ISystemProvider>();
            var logger = new Mock<ILoggerAdapter<VirusScanner>>();

            var virusScanner = new VirusScanner(
               directoriesManager.Object,
               antivirus.Object,
               cosmosDb.Object,
               systemProvider.Object,
               new VirusScannerConfiguration
               {
                   AgencyDirectoryKey = "agency-directory-key",
                   OrganisationDirectoryKey = "organisation-directory-key"
               },
               logger.Object);

            var fileInfo = new FileMetadata
            {
                FileName = "non-existing-file.pdf",
                ProductIdentifier = "pdf",
                OriginalFileName = "original-filename.pdf",
                Processed = false,
                FromUkprn = 123,
                ToUkprn = 999,
                Version = 1
            };

            directoriesManager.Setup(fileStorage => fileStorage.GetDirectory("organisation-directory-key"))
                .ReturnsAsync(organisationDirectory.Object);

            organisationDirectory.Setup(fileStorage => fileStorage.Read(fileInfo.FileName))
                .ReturnsAsync((Stream)null);

            systemProvider.Setup(systemProvider => systemProvider.DateTime.Now())
                .Returns(new DateTime(2020, 1, 1, 12, 15, 0));

            // Act
            var scanResult = await virusScanner.ScanFile(fileInfo, "batch-id", "parent-batch-id", ExchangeDocumentDirection.SentByOrganisation);

            // Assert
            var expectedScanResult = new ScannedFileInfo
            {
                BatchIdentifier = "batch-id",
                ParentBatchIdentifier = "parent-batch-id",
                FileInfo = fileInfo,
                ScanResult = FileSafety.InternalError,
                DocumentDirection = ExchangeDocumentDirection.SentByOrganisation
            };

            var expectedFileInfo = new FileMetadata
            {
                FileName = "non-existing-file.pdf",
                ProductIdentifier = "pdf",
                Metadata = null,
                OriginalFileName = "original-filename.pdf",
                History = null,
                Processed = true,
                VirusScanSuccessful = false,
                FromUkprn = 123,
                ToUkprn = 999,
                ReplaceRequested = false,
                Version = 1,
                ExpiresAtDateTime = null
            };

            scanResult.Should().BeEquivalentTo(expectedScanResult);
            fileInfo.Should().BeEquivalentTo(expectedFileInfo);
        }

        [TestMethod, TestCategory("Unit")]
        public async Task FileAlreadyProcessedTest()
        {
            // Arrange
            var directoriesManager = new Mock<IDirectoriesManager>();
            var antivirus = new Mock<IAntivirus>();
            var cosmosDb = new Mock<ICosmosDbService>();
            var systemProvider = new Mock<ISystemProvider>();
            var logger = new Mock<ILoggerAdapter<VirusScanner>>();

            var virusScanner = new VirusScanner(
               directoriesManager.Object,
               antivirus.Object,
               cosmosDb.Object,
               systemProvider.Object,
               new VirusScannerConfiguration
               {
                   AgencyDirectoryKey = "agency-directory-key",
                   OrganisationDirectoryKey = "organisation-directory-key"
               },
               logger.Object);

            var fileInfo = new FileMetadata
            {
                FileName = "filename.pdf",
                ProductIdentifier = "pdf",
                OriginalFileName = "original-filename.pdf",
                Processed = true,
                FromUkprn = 123,
                ToUkprn = 999,
                Version = 1
            };

            // Act
            var scanResult = await virusScanner.ScanFile(fileInfo, "batch-id", "parent-batch-id", ExchangeDocumentDirection.SentByOrganisation);

            // Assert
            var expectedScanResult = new ScannedFileInfo
            {
                BatchIdentifier = "batch-id",
                ParentBatchIdentifier = "parent-batch-id",
                FileInfo = fileInfo,
                ScanResult = FileSafety.AlreadyScanned,
                DocumentDirection = ExchangeDocumentDirection.SentByOrganisation
            };

            var expectedFileInfo = new FileMetadata
            {
                FileName = "filename.pdf",
                ProductIdentifier = "pdf",
                Metadata = null,
                OriginalFileName = "original-filename.pdf",
                History = null,
                Processed = true,
                VirusScanSuccessful = false,
                FromUkprn = 123,
                ToUkprn = 999,
                ReplaceRequested = false,
                Version = 1,
                ExpiresAtDateTime = null
            };

            scanResult.Should().BeEquivalentTo(expectedScanResult);
            fileInfo.Should().BeEquivalentTo(expectedFileInfo);
        }

        [TestMethod, TestCategory("Unit")]
        public async Task ScanFileStorageExceptionTest()
        {
            // Arrange
            var directoriesManager = new Mock<IDirectoriesManager>();
            var organisationDirectory = new Mock<IDirectory>();
            var antivirus = new Mock<IAntivirus>();
            var cosmosDb = new Mock<ICosmosDbService>();
            var systemProvider = new Mock<ISystemProvider>();
            var logger = new Mock<ILoggerAdapter<VirusScanner>>();

            var virusScanner = new VirusScanner(
               directoriesManager.Object,
               antivirus.Object,
               cosmosDb.Object,
               systemProvider.Object,
               new VirusScannerConfiguration
               {
                   AgencyDirectoryKey = "agency-directory-key",
                   OrganisationDirectoryKey = "organisation-directory-key"
               },
               logger.Object);

            var fileInfo = new FileMetadata
            {
                FileName = "filename.pdf",
                ProductIdentifier = "pdf",
                OriginalFileName = "original-filename.pdf",
                Processed = false,
                FromUkprn = 123,
                ToUkprn = 999,
                Version = 1
            };

            directoriesManager.Setup(fileStorage => fileStorage.GetDirectory("organisation-directory-key"))
                .ReturnsAsync(organisationDirectory.Object);

            organisationDirectory.Setup(fileStorage => fileStorage.Read(fileInfo.FileName))
                .Throws(new Exception());

            // Act
            var scanResult = await virusScanner.ScanFile(fileInfo, "batch-id", "parent-batch-id", ExchangeDocumentDirection.SentByOrganisation);

            // Assert
            var expectedScanResult = new ScannedFileInfo
            {
                BatchIdentifier = "batch-id",
                ParentBatchIdentifier = "parent-batch-id",
                FileInfo = fileInfo,
                ScanResult = FileSafety.InternalError,
                DocumentDirection = ExchangeDocumentDirection.SentByOrganisation
            };

            var expectedFileInfo = new FileMetadata
            {
                FileName = "filename.pdf",
                ProductIdentifier = "pdf",
                Metadata = null,
                OriginalFileName = "original-filename.pdf",
                History = null,
                Processed = false,
                VirusScanSuccessful = false,
                FromUkprn = 123,
                ToUkprn = 999,
                ReplaceRequested = false,
                Version = 1,
                ExpiresAtDateTime = null
            };

            scanResult.Should().BeEquivalentTo(expectedScanResult);
            fileInfo.Should().BeEquivalentTo(expectedFileInfo);
        }

        [TestMethod, TestCategory("Unit")]
        public async Task ScanAntivirusExceptionTest()
        {
            // Arrange
            var directoriesManager = new Mock<IDirectoriesManager>();
            var agencyDirectory = new Mock<IDirectory>();
            var organisationDirectory = new Mock<IDirectory>();
            var antivirus = new Mock<IAntivirus>();
            var cosmosDb = new Mock<ICosmosDbService>();
            var systemProvider = new Mock<ISystemProvider>();
            var logger = new Mock<ILoggerAdapter<VirusScanner>>();

            var virusScanner = new VirusScanner(
               directoriesManager.Object,
               antivirus.Object,
               cosmosDb.Object,
               systemProvider.Object,
               new VirusScannerConfiguration
               {
                   AgencyDirectoryKey = "agency-directory-key",
                   OrganisationDirectoryKey = "organisation-directory-key"
               },
               logger.Object);

            var fileInfo = new FileMetadata
            {
                FileName = "filename.pdf",
                ProductIdentifier = "pdf",
                OriginalFileName = "original-filename.pdf",
                Processed = false,
                FromUkprn = 123,
                ToUkprn = 999,
                Version = 1
            };

            directoriesManager.Setup(fileStorage => fileStorage.GetDirectory("agency-directory-key"))
                .ReturnsAsync(agencyDirectory.Object);

            directoriesManager.Setup(fileStorage => fileStorage.GetDirectory("organisation-directory-key"))
                .ReturnsAsync(organisationDirectory.Object);

            var mockStream = new Mock<Stream>().Object;

            organisationDirectory.Setup(fileStorage => fileStorage.Read(fileInfo.FileName))
                .ReturnsAsync(mockStream);

            antivirus.Setup(antivirus => antivirus.Scan(fileInfo.FileName, mockStream))
                .Throws(new Exception());

            // Act
            var scanResult = await virusScanner.ScanFile(fileInfo, "batch-id", "parent-batch-id", ExchangeDocumentDirection.SentByOrganisation);

            // Assert
            var expectedScanResult = new ScannedFileInfo
            {
                BatchIdentifier = "batch-id",
                ParentBatchIdentifier = "parent-batch-id",
                FileInfo = fileInfo,
                ScanResult = FileSafety.InternalError,
                DocumentDirection = ExchangeDocumentDirection.SentByOrganisation
            };

            var expectedFileInfo = new FileMetadata
            {
                FileName = "filename.pdf",
                ProductIdentifier = "pdf",
                Metadata = null,
                OriginalFileName = "original-filename.pdf",
                History = null,
                Processed = false,
                VirusScanSuccessful = false,
                FromUkprn = 123,
                ToUkprn = 999,
                ReplaceRequested = false,
                Version = 1,
                ExpiresAtDateTime = null
            };

            scanResult.Should().BeEquivalentTo(expectedScanResult);
            fileInfo.Should().BeEquivalentTo(expectedFileInfo);
        }

        [TestMethod, TestCategory("Unit")]
        public async Task ScanCosmosDbExceptionTest()
        {
            // Arrange
            var directoriesManager = new Mock<IDirectoriesManager>();
            var agencyDirectory = new Mock<IDirectory>();
            var antivirus = new Mock<IAntivirus>();
            var cosmosDb = new Mock<ICosmosDbService>();
            var systemProvider = new Mock<ISystemProvider>();
            var logger = new Mock<ILoggerAdapter<VirusScanner>>();

            var virusScanner = new VirusScanner(
               directoriesManager.Object,
               antivirus.Object,
               cosmosDb.Object,
               systemProvider.Object,
               new VirusScannerConfiguration
               {
                   AgencyDirectoryKey = "agency-directory-key",
                   OrganisationDirectoryKey = "organisation-directory-key"
               },
               logger.Object);

            var fileInfo = new FileMetadata
            {
                FileName = "filename.pdf",
                ProductIdentifier = "pdf",
                OriginalFileName = "original-filename.pdf",
                Processed = false,
                FromUkprn = 123,
                ToUkprn = 999,
                Version = 1
            };

            var batchIdentifier = "batch-id";
            var parentBatchIdentifier = "parent-batch-id";

            var mockStream = new Mock<Stream>().Object;
            var utcNow = new DateTime(2020, 1, 1);

            var antiVirusScanResult = new ScanResult
            {
                ScanServiceName = "MultipleAntivirus",
                FileSafety = FileSafety.Okay
            };

            directoriesManager.Setup(fileStorage => fileStorage.GetDirectory("agency-directory-key"))
                .ReturnsAsync(agencyDirectory.Object);

            agencyDirectory.Setup(fileStorage => fileStorage.Read(fileInfo.FileName))
                .ReturnsAsync(mockStream);

            antivirus.Setup(antivirus => antivirus.Scan(fileInfo.FileName, mockStream))
                .ReturnsAsync(antiVirusScanResult);

            cosmosDb.Setup(antivirus => antivirus.AddHistoryToFileMetadata("batch-id", fileInfo))
                .Throws(new Exception());

            systemProvider.Setup(systemProvider => systemProvider.DateTime.UtcNow())
              .Returns(utcNow);

            systemProvider.SetupSequence(systemProvider => systemProvider.DateTime.Now())
                .Returns(new DateTime(2020, 1, 1, 12, 15, 0))
                .Returns(new DateTime(2020, 1, 1, 12, 16, 15));

            // Act
            var scanResult = await virusScanner.ScanFile(fileInfo, "batch-id", "parent-batch-id", ExchangeDocumentDirection.PublishedByAgency);

            // Assert
            var expectedScanResult = new ScannedFileInfo
            {
                BatchIdentifier = batchIdentifier,
                ParentBatchIdentifier = parentBatchIdentifier,
                FileInfo = fileInfo,
                ScanResult = FileSafety.InternalError,
                DocumentDirection = ExchangeDocumentDirection.PublishedByAgency
            };

            var expectedFileInfo = new FileMetadata
            {
                FileName = "filename.pdf",
                ProductIdentifier = "pdf",
                Metadata = null,
                OriginalFileName = "original-filename.pdf",
                History = new[]
                {
                    new FileMetadataHistory
                    {
                        Action = FileAction.FileScanned,
                        ActionDateTimeUtc = utcNow,
                        User = null,
                        Message = JsonConvert.SerializeObject(antiVirusScanResult)
                    }
                },
                Processed = false,
                VirusScanSuccessful = false,
                FromUkprn = 123,
                ToUkprn = 999,
                ReplaceRequested = false,
                Version = 1,
                ExpiresAtDateTime = null
            };

            scanResult.Should().BeEquivalentTo(expectedScanResult);
            fileInfo.Should().BeEquivalentTo(expectedFileInfo);
        }

        [TestMethod, TestCategory("Unit")]
        public async Task ScanOkTest()
        {
            // Arrange
            var directoriesManager = new Mock<IDirectoriesManager>();
            var agencyDirectory = new Mock<IDirectory>();
            var organisationDirectory = new Mock<IDirectory>();
            var antivirus = new Mock<IAntivirus>();
            var cosmosDb = new Mock<ICosmosDbService>();
            var systemProvider = new Mock<ISystemProvider>();
            var logger = new Mock<ILoggerAdapter<VirusScanner>>();

            var virusScanner = new VirusScanner(
               directoriesManager.Object,
               antivirus.Object,
               cosmosDb.Object,
               systemProvider.Object,
               new VirusScannerConfiguration
               {
                   AgencyDirectoryKey = "agency-directory-key",
                   OrganisationDirectoryKey = "organisation-directory-key"
               },
               logger.Object);

            var fileInfo = new FileMetadata
            {
                FileName = "filename.pdf",
                ProductIdentifier = "pdf",
                OriginalFileName = "original-filename.pdf",
                Processed = false,
                FromUkprn = 123,
                ToUkprn = 999,
                Version = 1
            };

            var mockStream = new Mock<Stream>().Object;
            var utcNow = new DateTime(2020, 1, 1);

            var antiVirusScanResult = new ScanResult
            {
                ScanServiceName = "MultipleAntivirus",
                FileSafety = FileSafety.Okay
            };

            directoriesManager.Setup(fileStorage => fileStorage.GetDirectory("organisation-directory-key"))
                .ReturnsAsync(organisationDirectory.Object);

            organisationDirectory.Setup(fileStorage => fileStorage.Read(fileInfo.FileName))
                .ReturnsAsync(mockStream);

            antivirus.Setup(antivirus => antivirus.Scan(fileInfo.FileName, mockStream))
                .ReturnsAsync(antiVirusScanResult);

            systemProvider.Setup(systemProvider => systemProvider.DateTime.UtcNow())
               .Returns(utcNow);

            systemProvider.SetupSequence(systemProvider => systemProvider.DateTime.Now())
                .Returns(new DateTime(2020, 1, 1, 12, 15, 0))
                .Returns(new DateTime(2020, 1, 1, 12, 16, 15));

            //Act
            var scanResult = await virusScanner.ScanFile(fileInfo, "batch-id", "parent-batch-id", ExchangeDocumentDirection.SentByOrganisation);

            // Assert
            var expectedScanResult = new ScannedFileInfo
            {
                BatchIdentifier = "batch-id",
                ParentBatchIdentifier = "parent-batch-id",
                FileInfo = fileInfo,
                ScanResult = FileSafety.Okay,
                DocumentDirection = ExchangeDocumentDirection.SentByOrganisation
            };

            var expectedFileInfo = new FileMetadata
            {
                FileName = "filename.pdf",
                ProductIdentifier = "pdf",
                Metadata = null,
                OriginalFileName = "original-filename.pdf",
                History = new[]
                {
                    new FileMetadataHistory
                    {
                        Action = FileAction.FileScanned,
                        ActionDateTimeUtc = utcNow,
                        User = null,
                        Message = JsonConvert.SerializeObject(antiVirusScanResult)
                    }
                },
                Processed = false,
                VirusScanSuccessful = false,
                FromUkprn = 123,
                ToUkprn = 999,
                ReplaceRequested = false,
                Version = 1,
                ExpiresAtDateTime = null
            };

            scanResult.Should().BeEquivalentTo(expectedScanResult);
            fileInfo.Should().BeEquivalentTo(expectedFileInfo);
        }

        [TestMethod, TestCategory("Unit")]
        public async Task ScanVirusFoundTest()
        {
            // Arrange
            var directoriesManager = new Mock<IDirectoriesManager>();
            var agencyDirectory = new Mock<IDirectory>();
            var organisationDirectory = new Mock<IDirectory>();
            var antivirus = new Mock<IAntivirus>();
            var cosmosDb = new Mock<ICosmosDbService>();
            var systemProvider = new Mock<ISystemProvider>();
            var logger = new Mock<ILoggerAdapter<VirusScanner>>();

            var virusScanner = new VirusScanner(
               directoriesManager.Object,
               antivirus.Object,
               cosmosDb.Object,
               systemProvider.Object,
               new VirusScannerConfiguration
               {
                   AgencyDirectoryKey = "agency-directory-key",
                   OrganisationDirectoryKey = "organisation-directory-key"
               },
               logger.Object);

            var fileInfo = new FileMetadata
            {
                FileName = "filename.pdf",
                ProductIdentifier = "pdf",
                OriginalFileName = "original-filename.pdf",
                Processed = false,
                FromUkprn = 123,
                ToUkprn = 999,
                Version = 1
            };

            var mockStream = new Mock<Stream>().Object;
            var utcNow = new DateTime(2020, 1, 1);

            var antiVirusScanResult = new ScanResult
            {
                ScanServiceName = "MultipleAntivirus",
                FileSafety = FileSafety.Virus
            };

            directoriesManager.Setup(fileStorage => fileStorage.GetDirectory("organisation-directory-key"))
                .ReturnsAsync(organisationDirectory.Object);

            organisationDirectory.Setup(fileStorage => fileStorage.Read(fileInfo.FileName))
                .ReturnsAsync(mockStream);

            antivirus.Setup(antivirus => antivirus.Scan(fileInfo.FileName, mockStream))
                .ReturnsAsync(antiVirusScanResult);

            systemProvider.Setup(systemProvider => systemProvider.DateTime.UtcNow())
                .Returns(utcNow);

            systemProvider.SetupSequence(systemProvider => systemProvider.DateTime.Now())
                .Returns(new DateTime(2020, 1, 1, 12, 15, 0))
                .Returns(new DateTime(2020, 1, 1, 12, 16, 15));

            // Act
            var scanResult = await virusScanner.ScanFile(fileInfo, "batch-id", "parent-batch-id", ExchangeDocumentDirection.SentByOrganisation);

            // Assert
            var expectedScanResult = new ScannedFileInfo
            {
                BatchIdentifier = "batch-id",
                ParentBatchIdentifier = "parent-batch-id",
                FileInfo = fileInfo,
                ScanResult = FileSafety.Virus,
                DocumentDirection = ExchangeDocumentDirection.SentByOrganisation
            };

            var expectedFileInfo = new FileMetadata
            {
                FileName = "filename.pdf",
                ProductIdentifier = "pdf",
                Metadata = null,
                OriginalFileName = "original-filename.pdf",
                History = new[]
                {
                    new FileMetadataHistory
                    {
                       Action = FileAction.FileScanned,
                       ActionDateTimeUtc = utcNow,
                       User = null,
                       Message = JsonConvert.SerializeObject(antiVirusScanResult)
                    }
                },
                Processed = false,
                VirusScanSuccessful = false,
                FromUkprn = 123,
                ToUkprn = 999,
                ReplaceRequested = false,
                Version = 1,
                ExpiresAtDateTime = null
            };

            scanResult.Should().BeEquivalentTo(expectedScanResult);
            fileInfo.Should().BeEquivalentTo(expectedFileInfo);
        }

        [TestMethod, TestCategory("Unit")]
        public async Task ScanAlreadyScannedResultTest()
        {
            // Arrange
            var directoriesManager = new Mock<IDirectoriesManager>();
            var organisationDirectory = new Mock<IDirectory>();
            var antivirus = new Mock<IAntivirus>();
            var cosmosDb = new Mock<ICosmosDbService>();
            var systemProvider = new Mock<ISystemProvider>();
            var logger = new Mock<ILoggerAdapter<VirusScanner>>();

            var virusScanner = new VirusScanner(
               directoriesManager.Object,
               antivirus.Object,
               cosmosDb.Object,
               systemProvider.Object,
               new VirusScannerConfiguration
               {
                   AgencyDirectoryKey = "agency-directory-key",
                   OrganisationDirectoryKey = "organisation-directory-key"
               },
               logger.Object);

            var fileInfo = new FileMetadata
            {
                FileName = "filename.pdf",
                ProductIdentifier = "pdf",
                OriginalFileName = "original-filename.pdf",
                Processed = false,
                FromUkprn = 123,
                ToUkprn = 999,
                Version = 1
            };

            var antiVirusScanResult = new ScanResult
            {
                ScanServiceName = "MultipleAntivirus",
                FileSafety = FileSafety.AlreadyScanned
            };

            var mockStream = new Mock<Stream>().Object;
            var utcNow = new DateTime(2020, 1, 1);

            directoriesManager.Setup(fileStorage => fileStorage.GetDirectory("organisation-directory-key"))
                .ReturnsAsync(organisationDirectory.Object);

            organisationDirectory.Setup(fileStorage => fileStorage.Read(fileInfo.FileName))
                .ReturnsAsync(mockStream);

            antivirus.Setup(antivirus => antivirus.Scan(fileInfo.FileName, mockStream))
                .ReturnsAsync(antiVirusScanResult);

            systemProvider.Setup(systemProvider => systemProvider.DateTime.UtcNow())
               .Returns(utcNow);

            systemProvider.SetupSequence(systemProvider => systemProvider.DateTime.Now())
                .Returns(new DateTime(2020, 1, 1, 12, 15, 0))
                .Returns(new DateTime(2020, 1, 1, 14, 16, 32));

            // Act
            var scanResult = await virusScanner.ScanFile(fileInfo, "batch-id", "parent-batch-id", ExchangeDocumentDirection.SentByOrganisation);

            // Assert
            var expectedScanResult = new ScannedFileInfo
            {
                BatchIdentifier = "batch-id",
                ParentBatchIdentifier = "parent-batch-id",
                FileInfo = fileInfo,
                ScanResult = FileSafety.AlreadyScanned,
                DocumentDirection = ExchangeDocumentDirection.SentByOrganisation
            };

            var expectedFileInfo = new FileMetadata
            {
                FileName = "filename.pdf",
                ProductIdentifier = "pdf",
                Metadata = null,
                OriginalFileName = "original-filename.pdf",
                History = new FileMetadataHistory[]
                {
                    new FileMetadataHistory
                    {
                        Action = FileAction.FileScanned,
                        ActionDateTimeUtc = utcNow,
                        User = null,
                        Message = JsonConvert.SerializeObject(antiVirusScanResult)
                    }
                },
                Processed = false,
                VirusScanSuccessful = false,
                FromUkprn = 123,
                ToUkprn = 999,
                ReplaceRequested = false,
                Version = 1,
                ExpiresAtDateTime = null
            };

            scanResult.Should().BeEquivalentTo(expectedScanResult);
            fileInfo.Should().BeEquivalentTo(expectedFileInfo);
        }
    }
}