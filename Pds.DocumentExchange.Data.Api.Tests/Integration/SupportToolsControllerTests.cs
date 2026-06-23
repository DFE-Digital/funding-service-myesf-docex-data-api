using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pds.Core.Caching.Models;
using Pds.Core.Common.Organisation.Models;
using Pds.DocumentExchange.Data.Api.Controllers;
using Pds.DocumentExchange.Data.Api.Enums;
using Pds.DocumentExchange.Data.Api.Models.Filters;
using Pds.DocumentExchange.Data.Api.Models.SupportTools;
using Pds.DocumentExchange.Data.Populator.Models;
using Pds.DocumentExchange.Data.Populator.Scenarios;
using Pds.DocumentExchange.Data.Populator.Storage;
using Pds.DocumentExchange.Data.Services.DTOs;
using Pds.DocumentExchange.Data.Services.DTOs.Notification;
using Pds.DocumentExchange.Data.Services.DTOs.SupportTools;
using Pds.DocumentExchange.Data.Services.Implementations;
using Pds.DocumentExchange.Data.Services.Implementations.Lookup;
using Pds.DocumentExchange.Data.Services.Implementations.Settings;
using Pds.DocumentExchange.Data.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Api.Tests.Integration
{
    [TestClass]
    public class SupportToolsControllerTests : BaseIntegration, IDisposable
    {
        public SupportToolsControllerTests()
        {
            SetUpConfig();
        }

        public void Dispose()
        {
            Task.Run(() => DocumentsPublishedByAgencyPopulator.TearDownScenarioData(GetFilesInDocumentsUploadedConfiguration())).GetAwaiter().GetResult();
        }

        #region DocumentsPublishedByESFA

        [TestMethod]
        public async Task DocumentsPublishedByESFA_Returns()
        {
            // Arrange
            var createdDate = new DateTime(2020, 1, 1);
            var parentBatchId = Guid.Parse("78cface4-e78f-4c31-969f-6f6b2a7d49e1");

            var configuration = GetFilesInDocumentsUploadedConfiguration();
            var parameters = new DocumentsUploadedScenario.Parameters
            {
                Records = CreateDocumentRecords(parentBatchId, createdDate)
            };

            await DocumentsPublishedByAgencyPopulator.PopulateScenarioData(configuration, parameters);

            var batchNotificationSummary = CreateBatchNotificationSummary(parentBatchId);

            using (var client = new DocumentsCosmosDb(configuration.CosmosDb))
            {
                await client.CreateDocument(batchNotificationSummary);
            }

            var publishedBatch = new Models.SupportTools.PublishedBatch
            {
                DateAndTime = createdDate,
                NumberOfDocuments = 1,
                NumberOfEmails = 2,
                ParentBatchIdentifier = parentBatchId,
                EmailAddress = "test@test.com"
            };

            var expected = new Models.ListResult<Models.SupportTools.PublishedBatch>
            {
                TotalItems = 1,
                TotalPages = 1,
                Items = new[] { publishedBatch },
                Filters = Enumerable.Empty<IFilter>()
            };

            var supportToolsController = GetSupportToolsController();

            int pageNumber = 1;
            int pageSize = 25;

            // Act
            var result = await supportToolsController.DocumentsPublishedByESFA(pageNumber, pageSize);

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>()
                .Which.Value.Should().BeEquivalentTo(expected);
        }

        #endregion


        #region DocumentsPublished

        [TestMethod]
        public async Task DocumentsPublished_WhenNotFound_ReturnsNotFound()
        {
            // Arrange
            var parentBatchId = Guid.Parse("78cface4-e78f-4c31-969f-6f6b2a7d49e1");
            var supportToolsController = GetSupportToolsController();

            // Act
            var result = await supportToolsController.DocumentsPublished(parentBatchId);

            // Assert
            result.Result.Should().BeOfType<NotFoundResult>();
        }

        [TestMethod]
        public async Task DocumentsPublished_WhenFound_ReturnsOk()
        {
            // Arrange
            var createdDate = new DateTime(2020, 1, 1);
            var parentBatchId = Guid.Parse("78cface4-e78f-4c31-969f-6f6b2a7d49e1");

            var configuration = GetFilesInDocumentsUploadedConfiguration();
            var parameters = new DocumentsUploadedScenario.Parameters
            {
                Records = CreateDocumentRecords(parentBatchId, createdDate)
            };

            await DocumentsPublishedByAgencyPopulator.PopulateScenarioData(configuration, parameters);

            IEnumerable<BatchMetadata> batches;
            using (var client = new DocumentsCosmosDb(configuration.CosmosDb))
            {
                batches = await client.GetEntityByParentId<BatchMetadata>(parentBatchId.ToString());
            }

            var file = batches.First().Files.First();

            var publishedDocument = new PublishedDocument
            {
                FileName = file.FileName,
                FileType = file.ProductIdentifier,
                Year = file.Metadata["Year"],
                ToUKPRN = file.ToUkprn,
                Version = file.Version,
                VirusScanSuccessful = file.VirusScanSuccessful,
                EmailPrepared = false
            };

            var expected = new[] { publishedDocument };

            var supportToolsController = GetSupportToolsController();

            // Act
            var result = await supportToolsController.DocumentsPublished(parentBatchId);

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>()
                .Which.Value.Should().BeEquivalentTo(expected);
        }

        [TestMethod]
        public async Task DocumentsPublishedCsv_WhenNotFound_ReturnsNotFound()
        {
            // Arrange
            var parentBatchId = Guid.Parse("78cface4-e78f-4c31-969f-6f6b2a7d49e1");
            var supportToolsController = GetSupportToolsController();

            // Act
            var result = await supportToolsController.DocumentsPublishedCsv(parentBatchId);

            // Assert
            result.Result.Should().BeOfType<NotFoundResult>();
        }

        [TestMethod]
        public async Task DocumentsPublishedCsv_WhenFound_ReturnsOk()
        {
            // Arrange
            var createdDate = new DateTime(2020, 1, 1);
            var parentBatchId = Guid.Parse("78cface4-e78f-4c31-969f-6f6b2a7d49e1");

            var configuration = GetFilesInDocumentsUploadedConfiguration();
            var parameters = new DocumentsUploadedScenario.Parameters
            {
                Records = CreateDocumentRecords(parentBatchId, createdDate)
            };

            await DocumentsPublishedByAgencyPopulator.PopulateScenarioData(configuration, parameters);

            var supportToolsController = GetSupportToolsController();

            // Act
            var result = await supportToolsController.DocumentsPublishedCsv(parentBatchId);

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>()
                 .Which.Value.Should().BeOfType(typeof(byte[]));
        }

        #endregion


        #region NotificationRecipients

        [TestMethod]
        public async Task NotificationRecipients_Returns()
        {
            // Arrange
            var parentBatchId = Guid.Parse("78cface4-e78f-4c31-969f-6f6b2a7d49e1");
            var batchNotificationSummary = CreateBatchNotificationSummary(parentBatchId);

            var configuration = GetFilesInDocumentsUploadedConfiguration();
            using (var client = new DocumentsCosmosDb(configuration.CosmosDb))
            {
                await client.CreateDocument(batchNotificationSummary);
            }

            var expected = new[]
            {
                new NotificationRecipient
                {
                    Ukprn = 12345678,
                    EmailAddress = "recipient-01@gmail.com"
                },
                new NotificationRecipient
                {
                    Ukprn = 12345678,
                    EmailAddress = "recipient-02@gmail.com"
                }
            };

            var supportToolsController = GetSupportToolsController();

            // Act
            var result = await supportToolsController.NotificationRecipients(parentBatchId);

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>()
                .Which.Value.Should().BeEquivalentTo(expected);
        }

        [TestMethod]
        public async Task NotificationRecipientsCsv_Returns()
        {
            // Arrange
            var parentBatchId = Guid.Parse("78cface4-e78f-4c31-969f-6f6b2a7d49e1");
            var batchNotificationSummary = CreateBatchNotificationSummary(parentBatchId);

            var configuration = GetFilesInDocumentsUploadedConfiguration();
            using (var client = new DocumentsCosmosDb(configuration.CosmosDb))
            {
                await client.CreateDocument(batchNotificationSummary);
            }

            var supportToolsController = GetSupportToolsController();

            // Act
            var result = await supportToolsController.NotificationRecipientsCsv(parentBatchId);

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>()
                 .Which.Value.Should().BeOfType(typeof(byte[]));
        }

        #endregion


        #region DeleteDocuments

        [TestMethod]
        public async Task DeleteDocuments_WhenNotFound_ReturnsNotFound()
        {
            // Arrange
            var direction = ExchangeDocumentDirection.SentByOrganisation;
            int ukprn = 12345678;
            string fileType = "10085";
            string year = "202021";

            var supportToolsController = GetSupportToolsController();

            // Act
            var result = await supportToolsController.DeleteDocumentsSearch(direction, ukprn, fileType, year);

            // Assert
            result.Result.Should().BeOfType<NotFoundResult>();
        }

        [TestMethod]
        public async Task DeleteDocuments_PublishedByAgency_Returns()
        {
            // Arrange
            var direction = Api.Enums.ExchangeDocumentDirection.PublishedByAgency;
            var parentBatchId = Guid.Parse("78cface4-e78f-4c31-969f-6f6b2a7d49e1");
            var createdDate = new DateTime(2020, 1, 1);

            int ukprn = 12345678;
            string fileType = "10085";
            string year = "202021";

            var supportToolsController = GetSupportToolsController();

            var configuration = GetFilesInDocumentsUploadedConfiguration();
            var parameters = new DocumentsUploadedScenario.Parameters
            {
                Records = CreateDocumentRecords(new[] { parentBatchId }, createdDate, ukprn, fileType, year)
            };

            await DocumentsPublishedByAgencyPopulator.PopulateScenarioData(configuration, parameters);

            SetUpAllCaches<IEnumerable<Product>>();

            var organisation = new Organisation
            {
                Name = "organisation",
                Identifiers = new[]
                {
                    new OrganisationIdentifier
                    {
                        Type = Pds.Core.Common.Organisation.Enums.OrganisationIdentifierType.Ukprn,
                        Value = "10000001"
                    }
                }
            };

            var organisations = new Dictionary<string, Organisation>
            {
                { "10000001", organisation }
            };

            var products = new List<Product>
            {
                new Product()
            };

            Mock.Get(MockMemoryCache)
                .Setup(x => x.Get<IDictionary<string, Organisation>>(It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(new CacheItem<IDictionary<string, Organisation>>(organisations));

            Mock.Get(OrganisationService).Setup(
               o => o.GetOrganisation(It.IsAny<string>()))
               .ReturnsAsync((Organisation)null);

            var expected = new Models.ListResult<Models.ExchangeDocument>
            {
                Items = new List<Models.ExchangeDocument>
                {
                    new Models.ExchangeDocument
                    {
                        AgencyTeam = string.Empty,
                        DocumentReference = new Models.DocumentReference
                        {
                            BatchIdentifier = parentBatchId.ToString(),
                            FileName = "12345678_10085_202021.pdf",
                            ParentBatchIdentifier = parentBatchId.ToString()
                        },
                        EventHistory = new List<Models.ExchangeDocumentEvent>
                        {
                            new Models.ExchangeDocumentEvent
                            {
                                EventDateTime = createdDate,
                                EventType = ExchangeDocumentEventType.PublishedByAgency,
                                UserInfo = new Models.UserInfo
                                {
                                   EmailAddress = "test@test.com",
                                   FullName = "Test",
                                   IsViewAsProvider = false,
                                   OrganisationInfo = null,
                                   Principal = "Test1"
                                }
                            }
                        },
                        ExchangeDirection = direction,
                        OrganisationInfo = new Models.OrganisationInfo
                        {
                            Name = "Unknown organisation",
                            OrganisationIdentifier = new Models.OrganisationIdentifier
                            {
                                Type = OrganisationIdentifierType.Ukprn,
                                Value = "12345678"
                            }
                        },
                        PreviousVersions = new List<Data.Api.Models.ExchangeDocument>(),
                        Product = new Models.Product
                        {
                            AgencyTeams = new List<string> { "DocumentExchangeAdministratorFundingCentre" },
                            CanOrganisationsUpload = true,
                            Identifier = 10085,
                            Name = "Business case",
                            PluralName = "Business cases"
                        },
                        Version = 1,
                        Year = 202021
                    }
                },
                Filters = new List<IFilter>(),
                TotalItems = 1,
                TotalPages = 0
            };

            // Act
            var result = await supportToolsController.DeleteDocumentsSearch(direction, ukprn, fileType, year);

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>()
                .Which.Value.Should().BeEquivalentTo(expected);
        }

        [TestMethod]
        public async Task DeleteDocuments_SentByOrganisation_Returns()
        {
            // Arrange
            var direction = Api.Enums.ExchangeDocumentDirection.SentByOrganisation;
            var parentBatchId = Guid.Parse("dd58cbb9-3f5b-4f37-b7d4-4dd852500c5b");
            var createdDate = new DateTime(2020, 1, 1);

            int ukprn = 12345678;
            string fileType = "10085";

            var supportToolsController = GetSupportToolsController();

            var configuration = GetFilesInDocumentsUploadedConfiguration();
            var parameters = new DocumentsUploadedScenario.Parameters
            {
                Records = CreateDocumentRecords(new[] { parentBatchId }, createdDate, ukprn, fileType, null)
            };

            await DocumentsUploadedByExternalPopulator.PopulateScenarioData(configuration, parameters);

            SetUpAllCaches<IEnumerable<Product>>();

            var organisation = new Organisation
            {
                Name = "organisation",
                Identifiers = new[]
                {
                    new OrganisationIdentifier
                    {
                        Type = Pds.Core.Common.Organisation.Enums.OrganisationIdentifierType.Ukprn,
                        Value = "10000001"
                    }
                }
            };

            var organisations = new Dictionary<string, Organisation>
            {
                { "10000001", organisation }
            };

            var products = new List<Product>
            {
                new Product()
            };

            Mock.Get(MockMemoryCache)
                .Setup(x => x.Get<IDictionary<string, Organisation>>(It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(new CacheItem<IDictionary<string, Organisation>>(organisations));

            Mock.Get(OrganisationService).Setup(
               o => o.GetOrganisation(It.IsAny<string>()))
               .ReturnsAsync((Organisation)null);

            var expected = new Models.ListResult<Models.ExchangeDocument>
            {
                Items = new List<Models.ExchangeDocument>
                {
                    new Models.ExchangeDocument
                    {
                        AgencyTeam = string.Empty,
                        DocumentReference = new Models.DocumentReference
                        {
                            BatchIdentifier = parentBatchId.ToString(),
                            FileName = "12345678_10085_.pdf",
                            ParentBatchIdentifier = parentBatchId.ToString()
                        },
                        EventHistory = new List<Models.ExchangeDocumentEvent>
                        {
                            new Models.ExchangeDocumentEvent
                            {
                                EventDateTime = createdDate,
                                EventType = ExchangeDocumentEventType.SentByOrganisation,
                                UserInfo = new Models.UserInfo
                                {
                                   EmailAddress = "test@test.com",
                                   FullName = "Test",
                                   IsViewAsProvider = false,
                                   OrganisationInfo = null,
                                   Principal = "Test1"
                                }
                            }
                        },
                        ExchangeDirection = direction,
                        OrganisationInfo = new Models.OrganisationInfo
                        {
                            Name = "Unknown organisation",
                            OrganisationIdentifier = new Models.OrganisationIdentifier
                            {
                                Type = OrganisationIdentifierType.Ukprn,
                                Value = "12345678"
                            }
                        },
                        PreviousVersions = new List<Data.Api.Models.ExchangeDocument>(),
                        Product = new Models.Product
                        {
                            AgencyTeams = new List<string> { "DocumentExchangeAdministratorFundingCentre" },
                            CanOrganisationsUpload = true,
                            Identifier = 10085,
                            Name = "Business case",
                            PluralName = "Business cases"
                        },
                        Version = 1,
                        Year = -1
                    }
                },
                Filters = new List<IFilter>(),
                TotalItems = 1,
                TotalPages = 0
            };

            // Act
            var result = await supportToolsController.DeleteDocumentsSearch(direction, ukprn, fileType, null);

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>()
                .Which.Value.Should().BeEquivalentTo(expected);
        }

        #endregion


        #region Reports

        [TestMethod]
        public async Task DownloadMIReport_WhenSuccessfullyExecuted_ReturnsOkResult()
        {
            // Arrange
            Mock.Get(DateTimeProvider)
                .Setup(dtp => dtp.ConvertToUKTime(It.IsAny<DateTime>()))
                .Returns(DateTime.UtcNow);

            var organisation = new Organisation
            {
                Name = "organisation",
                Identifiers = new[]
                {
                    new OrganisationIdentifier
                    {
                        Type = Pds.Core.Common.Organisation.Enums.OrganisationIdentifierType.Ukprn,
                        Value = "10000001"
                    }
                }
            };

            var organisations = new Dictionary<string, Organisation>
            {
                { "10000001", organisation }
            };

            var products = new List<Product>
            {
                new Product()
            };

            Mock.Get(MockMemoryCache)
                .Setup(x => x.Get<IDictionary<string, Organisation>>(It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(new CacheItem<IDictionary<string, Organisation>>(organisations));

            Mock.Get(MockMemoryCache)
                .Setup(x => x.Get<IEnumerable<Product>>(It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(new CacheItem<IEnumerable<Product>>(products));

            var supportToolsController = GetSupportToolsController();

            // Act
            var result = await supportToolsController.DownloadMIReport(DateTime.Now.AddDays(-1), DateTime.Now);

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>();
        }

        [TestMethod]
        public async Task DownloadMIReport_WhenNotExecutedSuccessfully_ReturnsObjectResultWithStatusCodeOf500()
        {
            // Arrange
            var supportToolsController = GetSupportToolsController();

            // Act
            var result = await supportToolsController.DownloadMIReport(DateTime.Now.AddDays(-1), DateTime.Now);

            // Assert
            result.Result.Should().BeOfType<ObjectResult>().Which.StatusCode.Should().Be(500);
        }

        #endregion


        private SupportToolsController GetSupportToolsController()
        {
            var reportService = GetReportService();
            var cosmosDbService = GetCosmosDbService();
            var agencyExchangeDocumentsService = GetAgencyExchangeDocumentsService();

            return new SupportToolsController(cosmosDbService, reportService, Mapper, new CsvWriter(), agencyExchangeDocumentsService, CreateMockLoggerAdapter<SupportToolsController>());
        }

        private IAgencyExchangeDocumentsService GetAgencyExchangeDocumentsService()
        {
            var batchesService = GetBatchesService();
            var batchToExchangeDocumentConverter = GetBatchToExchangeDocumentConverter();
            var cacheManager = GetCacheManager();
            var configurationDataService = new ConfigurationDataService(
                GetCosmosDbService(),
                cacheManager,
                CreateMockLoggerAdapter<ConfigurationDataService>());

            return new AgencyExchangeDocumentsService(
                batchesService,
                batchToExchangeDocumentConverter,
                cacheManager,
                configurationDataService,
                CreateMockLoggerAdapter<AgencyExchangeDocumentsService>());
        }

        private IReportService GetReportService()
        {
            var batchToExchangeDocumentConverter = GetBatchToExchangeDocumentConverter();
            var cacheManager = GetCacheManager();
            var configurationDataService = new ConfigurationDataService(
                GetCosmosDbService(),
                cacheManager,
                CreateMockLoggerAdapter<ConfigurationDataService>());
            var logger = CreateMockLoggerAdapter<ReportService>();

            return new ReportService(
                DateTimeProvider,
                GetCosmosDbService(),
                new EncryptionService(),
                logger,
                GetOrganisationsLookup(),
                configurationDataService,
                GetCosmosDbConfiguration());
        }

        private IBatchesService GetBatchesService()
        {
            return new BatchesService(
                GetCosmosDbService(),
                CreateMockLoggerAdapter<BatchesService>());
        }

        private IBatchToExchangeDocumentConverter GetBatchToExchangeDocumentConverter()
        {
            var cosmosDbService = GetCosmosDbService();
            var cacheManager = GetCacheManager();
            var configurationDataService = new ConfigurationDataService(cosmosDbService, cacheManager, CreateMockLoggerAdapter<ConfigurationDataService>());
            var productsLookup = new ProductsLookup(configurationDataService);
            var organisationsLookup = GetOrganisationsLookup();
            var fileMetadataUserEncryptor = GetFileMetadataUserEncryptor();
            var logger = CreateMockLoggerAdapter<BatchToExchangeDocumentConverter>();
            return new BatchToExchangeDocumentConverter(productsLookup, organisationsLookup, fileMetadataUserEncryptor, Mapper, logger);
        }

        private DocumentRecord[] CreateDocumentRecords(Guid parentBatchId, DateTime createdDate)
            => new[]
            {
                new DocumentRecord
                {
                    ParentBatchIdentifier = parentBatchId.ToString(),
                    Ukprn = 12345678,
                    ProviderId = "10001",
                    AcademicYear = "202021",
                    FileType = "pdf",
                    FileSize = "100KB",
                    BatchCreatedDate = createdDate
                }
            };

        private DocumentRecord[] CreateDocumentRecords(
            IEnumerable<Guid> parentBatchIds,
            DateTime createdDate,
            int ukprn,
            string fileType,
            string year)
        {
            return parentBatchIds.Select((p, i) => new DocumentRecord
            {
                ParentBatchIdentifier = p.ToString(),
                Ukprn = ukprn,
                ProviderId = fileType,
                AcademicYear = year,
                FileType = "pdf",
                FileSize = "100KB",
                BatchCreatedDate = createdDate
            }).ToArray();
        }

        private BatchNotificationSummary CreateBatchNotificationSummary(Guid parentBatchId)
            => new BatchNotificationSummary
            {
                Id = "645b1aff-6285-406d-a544-0dd55e21e4f7",
                ParentBatchIdentifier = parentBatchId.ToString(),
                DocumentType = "BatchNotificationSummary",
                RecipientNotificationSummaries = new[]
                {
                    new NotificationSummary
                    {
                        NotificationType = NotificationType.AgencyPublishClear,
                        Ukprn = 12345678,
                        From = new[] { "service_relay_noreply@docex.education.gov.uk" },
                        To = new[] { "recipient-01@gmail.com", "recipient-02@gmail.com" },
                        Notes = "notes here"
                    }
                }
            };
    }
}