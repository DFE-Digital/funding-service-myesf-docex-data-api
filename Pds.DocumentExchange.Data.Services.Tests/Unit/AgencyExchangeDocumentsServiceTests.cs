using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pds.Core.Logging;
using Pds.DocumentExchange.Data.Services.Constants;
using Pds.DocumentExchange.Data.Services.DTOs;
using Pds.DocumentExchange.Data.Services.Enums;
using Pds.DocumentExchange.Data.Services.Implementations;
using Pds.DocumentExchange.Data.Services.Interfaces;
using Pds.DocumentExchange.Data.Services.Interfaces.Settings;
using Pds.DocumentExchange.Data.Services.Tests.Unit.Caching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Tests.Unit
{
    [TestClass, TestCategory("Unit")]
    public class AgencyExchangeDocumentsServiceTests : BaseCacheTests
    {
        private readonly Mock<IBatchesService> _batchesService = new Mock<IBatchesService>(MockBehavior.Strict);
        private readonly Mock<IBatchToExchangeDocumentConverter> _batchToExchangeDocumentConverter = new Mock<IBatchToExchangeDocumentConverter>(MockBehavior.Strict);
        private readonly Mock<IConfigurationDataService> _configurationService = new Mock<IConfigurationDataService>(MockBehavior.Strict);
        private readonly Mock<ILoggerAdapter<AgencyExchangeDocumentsService>> _logger = new Mock<ILoggerAdapter<AgencyExchangeDocumentsService>>();

        private readonly AgencyExchangeDocumentsService _agencyExchangeDocumentsService;

        public AgencyExchangeDocumentsServiceTests()
        {
            _agencyExchangeDocumentsService = new AgencyExchangeDocumentsService(
                _batchesService.Object,
                _batchToExchangeDocumentConverter.Object,
                CacheManager,
                _configurationService.Object,
                _logger.Object);
        }

        [TestMethod]
        public async Task GetReceivedFilesForAgencyWithSeenHistory_WhenTeamsAreValid_ReturnsDocumentsWithSeenStatus()
        {
            // Arrange
            var teams = new[] { "team-1" };

            var products = CreateTestProducts();

            var expectedResult = CreateDocumentsWithSeenStatus();

            _configurationService
                .Setup(b => b.GetProducts())
                .ReturnsAsync(products);

            _batchesService
               .Setup(b => b.GetReceivedFilesForAgencyWithSeenHistory(products))
               .ReturnsAsync(expectedResult);

            // Act
            var result = await _agencyExchangeDocumentsService.GetReceivedFilesForAgencyWithSeenHistory(teams);

            // Assert
            result.Should().BeEquivalentTo(expectedResult);

            Mock.VerifyAll(_configurationService, _batchesService);
        }

        [TestMethod]
        public void GetExchangeDocuments_WhenTeamsListIsNull_Throws()
        {
            // Act
            Func<Task<IEnumerable<ExchangeDocument>>> func =
                () => _agencyExchangeDocumentsService.GetExchangeDocuments(null, ExchangeDocumentDirection.PublishedByAgency);

            // Assert
            func.Should().ThrowAsync<ArgumentNullException>();
        }

        [TestMethod]
        public void GetExchangeDocuments_WhenTeamsListIsEmpty_Throws()
        {
            // Act
            Func<Task<IEnumerable<ExchangeDocument>>> func =
                () => _agencyExchangeDocumentsService.GetExchangeDocuments(Enumerable.Empty<string>(), ExchangeDocumentDirection.PublishedByAgency);

            // Assert
            func.Should().ThrowAsync<ArgumentNullException>();
        }

        [TestMethod]
        public async Task GetExchangeDocuments_WhenSingleTeamSpecifiedAndDirectionPublishedByAgency_ReturnsExchangeDocuments()
        {
            // Arrange
            var teams = new[] { "team-1" };
            var direction = ExchangeDocumentDirection.PublishedByAgency;

            var batches = CreateTestBatches();

            _batchesService
                .Setup(b => b.GetSentBatches(new[] { EsfaOrganisationInfo.Identifier }))
                .ReturnsAsync(batches);

            var exchangeDocuments = CreateExchangeDocumentsForTeams(new[] { "team-1", "team-2", "team-3" });

            _batchToExchangeDocumentConverter
                .Setup(c => c.ConvertToOrganisationExchangeDocuments(batches, direction))
                .ReturnsAsync(exchangeDocuments);

            SetupCacheGetMocks<IEnumerable<ExchangeDocument>>(
                cacheOptionsProvider => cacheOptionsProvider.DocumentList,
                cacheKeyBuilder => cacheKeyBuilder.BuildAgencyTeamExchangedDocumentsKey("AllTeams", direction),
                cacheKeyBuilder => cacheKeyBuilder.BuildAgencyTeamExchangedDocumentsKey("team-1", direction));

            var expectedResult = CreateExchangeDocumentsForTeam("team-1");

            // Act
            var result = await _agencyExchangeDocumentsService.GetExchangeDocuments(teams, direction);

            // Assert
            result.Should().BeEquivalentTo(expectedResult);

            Mock.VerifyAll(_batchesService, _batchToExchangeDocumentConverter);
            VerifyCacheMocks();
        }

        [TestMethod]
        public async Task GetExchangeDocuments_WhenSingleTeamSpecifiedAndDirectionSentByOrganisation_ReturnsExchangeDocuments()
        {
            // Arrange
            var teams = new[] { "team-1" };
            var direction = ExchangeDocumentDirection.SentByOrganisation;

            var batches = CreateTestBatches();

            _batchesService
                .Setup(b => b.GetReceivedBatches(new[] { EsfaOrganisationInfo.Identifier }))
                .ReturnsAsync(batches);

            var exchangeDocuments = CreateExchangeDocumentsForTeams(new[] { "team-1", "team-2", "team-3" });

            _batchToExchangeDocumentConverter
                .Setup(c => c.ConvertToOrganisationExchangeDocuments(batches, direction))
                .ReturnsAsync(exchangeDocuments);

            SetupCacheGetMocks<IEnumerable<ExchangeDocument>>(
                cacheOptionsProvider => cacheOptionsProvider.DocumentList,
                cacheKeyBuilder => cacheKeyBuilder.BuildAgencyTeamExchangedDocumentsKey("AllTeams", direction),
                cacheKeyBuilder => cacheKeyBuilder.BuildAgencyTeamExchangedDocumentsKey("team-1", direction));

            var expectedResult = CreateExchangeDocumentsForTeam("team-1");

            // Act
            var result = await _agencyExchangeDocumentsService.GetExchangeDocuments(teams, direction);

            // Assert
            result.Should().BeEquivalentTo(expectedResult);

            Mock.VerifyAll(_batchesService, _batchToExchangeDocumentConverter);
            VerifyCacheMocks();
        }

        [TestMethod]
        public async Task GetExchangeDocuments_WhenMultipleTeamsSpecifiedAndDirectionPublishedByAgency_ReturnsExchangeDocuments()
        {
            // Arrange
            var teams = new[] { "team-1", "team-2" };
            var direction = ExchangeDocumentDirection.PublishedByAgency;

            var batches = CreateTestBatches();

            _batchesService
                .Setup(b => b.GetSentBatches(new[] { EsfaOrganisationInfo.Identifier }))
                .ReturnsAsync(batches);

            var exchangeDocuments = CreateExchangeDocumentsForTeams(new[] { "team-1", "team-2", "team-3" });

            _batchToExchangeDocumentConverter
                .Setup(c => c.ConvertToOrganisationExchangeDocuments(batches, direction))
                .ReturnsAsync(exchangeDocuments);

            SetupCacheGetMocks<IEnumerable<ExchangeDocument>>(
                cacheOptionsProvider => cacheOptionsProvider.DocumentList,
                cacheKeyBuilder => cacheKeyBuilder.BuildAgencyTeamExchangedDocumentsKey("AllTeams", direction),
                cacheKeyBuilder => cacheKeyBuilder.BuildAgencyTeamExchangedDocumentsKey("team-1", direction),
                cacheKeyBuilder => cacheKeyBuilder.BuildAgencyTeamExchangedDocumentsKey("team-2", direction));

            var expectedResult = CreateExchangeDocumentsForTeams(teams);

            // Act
            var result = await _agencyExchangeDocumentsService.GetExchangeDocuments(teams, direction);

            // Assert
            result.Should().BeEquivalentTo(expectedResult);

            Mock.VerifyAll(_batchesService, _batchToExchangeDocumentConverter);
            VerifyCacheMocks();
        }

        [TestMethod]
        public async Task GetExchangeDocuments_WhenMultipleTeamsSpecifiedAndDirectionSentByOrganisation_ReturnsExchangeDocuments()
        {
            // Arrange
            var teams = new[] { "team-1", "team-2" };
            var direction = ExchangeDocumentDirection.SentByOrganisation;

            var batches = CreateTestBatches();

            _batchesService
                .Setup(b => b.GetReceivedBatches(new[] { EsfaOrganisationInfo.Identifier }))
                .ReturnsAsync(batches);

            var exchangeDocuments = CreateExchangeDocumentsForTeams(new[] { "team-1", "team-2", "team-3" });

            _batchToExchangeDocumentConverter
                .Setup(c => c.ConvertToOrganisationExchangeDocuments(batches, direction))
                .ReturnsAsync(exchangeDocuments);

            SetupCacheGetMocks<IEnumerable<ExchangeDocument>>(
                cacheOptionsProvider => cacheOptionsProvider.DocumentList,
                cacheKeyBuilder => cacheKeyBuilder.BuildAgencyTeamExchangedDocumentsKey("AllTeams", direction),
                cacheKeyBuilder => cacheKeyBuilder.BuildAgencyTeamExchangedDocumentsKey("team-1", direction),
                cacheKeyBuilder => cacheKeyBuilder.BuildAgencyTeamExchangedDocumentsKey("team-2", direction));

            var expectedResult = CreateExchangeDocumentsForTeams(teams);

            // Act
            var result = await _agencyExchangeDocumentsService.GetExchangeDocuments(teams, direction);

            // Assert
            result.Should().BeEquivalentTo(expectedResult);

            Mock.VerifyAll(_batchesService, _batchToExchangeDocumentConverter);
            VerifyCacheMocks();
        }


        [TestMethod]
        public void GetDeleteFiles_WhenInnerExceptionHappens_Throws()
        {
            // Arrange
            _batchesService
               .Setup(b => b.GetFileInfoForDelete(It.IsAny<ExchangeDocumentDirection>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception("Error"));

            // Act
            Func<Task<ListResult<ExchangeDocument>>> func =
                () => _agencyExchangeDocumentsService.GetDeleteFiles(ExchangeDocumentDirection.PublishedByAgency, 0, string.Empty, string.Empty);

            // Assert
            func.Should().ThrowAsync<Exception>();
        }

        //(ExchangeDocumentDirection direction, int ukprn, string fileType, string year);
        [TestMethod]
        public async Task GetDeleteFiles_ReturnsExchangeDocuments()
        {
            // Arrange
            var direction = ExchangeDocumentDirection.PublishedByAgency;
            int ukprn = 12345678;
            string fileType = "file-type1";
            string year = "202021";

            var batches = CreateTestBatches();

            _batchesService
                .Setup(b => b.GetFileInfoForDelete(direction, ukprn, fileType, year))
                .ReturnsAsync(batches);

            var exchangeDocuments = CreateExchangeDocumentsForTeams(new[] { "team-1" });

            _batchToExchangeDocumentConverter
                .Setup(c => c.ConvertToOrganisationExchangeDocuments(batches, direction))
                .ReturnsAsync(exchangeDocuments);

            var items = CreateExchangeDocumentsForTeam("team-1");

            var expectedResult = new ListResult<ExchangeDocument>
            {
                Items = items,
                Filters = null,
                TotalItems = items.Count(),
                TotalPages = 0
            };

            // Act
            var result = await _agencyExchangeDocumentsService.GetDeleteFiles(direction, ukprn, fileType, year);

            // Assert
            result.Should().BeEquivalentTo(expectedResult);

            Mock.VerifyAll(_batchesService, _batchToExchangeDocumentConverter);
            VerifyCacheMocks();
        }

        private IEnumerable<BatchMetadata> CreateTestBatches()
            => Enumerable.Range(1, 10)
                .Select(i => new BatchMetadata
                {
                    Id = $"batch-id-{i}",
                    ParentBatchIdentifier = $"parent-batch-id-{i}",
                    Files = Enumerable.Range(1, 5)
                        .Select(j => new FileMetadata
                        {
                            FileName = $"file-{j}.pdf",
                            Deleted = false
                        })
                })
                .ToList();

        private IReadOnlyCollection<Product> CreateTestProducts()
            => Enumerable.Range(1, 10)
                .Select(i => new Product
                {
                    AgencyTeams = new List<string> { "team-1" },
                    Identifier = 1
                })
                .ToList();

        private IReadOnlyCollection<DocumentWithViewedStatus> CreateDocumentsWithSeenStatus()
            => Enumerable.Range(1, 10)
                .Select(i => new DocumentWithViewedStatus
                {
                    FileType = $"fileType-{i}",
                    FromUKPRN = 1,
                    Version = 1,
                    Viewed = true,
                    Year = "202021"
                })
                .ToList();

        private IEnumerable<ExchangeDocument> CreateExchangeDocumentsForTeams(IEnumerable<string> teams)
            => teams.Select(team => CreateExchangeDocumentsForTeam(team)).SelectMany(exchangeDoc => exchangeDoc);

        private IEnumerable<ExchangeDocument> CreateExchangeDocumentsForTeam(string team)
           => Enumerable.Range(1, 5)
               .Select(i => new ExchangeDocument
               {
                   DocumentReference = new DocumentReference
                   {
                       BatchIdentifier = $"batch-id-{i}-{team}",
                       ParentBatchIdentifier = $"parent-batch-id-{i}-{team}",
                       FileName = $"file-{i}-{team}.pdf"
                   },
                   Product = new Product
                   {
                       Identifier = i,
                       Name = $"product-{i}-{team}",
                       AgencyTeams = new[] { team }
                   }
               })
               .ToList();
    }
}