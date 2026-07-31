using FluentAssertions;
using MapsterMapper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pds.Core.Common.Organisation.Enums;
using Pds.Core.Common.Organisation.Models;
using Pds.Core.Logging;
using Pds.Core.Utils;
using Pds.DocumentExchange.Data.Services.DTOs;
using Pds.DocumentExchange.Data.Services.DTOs.Configuration;
using Pds.DocumentExchange.Data.Services.DTOs.User;
using Pds.DocumentExchange.Data.Services.Implementations;
using Pds.DocumentExchange.Data.Services.Interfaces;
using Pds.DocumentExchange.Data.Services.Interfaces.Storage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Tests.Unit
{
    [TestClass]
    public class DocumentDownloaderTests
    {
        private readonly Mock<IDirectoriesManager> _mockDirectoriesManager = new Mock<IDirectoriesManager>(MockBehavior.Strict);
        private readonly Mock<ICosmosDbService> _mockCosmosDbService = new Mock<ICosmosDbService>(MockBehavior.Strict);
        private readonly Mock<IFileMetadataUserEncryptor> _fileMetadataUserEncryptor = new Mock<IFileMetadataUserEncryptor>(MockBehavior.Strict);
        private readonly Mock<ISystemProvider> _systemProvider = new Mock<ISystemProvider>(MockBehavior.Strict);
        private readonly Mock<IMapper> _mapper = new Mock<IMapper>(MockBehavior.Strict);
        private readonly Mock<IZipService> _zipService = new Mock<IZipService>(MockBehavior.Strict);
        private readonly Mock<IDirectory> _downloadMockDirectory = new Mock<IDirectory>(MockBehavior.Strict);
        private readonly Mock<ILoggerAdapter<DocumentDownloader>> _mockLogger = new Mock<ILoggerAdapter<DocumentDownloader>>(MockBehavior.Loose);

        private readonly DocumentDownloaderConfiguration _downloaderConfiguration = new DocumentDownloaderConfiguration
        {
            DownloadDirectoryKey = "container-all",
            ExpiresInHours = 168
        };

        private readonly DocumentDownloader _documentDownloader;

        public DocumentDownloaderTests()
        {
            _documentDownloader = new DocumentDownloader(
                _mockCosmosDbService.Object,
                _mockDirectoriesManager.Object,
                _fileMetadataUserEncryptor.Object,
                _systemProvider.Object,
                _mapper.Object,
                _zipService.Object,
                _downloaderConfiguration,
                _mockLogger.Object);
        }

        #region Single document download

        [TestMethod]
        [TestCategory("Unit")]
        public async Task DownloadExchangeDocument_Returns200OKAndUpdatesFileHistory_WhenSingleDocumentIsRequestedAndUserIsSender()
        {
            // Arrange
            var documentReference = CreateDocumentReference("file-name.pdf", "batch-id", "parent-batch-id");

            var organisationIdentifier = new OrganisationIdentifier { Value = "12345678" };
            var user = CreateTestUserInfo(organisationIdentifier);
            var encryptedFileMetadataUser = CreateTestEncryptedFileMetadataUser();

            var downloadRequest = CreateDownloadRequest(user, documentReference);

            var fileMetadata = CreateFileMetadata(documentReference.FileName, organisationIdentifier.Value);

            ConfigureCosmosDbGetFile(documentReference, fileMetadata);
            ConfigureCosmosDbUpdateDocumentMetadata(documentReference.BatchIdentifier, fileMetadata);
            ConfigureMapper(user);
            ConfigureFileMetadataEncryptor(user, encryptedFileMetadataUser);
            ConfigureDirectoriesManager();

            var utcNow = new DateTime(2020, 6, 1);
            ConfigureSystemProvider(utcNow);

            ConfigureDownloadDirectory(documentReference.FileName);

            var expectedHistory = new FileMetadataHistory
            {
                Action = Enums.FileAction.ViewedBySender,
                ActionDateTimeUtc = utcNow,
                User = encryptedFileMetadataUser
            };

            // Act
            var result = await _documentDownloader.DownloadExchangeDocument(downloadRequest);

            // Assert
            result.Should().NotBeEmpty();
            fileMetadata.History.Should().ContainEquivalentOf(expectedHistory);

            VerifyMocks();
        }

        [TestMethod]
        [TestCategory("Unit")]
        public async Task DownloadExchangeDocument_Returns200OKAndUpdatesFileHistory_WhenSingleDocumentIsRequestedAndUserIsViewAsProviderSender()
        {
            // Arrange
            var documentReference = CreateDocumentReference("file-name.pdf", "batch-id", "parent-batch-id");

            var organisationIdentifier = new OrganisationIdentifier { Value = "12345678" };
            var user = CreateTestUserInfo(organisationIdentifier, true);
            var encryptedFileMetadataUser = CreateTestEncryptedFileMetadataUser();

            var downloadRequest = CreateDownloadRequest(user, documentReference);

            var fileMetadata = CreateFileMetadata(documentReference.FileName, organisationIdentifier.Value);

            ConfigureCosmosDbGetFile(documentReference, fileMetadata);
            ConfigureCosmosDbUpdateDocumentMetadata(documentReference.BatchIdentifier, fileMetadata);
            ConfigureMapper(user);
            ConfigureFileMetadataEncryptor(user, encryptedFileMetadataUser);
            ConfigureDirectoriesManager();

            var utcNow = new DateTime(2020, 6, 1);
            ConfigureSystemProvider(utcNow);

            ConfigureDownloadDirectory(documentReference.FileName);

            var expectedHistory = new FileMetadataHistory
            {
                Action = Enums.FileAction.ViewAsProviderDownloadedSent,
                ActionDateTimeUtc = utcNow,
                User = encryptedFileMetadataUser
            };

            // Act
            var result = await _documentDownloader.DownloadExchangeDocument(downloadRequest);

            // Assert
            result.Should().NotBeEmpty();
            fileMetadata.History.Should().ContainEquivalentOf(expectedHistory);

            VerifyMocks();
        }

        [TestMethod]
        [TestCategory("Unit")]
        public async Task DownloadExchangeDocument_Returns200OKAndUpdatesFileHistory_WhenSingleDocumentIsRequestedAndUserIsReceiver()
        {
            // Arrange
            var documentReference = CreateDocumentReference("file-name.pdf", "batch-id", "parent-batch-id");

            var organisationIdentifier = new OrganisationIdentifier { Value = "12345678" };
            var user = CreateTestUserInfo(organisationIdentifier);
            var encryptedFileMetadataUser = CreateTestEncryptedFileMetadataUser();

            var downloadRequest = CreateDownloadRequest(user, documentReference);

            var fileMetadata = CreateFileMetadata(documentReference.FileName, "-999", organisationIdentifier.Value);

            ConfigureCosmosDbGetFile(documentReference, fileMetadata);
            ConfigureCosmosDbUpdateDocumentMetadata(documentReference.BatchIdentifier, fileMetadata);
            ConfigureMapper(user);
            ConfigureFileMetadataEncryptor(user, encryptedFileMetadataUser);
            ConfigureDirectoriesManager();

            var utcNow = new DateTime(2020, 6, 1);
            ConfigureSystemProvider(utcNow);

            ConfigureDownloadDirectory(documentReference.FileName);

            var expectedHistory = new FileMetadataHistory
            {
                Action = Enums.FileAction.ViewedByReceiver,
                ActionDateTimeUtc = utcNow,
                User = encryptedFileMetadataUser
            };

            // Act
            var result = await _documentDownloader.DownloadExchangeDocument(downloadRequest);

            // Assert
            result.Should().NotBeEmpty();
            fileMetadata.History.Should().ContainEquivalentOf(expectedHistory);

            VerifyMocks();
        }

        [TestMethod]
        [TestCategory("Unit")]
        public async Task DownloadExchangeDocument_Returns200OKAndUpdatesFileHistory_WhenSingleDocumentIsRequestedAndUserIsViewAsProviderReceiver()
        {
            // Arrange
            var documentReference = CreateDocumentReference("file-name.pdf", "batch-id", "parent-batch-id");

            var organisationIdentifier = new OrganisationIdentifier { Value = "12345678" };
            var user = CreateTestUserInfo(organisationIdentifier, true);
            var encryptedFileMetadataUser = CreateTestEncryptedFileMetadataUser();

            var downloadRequest = CreateDownloadRequest(user, documentReference);

            var fileMetadata = CreateFileMetadata(documentReference.FileName, "-999", organisationIdentifier.Value);

            ConfigureCosmosDbGetFile(documentReference, fileMetadata);
            ConfigureCosmosDbUpdateDocumentMetadata(documentReference.BatchIdentifier, fileMetadata);
            ConfigureMapper(user);
            ConfigureFileMetadataEncryptor(user, encryptedFileMetadataUser);
            ConfigureDirectoriesManager();

            var utcNow = new DateTime(2020, 6, 1);
            ConfigureSystemProvider(utcNow);

            ConfigureDownloadDirectory(documentReference.FileName);

            var expectedHistory = new FileMetadataHistory
            {
                Action = Enums.FileAction.ViewAsProviderDownloadedReceived,
                ActionDateTimeUtc = utcNow,
                User = encryptedFileMetadataUser
            };

            // Act
            var result = await _documentDownloader.DownloadExchangeDocument(downloadRequest);

            // Assert
            result.Should().NotBeEmpty();
            fileMetadata.History.Should().ContainEquivalentOf(expectedHistory);

            VerifyMocks();
        }

        #endregion


        #region Multiple documents download

        [TestMethod]
        [TestCategory("Unit")]
        public async Task DownloadExchangeDocument_Returns200OKAndUpdatesFileHistory_WhenMultipleDocumentsAreRequestedAndUserIsSender()
        {
            // Arrange
            var documentReference1 = CreateDocumentReference("file-name-1.pdf", "batch-id-1", "parent-batch-id-1");
            var documentReference2 = CreateDocumentReference("file-name-2.pdf", "batch-id-2", "parent-batch-id-2");
            var documentReferences = new[] { documentReference1, documentReference2 };

            var organisationIdentifier = new OrganisationIdentifier { Value = "12345678" };
            var user = CreateTestUserInfo(organisationIdentifier);
            var encryptedFileMetadataUser = CreateTestEncryptedFileMetadataUser();

            var downloadRequest = CreateDownloadRequest(user, documentReference1, documentReference2);

            var fileMetadata1 = CreateFileMetadata(documentReference1.FileName, organisationIdentifier.Value);
            var fileMetadata2 = CreateFileMetadata(documentReference2.FileName, organisationIdentifier.Value);
            var files = new[] { fileMetadata1, fileMetadata2 };

            ConfigureCosmosDbGetFiles(documentReferences, files);
            ConfigureCosmosDbUpdateDocumentMetadata(files, documentReferences);
            ConfigureMapper(user);
            ConfigureFileMetadataEncryptor(user, encryptedFileMetadataUser);

            var utcNow = new DateTime(2020, 6, 1);
            ConfigureSystemProvider(utcNow);

            ConfigureDirectoriesManager();
            ConfigureDownloadDirectory(documentReferences);

            var mockZippedFilesContent = new byte[] { 1, 2, 3, 4, 5 };
            ConfigureZipService(documentReferences, mockZippedFilesContent);

            var expectedHistory = new FileMetadataHistory
            {
                Action = Enums.FileAction.ViewedBySender,
                ActionDateTimeUtc = utcNow,
                User = encryptedFileMetadataUser
            };

            // Act
            var result = await _documentDownloader.DownloadExchangeDocument(downloadRequest);

            // Assert
            result.Should().BeEquivalentTo(mockZippedFilesContent);

            foreach (var currentFile in files)
            {
                currentFile.History.Should().ContainEquivalentOf(expectedHistory);
            }

            VerifyMocks();
        }

        [TestMethod]
        [TestCategory("Unit")]
        public async Task DownloadExchangeDocument_Returns200OKAndUpdatesFileHistory_WhenMultipleDocumentsAreRequestedAndUserIsViewAsProviderSender()
        {
            // Arrange
            var documentReference1 = CreateDocumentReference("file-name-1.pdf", "batch-id-1", "parent-batch-id-1");
            var documentReference2 = CreateDocumentReference("file-name-2.pdf", "batch-id-2", "parent-batch-id-2");
            var documentReferences = new[] { documentReference1, documentReference2 };

            var organisationIdentifier = new OrganisationIdentifier { Value = "12345678" };
            var user = CreateTestUserInfo(organisationIdentifier, true);
            var encryptedFileMetadataUser = CreateTestEncryptedFileMetadataUser();

            var downloadRequest = CreateDownloadRequest(user, documentReference1, documentReference2);

            var fileMetadata1 = CreateFileMetadata(documentReference1.FileName, organisationIdentifier.Value);
            var fileMetadata2 = CreateFileMetadata(documentReference2.FileName, organisationIdentifier.Value);
            var files = new[] { fileMetadata1, fileMetadata2 };

            ConfigureCosmosDbGetFiles(documentReferences, files);
            ConfigureCosmosDbUpdateDocumentMetadata(files, documentReferences);
            ConfigureMapper(user);
            ConfigureFileMetadataEncryptor(user, encryptedFileMetadataUser);

            var utcNow = new DateTime(2020, 6, 1);
            ConfigureSystemProvider(utcNow);

            ConfigureDirectoriesManager();
            ConfigureDownloadDirectory(documentReferences);

            var mockZippedFilesContent = new byte[] { 1, 2, 3, 4, 5 };
            ConfigureZipService(documentReferences, mockZippedFilesContent);

            var expectedHistory = new FileMetadataHistory
            {
                Action = Enums.FileAction.ViewAsProviderDownloadedSent,
                ActionDateTimeUtc = utcNow,
                User = encryptedFileMetadataUser
            };

            // Act
            var result = await _documentDownloader.DownloadExchangeDocument(downloadRequest);

            // Assert
            result.Should().BeEquivalentTo(mockZippedFilesContent);

            foreach (var currentFile in files)
            {
                currentFile.History.Should().ContainEquivalentOf(expectedHistory);
            }

            VerifyMocks();
        }

        [TestMethod]
        [TestCategory("Unit")]
        public async Task DownloadExchangeDocument_Returns200OKAndUpdatesFileHistory_WhenMultipleDocumentsAreRequestedAndUserIsReceiver()
        {
            // Arrange
            var documentReference1 = CreateDocumentReference("file-name-1.pdf", "batch-id-1", "parent-batch-id-1");
            var documentReference2 = CreateDocumentReference("file-name-2.pdf", "batch-id-2", "parent-batch-id-2");
            var documentReferences = new[] { documentReference1, documentReference2 };

            var organisationIdentifier = new OrganisationIdentifier { Value = "12345678" };
            var user = CreateTestUserInfo(organisationIdentifier);
            var encryptedFileMetadataUser = CreateTestEncryptedFileMetadataUser();

            var downloadRequest = CreateDownloadRequest(user, documentReference1, documentReference2);

            var fileMetadata1 = CreateFileMetadata(documentReference1.FileName, "-999", organisationIdentifier.Value);
            var fileMetadata2 = CreateFileMetadata(documentReference2.FileName, "-999", organisationIdentifier.Value);
            var files = new[] { fileMetadata1, fileMetadata2 };

            ConfigureCosmosDbGetFiles(documentReferences, files);
            ConfigureCosmosDbUpdateDocumentMetadata(files, documentReferences);
            ConfigureMapper(user);
            ConfigureFileMetadataEncryptor(user, encryptedFileMetadataUser);

            var utcNow = new DateTime(2020, 6, 1);
            ConfigureSystemProvider(utcNow);

            ConfigureDirectoriesManager();
            ConfigureDownloadDirectory(documentReferences);

            var mockZippedFilesContent = new byte[] { 1, 2, 3, 4, 5 };
            ConfigureZipService(documentReferences, mockZippedFilesContent);

            var expectedHistory = new FileMetadataHistory
            {
                Action = Enums.FileAction.ViewedByReceiver,
                ActionDateTimeUtc = utcNow,
                User = encryptedFileMetadataUser
            };

            // Act
            var result = await _documentDownloader.DownloadExchangeDocument(downloadRequest);

            // Assert
            result.Should().BeEquivalentTo(mockZippedFilesContent);

            foreach (var currentFile in files)
            {
                currentFile.History.Should().ContainEquivalentOf(expectedHistory);
            }

            VerifyMocks();
        }

        [TestMethod]
        [TestCategory("Unit")]
        public async Task DownloadExchangeDocument_Returns200OKAndUpdatesFileHistory_WhenMultipleDocumentsAreRequestedAndUserIsViewAsProviderReceiver()
        {
            // Arrange
            var documentReference1 = CreateDocumentReference("file-name-1.pdf", "batch-id-1", "parent-batch-id-1");
            var documentReference2 = CreateDocumentReference("file-name-2.pdf", "batch-id-2", "parent-batch-id-2");
            var documentReferences = new[] { documentReference1, documentReference2 };

            var organisationIdentifier = new OrganisationIdentifier { Value = "12345678" };
            var user = CreateTestUserInfo(organisationIdentifier, true);
            var encryptedFileMetadataUser = CreateTestEncryptedFileMetadataUser();

            var downloadRequest = CreateDownloadRequest(user, documentReference1, documentReference2);

            var fileMetadata1 = CreateFileMetadata(documentReference1.FileName, "-999", organisationIdentifier.Value);
            var fileMetadata2 = CreateFileMetadata(documentReference2.FileName, "-999", organisationIdentifier.Value);
            var files = new[] { fileMetadata1, fileMetadata2 };

            ConfigureCosmosDbGetFiles(documentReferences, files);
            ConfigureCosmosDbUpdateDocumentMetadata(files, documentReferences);
            ConfigureMapper(user);
            ConfigureFileMetadataEncryptor(user, encryptedFileMetadataUser);

            var utcNow = new DateTime(2020, 6, 1);
            ConfigureSystemProvider(utcNow);

            ConfigureDirectoriesManager();
            ConfigureDownloadDirectory(documentReferences);

            var mockZippedFilesContent = new byte[] { 1, 2, 3, 4, 5 };
            ConfigureZipService(documentReferences, mockZippedFilesContent);

            var expectedHistory = new FileMetadataHistory
            {
                Action = Enums.FileAction.ViewAsProviderDownloadedReceived,
                ActionDateTimeUtc = utcNow,
                User = encryptedFileMetadataUser
            };

            // Act
            var result = await _documentDownloader.DownloadExchangeDocument(downloadRequest);

            // Assert
            result.Should().BeEquivalentTo(mockZippedFilesContent);

            foreach (var currentFile in files)
            {
                currentFile.History.Should().ContainEquivalentOf(expectedHistory);
            }

            VerifyMocks();
        }

        #endregion


        [TestMethod]
        [TestCategory("Unit")]
        public async Task DownloadExchangeDocument_ReturnsNullAndLogsError_WhenGetFileReturnsNull()
        {
            // Arrange
            var documentReference = CreateDocumentReference("file-name.pdf", "batch-id", "parent-batch-id");

            var organisationIdentifier = new OrganisationIdentifier { Value = "12345678" };
            var user = CreateTestUserInfo(organisationIdentifier);
            var encryptedFileMetadataUser = CreateTestEncryptedFileMetadataUser();

            var downloadRequest = CreateDownloadRequest(user, documentReference);

            FileMetadata fileMetadata = null;
            ConfigureCosmosDbGetFile(documentReference, fileMetadata);

            ConfigureMapper(user);
            ConfigureFileMetadataEncryptor(user, encryptedFileMetadataUser);

            _mockLogger.Setup(l => l.LogError($"File: {documentReference.FileName} not found."));

            // Act
            var result = await _documentDownloader.DownloadExchangeDocument(downloadRequest);

            // Assert
            result.Should().BeNull();
            VerifyMocks();
        }

        [TestMethod]
        [TestCategory("Unit")]
        public async Task DownloadExchangeDocument_ReturnsNullAndLogsError_WhenRequestIsEmpty()
        {
            // Arrange
            var doc = new List<DocumentReference>();

            var user = new UserInfo
            {
                EmailAddress = "abc@test.com",
                FullName = "Docex User",
                Principal = "someString",
                OrganisationInfo = new OrganisationInfo
                {
                    OrganisationIdentifier = new OrganisationIdentifier
                    {
                        Type = OrganisationIdentifierType.Ukprn,
                        Value = "12345678"
                    }
                }
            };

            var downloadRequest = new ExchangeDocumentDownloadRequest
            {
                ListOptions = new ExchangeListDocumentOptions
                {
                    DocumentReferences = doc
                },
                UserInfo = user
            };

            _mockLogger.Setup(l => l.LogError("Empty request received, cannot process further."));

            // Act
            await _documentDownloader.DownloadExchangeDocument(downloadRequest);

            // Assert
            VerifyMocks();
        }

        [TestMethod]
        [TestCategory("Unit")]
        public async Task DownloadAgencyDocument_ReturnsFileContent_WhenCalled()
        {
            // Arrange
            var team = "team";
            var fileName = "file name";

            _mockDirectoriesManager
                .Setup(manager => manager.GetDirectory(team))
                .ReturnsAsync(_downloadMockDirectory.Object);

            var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes("file content"));
            Stream stream = memoryStream;

            _downloadMockDirectory.Setup(a => a.Read(fileName))
                .ReturnsAsync(stream);

            // Act
            await _documentDownloader.DownloadAgencyDocument(team, fileName);

            // Assert
            VerifyMocks();
        }

        #region Private

        private DocumentReference CreateDocumentReference(string fileName, string batchId, string parentBatchId)
        {
            return new DocumentReference
            {
                FileName = fileName,
                BatchIdentifier = batchId,
                ParentBatchIdentifier = parentBatchId
            };
        }

        private UserInfo CreateTestUserInfo(
            OrganisationIdentifier organisationIdentifier,
            bool isViewAsProvider = false)
        {
            return new UserInfo
            {
                Principal = "user-principal",
                FullName = "user-full-name",
                EmailAddress = "user-email@education.gov.uk",
                OrganisationInfo = new OrganisationInfo
                {
                    OrganisationIdentifier = organisationIdentifier
                },
                IsViewAsProvider = isViewAsProvider
            };
        }

        private ExchangeDocumentDownloadRequest CreateDownloadRequest(
            UserInfo userInfo,
            params DocumentReference[] documentReferences)
        {
            return new ExchangeDocumentDownloadRequest
            {
                ListOptions = new ExchangeListDocumentOptions
                {
                    DocumentReferences = documentReferences
                },
                UserInfo = userInfo
            };
        }

        private FileMetadata CreateFileMetadata(string fileName, string fromUkprn, string toUkprn = null)
        {
            return new FileMetadata
            {
                FileName = fileName,
                FromUkprn = int.Parse(fromUkprn),
                ToUkprn = string.IsNullOrWhiteSpace(toUkprn) ? default : int.Parse(toUkprn)
            };
        }

        private FileMetadataUser CreateTestEncryptedFileMetadataUser()
        {
            return new FileMetadataUser
            {
                Principal = "encrypted-principal",
                FullName = "encrypted-full-name",
                EmailAddress = "encrypted-email-address",
                IsEncrypted = true
            };
        }

        private void ConfigureCosmosDbGetFile(
            DocumentReference documentReference,
            FileMetadata fileMetadata)
        {
            _mockCosmosDbService
                .Setup(dbService => dbService.GetFile(It.Is<DocumentReference>(docRef =>
                    docRef.ParentBatchIdentifier == documentReference.ParentBatchIdentifier
                    && docRef.BatchIdentifier == documentReference.BatchIdentifier
                    && docRef.FileName == documentReference.FileName)))
                .ReturnsAsync(fileMetadata);
        }

        private void ConfigureCosmosDbGetFiles(
            IEnumerable<DocumentReference> documentReferences,
            IEnumerable<FileMetadata> files)
        {
            foreach (var currentDocumentReference in documentReferences)
            {
                _mockCosmosDbService
                .Setup(db => db.GetFile(It.Is<DocumentReference>(docRef =>
                    docRef.ParentBatchIdentifier == currentDocumentReference.ParentBatchIdentifier
                    && docRef.BatchIdentifier == currentDocumentReference.BatchIdentifier
                    && docRef.FileName == currentDocumentReference.FileName)))
                .ReturnsAsync((DocumentReference docRef) => files.Single(file => file.FileName == docRef.FileName));
            }
        }

        private void ConfigureCosmosDbUpdateDocumentMetadata(string batchIdentifier, FileMetadata fileMetadata)
        {
            _mockCosmosDbService
                .Setup(
                    dbService => dbService.UpdateDocumentMetadata(
                        fileMetadata,
                        batchIdentifier,
                        It.Is<List<string>>(list => list.Contains("History")),
                        null))
                .Returns(Task.CompletedTask);
        }

        private void ConfigureCosmosDbUpdateDocumentMetadata(
            IEnumerable<FileMetadata> files,
            IEnumerable<DocumentReference> documentReferences)
        {
            _mockCosmosDbService
                .Setup(
                    dbService => dbService.UpdateDocumentMetadata(
                        It.Is<FileMetadata>(fileMetadata => files.Contains(fileMetadata)),
                        It.Is<string>(batchId => documentReferences.Any(docRef => docRef.BatchIdentifier == batchId)),
                        It.Is<List<string>>(list => list.Contains("History")),
                        null))
                .Returns(Task.CompletedTask);
        }

        private void ConfigureMapper(UserInfo user)
        {
            _mapper
                .Setup(m => m.Map<UserInfo, FileMetadataUser>(user))
                .Returns(
                    new FileMetadataUser
                    {
                        Principal = user.Principal,
                        FullName = user.FullName,
                        EmailAddress = user.EmailAddress,
                        IsEncrypted = false
                    });
        }

        private void ConfigureFileMetadataEncryptor(UserInfo user, FileMetadataUser fileMetadataUser)
        {
            Expression<Func<FileMetadataUser, bool>> encryptUserInputExpression =
                fileMetadataUser => fileMetadataUser.Principal == user.Principal &&
                                    fileMetadataUser.FullName == user.FullName &&
                                    fileMetadataUser.EmailAddress == user.EmailAddress &&
                                    !fileMetadataUser.IsEncrypted;

            _fileMetadataUserEncryptor
                .Setup(e => e.Encrypt(It.Is(encryptUserInputExpression)))
                .Returns(fileMetadataUser);
        }

        private void ConfigureSystemProvider(DateTime dateTime)
        {
            _systemProvider
                .Setup(systemProvider => systemProvider.DateTime.UtcNow())
                .Returns(dateTime);
        }

        private void ConfigureDownloadDirectory(string fileName)
        {
            var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes("whatever"));

            _downloadMockDirectory.Setup(a => a.Read(fileName))
                .ReturnsAsync(memoryStream);
        }

        private void ConfigureDownloadDirectory(IEnumerable<DocumentReference> documentReferences)
        {
            var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes("whatever"));

            _downloadMockDirectory.Setup(
                    a => a.Read(
                        It.Is<string>(fileName => documentReferences.Any(docRef => docRef.FileName == fileName))))
                .ReturnsAsync(memoryStream);
        }

        private void ConfigureZipService(IEnumerable<DocumentReference> documentReferences, byte[] zipContent)
        {
            Expression<Func<IEnumerable<(string Name, byte[] Content)>, bool>> zipInputExpression =
                fileTuple => fileTuple.Select(f => f.Name)
                    .All(fileName => documentReferences.Any(d => d.FileName == fileName));

            _zipService
                .Setup(zip => zip.ZipFiles(It.Is(zipInputExpression)))
                .Returns(zipContent);
        }

        private void ConfigureDirectoriesManager()
            => _mockDirectoriesManager
                .Setup(manager => manager.GetDirectory(_downloaderConfiguration.DownloadDirectoryKey))
                .ReturnsAsync(_downloadMockDirectory.Object);

        private void VerifyMocks()
        {
            Mock.VerifyAll(
                _mockDirectoriesManager,
                _mockCosmosDbService,
                _fileMetadataUserEncryptor,
                _systemProvider,
                _mapper,
                _zipService,
                _downloadMockDirectory,
                _mockLogger);
        }

        #endregion
    }
}