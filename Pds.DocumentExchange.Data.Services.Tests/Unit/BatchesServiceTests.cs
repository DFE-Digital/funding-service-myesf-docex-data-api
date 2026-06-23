using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pds.Core.Common.Organisation.Models;
using Pds.Core.Logging;
using Pds.DocumentExchange.Data.Services.DTOs;
using Pds.DocumentExchange.Data.Services.Enums;
using Pds.DocumentExchange.Data.Services.Implementations;
using Pds.DocumentExchange.Data.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Tests.Unit
{
    [TestClass, TestCategory("Unit")]
    public class BatchesServiceTests
    {
        private readonly Mock<ICosmosDbService> _cosmosDbService = new Mock<ICosmosDbService>(MockBehavior.Strict);
        private readonly Mock<ILoggerAdapter<BatchesService>> _logger = new Mock<ILoggerAdapter<BatchesService>>();

        private readonly BatchesService _batchesService;

        public BatchesServiceTests()
        {
            _batchesService = new BatchesService(_cosmosDbService.Object, _logger.Object);
        }

        #region GetFileInfoForDelete

        [TestMethod]
        [DataRow(ExchangeDocumentDirection.PublishedByAgency)]
        [DataRow(ExchangeDocumentDirection.SentByOrganisation)]
        public async Task GetFileInfoForDelete_WhenCalledWithDirection_CallsCorrectStoredProcedureAsync(ExchangeDocumentDirection direction)
        {
            //Arrange
            int ukprn = 12345678;
            string fileType = "filetype1";
            string year = "202021";

            _cosmosDbService.Setup(c => c.GetOrganisationDeleteFilesInfo(ukprn, fileType))
            .ReturnsAsync(new List<BatchMetadata>());

            _cosmosDbService.Setup(c => c.GetAgencyDeleteFilesInfo(ukprn, fileType, year))
           .ReturnsAsync(new List<BatchMetadata>());

            // Act
            await _batchesService.GetFileInfoForDelete(direction, ukprn, fileType, year);

            // Assert
            if (direction == ExchangeDocumentDirection.PublishedByAgency)
            {
                _cosmosDbService.Verify(c => c.GetAgencyDeleteFilesInfo(ukprn, fileType, year), Times.Once);
                _cosmosDbService.Verify(c => c.GetOrganisationDeleteFilesInfo(ukprn, fileType), Times.Never);
            }
            else
            {
                _cosmosDbService.Verify(c => c.GetOrganisationDeleteFilesInfo(ukprn, fileType), Times.Once);
                _cosmosDbService.Verify(c => c.GetAgencyDeleteFilesInfo(ukprn, fileType, year), Times.Never);
            }
        }

        #endregion

        #region GetReceivedBatches

        [TestMethod]
        public void GetReceivedBatches_WhenOrganisationIdentifiersListIsNull_Throws()
        {
            // Act
            Func<Task<IEnumerable<BatchMetadata>>> func = () => _batchesService.GetReceivedBatches(null);

            // Assert
            func.Should().ThrowAsync<ArgumentNullException>();
        }

        [TestMethod]
        public void GetReceivedBatches_WhenOrganisationIdentifiersListIsEmpty_Throws()
        {
            // Act
            Func<Task<IEnumerable<BatchMetadata>>> func = () => _batchesService.GetReceivedBatches(Enumerable.Empty<OrganisationIdentifier>());

            // Assert
            func.Should().ThrowAsync<ArgumentNullException>();
        }

        [TestMethod]
        public void GetReceivedBatches_WhenOrganisationIdentifiersListContainsNull_Throws()
        {
            // Arrange
            var ids = new[]
            {
                new OrganisationIdentifier
                {
                    Value = "organisation-value"
                },
                null
            };

            // Act
            Func<Task<IEnumerable<BatchMetadata>>> func = () => _batchesService.GetReceivedBatches(ids);

            // Assert
            func.Should().ThrowAsync<ArgumentNullException>();
        }

        [TestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow(" ")]
        public void GetReceivedBatches_WhenOrganisationIdentifiersListContainsItemWithNullOrEmptyValue_Throws(string value)
        {
            // Arrange
            var ids = new[]
            {
                new OrganisationIdentifier
                {
                    Value = "organisation-value"
                },
                new OrganisationIdentifier
                {
                    Value = value
                }
            };

            // Act
            Func<Task<IEnumerable<BatchMetadata>>> func = () => _batchesService.GetReceivedBatches(ids);

            // Assert
            func.Should().ThrowAsync<ArgumentNullException>();
        }

        #endregion


        #region GetSentBatches

        [TestMethod]
        public void GetSentBatches_WhenOrganisationIdentifiersListIsNull_Throws()
        {
            // Act
            Func<Task<IEnumerable<BatchMetadata>>> func = () => _batchesService.GetSentBatches(null);

            // Assert
            func.Should().ThrowAsync<ArgumentNullException>();
        }

        [TestMethod]
        public void GetSentBatches_WhenOrganisationIdentifiersListIsEmpty_Throws()
        {
            // Act
            Func<Task<IEnumerable<BatchMetadata>>> func = () => _batchesService.GetSentBatches(Enumerable.Empty<OrganisationIdentifier>());

            // Assert
            func.Should().ThrowAsync<ArgumentNullException>();
        }

        [TestMethod]
        public void GetSentBatches_WhenOrganisationIdentifiersListContainsNull_Throws()
        {
            // Arrange
            var ids = new[]
            {
                new OrganisationIdentifier
                {
                    Value = "organisation-value"
                },
                null
            };

            // Act
            Func<Task<IEnumerable<BatchMetadata>>> func = () => _batchesService.GetSentBatches(ids);

            // Assert
            func.Should().ThrowAsync<ArgumentNullException>();
        }

        [TestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow(" ")]
        public void GetSentBatches_WhenOrganisationIdentifiersListContainsItemWithNullOrEmptyValue_Throws(string value)
        {
            // Arrange
            var ids = new[]
            {
                new OrganisationIdentifier
                {
                    Value = "organisation-value"
                },
                new OrganisationIdentifier
                {
                    Value = value
                }
            };

            // Act
            Func<Task<IEnumerable<BatchMetadata>>> func = () => _batchesService.GetSentBatches(ids);

            // Assert
            func.Should().ThrowAsync<ArgumentNullException>();
        }

        #endregion

        private IEnumerable<BatchMetadata> CreateTestBatches()
        {
            var batchesContainingDeletedFiles = Enumerable.Range(1, 10)
                .Select(i => new BatchMetadata
                {
                    Id = $"batch-id-{i}",
                    ParentBatchIdentifier = $"parent-batch-id-{i}",
                    Files = Enumerable.Range(1, 10)
                        .Select(j => new FileMetadata
                        {
                            FileName = $"file-{j}.pdf",
                            Deleted = j % 2 == 0
                        })
                });

            var batchesWithAllFilesDeleted = Enumerable.Range(11, 5)
                .Select(i => new BatchMetadata
                {
                    Id = $"batch-id-{i}",
                    ParentBatchIdentifier = $"parent-batch-id-{i}",
                    Files = Enumerable.Range(1, 10)
                        .Select(j => new FileMetadata
                        {
                            FileName = $"file-{j}.pdf",
                            Deleted = true
                        })
                });

            return batchesContainingDeletedFiles.Concat(batchesWithAllFilesDeleted).ToList();
        }

        private IEnumerable<BatchMetadata> CreateTestResultBatches()
        {
            return Enumerable.Range(1, 10)
                .Select(i => new BatchMetadata
                {
                    Id = $"batch-id-{i}",
                    ParentBatchIdentifier = $"parent-batch-id-{i}",
                    Files = Enumerable.Range(1, 10)
                        .Where(j => j % 2 > 0)
                        .Select(j => new FileMetadata
                        {
                            FileName = $"file-{j}.pdf",
                            Deleted = false
                        })
                })
                .ToList();
        }
    }
}