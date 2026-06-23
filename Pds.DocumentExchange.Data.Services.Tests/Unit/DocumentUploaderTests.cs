using AutoMapper;
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
using System.IO;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Tests.Unit
{
    [TestClass]
    public class DocumentUploaderTests
    {
        private readonly Mock<IDirectoriesManager> _mockDirectoriesManager = new Mock<IDirectoriesManager>();
        private readonly Mock<IServiceBusQueueManager> _mockServiceQueueManager = new Mock<IServiceBusQueueManager>();
        private readonly Mock<ICosmosDbService> _mockCosmosDbService = new Mock<ICosmosDbService>();
        private readonly Mock<IFileNameProvider> _mockFileNameProvider = new Mock<IFileNameProvider>();
        private readonly Mock<IFileMetadataUserEncryptor> _mockFileMetadataUserEncryptor = new Mock<IFileMetadataUserEncryptor>();
        private readonly Mock<IAcademicYearCalculator> _mockAcademicYearCalculator = new Mock<IAcademicYearCalculator>();
        private readonly Mock<ISystemProvider> _mockSystemProvider = new Mock<ISystemProvider>();
        private readonly Mock<IMapper> _mockMapper = new Mock<IMapper>();
        private readonly Mock<ILoggerAdapter<DocumentUploader>> _mockLogger = new Mock<ILoggerAdapter<DocumentUploader>>();

        private readonly Mock<IDirectory> _uploadMockDirectory = new Mock<IDirectory>();
        private readonly Mock<IJsonMessageServiceBusQueue> _virusScanRequiredMockQueue = new Mock<IJsonMessageServiceBusQueue>();

        private readonly DocumentUploader _documentUploader;

        public DocumentUploaderTests()
        {
            var documentUploaderConfiguration = new DocumentUploaderConfiguration
            {
                UploadDirectoryKey = "upload-directory-key",
                VirusScanRequiredQueueName = "virus-scan-required-queue"
            };

            _mockDirectoriesManager
              .Setup(manager => manager.GetDirectory(documentUploaderConfiguration.UploadDirectoryKey))
              .ReturnsAsync(_uploadMockDirectory.Object);

            _mockServiceQueueManager
               .Setup(manager => manager.GetQueue(documentUploaderConfiguration.VirusScanRequiredQueueName))
               .Returns(_virusScanRequiredMockQueue.Object);

            _documentUploader = new DocumentUploader(
                _mockDirectoriesManager.Object,
                _mockServiceQueueManager.Object,
                _mockCosmosDbService.Object,
                _mockFileNameProvider.Object,
                _mockFileMetadataUserEncryptor.Object,
                _mockAcademicYearCalculator.Object,
                _mockSystemProvider.Object,
                _mockMapper.Object,
                documentUploaderConfiguration,
                _mockLogger.Object);
        }

        [TestMethod, TestCategory("Unit")]
        public async Task UploadDocument_SavesDocumentAndPushesMessageToQueue()
        {
            // Arrange
            var utcNow = new DateTime(2020, 1, 1);
            var batchIdentifier = new Guid("ea5e2974-8cf9-495d-8659-b1a32d42f0e3");

            var organisationIdentifier = "12345678";
            var productIdentifier = 10090;
            var academicYear = "201920";
            var originalFileName = "TestDocument.docx";
            var generatedFileName = "12345678_10090_201920_TestDocument.docx";

            var fileBytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

            var userInfo = new UserInfo
            {
                Principal = "test-user-principal",
                FullName = "test-user-full-name",
                EmailAddress = "test-user@email.com",
                OrganisationInfo = new OrganisationInfo
                {
                    OrganisationIdentifier = new OrganisationIdentifier
                    {
                        Type = OrganisationIdentifierType.Ukprn,
                        Value = organisationIdentifier
                    },
                    Name = "Organisation1"
                }
            };

            var fromOrganisation = new OrganisationIdentifier
            {
                Type = OrganisationIdentifierType.Ukprn,
                Value = "10000000"
            };

            var uploadRequest = new UploadDocumentRequest
            {
                FileName = originalFileName,
                ProductIdentifier = productIdentifier,
                Bytes = fileBytes,
                UserInfo = userInfo,
                FromOrganisation = fromOrganisation
            };

            _mockSystemProvider
               .Setup(systemProvider => systemProvider.DateTime.UtcNow())
               .Returns(utcNow);

            _mockSystemProvider
               .Setup(systemProvider => systemProvider.Guid.NewGuid())
               .Returns(batchIdentifier);

            _mockFileNameProvider
                .Setup(fileNameProvider => fileNameProvider.GenerateFileName(
                    fromOrganisation.Value,
                    productIdentifier,
                    academicYear,
                    originalFileName))
                .Returns(generatedFileName);

            _uploadMockDirectory
                .Setup(dir => dir.Save(It.IsAny<Stream>(), generatedFileName))
                .ReturnsAsync(generatedFileName);

            _mockMapper
               .Setup(m => m.Map<UserInfo, FileMetadataUser>(userInfo))
               .Returns(new FileMetadataUser
               {
                   Principal = userInfo.Principal,
                   FullName = userInfo.FullName,
                   EmailAddress = userInfo.EmailAddress,
                   IsEncrypted = false
               });

            _mockAcademicYearCalculator
                .Setup(calculator => calculator.GetCurrentAcademicYear())
                .Returns(201920);

            Expression<Func<FileMetadataUser, bool>> encryptUserInputExpression =
                fileMetadataUser => fileMetadataUser.Principal == userInfo.Principal
                && fileMetadataUser.FullName == userInfo.FullName
                && fileMetadataUser.EmailAddress == userInfo.EmailAddress
                && !fileMetadataUser.IsEncrypted;

            _mockFileMetadataUserEncryptor
                .Setup(encryptor => encryptor.Encrypt(It.Is(encryptUserInputExpression)))
                .Returns(new FileMetadataUser
                {
                    Principal = "encrypted-principal",
                    FullName = "encrypted-full-name",
                    EmailAddress = "encrypted-email-address",
                    IsEncrypted = true
                });

            // Act
            await _documentUploader.UploadDocument(uploadRequest);

            // Assert
            _mockFileNameProvider.Verify(
                fileNameProvider => fileNameProvider.GenerateFileName(
                    fromOrganisation.Value,
                    productIdentifier,
                    academicYear,
                    originalFileName),
                Times.Once);

            _uploadMockDirectory.Verify(
                dir => dir.Save(It.IsAny<Stream>(), generatedFileName),
                Times.Once);

            _mockFileMetadataUserEncryptor.Verify(
               encryptor => encryptor.Encrypt(It.Is(encryptUserInputExpression)),
               Times.Once);

            _mockCosmosDbService.Verify(
                cosmosDb => cosmosDb.AddBatch(It.IsAny<BatchMetadata>()),
                Times.Once);

            _virusScanRequiredMockQueue.Verify(
                queue => queue.PushMessage(It.IsAny<VirusScanRequestMessage>()),
                Times.Once);
        }
    }
}