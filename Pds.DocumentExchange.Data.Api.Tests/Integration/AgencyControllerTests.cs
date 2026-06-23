using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pds.Core.BulkJobs.Interfaces;
using Pds.Core.BulkJobs.Models;
using Pds.Core.Caching.Models;
using Pds.Core.Common.Organisation.Enums;
using Pds.Core.Common.Organisation.Models;
using Pds.Core.Utils.Implementations;
using Pds.DocumentExchange.Data.Api.Controllers;
using Pds.DocumentExchange.Data.Populator.Models;
using Pds.DocumentExchange.Data.Populator.Scenarios;
using Pds.DocumentExchange.Data.Populator.Storage;
using Pds.DocumentExchange.Data.Services.DTOs;
using Pds.DocumentExchange.Data.Services.DTOs.Configuration;
using Pds.DocumentExchange.Data.Services.Enums;
using Pds.DocumentExchange.Data.Services.Implementations;
using Pds.DocumentExchange.Data.Services.Implementations.Converters;
using Pds.DocumentExchange.Data.Services.Implementations.Filters;
using Pds.DocumentExchange.Data.Services.Implementations.Lookup;
using Pds.DocumentExchange.Data.Services.Implementations.Settings;
using Pds.DocumentExchange.Data.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Api.Tests.Integration
{
    [TestClass, TestCategory("Integration")]
    public class AgencyControllerTests : BaseIntegration, IDisposable
    {
        private readonly IBulkJobManager _bulkJobManager = Mock.Of<IBulkJobManager>(MockBehavior.Strict);

        public AgencyControllerTests()
        {
            SetupSystemProvider();
            SetUpConfig();
        }

        private FilesInTeamShareScenario _filesInTeamSharePopulator = new FilesInTeamShareScenario();

        #region Summary

        [TestMethod]
        public async Task Summary_WhenTeamIsInvalid_ReturnsUnprocessableEntityObjectResult()
        {
            // Arrange
            var team = "invalid-team-name";

            Mock.Get(MockMemoryCache)
                 .Setup(mc => mc.Get<IEnumerable<AgencyTeam>>(It.IsAny<string>(), It.IsAny<bool>()))
                 .ReturnsAsync(new CacheItem<IEnumerable<AgencyTeam>>());

            var controller = GetAgencyController();

            // Act
            var result = await controller.Summary(team);

            // Assert
            result.Result.Should().BeOfType<UnprocessableEntityObjectResult>();

            Mock.VerifyAll(
              Mock.Get(MockMemoryCache));
        }

        [TestMethod]
        [DataRow(1, 0, "10001")]
        [DataRow(0, 1, "10002")]
        [DataRow(1, 1, "10003")]
        [DataRow(10, 3, "10004")]
        public async Task Summary_WhenTeamIsValid_ReturnsFileShareSummary(int validCount, int invalidCount, string providerId)
        {
            // Arrange
            var mappedFileShareSummary = new FileShareSummary
            {
                ValidCount = validCount,
                InvalidCount = invalidCount
            };

            SetUpAllCaches<IEnumerable<AgencyTeam>>();
            SetUpAllCaches<List<FileReferenceInfo>>();
            SetUpAllCaches<AgencyDocumentErrorType>();
            SetUpMemoryCache(GetOrganisationDictionary(validCount + invalidCount));

            if (validCount > 0)
            {
                SetUpAllCaches<IEnumerable<FileExtensionInfo>>();
                SetUpAllCaches<IEnumerable<Product>>();
            }

            Mock.Get(OrganisationService).Setup(
                o => o.GetOrganisation(It.IsAny<string>()))
                .ReturnsAsync(new Organisation());

            var controller = GetAgencyController();

            var configuration = GetFilesInTeamShareConfiguration(Team1.ToLower());
            var parameters = new FilesInTeamShareScenario.Parameters()
            {
                Records = GetDocumentRecords(validCount, invalidCount, providerId)
            };

            await _filesInTeamSharePopulator.PopulateScenarioData(configuration, parameters);

            // Act
            var result = await controller.Summary(Team1);

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>().Which.Value.Should().BeEquivalentTo(mappedFileShareSummary);

            Mock.VerifyAll(
               Mock.Get(MockMemoryCache),
               Mock.Get(MockDistributedCache));
        }

        [TestMethod]
        [DataRow(1, 0, "10001", 1, 0, "10036")]
        [DataRow(1, 0, "10002", 0, 1, "10036")]
        [DataRow(0, 1, "10003", 1, 0, "10036")]
        [DataRow(0, 1, "10004", 0, 1, "10036")]
        [DataRow(1, 1, "10005", 1, 1, "10036")]
        [DataRow(8, 3, "10006", 5, 2, "10036")]
        public async Task Summary_WhenMultipleTeamsValid_ReturnsFileShareSummary(int validCountT1, int invalidCountT1, string providerIdT1, int validCountT2, int invalidCountT2, string providerIdT2)
        {
            // Arrange
            var teamsParam = string.Concat(Team1, ",", Team2);
            var teamsList = new List<string>
            {
                Team1,
                Team2
            };

            var mappedFileShareSummary = new FileShareSummary
            {
                ValidCount = validCountT1 + validCountT2,
                InvalidCount = invalidCountT1 + invalidCountT2
            };

            SetUpAllCaches<IEnumerable<AgencyTeam>>();
            SetUpAllCaches<List<FileReferenceInfo>>();
            SetUpAllCaches<AgencyDocumentErrorType>();
            SetUpMemoryCache(GetOrganisationDictionary(validCountT1 + invalidCountT1));

            if (validCountT1 > 0 || validCountT2 > 0)
            {
                SetUpAllCaches<IEnumerable<FileExtensionInfo>>();
                SetUpAllCaches<IEnumerable<Product>>();
            }

            var controller = GetAgencyController();

            var configuration1 = GetFilesInTeamShareConfiguration(teamsList[0].ToLower());
            var configuration2 = GetFilesInTeamShareConfiguration(teamsList[1].ToLower());

            var parameters1 = new FilesInTeamShareScenario.Parameters()
            {
                Records = GetDocumentRecords(validCountT1, invalidCountT1, providerIdT1)
            };

            var parameters2 = new FilesInTeamShareScenario.Parameters()
            {
                Records = GetDocumentRecords(validCountT2, invalidCountT2, providerIdT2)
            };

            await _filesInTeamSharePopulator.PopulateScenarioData(configuration1, parameters1);
            await _filesInTeamSharePopulator.PopulateScenarioData(configuration2, parameters2);

            // Act
            var result = await controller.Summary(teamsParam);

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>().Which.Value.Should().BeEquivalentTo(mappedFileShareSummary);

            Mock.VerifyAll(
                Mock.Get(MockMemoryCache),
                Mock.Get(MockDistributedCache));
        }

        #endregion


        #region Documents

        [TestMethod]
        public async Task Documents_WhenTeamIsInvalid_ReturnsUnprocessableEntityObjectResult()
        {
            // Arrange
            var team = "invalid-team-name";
            var agencyListDocumentOption = new Models.AgencyListDocumentOptions();

            var controller = GetAgencyController();

            // Act
            var result = await controller.Documents(team, agencyListDocumentOption);

            // Assert
            result.Result.Should().BeOfType<UnprocessableEntityObjectResult>();
        }

        [TestMethod]
        public async Task Documents_WhenAgencyListDocumentOptionsIsInvalid_ReturnsUnprocessableEntityObjectResult()
        {
            // Arrange
            var agencyListDocumentOption = new Models.AgencyListDocumentOptions();

            var controller = GetAgencyController();

            // Act
            var result = await controller.Documents(Team1, agencyListDocumentOption);

            // Assert
            result.Result.Should().BeOfType<UnprocessableEntityObjectResult>();
        }

        [TestMethod]
        public async Task Documents_WhenTeamAndAgencyListDocumentOptionsAreValid_ReturnsAgencyDocumentListResult()
        {
            // Arrange
            var agencyListDocumentOption = new Models.AgencyListDocumentOptions
            {
                PageNumber = 1,
                PageSize = 1
            };

            var organisationIdentifier = new OrganisationIdentifier
            {
                Type = OrganisationIdentifierType.Ukprn,
                Value = "10000001"
            };

            var organisation = new Organisation
            {
                Name = "organisation",
                Identifiers = new[] { organisationIdentifier }
            };

            var expectedResult = new Models.ListResult<Models.AgencyDocument>
            {
                Filters = new List<Models.Filters.ListFilter>
                {
                    new Models.Filters.ListFilter
                    {
                         Groups = new List<Models.Filters.FilterGroup>(),
                         Key = "ProductIdRadio",
                         Title = "Select a document type",
                         Type = "RadioFilter",
                         Values = new List<Models.Filters.FilterValue>
                         {
                         new Models.Filters.FilterValue
                             {
                                   Count = 1,
                                   Selected = false,
                                   Title = "Allocation calculation toolkits",
                                   Value = "10001"
                             }
                         }
                    }
                },
                Items = new List<Models.AgencyDocument>
                {
                    new Models.AgencyDocument
                    {
                        FileName = "10000001_10001_202021.pdf",
                        IsValid = true,
                        OrganisationInfo = new Models.OrganisationInfo
                        {
                            Name = organisation.Name,
                            OrganisationIdentifier = new Models.OrganisationIdentifier
                            {
                               Type = Enums.OrganisationIdentifierType.Ukprn,
                               Value = organisationIdentifier.Value
                            }
                        },
                        Product = new Models.Product
                        {
                            AgencyTeams = new List<string> { Team1 },
                            CanOrganisationsUpload = false,
                            Identifier = 10001,
                            Name = "Allocation calculation toolkit",
                            PluralName = "Allocation calculation toolkits"
                        },
                        Team = "DocumentExchangeAdministratorFundingCentre",
                        Year = 202021
                    }
                },
                TotalItems = 1,
                TotalPages = 1
            };

            SetUpAllCaches<IEnumerable<AgencyTeam>>();
            SetUpAllCaches<List<FileReferenceInfo>>();
            SetUpAllCaches<AgencyDocumentErrorType>();
            SetUpAllCaches<IEnumerable<Product>>();
            SetUpAllCaches<IEnumerable<FileExtensionInfo>>();
            SetUpMemoryCache(GetOrganisationDictionary(1));

            var configuration = GetFilesInTeamShareConfiguration(Team1.ToLower());
            var parameters = new FilesInTeamShareScenario.Parameters()
            {
                Records = GetDocumentRecords(1, 0, "10001")
            };

            await _filesInTeamSharePopulator.PopulateScenarioData(configuration, parameters);

            var controller = GetAgencyController();

            // Act
            var result = await controller.Documents(Team1, agencyListDocumentOption);

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>().Which.Value.Should().BeEquivalentTo(expectedResult);

            Mock.VerifyAll(
                Mock.Get(MockMemoryCache),
                Mock.Get(MockDistributedCache));
        }

        #endregion


        #region Remove

        [TestMethod]
        public async Task Remove_WhenTeamIsInvalid_ReturnsUnprocessableEntityObjectResult()
        {
            // Arrange
            var team = "invalid-team-name";
            var fileNames = new[] { "file-01.pdf", "file-02.pdf" };

            SetUpAllCaches<IEnumerable<AgencyTeam>>();

            var agencyController = GetAgencyController();

            // Act
            var result = await agencyController.Remove(team, fileNames);

            // Assert
            result.Should().BeOfType<UnprocessableEntityObjectResult>();

            Mock.VerifyAll(
                Mock.Get(MockMemoryCache),
                Mock.Get(MockDistributedCache));
        }

        [TestMethod]
        public async Task Remove_WhenFileNamesIsNull_ReturnsUnprocessableEntityObjectResult()
        {
            // Arrange
            SetUpAllCaches<IEnumerable<AgencyTeam>>();

            var agencyController = GetAgencyController();

            // Act
            var result = await agencyController.Remove(Team1, null);

            // Assert
            result.Should().BeOfType<UnprocessableEntityObjectResult>();

            Mock.VerifyAll(
               Mock.Get(MockMemoryCache),
               Mock.Get(MockDistributedCache));
        }

        [TestMethod]
        public async Task Remove_WhenSingleFileExists_Removes()
        {
            // Arrange
            var team = Team1;

            var documentToRemove = new DocumentRecord
            {
                Ukprn = 12345678,
                ProviderId = "10001",
                AcademicYear = "202021",
                FileType = "docx",
                FileSize = "100KB"
            };

            var documentToKeep = new DocumentRecord
            {
                Ukprn = 10079319,
                ProviderId = "10084",
                AcademicYear = "202021",
                FileType = "csv",
                FileSize = "100KB"
            };

            var fileNameToRemove = GetFileNameFromDocumentRecord(documentToRemove);
            var fileNameToKeep = GetFileNameFromDocumentRecord(documentToKeep);

            SetUpAllCaches<IEnumerable<AgencyTeam>>();

            var agencyController = GetAgencyController();

            var configuration = GetFilesInTeamShareConfiguration(team.ToLower());
            var parameters = new FilesInTeamShareScenario.Parameters()
            {
                Records = new[]
                {
                    documentToRemove,
                    documentToKeep
                }
            };

            await _filesInTeamSharePopulator.PopulateScenarioData(configuration, parameters);

            // Act
            var response = await agencyController.Remove(team, new[] { fileNameToRemove });
            var fileShareFiles = _filesInTeamSharePopulator.GetScenarioFileNames(configuration);

            // Assert
            response.Should().BeOfType(typeof(OkResult));
            fileShareFiles.Should().ContainSingle(file => file == fileNameToKeep);

            Mock.VerifyAll(
               Mock.Get(MockMemoryCache),
               Mock.Get(MockDistributedCache));
        }

        [TestMethod]
        public async Task Remove_WhenMultipleFileExists_Removes()
        {
            // Arrange
            var team = Team1;

            var documentToRemove1 = new DocumentRecord
            {
                Ukprn = 12345678,
                ProviderId = "10001",
                AcademicYear = "202021",
                FileType = "docx",
                FileSize = "100KB"
            };

            var documentToRemove2 = new DocumentRecord
            {
                Ukprn = 10013279,
                ProviderId = "10087",
                AcademicYear = "201920",
                FileType = "xlsx",
                FileSize = "100KB"
            };

            var documentToKeep1 = new DocumentRecord
            {
                Ukprn = 10079319,
                ProviderId = "10084",
                AcademicYear = "202021",
                FileType = "csv",
                FileSize = "100KB"
            };

            var documentToKeep2 = new DocumentRecord
            {
                Ukprn = 10016604,
                ProviderId = "10090",
                AcademicYear = "202021",
                FileType = "txt",
                FileSize = "100KB"
            };

            var fileNamesToRemove = new[]
            {
                GetFileNameFromDocumentRecord(documentToRemove1),
                GetFileNameFromDocumentRecord(documentToRemove2)
            };

            var fileNamesToKeep = new[]
            {
                GetFileNameFromDocumentRecord(documentToKeep1),
                GetFileNameFromDocumentRecord(documentToKeep2)
            };

            SetUpAllCaches<IEnumerable<AgencyTeam>>();

            var agencyController = GetAgencyController();

            var configuration = GetFilesInTeamShareConfiguration(team.ToLower());
            var parameters = new FilesInTeamShareScenario.Parameters()
            {
                Records = new[]
                {
                    documentToRemove1,
                    documentToRemove2,
                    documentToKeep1,
                    documentToKeep2
                }
            };

            await _filesInTeamSharePopulator.PopulateScenarioData(configuration, parameters);

            // Act
            var response = await agencyController.Remove(team, fileNamesToRemove);
            var fileShareFiles = _filesInTeamSharePopulator.GetScenarioFileNames(configuration);

            // Assert
            response.Should().BeOfType(typeof(OkResult));
            fileShareFiles.Should().OnlyContain(file => fileNamesToKeep.Contains(file));

            Mock.VerifyAll(
               Mock.Get(MockMemoryCache),
               Mock.Get(MockDistributedCache));
        }

        [TestMethod]
        public async Task Remove_WhenFileDoesntExist_ReturnsNotFoundResult()
        {
            // Arrange
            var fileNames = new[] { "non-existing-file.pdf" };

            SetUpAllCaches<IEnumerable<AgencyTeam>>();

            var agencyController = GetAgencyController();

            // Act
            var response = await agencyController.Remove(Team1, fileNames);

            // Assert
            response.Should().BeOfType(typeof(NotFoundResult));

            Mock.VerifyAll(
               Mock.Get(MockMemoryCache),
               Mock.Get(MockDistributedCache));
        }

        #endregion


        #region Download

        [TestMethod]
        public async Task Download_WhenTeamIsInvalid_ReturnsUnprocessableEntityObjectResult()
        {
            // Arrange
            var team = "invalid-team-name";
            var fileName = "file-name.pdf";

            Mock.Get(MockMemoryCache)
                 .Setup(mc => mc.Get<IEnumerable<AgencyTeam>>(It.IsAny<string>(), It.IsAny<bool>()))
                 .ReturnsAsync(new CacheItem<IEnumerable<AgencyTeam>>());

            var controller = GetAgencyController();

            // Act
            var result = await controller.Download(team, fileName);

            // Assert
            result.Result.Should().BeOfType<UnprocessableEntityObjectResult>();

            Mock.VerifyAll(
              Mock.Get(MockMemoryCache));
        }

        [TestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow(" ")]
        public async Task Download_WhenFilenameIsNullOrEmpty_ReturnsUnprocessableEntityObjectResult(string fileName)
        {
            // Arrange
            var team = "valid-team-name";

            Mock.Get(MockMemoryCache)
                 .Setup(mc => mc.Get<IEnumerable<AgencyTeam>>(It.IsAny<string>(), It.IsAny<bool>()))
                 .ReturnsAsync(new CacheItem<IEnumerable<AgencyTeam>>());

            var controller = GetAgencyController();

            // Act
            var result = await controller.Download(team, fileName);

            // Assert
            result.Result.Should().BeOfType<UnprocessableEntityObjectResult>();

            Mock.VerifyAll(
              Mock.Get(MockMemoryCache));
        }

        [TestMethod]
        public async Task Download_WhenFileExists_ReturnsFileContents()
        {
            // Arrange
            var fileName = "10000001_10001_202021.pdf";

            SetUpAllCaches<IEnumerable<AgencyTeam>>();

            var controller = GetAgencyController();

            var configuration = GetFilesInTeamShareConfiguration(Team2.ToLower());

            var parameters = new FilesInTeamShareScenario.Parameters()
            {
                Records = new List<DocumentRecord>
                {
                    new DocumentRecord
                    {
                        Ukprn = 10000001,
                        ProviderId = "10001",
                        AcademicYear = "202021",
                        FileType = "pdf",
                        FileSize = "100KB"
                    }
                }
            };

            await LocalFileHelper.LoadExampleDocuments();
            var fileContent = LocalFileHelper.GetExampleDocumentBySize(parameters.Records.First().FileSize);

            await _filesInTeamSharePopulator.PopulateScenarioData(configuration, parameters);

            // Act
            var result = await controller.Download(Team2, fileName);

            // Assert
            result.Value.Should().BeEquivalentTo(fileContent);

            Mock.VerifyAll(
              Mock.Get(MockMemoryCache),
              Mock.Get(MockDistributedCache));
        }

        [TestMethod]
        public async Task Download_WhenFileDoesntExist_ReturnsNotFoundResult()
        {
            // Arrange
            var fileName = "10000001_10001_202021.pdf";

            SetUpAllCaches<IEnumerable<AgencyTeam>>();

            var controller = GetAgencyController();

            var configuration = GetFilesInTeamShareConfiguration(Team2.ToLower());

            var parameters = new FilesInTeamShareScenario.Parameters()
            {
                Records = new List<DocumentRecord>
                {
                    new DocumentRecord
                    {
                        Ukprn = 80000001,
                        ProviderId = "10001",
                        AcademicYear = "202021",
                        FileType = "pdf",
                        FileSize = "100KB"
                    }
                }
            };

            await LocalFileHelper.LoadExampleDocuments();
            LocalFileHelper.GetExampleDocumentBySize(parameters.Records.First().FileSize);

            await _filesInTeamSharePopulator.PopulateScenarioData(configuration, parameters);

            // Act
            var response = await controller.Download(Team2, fileName);

            // Assert
            response.Result.Should().BeOfType(typeof(NotFoundResult));

            Mock.VerifyAll(
              Mock.Get(MockMemoryCache),
              Mock.Get(MockDistributedCache));
        }
        #endregion


        #region Publish

        [TestMethod]
        public async Task Publish_WhenTeamIsInvalid_ReturnsUnprocessableEntityObjectResult()
        {
            // Arrange
            var team = "invalid-team-name";

            SetUpAllCaches<IEnumerable<AgencyTeam>>();

            var agencyPublishRequest = new Models.AgencyPublishRequest
            {
                ProductId = 1,
                UserInfo = new Models.UserInfo
                {
                    Principal = "user-principal"
                }
            };

            var controller = GetAgencyController();

            // Act
            var result = await controller.Publish(team, agencyPublishRequest);

            // Assert
            result.Result.Should().BeOfType<UnprocessableEntityObjectResult>();
            Mock.VerifyAll(
              Mock.Get(MockMemoryCache));
        }

        [TestMethod]
        public async Task Publish_WhenAgencyPublishRequestIsInvalid_ReturnsUnprocessableEntityObjectResult()
        {
            // Arrange
            var team = Team2;

            SetUpAllCaches<IEnumerable<AgencyTeam>>();
            SetUpAllCaches<IEnumerable<Product>>();

            var agencyPublishRequest = new Models.AgencyPublishRequest
            {
                ProductId = 0,
                UserInfo = new Models.UserInfo
                {
                    Principal = "user-principal"
                }
            };

            var controller = GetAgencyController();

            // Act
            var result = await controller.Publish(team, agencyPublishRequest);

            // Assert
            result.Result.Should().BeOfType<UnprocessableEntityObjectResult>();
            Mock.VerifyAll(
              Mock.Get(MockMemoryCache));
        }

        [TestMethod]
        public async Task Publish_WhenAgencyPublishRequestIsValid_ReturnsProductDetails()
        {
            // Arrange
            var team = Team2;

            SetUpAllCaches<IEnumerable<AgencyTeam>>();
            SetUpAllCaches<List<FileReferenceInfo>>();
            SetUpAllCaches<AgencyDocumentErrorType>();
            SetUpAllCaches<IEnumerable<Product>>();
            SetUpAllCaches<IEnumerable<FileExtensionInfo>>();
            SetUpMemoryCache(GetOrganisationDictionary(1));

            var configuration = GetFilesInTeamShareConfiguration(team.ToLower());
            var parameters = new FilesInTeamShareScenario.Parameters()
            {
                Records = GetDocumentRecords(1, 0, "10036")
            };

            await _filesInTeamSharePopulator.PopulateScenarioData(configuration, parameters);

            var agencyPublishRequest = new Models.AgencyPublishRequest
            {
                UserInfo = new Models.UserInfo
                {
                    Principal = "user-principal"
                },
                ProductId = 10036
            };

            var expected =
                new KeyValuePair<Models.Product, int>(
                    new Models.Product
                    {
                        AgencyTeams = new List<string> { team },
                        Identifier = 10036,
                        Name = "Post 16 grant assurance",
                        PluralName = "Post 16 grant assurances"
                    },
                    1);

            Mock.Get(SystemProvider.Guid)
                .Setup(gp => gp.NewGuid())
                .Returns(Guid.NewGuid());

            Mock.Get(DateTimeProvider)
                .Setup(dtp => dtp.UtcNow())
                .Returns(DateTime.UtcNow);

            Mock.Get(QueueManager)
                .Setup(qm => qm.GetQueue("virusscanrequired"))
                .Returns(
                    new AzureServiceBusQueue(MockConfig["ServiceBusConnectionString"], "virusscanrequired"));

            Mock.Get(_bulkJobManager)
                .Setup(manager => manager.CreateBulkJob(
                    It.IsAny<IEnumerable<Job<string, string>>>(),
                    It.IsAny<Func<string, Task<string>>>()))
                .ReturnsAsync(
                    (IEnumerable<Job<string, string>> _, Func<string, Task<string>> publishFunction) =>
                    {
                        publishFunction(string.Empty).GetAwaiter().GetResult();
                        return Guid.NewGuid();
                    });

            var controller = GetAgencyController();

            // Act
            var result = await controller.Publish(team, agencyPublishRequest);

            // Assert
            result.Result.Should()
                .BeOfType<OkObjectResult>()
                .Which.Value.Should()
                .BeEquivalentTo(expected);

            Mock.Get(QueueManager)
                .Verify(
                    qm => qm.GetQueue("virusscanrequired"),
                    Times.Once);

            Mock.VerifyAll(
              Mock.Get(MockMemoryCache),
              Mock.Get(MockDistributedCache),
              Mock.Get(SystemProvider),
              Mock.Get(DateTimeProvider));
        }

        #endregion


        public void Dispose()
        {
            Task.Run(() => _filesInTeamSharePopulator.TearDownScenarioData(GetFilesInTeamShareConfiguration(Team1.ToLower()))).GetAwaiter().GetResult();
            Task.Run(() => _filesInTeamSharePopulator.TearDownScenarioData(GetFilesInTeamShareConfiguration(Team2.ToLower()))).GetAwaiter().GetResult();
        }

        private AgencyController GetAgencyController()
        {
            var virusScanProcessor = GetVirusScanProcessor();
            var mockLogger = CreateMockLoggerAdapter<AgencyController>();
            var virusScanResultProcessor = GetVirusScanResultProcessor();
            var documentDownloader = GetDocumentDownloader();
            var documentManager = GetDocumentManager();
            var agencyService = GetAgencyService();
            var documentPublisher = GetDocumentPublisher();
            var validationService = GetValidationService();

            return new AgencyController(
                virusScanProcessor,
                virusScanResultProcessor,
                documentDownloader,
                documentManager,
                agencyService,
                documentPublisher,
                Mapper,
                validationService,
                mockLogger,
                null);
        }

        private IDocumentManager GetDocumentManager()
        {
            return new DocumentManager(
                GetDirectoriesManager(),
                CreateMockLoggerAdapter<DocumentManager>());
        }

        private IAgencyService GetAgencyService()
        {
            var directoriesManager = GetDirectoriesManager();
            var organisationsLookup = GetOrganisationsLookup();
            var cosmosDbService = GetCosmosDbService();
            var cacheManager = GetCacheManager();
            var configurationDataService = new ConfigurationDataService(cosmosDbService, cacheManager, CreateMockLoggerAdapter<ConfigurationDataService>());
            var productsLookup = new ProductsLookup(configurationDataService);
            var fileNameProvider = new FileNameProvider();
            var converter = new AgencyDocumentErrorTypeConverter();
            var filtersFactory = new AgencyDocumentFiltersFactory(productsLookup, configurationDataService, converter);

            var filtersManager = GetFiltersManager(filtersFactory);
            var teamsLookup = new TeamsLookup(configurationDataService);
            var agencyDocumentValidator = new AgencyDocumentValidator(productsLookup, configurationDataService);
            var filterToListResultConverter = new FilterToListResultConverter<AgencyDocument>(new PagingService());

            return new AgencyService(
                directoriesManager,
                organisationsLookup,
                productsLookup,
                fileNameProvider,
                filtersManager,
                teamsLookup,
                agencyDocumentValidator,
                converter,
                cacheManager,
                filterToListResultConverter,
                CreateMockLoggerAdapter<AgencyService>());
        }

        private IDocumentPublisher GetDocumentPublisher()
        {
            var cosmosDbService = GetCosmosDbService();

            return new DocumentPublisher(
                GetDirectoriesManager(),
                QueueManager,
                cosmosDbService,
                new FileNameProvider(),
                new ProductsLookup(new ConfigurationDataService(cosmosDbService, GetCacheManager(), CreateMockLoggerAdapter<ConfigurationDataService>())),
                GetFileMetadataUserEncryptor(),
                SystemProvider,
                Mapper,
                new DocumentPublisherConfiguration(),
                CreateMockLoggerAdapter<DocumentPublisher>(),
                GetAgencyService(),
                _bulkJobManager,
                new RetryMechanism(CreateMockLoggerAdapter<RetryMechanism>()));
        }

        private FilesInTeamShareScenario.Configuration GetFilesInTeamShareConfiguration(string shareName)
        {
            return new FilesInTeamShareScenario.Configuration
            {
                FileShareDirectory = new AzureFileShareDirectory.Configuration
                {
                    ShareName = shareName,
                    ConnectionString = MockConfig["TeamsFileShareConnectionString"]
                }
            };
        }

        private IEnumerable<DocumentRecord> GetDocumentRecords(int validRecords, int invalidRecords, string providerId)
        {
            var validDocuments = GetValidDocumentRecords(validRecords, providerId);
            var invalidDocuments = GetInvalidDocumentRecords(invalidRecords, providerId);

            return validDocuments.Concat(invalidDocuments);
        }

        private IEnumerable<DocumentRecord> GetValidDocumentRecords(int numberOfRecords, string providerId)
        {
            return Enumerable
                .Range(1, numberOfRecords)
                .Select(d => new DocumentRecord
                {
                    Ukprn = 10000000 + d,
                    ProviderId = providerId,
                    AcademicYear = "202021",
                    FileType = "pdf",
                    FileSize = "100KB"
                });
        }

        private IEnumerable<DocumentRecord> GetInvalidDocumentRecords(int numberOfRecords, string providerId)
        {
            return Enumerable
                .Range(1, numberOfRecords)
                .Select(d => new DocumentRecord
                {
                    Ukprn = (d % 2 == 0) ? d : 12345678,
                    ProviderId = (d % 2 == 0) ? providerId : d.ToString(),
                    AcademicYear = "202021",
                    FileType = "pdf",
                    FileSize = "100KB"
                });
        }

        private string GetFileNameFromDocumentRecord(DocumentRecord doc)
            => $"{doc.Ukprn}_{doc.ProviderId}_{doc.AcademicYear}.{doc.FileType}";

        private IDictionary<string, Organisation> GetOrganisationDictionary(int numberOfProviders)
        {
            Dictionary<string, Organisation> dict = new Dictionary<string, Organisation>();
            for (int i = 0; i < numberOfProviders; i++)
            {
                dict.Add((10000000 + (i + 1)).ToString(), new Organisation
                {
                    Name = "organisation",
                    Identifiers = new[]
                    {
                        new OrganisationIdentifier
                        {
                            Type = OrganisationIdentifierType.Ukprn,
                            Value = (10000000 + (i + 1)).ToString()
                        }
                    }
                });
            }

            return dict;
        }
    }
}