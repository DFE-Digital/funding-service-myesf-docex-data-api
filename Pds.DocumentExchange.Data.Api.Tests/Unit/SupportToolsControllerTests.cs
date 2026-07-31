using FluentAssertions;
using MapsterMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pds.Core.Common.Organisation.Enums;
using Pds.Core.Common.Organisation.Models;
using Pds.Core.Logging;
using Pds.DocumentExchange.Data.Api.Controllers;
using Pds.DocumentExchange.Data.Api.Models.SupportTools;
using Pds.DocumentExchange.Data.Services.DTOs;
using Pds.DocumentExchange.Data.Services.DTOs.SupportTools;
using Pds.DocumentExchange.Data.Services.Enums;
using Pds.DocumentExchange.Data.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PublishedBatch = Pds.DocumentExchange.Data.Services.DTOs.SupportTools.PublishedBatch;

namespace Pds.DocumentExchange.Data.Api.Tests.Unit
{
    [TestClass]
    public class SupportToolsControllerTests
    {
        private readonly Mock<ICosmosDbService> _cosmosDbService = new Mock<ICosmosDbService>(MockBehavior.Strict);
        private readonly Mock<IMapper> _mapper = new Mock<IMapper>(MockBehavior.Strict);
        private readonly Mock<IReportService> _reportService = new Mock<IReportService>(MockBehavior.Strict);
        private readonly Mock<ICsvWriter> _csvWriter = new Mock<ICsvWriter>(MockBehavior.Strict);
        private readonly Mock<IAgencyExchangeDocumentsService> _agencyExchangeDocumentsService = new Mock<IAgencyExchangeDocumentsService>(MockBehavior.Strict);
        private readonly Mock<ILoggerAdapter<SupportToolsController>> _logger = new Mock<ILoggerAdapter<SupportToolsController>>(MockBehavior.Loose);

        private readonly SupportToolsController _supportToolsController;

        public SupportToolsControllerTests()
        {
            _supportToolsController = new SupportToolsController(
                _cosmosDbService.Object,
                _reportService.Object,
                _mapper.Object,
                _csvWriter.Object,
                _agencyExchangeDocumentsService.Object,
                _logger.Object);
        }

        #region DocumentsPublishedByESFA

        [TestMethod]
        public async Task DocumentsPublishedByESFA_Returns()
        {
            // Arrange
            var publishedBatch = new PublishedBatch
            {
                ParentBatchIdentifier = Guid.Parse("1e7760b7-409a-478f-9b06-daed444cd144"),
                DateAndTime = new DateTime(2020, 1, 1),
                NumberOfDocuments = 100,
                NumberOfEmails = 250,
                UploadedBy = new FileMetadataUser
                {
                    Principal = "encrypted-text",
                    FullName = "encrypted-text",
                    EmailAddress = "encrypted-text",
                    IsEncrypted = true
                }
            };

            var publishedBatches = new ListResult<PublishedBatch>
            {
                TotalItems = 1,
                TotalPages = 1,
                Items = new[] { publishedBatch }
            };

            int pageNumber = 1;
            int pageSize = 25;

            _cosmosDbService
                .Setup(c => c.GetPublishedBatches(pageNumber, pageSize))
                .ReturnsAsync(publishedBatches);

            var expectedBatch = new Models.SupportTools.PublishedBatch
            {
                ParentBatchIdentifier = publishedBatch.ParentBatchIdentifier,
                DateAndTime = publishedBatch.DateAndTime,
                NumberOfDocuments = publishedBatch.NumberOfDocuments,
                NumberOfEmails = publishedBatch.NumberOfEmails,
                EmailAddress = "email.address@education.co.uk"
            };

            var expectedBatches = new Models.ListResult<Models.SupportTools.PublishedBatch>
            {
                TotalItems = 1,
                TotalPages = 1,
                Items = new[] { expectedBatch }
            };

            _mapper
                .Setup(m => m.Map<ListResult<PublishedBatch>, Models.ListResult<Models.SupportTools.PublishedBatch>>(publishedBatches))
                .Returns(expectedBatches);

            // Act
            var result = await _supportToolsController.DocumentsPublishedByESFA(pageNumber, pageSize);

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>()
                .Which.Value.Should().Be(expectedBatches);

            VerifyMocks();
        }

        #endregion


        #region DocumentsPublished

        [TestMethod]
        public async Task DocumentsPublished_WhenNotFound_ReturnsNotFound()
        {
            // Arrange
            var parentBatchId = Guid.Parse("78cface4-e78f-4c31-969f-6f6b2a7d49e1");

            _cosmosDbService
                .Setup(c => c.GetBatchesByParent(parentBatchId.ToString()))
                .ReturnsAsync(null as IEnumerable<BatchMetadata>);

            // Act
            var result = await _supportToolsController.DocumentsPublished(parentBatchId);

            // Assert
            result.Result.Should().BeOfType<NotFoundResult>();
            VerifyMocks();
        }

        [TestMethod]
        public async Task DocumentsPublished_WhenFound_ReturnsOk()
        {
            // Arrange
            var parentBatchId = Guid.Parse("78cface4-e78f-4c31-969f-6f6b2a7d49e1");

            var file = new FileMetadata
            {
                FileName = "file-01.docx",
                ProductIdentifier = "file-type-01",
                Metadata = new Dictionary<string, string>
                {
                    ["Year"] = "202021"
                },
                ToUkprn = 12345678,
                Version = 1,
                VirusScanSuccessful = true,
                History = new[]
                {
                    new FileMetadataHistory { Action = FileAction.EmailSent }
                }
            };

            var batch = new BatchMetadata
            {
                Id = "batch-01",
                Files = new[] { file }
            };

            var batches = new[] { batch };

            var publishedDocument = new PublishedDocument
            {
                FileName = file.FileName,
                FileType = file.ProductIdentifier,
                Year = file.Metadata["Year"],
                ToUKPRN = file.ToUkprn,
                Version = file.Version,
                VirusScanSuccessful = file.VirusScanSuccessful,
                EmailPrepared = true
            };

            var expected = new[] { publishedDocument };

            _cosmosDbService
                .Setup(c => c.GetBatchesByParent(parentBatchId.ToString()))
                .ReturnsAsync(batches);

            _mapper
               .Setup(m => m.Map<IEnumerable<BatchMetadata>, IEnumerable<PublishedDocument>>(batches))
               .Returns(expected);

            // Act
            var result = await _supportToolsController.DocumentsPublished(parentBatchId);

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>()
                .Which.Value.Should().Be(expected);

            VerifyMocks();
        }

        #endregion


        #region DocumentsPublishedCsv

        [TestMethod]
        public async Task DocumentsPublishedCsv_WhenNotFound_ReturnsNotFound()
        {
            // Arrange
            var parentBatchId = Guid.Parse("78cface4-e78f-4c31-969f-6f6b2a7d49e1");

            _cosmosDbService
                .Setup(c => c.GetBatchesByParent(parentBatchId.ToString()))
                .ReturnsAsync(null as IEnumerable<BatchMetadata>);

            // Act
            var result = await _supportToolsController.DocumentsPublishedCsv(parentBatchId);

            // Assert
            result.Result.Should().BeOfType<NotFoundResult>();
            VerifyMocks();
        }

        [TestMethod]
        public async Task DocumentsPublishedCsv_WhenFound_ReturnsOk()
        {
            // Arrange
            var parentBatchId = Guid.Parse("78cface4-e78f-4c31-969f-6f6b2a7d49e1");

            var file = new FileMetadata
            {
                FileName = "file-01.docx",
                ProductIdentifier = "file-type-01",
                Metadata = new Dictionary<string, string>
                {
                    ["Year"] = "202021"
                },
                ToUkprn = 12345678,
                Version = 1,
                VirusScanSuccessful = true,
                History = new[]
                {
                    new FileMetadataHistory { Action = FileAction.EmailSent }
                }
            };

            var batch = new BatchMetadata
            {
                Id = "batch-01",
                Files = new[] { file }
            };

            var batches = new[] { batch };

            var publishedDocument = new PublishedDocument
            {
                FileName = file.FileName,
                FileType = file.ProductIdentifier,
                Year = file.Metadata["Year"],
                ToUKPRN = file.ToUkprn,
                Version = file.Version,
                VirusScanSuccessful = file.VirusScanSuccessful,
                EmailPrepared = true
            };

            var publishedDocuments = new[] { publishedDocument };

            var csvFileContent = new byte[] { 1, 2, 3, 4, 5 };

            _cosmosDbService
                .Setup(c => c.GetBatchesByParent(parentBatchId.ToString()))
                .ReturnsAsync(batches);

            _mapper
               .Setup(m => m.Map<IEnumerable<BatchMetadata>, IEnumerable<PublishedDocument>>(batches))
               .Returns(publishedDocuments);

            _csvWriter
                .Setup(c => c.WriteCsvFile(publishedDocuments))
                .Returns(csvFileContent);

            // Act
            var result = await _supportToolsController.DocumentsPublishedCsv(parentBatchId);

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>()
                .Which.Value.Should().Be(csvFileContent);

            VerifyMocks();
        }

        #endregion


        #region NotificationRecipients

        [TestMethod]
        public async Task NotificationRecipients_Returns()
        {
            // Arrange
            var parentBatchId = Guid.Parse("78cface4-e78f-4c31-969f-6f6b2a7d49e1");
            var expected = CreateNotificationRecipients(10);

            _cosmosDbService
                .Setup(db => db.GetNotificationRecipientsByParentBatch(parentBatchId))
                .ReturnsAsync(expected);

            // Act
            var result = await _supportToolsController.NotificationRecipients(parentBatchId);

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>()
                .Which.Value.Should().Be(expected);

            VerifyMocks();
        }

        [TestMethod]
        public async Task NotificationRecipientsCsv_Returns()
        {
            // Arrange
            var parentBatchId = Guid.Parse("78cface4-e78f-4c31-969f-6f6b2a7d49e1");
            var notificationRecipients = CreateNotificationRecipients(10);
            var csvFileContent = new byte[] { 1, 2, 3, 4, 5 };

            _cosmosDbService
                .Setup(db => db.GetNotificationRecipientsByParentBatch(parentBatchId))
                .ReturnsAsync(notificationRecipients);

            _csvWriter
                .Setup(c => c.WriteCsvFile(notificationRecipients))
                .Returns(csvFileContent);

            // Act
            var result = await _supportToolsController.NotificationRecipientsCsv(parentBatchId);

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>()
                .Which.Value.Should().Be(csvFileContent);

            VerifyMocks();
        }

        #endregion


        #region DeleteDocuments

        [TestMethod]
        [DataRow(Api.Enums.ExchangeDocumentDirection.PublishedByAgency)]
        [DataRow(Api.Enums.ExchangeDocumentDirection.SentByOrganisation)]
        public async Task DeleteDocuments_WhenNotFound_ReturnsNotFound(Api.Enums.ExchangeDocumentDirection direction)
        {
            // Arrange
            int ukprn = 12345678;
            string fileType = "10085";
            string year = "202021";

            var mappedDirection = (Services.Enums.ExchangeDocumentDirection)(int)direction;

            _mapper
              .Setup(mapper => mapper.Map<Api.Enums.ExchangeDocumentDirection, Services.Enums.ExchangeDocumentDirection>(direction))
              .Returns(mappedDirection);

            _agencyExchangeDocumentsService
                  .Setup(s => s.GetDeleteFiles(It.IsAny<ExchangeDocumentDirection>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(null as ListResult<ExchangeDocument>);

            // Act
            var result = await _supportToolsController.DeleteDocumentsSearch(direction, ukprn, fileType, year);

            // Assert
            result.Result.Should().BeOfType<NotFoundResult>();
            VerifyMocks();
        }

        [TestMethod]
        [DataRow(Api.Enums.ExchangeDocumentDirection.PublishedByAgency, Services.Enums.ExchangeDocumentDirection.PublishedByAgency)]
        [DataRow(Api.Enums.ExchangeDocumentDirection.SentByOrganisation, Services.Enums.ExchangeDocumentDirection.SentByOrganisation)]
        public async Task DeleteDocuments_Returns(Api.Enums.ExchangeDocumentDirection direction, Services.Enums.ExchangeDocumentDirection directionMapped)
        {
            // Arrange
            var parentBatchId = "78cface4-e78f-4c31-969f-6f6b2a7d49e1";
            var createdDate = new DateTime(2020, 1, 1);

            int ukprn = 12345678;
            string fileType = "10085";
            string year = "202021";

            var mappedDirection = (Services.Enums.ExchangeDocumentDirection)(int)direction;

            _mapper
              .Setup(mapper => mapper.Map<Api.Enums.ExchangeDocumentDirection, Services.Enums.ExchangeDocumentDirection>(direction))
              .Returns(mappedDirection);

            var dbFile =
                new ExchangeDocument
                {
                    DocumentReference = new DocumentReference
                    {
                        ParentBatchIdentifier = parentBatchId,
                        BatchIdentifier = parentBatchId,
                        FileName = $"{ukprn}_{fileType}_{year}.pdf",
                    },
                    Product = new Product
                    {
                        Identifier = Convert.ToInt32(fileType)
                    },
                    ExchangeDirection = directionMapped,
                    OrganisationInfo = new Services.DTOs.User.OrganisationInfo
                    {
                        OrganisationIdentifier = new OrganisationIdentifier
                        {
                            Type = OrganisationIdentifierType.Ukprn,
                            Value = Convert.ToString(ukprn)
                        }
                    },
                    Year = Convert.ToInt32(year),
                    Version = 1
                };
            var dbFiles = new ListResult<ExchangeDocument>();
            dbFiles.Items = new List<ExchangeDocument>();
            dbFiles.Items = dbFiles.Items.Append(dbFile);

            Models.ExchangeDocument exchangeDocument =
                new Models.ExchangeDocument
                {
                    DocumentReference = new Models.DocumentReference
                    {
                        ParentBatchIdentifier = parentBatchId,
                        BatchIdentifier = parentBatchId,
                        FileName = $"{ukprn}_{fileType}_{year}.pdf",
                    },
                    Product = new Models.Product
                    {
                        Identifier = Convert.ToInt32(fileType)
                    },
                    ExchangeDirection = direction,
                    OrganisationInfo = new Models.OrganisationInfo
                    {
                        OrganisationIdentifier = new Models.OrganisationIdentifier
                        {
                            Type = Enums.OrganisationIdentifierType.Ukprn,
                            Value = Convert.ToString(ukprn)
                        }
                    },
                    Year = Convert.ToInt32(year),
                    Version = 1
                };
            var expected = new Models.ListResult<Models.ExchangeDocument>();
            expected.Items = new List<Models.ExchangeDocument>();
            expected.Items = expected.Items.Append(exchangeDocument);

            _agencyExchangeDocumentsService
                .Setup(s => s.GetDeleteFiles(mappedDirection, ukprn, fileType, year))
                  .ReturnsAsync(dbFiles);

            _mapper
              .Setup(m => m.Map<ListResult<ExchangeDocument>, Models.ListResult<Models.ExchangeDocument>>(It.IsAny<ListResult<ExchangeDocument>>()))
              .Returns(expected);

            // Act
            var result = await _supportToolsController.DeleteDocumentsSearch(direction, ukprn, fileType, year);

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>()
                    .Which.Value.Should().BeEquivalentTo(expected);

            VerifyMocks();
        }

        #endregion


        #region Reports

        [TestMethod]
        public async Task DownloadMIReport_WhenUnsuccessfulExecution_ReturnsInternalServerError()
        {
            // Arrange
            _reportService
                .Setup(x => x.GetMIReport(It.IsAny<DateTime>(), It.IsAny<DateTime>())).Throws<Exception>();

            // Act
            var result = await _supportToolsController.DownloadMIReport(It.IsAny<DateTime>(), It.IsAny<DateTime>());

            // Assert
            result.Result.Should().BeOfType<ObjectResult>().Which.StatusCode.Should().Be(500);

            VerifyMocks();
        }

        [TestMethod]
        public async Task DownloadMIReport_WhenSuccessfulExecution_ReturnsOkResult()
        {
            // Arrange
            var expected = new byte[] { 1, 2, 3, 4, 5 };

            _reportService
                .Setup(x => x.GetMIReport(It.IsAny<DateTime>(), It.IsAny<DateTime>())).ReturnsAsync(expected);

            // Act
            var result = await _supportToolsController.DownloadMIReport(It.IsAny<DateTime>(), It.IsAny<DateTime>());

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>().Which.Value.Should().Be(expected);
            VerifyMocks();
        }

        #endregion

        private void VerifyMocks()
            => Mock.VerifyAll(_cosmosDbService, _mapper, _csvWriter, _agencyExchangeDocumentsService, _logger);

        private IEnumerable<NotificationRecipient> CreateNotificationRecipients(int numberOfRecipients)
            => Enumerable.Range(1, numberOfRecipients).Select((item, i) => new NotificationRecipient
            {
                Ukprn = i,
                EmailAddress = $"email.address.{i}@education.co.uk"
            });
    }
}