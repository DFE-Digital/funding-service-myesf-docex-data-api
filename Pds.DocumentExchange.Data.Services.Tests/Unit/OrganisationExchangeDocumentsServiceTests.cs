using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pds.Core.Common.Organisation.Models;
using Pds.Core.Logging;
using Pds.DocumentExchange.Data.Services.DTOs;
using Pds.DocumentExchange.Data.Services.Enums;
using Pds.DocumentExchange.Data.Services.Implementations;
using Pds.DocumentExchange.Data.Services.Interfaces;
using Pds.DocumentExchange.Data.Services.Interfaces.Lookup;
using Pds.DocumentExchange.Data.Services.Tests.Unit.Caching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Tests.Unit
{
    [TestClass, TestCategory("Unit")]
    public class OrganisationExchangeDocumentsServiceTests : BaseCacheTests
    {
        private readonly Mock<IBatchesService> _batchesService = new Mock<IBatchesService>(MockBehavior.Strict);
        private readonly Mock<IBatchToExchangeDocumentConverter> _batchToExchangeDocumentConverter = new Mock<IBatchToExchangeDocumentConverter>(MockBehavior.Strict);
        private readonly Mock<IOrganisationsLookup> _organisationsLookup = new Mock<IOrganisationsLookup>(MockBehavior.Strict);
        private readonly Mock<ILoggerAdapter<OrganisationExchangeDocumentsService>> _logger = new Mock<ILoggerAdapter<OrganisationExchangeDocumentsService>>();

        private readonly OrganisationExchangeDocumentsService _organisationExchangeDocumentsService;

        public OrganisationExchangeDocumentsServiceTests()
        {
            _organisationExchangeDocumentsService = new OrganisationExchangeDocumentsService(
                _batchesService.Object,
                _batchToExchangeDocumentConverter.Object,
                CacheManager,
                _organisationsLookup.Object,
                _logger.Object);
        }

        [TestMethod]
        public void GetExchangeDocuments_WhenOrganisationIdentifierIsNull_Throws()
        {
            // Act
            Func<Task<IEnumerable<ExchangeDocument>>> func =
                () => _organisationExchangeDocumentsService.GetExchangeDocuments(null, ExchangeDocumentDirection.PublishedByAgency);

            // Assert
            func.Should().ThrowAsync<ArgumentNullException>();
        }

        [TestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow(" ")]
        public void GetExchangeDocuments_WhenOrganisationIdentifierValueIsNullOrEmpty_Throws(string value)
        {
            // Arrange
            var organisationIdentifier = new OrganisationIdentifier
            {
                Value = value
            };

            // Act
            Func<Task<IEnumerable<ExchangeDocument>>> func =
                () => _organisationExchangeDocumentsService.GetExchangeDocuments(organisationIdentifier, ExchangeDocumentDirection.PublishedByAgency);

            // Assert
            func.Should().ThrowAsync<ArgumentNullException>();
        }

        [TestMethod]
        public async Task GetExchangeDocuments_WhenOrganisationSpecifiedAndDirectionPublishedByAgency_ReturnsExchangeDocuments()
        {
            // Arrange
            var organisationIdentifier = new OrganisationIdentifier
            {
                Value = "organisation-value"
            };

            var organisationAllIdentifiers = new[] { organisationIdentifier };

            var direction = ExchangeDocumentDirection.PublishedByAgency;

            _organisationsLookup
                .Setup(lookup => lookup.GetSelfAndChildUkprns(organisationIdentifier))
                .ReturnsAsync(organisationAllIdentifiers);

            var batches = CreateTestBatches();

            _batchesService
                .Setup(b => b.GetReceivedBatches(organisationAllIdentifiers))
                .ReturnsAsync(batches);

            var exchangeDocuments = CreateExchangeDocuments();

            _batchToExchangeDocumentConverter
                .Setup(c => c.ConvertToOrganisationExchangeDocuments(batches, direction))
                .ReturnsAsync(exchangeDocuments);

            SetupCacheGetMocks<List<ExchangeDocument>>(
                cacheOptionsProvider => cacheOptionsProvider.DocumentList,
                cacheKeyBuilder => cacheKeyBuilder.BuildOrganisationExchangedDocumentsKey(organisationIdentifier, direction));

            // Act
            var result = await _organisationExchangeDocumentsService.GetExchangeDocuments(organisationIdentifier, direction);

            // Assert
            result.Should().BeEquivalentTo(exchangeDocuments);

            Mock.VerifyAll(_organisationsLookup, _batchesService, _batchToExchangeDocumentConverter);
            VerifyCacheMocks();
        }

        [TestMethod]
        public async Task GetExchangeDocuments_WhenOrganisationSpecifiedAndDirectionSentByOrganisation_ReturnsExchangeDocuments()
        {
            // Arrange
            var organisationIdentifier = new OrganisationIdentifier
            {
                Value = "organisation-value"
            };

            var organisationAllIdentifiers = new[] { organisationIdentifier };

            var direction = ExchangeDocumentDirection.SentByOrganisation;

            _organisationsLookup
                .Setup(lookup => lookup.GetSelfAndChildUkprns(organisationIdentifier))
                .ReturnsAsync(organisationAllIdentifiers);

            var batches = CreateTestBatches();

            _batchesService
                .Setup(b => b.GetSentBatches(organisationAllIdentifiers))
                .ReturnsAsync(batches);

            var exchangeDocuments = CreateExchangeDocuments();

            _batchToExchangeDocumentConverter
                .Setup(c => c.ConvertToOrganisationExchangeDocuments(batches, direction))
                .ReturnsAsync(exchangeDocuments);

            SetupCacheGetMocks<List<ExchangeDocument>>(
                cacheOptionsProvider => cacheOptionsProvider.DocumentList,
                cacheKeyBuilder => cacheKeyBuilder.BuildOrganisationExchangedDocumentsKey(organisationIdentifier, direction));

            // Act
            var result = await _organisationExchangeDocumentsService.GetExchangeDocuments(organisationIdentifier, direction);

            // Assert
            result.Should().BeEquivalentTo(exchangeDocuments);

            Mock.VerifyAll(_organisationsLookup, _batchesService, _batchToExchangeDocumentConverter);
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

        private IEnumerable<ExchangeDocument> CreateExchangeDocuments()
           => Enumerable.Range(1, 5)
               .Select(i => new ExchangeDocument
               {
                   DocumentReference = new DocumentReference
                   {
                       BatchIdentifier = $"batch-id-{i}",
                       ParentBatchIdentifier = $"parent-batch-id-{i}",
                       FileName = $"file-{i}.pdf"
                   }
               })
               .ToList();
    }
}