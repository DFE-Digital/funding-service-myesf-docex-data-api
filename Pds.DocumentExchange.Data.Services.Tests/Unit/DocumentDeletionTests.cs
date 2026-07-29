using FluentAssertions;
using MapsterMapper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pds.Core.Logging;
using Pds.Core.Utils;
using Pds.DocumentExchange.Data.Services.DTOs;
using Pds.DocumentExchange.Data.Services.DTOs.Configuration;
using Pds.DocumentExchange.Data.Services.DTOs.User;
using Pds.DocumentExchange.Data.Services.Enums;
using Pds.DocumentExchange.Data.Services.Implementations;
using Pds.DocumentExchange.Data.Services.Interfaces;
using Pds.DocumentExchange.Data.Services.Interfaces.Storage;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Tests.Unit
{
    [TestClass]
    public class DocumentDeletionTests
    {
        private readonly Mock<ICosmosDbService> _cosmosDbService = new Mock<ICosmosDbService>(MockBehavior.Strict);
        private readonly Mock<IDirectoriesManager> _directoriesManager = new Mock<IDirectoriesManager>(MockBehavior.Strict);
        private readonly Mock<ISystemProvider> _systemProvider = new Mock<ISystemProvider>(MockBehavior.Strict);
        private readonly Mock<IFileMetadataUserEncryptor> _fileMetadataUserEncryptor = new Mock<IFileMetadataUserEncryptor>(MockBehavior.Strict);
        private readonly Mock<IBatchToExchangeDocumentConverter> _batchToExchangeDocumentConverter = new Mock<IBatchToExchangeDocumentConverter>(MockBehavior.Strict);
        private readonly Mock<IMapper> _mapper = new Mock<IMapper>(MockBehavior.Strict);
        private readonly Mock<ILoggerAdapter<DocumentDeletion>> _logger = new Mock<ILoggerAdapter<DocumentDeletion>>(MockBehavior.Loose);

        private readonly VirusScanResultProcessorConfiguration _config = new VirusScanResultProcessorConfiguration
        {
            FileCopyDestinationDirectoryKey = "the-destination-copy-directory"
        };

        private readonly Mock<IDirectory> _directory = new Mock<IDirectory>(MockBehavior.Strict);

        private readonly DocumentDeletion _documentDeletion;

        public DocumentDeletionTests()
        {
            _documentDeletion = new DocumentDeletion(
                _cosmosDbService.Object,
                _directoriesManager.Object,
                _systemProvider.Object,
                _fileMetadataUserEncryptor.Object,
                _batchToExchangeDocumentConverter.Object,
                _mapper.Object,
                _config,
                _logger.Object);
        }

        [TestMethod, TestCategory("Unit")]
        public void DeleteDocuments_WhenExchangeDocumentDeleteRequestIsNull_Throws()
        {
            // Act
            Func<Task> func = async () => await _documentDeletion.DeleteDocuments(null);

            // Assert
            func.Should().ThrowAsync<ArgumentNullException>();
        }

        [TestMethod, TestCategory("Unit")]
        public void DeleteDocuments_WhenUserInfoIsNull_Throws()
        {
            // Arrange
            var request = new ExchangeDocumentDeleteRequest
            {
                UserInfo = null,
                DocumentReferences = Enumerable.Empty<DocumentReferenceWithPreviousVersions>()
            };

            // Act
            Func<Task> func = async () => await _documentDeletion.DeleteDocuments(request);

            // Assert
            func.Should().ThrowAsync<ArgumentNullException>();
        }

        [TestMethod, TestCategory("Unit")]
        public void DeleteDocuments_WhenDocumentReferencesIsNullOrEmpty_Throws()
        {
            // Arrange
            var userInfo = CreateUserInfo("user-principal");

            var requestWithNullDocRefs = new ExchangeDocumentDeleteRequest
            {
                UserInfo = userInfo,
                DocumentReferences = null
            };

            var requestWithEmptyDocRefs = new ExchangeDocumentDeleteRequest
            {
                UserInfo = userInfo,
                DocumentReferences = Enumerable.Empty<DocumentReferenceWithPreviousVersions>()
            };

            // Act
            Func<Task> funcNullDocRefs = async () => await _documentDeletion.DeleteDocuments(requestWithNullDocRefs);
            Func<Task> funcEmptyDocRefs = async () => await _documentDeletion.DeleteDocuments(requestWithEmptyDocRefs);

            // Assert
            funcNullDocRefs.Should().ThrowAsync<ArgumentNullException>();
            funcEmptyDocRefs.Should().ThrowAsync<ArgumentNullException>();
        }

        [TestMethod, TestCategory("Unit")]
        public async Task DeleteDocuments_WhenNoFilesFound_ReturnsEmptyCollection()
        {
            // Arrange
            var userInfo = CreateUserInfo("user-principal");
            var documentReference = CreateDocumentReference("file-name.pdf", "batch-identifier", "parent-batch-identifier");

            var request = new ExchangeDocumentDeleteRequest
            {
                UserInfo = userInfo,
                DocumentReferences = new[] { documentReference }
            };

            SetupCosmosDbGetFile(documentReference, null);

            // Act
            var result = await _documentDeletion.DeleteDocuments(request);

            // Assert
            result.Should().BeEmpty();
            VerifyAllMocks();
        }

        [TestMethod, TestCategory("Unit")]
        public async Task DeleteDocuments_WhenFilesAlreadyDeleted_ReturnsEmptyCollection()
        {
            // Arrange
            var userInfo = CreateUserInfo("user-principal");
            var documentReference = CreateDocumentReference("file-name.pdf", "batch-identifier", "parent-batch-identifier");

            var file = CreateFileMetadata(documentReference.FileName, true);

            var request = new ExchangeDocumentDeleteRequest
            {
                UserInfo = userInfo,
                DocumentReferences = new[] { documentReference }
            };

            SetupCosmosDbGetFile(documentReference, file);

            // Act
            var result = await _documentDeletion.DeleteDocuments(request);

            // Assert
            result.Should().BeEmpty();
            VerifyAllMocks();
        }

        [TestMethod, TestCategory("Unit")]
        public async Task DeleteDocuments_WhenFileIsFound_DeletesDocument()
        {
            // Arrange
            var userInfo = CreateUserInfo("user-principal");
            var documentReference = CreateDocumentReference("file-name.pdf", "batch-identifier", "parent-batch-identifier");

            var file = CreateFileMetadata(documentReference.FileName, false);

            var mappedFileMatadataUser = CreateFileMetadataUser(userInfo.Principal);
            var encryptedFileMetadataUser = CreateFileMetadataUser("encrypted-user-principal");

            var request = new ExchangeDocumentDeleteRequest
            {
                UserInfo = userInfo,
                DocumentReferences = new[] { documentReference }
            };

            var document = new ExchangeDocument { DocumentReference = documentReference };

            var expectedResult = new[]
            {
                document
            };

            SetupCosmosDbGetFile(documentReference, file);
            SetupCosmosDbUpdateDocumentMetadata(documentReference.BatchIdentifier, file);
            SetupMapper(userInfo, mappedFileMatadataUser);
            SetupFileMetadataUserEncryptor(mappedFileMatadataUser, encryptedFileMetadataUser);
            SetupDirectoriesManager();

            var actionDateTime = new DateTime(2020, 1, 1);
            SetupSystemProvider(actionDateTime);

            SetupDirectoryDelete(documentReference.FileName);
            SetupConvertBatchToExchangeDocument(documentReference.ParentBatchIdentifier, documentReference.BatchIdentifier, file, document);

            // Act
            var result = await _documentDeletion.DeleteDocuments(request);

            // Assert
            result.Should().BeEquivalentTo(expectedResult);
            VerifyAllMocks();
        }

        [TestMethod, TestCategory("Unit")]
        public async Task DeleteDocuments_WhenOnlySomeFilesAreFound_DeletesFoundDocuments()
        {
            // Arrange
            var userInfo = CreateUserInfo("user-principal");

            var documentReference1 = CreateDocumentReference("file-name-01.pdf", "batch-identifier-01", "parent-batch-identifier-01");
            var documentReference2 = CreateDocumentReference("file-name-02.pdf", "batch-identifier-02", "parent-batch-identifier-02");
            var documentReference3 = CreateDocumentReference("file-name-03.pdf", "batch-identifier-03", "parent-batch-identifier-03");

            var file1 = CreateFileMetadata(documentReference1.FileName, false);
            var file3 = CreateFileMetadata(documentReference3.FileName, false);

            var mappedFileMatadataUser = CreateFileMetadataUser(userInfo.Principal);
            var encryptedFileMetadataUser = CreateFileMetadataUser("encrypted-user-principal");

            var request = new ExchangeDocumentDeleteRequest
            {
                UserInfo = userInfo,
                DocumentReferences = new[] { documentReference1, documentReference2, documentReference3 }
            };

            var document1 = new ExchangeDocument { DocumentReference = documentReference1 };
            var document3 = new ExchangeDocument { DocumentReference = documentReference3 };

            var expectedResult = new[]
            {
                document1,
                document3
            };

            SetupCosmosDbGetFile(documentReference1, file1);
            SetupCosmosDbGetFile(documentReference2, null);
            SetupCosmosDbGetFile(documentReference3, file3);

            SetupCosmosDbUpdateDocumentMetadata(documentReference1.BatchIdentifier, file1);
            SetupCosmosDbUpdateDocumentMetadata(documentReference3.BatchIdentifier, file3);

            SetupMapper(userInfo, mappedFileMatadataUser);
            SetupFileMetadataUserEncryptor(mappedFileMatadataUser, encryptedFileMetadataUser);

            var actionDateTime = new DateTime(2020, 1, 1);
            SetupSystemProvider(actionDateTime);

            SetupDirectoriesManager();
            SetupDirectoryDelete(documentReference1.FileName);
            SetupDirectoryDelete(documentReference3.FileName);

            SetupConvertBatchToExchangeDocument(documentReference1.ParentBatchIdentifier, documentReference1.BatchIdentifier, file1, document1);
            SetupConvertBatchToExchangeDocument(documentReference3.ParentBatchIdentifier, documentReference3.BatchIdentifier, file3, document3);

            // Act
            var result = await _documentDeletion.DeleteDocuments(request);

            // Assert
            result.Should().BeEquivalentTo(expectedResult);
            VerifyAllMocks();
        }

        [TestMethod, TestCategory("Unit")]
        public void DeleteDocument_WhenDocumentReferencesIsNullOrEmpty_Throws()
        {
            // Arrange
            var userInfo = CreateUserInfo("user-principal");

            var requestWithNullDocRefs = new ExchangeDocumentDeleteRequest
            {
                UserInfo = userInfo,
                DocumentReferences = null
            };

            var requestWithEmptyDocRefs = new ExchangeDocumentDeleteRequest
            {
                UserInfo = userInfo,
                DocumentReferences = Enumerable.Empty<DocumentReferenceWithPreviousVersions>()
            };

            // Act
            Func<Task> funcNullDocRefs = async () => await _documentDeletion.DeleteDocument(requestWithNullDocRefs);
            Func<Task> funcEmptyDocRefs = async () => await _documentDeletion.DeleteDocument(requestWithEmptyDocRefs);

            // Assert
            funcNullDocRefs.Should().ThrowAsync<ArgumentNullException>();
            funcEmptyDocRefs.Should().ThrowAsync<ArgumentNullException>();
        }

        [TestMethod, TestCategory("Unit")]
        public async Task DeleteDocument_WhenNoFilesFound_ReturnsNullObject()
        {
            // Arrange
            var userInfo = CreateUserInfo("user-principal");
            var documentReference = CreateDocumentReference("file-name.pdf", "batch-identifier", "parent-batch-identifier");

            var request = new ExchangeDocumentDeleteRequest
            {
                UserInfo = userInfo,
                DocumentReferences = new[] { documentReference }
            };

            SetupCosmosDbGetFile(documentReference, null);

            // Act
            var result = await _documentDeletion.DeleteDocument(request);

            // Assert
            result.Should().BeNull();
            VerifyAllMocks();
        }

        [TestMethod, TestCategory("Unit")]
        public async Task DeleteDocument_WhenFileIsFound_DeletesDocument()
        {
            // Arrange
            var userInfo = CreateUserInfo("user-principal");
            var documentReference = CreateDocumentReference("file-name.pdf", "batch-identifier", "parent-batch-identifier");

            var file = CreateFileMetadata(documentReference.FileName, false);

            var mappedFileMatadataUser = CreateFileMetadataUser(userInfo.Principal);
            var encryptedFileMetadataUser = CreateFileMetadataUser("encrypted-user-principal");

            var request = new ExchangeDocumentDeleteRequest
            {
                UserInfo = userInfo,
                DocumentReferences = new[] { documentReference }
            };

            var document = new ExchangeDocument { DocumentReference = documentReference };

            var expectedResult = document;

            SetupCosmosDbGetFile(documentReference, file);
            SetupCosmosDbUpdateDocumentMetadata(documentReference.BatchIdentifier, file);
            SetupMapper(userInfo, mappedFileMatadataUser);
            SetupFileMetadataUserEncryptor(mappedFileMatadataUser, encryptedFileMetadataUser);
            SetupDirectoriesManager();

            var actionDateTime = new DateTime(2020, 1, 1);
            SetupSystemProvider(actionDateTime);

            SetupDirectoryDelete(documentReference.FileName);
            SetupConvertBatchToExchangeDocument(documentReference.ParentBatchIdentifier, documentReference.BatchIdentifier, file, document);

            // Act
            var result = await _documentDeletion.DeleteDocument(request);

            // Assert
            result.Should().BeEquivalentTo(expectedResult);
            VerifyAllMocks();
        }

        [TestMethod, TestCategory("Unit")]
        public async Task DeleteDocument_WithPreviousVersions_DeletesDocumentAndPreviousVersions()
        {
            // Arrange
            var userInfo = CreateUserInfo("user-principal");

            var documentReference = CreateDocumentReference("file-name.pdf", "batch-identifier", "parent-batch-identifier");
            var previousVersionRef1 = CreateDocumentReference("file-name-01.pdf", "batch-identifier-01", "parent-batch-identifier-01");
            var previousVersionRef2 = CreateDocumentReference("file-name-02.pdf", "batch-identifier-02", "parent-batch-identifier-02");
            var previousVersionRef3 = CreateDocumentReference("file-name-03.pdf", "batch-identifier-03", "parent-batch-identifier-03");

            documentReference.PreviousVersions = new[] { previousVersionRef1, previousVersionRef2, previousVersionRef3 };

            var file = CreateFileMetadata(documentReference.FileName, false);
            var previousVersionFile1 = CreateFileMetadata(previousVersionRef1.FileName, false);
            var previousVersionFile2 = CreateFileMetadata(previousVersionRef2.FileName, false);
            var previousVersionFile3 = CreateFileMetadata(previousVersionRef3.FileName, false);

            var mappedFileMatadataUser = CreateFileMetadataUser(userInfo.Principal);
            var encryptedFileMetadataUser = CreateFileMetadataUser("encrypted-user-principal");

            var request = new ExchangeDocumentDeleteRequest
            {
                UserInfo = userInfo,
                DocumentReferences = new[] { documentReference }
            };

            var document = new ExchangeDocument { DocumentReference = documentReference };
            var previousVersionDoc1 = new ExchangeDocument { DocumentReference = previousVersionRef1 };
            var previousVersionDoc2 = new ExchangeDocument { DocumentReference = previousVersionRef2 };
            var previousVersionDoc3 = new ExchangeDocument { DocumentReference = previousVersionRef3 };

            var expectedResult = new[]
            {
                new ExchangeDocument
                {
                    DocumentReference = documentReference,
                    PreviousVersions = new[]
                    {
                        previousVersionDoc1,
                        previousVersionDoc2,
                        previousVersionDoc3
                    }
                }
            };

            SetupCosmosDbGetFile(documentReference, file);
            SetupCosmosDbGetFile(previousVersionRef1, previousVersionFile1);
            SetupCosmosDbGetFile(previousVersionRef2, previousVersionFile2);
            SetupCosmosDbGetFile(previousVersionRef3, previousVersionFile3);

            SetupCosmosDbUpdateDocumentMetadata(documentReference.BatchIdentifier, file);
            SetupCosmosDbUpdateDocumentMetadata(previousVersionRef1.BatchIdentifier, previousVersionFile1);
            SetupCosmosDbUpdateDocumentMetadata(previousVersionRef2.BatchIdentifier, previousVersionFile2);
            SetupCosmosDbUpdateDocumentMetadata(previousVersionRef3.BatchIdentifier, previousVersionFile3);

            SetupMapper(userInfo, mappedFileMatadataUser);
            SetupFileMetadataUserEncryptor(mappedFileMatadataUser, encryptedFileMetadataUser);
            SetupDirectoriesManager();

            var actionDateTime = new DateTime(2020, 1, 1);
            SetupSystemProvider(actionDateTime);

            SetupDirectoryDelete(documentReference.FileName);
            SetupDirectoryDelete(previousVersionRef1.FileName);
            SetupDirectoryDelete(previousVersionRef2.FileName);
            SetupDirectoryDelete(previousVersionRef3.FileName);

            SetupConvertBatchToExchangeDocument(documentReference.ParentBatchIdentifier, documentReference.BatchIdentifier, file, document);
            SetupConvertBatchToExchangeDocument(previousVersionRef1.ParentBatchIdentifier, previousVersionRef1.BatchIdentifier, previousVersionFile1, previousVersionDoc1);
            SetupConvertBatchToExchangeDocument(previousVersionRef2.ParentBatchIdentifier, previousVersionRef2.BatchIdentifier, previousVersionFile2, previousVersionDoc2);
            SetupConvertBatchToExchangeDocument(previousVersionRef3.ParentBatchIdentifier, previousVersionRef3.BatchIdentifier, previousVersionFile3, previousVersionDoc3);

            // Act
            var result = await _documentDeletion.DeleteDocuments(request);

            // Assert
            result.Should().BeEquivalentTo(expectedResult);
            VerifyAllMocks();
        }

        [TestMethod, TestCategory("Unit")]
        public async Task DeleteDocument_WhenFileAlreadyDeleted_ReturnsNullObject()
        {
            // Arrange
            var userInfo = CreateUserInfo("user-principal");
            var documentReference = CreateDocumentReference("file-name.pdf", "batch-identifier", "parent-batch-identifier");

            var file = CreateFileMetadata(documentReference.FileName, true);

            var request = new ExchangeDocumentDeleteRequest
            {
                UserInfo = userInfo,
                DocumentReferences = new[] { documentReference }
            };

            SetupCosmosDbGetFile(documentReference, file);

            // Act
            var result = await _documentDeletion.DeleteDocument(request);

            // Assert
            result.Should().BeNull();
            VerifyAllMocks();
        }

        private UserInfo CreateUserInfo(string principal)
            => new UserInfo
            {
                Principal = principal
            };

        private DocumentReferenceWithPreviousVersions CreateDocumentReference(string fileName, string batchId, string parentBatchId)
            => new DocumentReferenceWithPreviousVersions
            {
                FileName = fileName,
                BatchIdentifier = batchId,
                ParentBatchIdentifier = parentBatchId
            };

        private FileMetadata CreateFileMetadata(string fileName, bool deleted)
            => new FileMetadata
            {
                FileName = fileName,
                Deleted = deleted
            };

        private FileMetadataUser CreateFileMetadataUser(string principal)
            => new FileMetadataUser
            {
                Principal = principal
            };

        private void SetupSystemProvider(DateTime dateTime)
            => _systemProvider
                .Setup(systemProvider => systemProvider.DateTime.UtcNow())
                .Returns(dateTime);

        private void SetupCosmosDbGetFile(
            DocumentReference documentReference,
            FileMetadata fileMetadata)
        {
            _cosmosDbService
                .Setup(db => db.GetFile(It.Is<DocumentReference>(docRef =>
                    docRef.ParentBatchIdentifier == documentReference.ParentBatchIdentifier
                    && docRef.BatchIdentifier == documentReference.BatchIdentifier
                    && docRef.FileName == documentReference.FileName)))
                .ReturnsAsync(fileMetadata?.Deleted == false ? fileMetadata : null);
        }

        private void SetupCosmosDbUpdateDocumentMetadata(string batchId, FileMetadata fileMetadata)
            => _cosmosDbService
                .Setup(
                    db => db.UpdateDocumentMetadata(
                    It.Is<FileMetadata>(param => param == fileMetadata && param.Deleted),
                    batchId,
                    new string[] { "Deleted", "History" },
                    null))
                .Returns(Task.CompletedTask);

        private void SetupMapper(UserInfo userInfo, FileMetadataUser fileMetadataUser)
            => _mapper
                .Setup(mapper => mapper.Map<UserInfo, FileMetadataUser>(userInfo))
                .Returns(fileMetadataUser);

        private void SetupFileMetadataUserEncryptor(FileMetadataUser fileMetadataUser, FileMetadataUser encryptedFileMetadataUser)
            => _fileMetadataUserEncryptor
                .Setup(encryptor => encryptor.Encrypt(fileMetadataUser))
                .Returns(encryptedFileMetadataUser);

        private void SetupDirectoryDelete(string fileName)
            => _directory
                .Setup(dir => dir.Delete(fileName))
                .Returns(Task.CompletedTask);

        private void SetupConvertBatchToExchangeDocument(string parentBatchId, string batchId, FileMetadata fileMetadata, ExchangeDocument exchangeDocument)
        {
            _batchToExchangeDocumentConverter
                .Setup(c => c.ConvertToAgencyExchangeDocument(
                    parentBatchId,
                    batchId,
                    fileMetadata,
                    ExchangeDocumentDirection.SentByOrganisation))
                .ReturnsAsync(exchangeDocument);
        }

        private void SetupDirectoriesManager()
            => _directoriesManager
                    .Setup(dirManager => dirManager.GetDirectory(_config.FileCopyDestinationDirectoryKey))
                    .ReturnsAsync(_directory.Object);

        private void VerifyAllMocks()
        {
            Mock.VerifyAll(
                _cosmosDbService,
                _directoriesManager,
                _systemProvider,
                _fileMetadataUserEncryptor,
                _batchToExchangeDocumentConverter,
                _mapper,
                _directory,
                _logger);
        }
    }
}