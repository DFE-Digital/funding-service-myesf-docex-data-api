using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pds.DocumentExchange.Data.Api.Controllers;
using Pds.DocumentExchange.Data.Api.Enums;
using Pds.DocumentExchange.Data.Api.Models;
using Pds.DocumentExchange.Data.Services.DTOs.Configuration;
using Pds.DocumentExchange.Data.Services.Implementations;
using Pds.DocumentExchange.Data.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Api.Tests.Integration
{
    [TestClass, TestCategory("Integration")]
    public class UploadControllerTests : BaseIntegration
    {
        public UploadControllerTests()
        {
            SetupSystemProvider();
            SetUpConfig();
        }

        #region Document

        [TestMethod]
        public async Task Document_WhenUploadDocumentRequestIsInvalid_ReturnsUnprocessableEntityObjectResult()
        {
            // Arrange
            var uploadDocumentRequest = new UploadDocumentRequest();

            var controller = GetUploadController();

            // Act
            var result = await controller.Document(uploadDocumentRequest);

            // Assert
            result.Should().BeOfType<UnprocessableEntityObjectResult>();
        }

        [TestMethod]
        public async Task Document_WhenUploadDocumentRequestIsValid_ReturnsOkResult()
        {
            // Arrange
            var uploadDocumentRequest = new UploadDocumentRequest
            {
                FileName = "10000001_10001_202021.pdf",
                Bytes = new byte[] { 1, 2, 3, 4, 5 },
                FromOrganisation = new OrganisationIdentifier
                {
                    Type = OrganisationIdentifierType.Ukprn,
                    Value = "10000001"
                },
                ProductIdentifier = 10001,
                UserInfo = new UserInfo
                {
                    Principal = "user-principal",
                    OrganisationInfo = new OrganisationInfo
                    {
                        Name = "organisation-name",
                        OrganisationIdentifier = new OrganisationIdentifier
                        {
                            Type = OrganisationIdentifierType.Ukprn,
                            Value = "10000001"
                        }
                    }
                }
            };

            Mock.Get(SystemProvider.DateTime)
                .Setup(d => d.UtcNow())
                .Returns(new DateTime(2020, 12, 25));

            SetUpAllCaches<IEnumerable<Services.DTOs.AgencyTeam>>();

            Mock.Get(SystemProvider.DateTime)
                .Setup(d => d.Now())
                .Returns(new DateTime(2020, 12, 25));

            Mock.Get(SystemProvider.Guid)
               .Setup(d => d.NewGuid())
               .Returns(Guid.NewGuid());

            Mock.Get(QueueManager)
              .Setup(qm => qm.GetQueue("virusscanrequired"))
              .Returns(
                  new AzureServiceBusQueue(MockConfig["ServiceBusConnectionString"], "virusscanrequired"));

            var controller = GetUploadController();

            // Act
            var result = await controller.Document(uploadDocumentRequest);

            // Assert
            result.Should().BeOfType<OkResult>();

            Mock.VerifyAll(
               Mock.Get(SystemProvider),
               Mock.Get(MockMemoryCache),
               Mock.Get(MockDistributedCache),
               Mock.Get(QueueManager));
        }

        #endregion

        private UploadController GetUploadController()
        {
            return new UploadController(
                GetVirusScanProcessor(),
                GetVirusScanResultProcessor(),
                GetDocumentUploader(),
                Mapper,
                GetValidationService(),
                CreateMockLoggerAdapter<UploadController>());
        }

        private IDocumentUploader GetDocumentUploader()
        {
            return new DocumentUploader(
                GetDirectoriesManager(),
                QueueManager,
                GetCosmosDbService(),
                new FileNameProvider(),
                GetFileMetadataUserEncryptor(),
                new AcademicYearCalculator(SystemProvider),
                SystemProvider,
                Mapper,
                new DocumentUploaderConfiguration(),
                CreateMockLoggerAdapter<DocumentUploader>());
        }
    }
}