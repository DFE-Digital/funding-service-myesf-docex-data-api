using FluentAssertions;
using MapsterMapper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pds.Core.Common.Organisation.Enums;
using Pds.Core.Common.Organisation.Models;
using Pds.Core.Logging;
using Pds.DocumentExchange.Data.Services.DTOs;
using Pds.DocumentExchange.Data.Services.DTOs.User;
using Pds.DocumentExchange.Data.Services.Enums;
using Pds.DocumentExchange.Data.Services.Implementations;
using Pds.DocumentExchange.Data.Services.Interfaces;
using Pds.DocumentExchange.Data.Services.Interfaces.Lookup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Tests.Unit
{
    [TestClass]
    public class BatchToExchangeDocumentConverterTests
    {
        private readonly Mock<IProductsLookup> _mockProductsLookup = new Mock<IProductsLookup>();
        private readonly Mock<IOrganisationsLookup> _mockOrganisationsLookup = new Mock<IOrganisationsLookup>();
        private readonly Mock<IFileMetadataUserEncryptor> _mockFileMetadataUserEncryptor = new Mock<IFileMetadataUserEncryptor>();
        private readonly Mock<IMapper> _mockMapper = new Mock<IMapper>();
        private readonly Mock<ILoggerAdapter<BatchToExchangeDocumentConverter>> _mockLoggingService = new Mock<ILoggerAdapter<BatchToExchangeDocumentConverter>>(MockBehavior.Loose);

        private readonly BatchToExchangeDocumentConverter _batchToExchangeDocumentConverter;

        public BatchToExchangeDocumentConverterTests()
        {
            _batchToExchangeDocumentConverter = new BatchToExchangeDocumentConverter(
                _mockProductsLookup.Object,
                _mockOrganisationsLookup.Object,
                _mockFileMetadataUserEncryptor.Object,
                _mockMapper.Object,
                _mockLoggingService.Object);
        }

        #region ConvertToAgencyExchangeDocument

        [TestMethod, TestCategory("Unit")]
        [DataRow(null)]
        [DataRow("")]
        [DataRow(" ")]
        public void ConvertToAgencyExchangeDocument_WhenParentBatchIdentifierIsNullOrEmpty_Throws(string parentBatchIdentifier)
        {
            // Arrange
            var fileMetadata = CreateFileMetadata(1, 12345678, 10001, 202021, 1);

            // Act
            Func<Task<ExchangeDocument>> func = () => _batchToExchangeDocumentConverter.ConvertToAgencyExchangeDocument(parentBatchIdentifier, "batch-id", fileMetadata, ExchangeDocumentDirection.SentByOrganisation);

            // Assert
            func.Should().ThrowAsync<ArgumentNullException>();
        }

        [TestMethod, TestCategory("Unit")]
        [DataRow(null)]
        [DataRow("")]
        [DataRow(" ")]
        public void ConvertToAgencyExchangeDocument_WhenBatchIdentifierIsNullOrEmpty_Throws(string batchIdentifier)
        {
            // Arrange
            var fileMetadata = CreateFileMetadata(1, 12345678, 10001, 202021, 1);

            // Act
            Func<Task<ExchangeDocument>> func = () => _batchToExchangeDocumentConverter.ConvertToAgencyExchangeDocument("parent-batch-id", batchIdentifier, fileMetadata, ExchangeDocumentDirection.SentByOrganisation);

            // Assert
            func.Should().ThrowAsync<ArgumentNullException>();
        }

        [TestMethod, TestCategory("Unit")]
        public void ConvertToAgencyExchangeDocument_WhenFileMetadataIsNull_Throws()
        {
            // Act
            Func<Task<ExchangeDocument>> func = () => _batchToExchangeDocumentConverter.ConvertToAgencyExchangeDocument("parent-batch-id", "batch-id", null, ExchangeDocumentDirection.SentByOrganisation);

            // Assert
            func.Should().ThrowAsync<ArgumentNullException>();
        }

        [TestMethod, TestCategory("Unit")]
        [DataRow(1, 12345678, 10001, 202021, 1, "team-1")]
        [DataRow(2, 10079316, 10084, 201819, 5, "funding-team")]
        [DataRow(99, 99999999, 99999, 190001, 10, "test-team")]
        public async Task ConvertToAgencyExchangeDocument_WhenInputIsCorrect_ReturnsExchangeDocument(int number, int ukprn, int productId, int year, int version, string team)
        {
            // Arrange
            var fileMetadata = CreateFileMetadata(number, ukprn, productId, year, version);
            var expectedResult = CreateExchangeDocument(number, ukprn, productId, year, version, team);

            SetupProductsLookup(team);
            SetupOrganisationLookup();
            SetupUserMapping();

            // Act
            var result = await _batchToExchangeDocumentConverter.ConvertToAgencyExchangeDocument(
                $"parent-batch-id-{number}",
                $"batch-id-{number}",
                fileMetadata,
                ExchangeDocumentDirection.SentByOrganisation);

            // Assert
            result.Should().BeEquivalentTo(expectedResult);
        }

        #endregion


        #region ConvertToAgencyExchangeDocuments

        [TestMethod, TestCategory("Unit")]
        public async Task ConvertToAgencyExchangeDocuments_WhenBatchCollectionIsEmpty_ReturnsEmptyExchangeDocumentsCollection()
        {
            // Arrange
            var emptyBatchesCollection = Enumerable.Empty<BatchMetadata>();
            var expectedResult = Enumerable.Empty<ExchangeDocument>();

            // Act
            var result = await _batchToExchangeDocumentConverter.ConvertToAgencyExchangeDocuments(
                emptyBatchesCollection,
                ExchangeDocumentDirection.SentByOrganisation);

            // Assert
            result.Should().BeEquivalentTo(expectedResult);
        }

        [TestMethod, TestCategory("Unit")]
        public async Task ConvertToAgencyExchangeDocuments_WhenBatchCollectionHasNoFiles_ReturnsEmptyExchangeDocumentsCollection()
        {
            // Arrange
            var batches = new[]
            {
                new BatchMetadata
                {
                    Id = "batch-id-1",
                    ParentBatchIdentifier = "parent-batch-id-1",
                    Files = Enumerable.Empty<FileMetadata>(),
                }
            };

            // Act
            var result = await _batchToExchangeDocumentConverter.ConvertToAgencyExchangeDocuments(
                batches,
                ExchangeDocumentDirection.SentByOrganisation);

            // Assert
            result.Should().BeEquivalentTo(Enumerable.Empty<ExchangeDocument>());
        }

        [TestMethod, TestCategory("Unit")]
        public async Task ConvertToAgencyExchangeDocuments_WhenBatchCollectionIsNull_ThrowArgumentNullException()
        {
            // Arrange
            IEnumerable<BatchMetadata> nullBatchesCollection = null;

            // Act
            Func<Task<IEnumerable<ExchangeDocument>>> func = async () => await _batchToExchangeDocumentConverter.ConvertToAgencyExchangeDocuments(
                nullBatchesCollection,
                ExchangeDocumentDirection.SentByOrganisation);

            // Assert
            await func.Should().ThrowAsync<ArgumentNullException>();
        }

        [TestMethod, TestCategory("Unit")]
        public async Task ConvertToAgencyExchangeDocuments_WhenBatchCollectionHasFiles_ReturnsExchangeDocumentsCollection()
        {
            // Arrange
            var team = "test-agency-team";

            var batches = CreateTestBatches();

            var exchangeDocuments = CreateTestExchangeDocuments(team);

            SetupProductsLookup(team);
            SetupOrganisationLookup();
            SetupUserMapping();

            // Act
            var result = await _batchToExchangeDocumentConverter.ConvertToAgencyExchangeDocuments(
                batches,
                ExchangeDocumentDirection.SentByOrganisation);

            // Assert
            result.Should().BeEquivalentTo(exchangeDocuments);
        }

        #endregion


        #region ConvertToOrganisationExchangeDocuments

        [TestMethod, TestCategory("Unit")]
        public async Task ConvertToOrganisationExchangeDocuments_WhenBatchCollectionIsEmpty_ReturnsEmptyExchangeDocumentsCollection()
        {
            // Arrange
            var emptyBatchesCollection = Enumerable.Empty<BatchMetadata>();
            var expectedResult = Enumerable.Empty<ExchangeDocument>();

            // Act
            var result = await _batchToExchangeDocumentConverter.ConvertToOrganisationExchangeDocuments(
                emptyBatchesCollection,
                ExchangeDocumentDirection.SentByOrganisation);

            // Assert
            result.Should().BeEquivalentTo(expectedResult);
        }

        [TestMethod, TestCategory("Unit")]
        public async Task ConvertToOrganisationExchangeDocuments_WhenBatchCollectionHasNoFiles_ReturnsEmptyExchangeDocumentsCollection()
        {
            // Arrange
            var batches = new[]
            {
                new BatchMetadata
                {
                    Id = "batch-id-1",
                    ParentBatchIdentifier = "parent-batch-id-1",
                    Files = Enumerable.Empty<FileMetadata>(),
                }
            };

            // Act
            var result = await _batchToExchangeDocumentConverter.ConvertToOrganisationExchangeDocuments(
                batches,
                ExchangeDocumentDirection.SentByOrganisation);

            // Assert
            result.Should().BeEquivalentTo(Enumerable.Empty<ExchangeDocument>());
        }

        [TestMethod, TestCategory("Unit")]
        public async Task ConvertToOrganisationExchangeDocuments_WhenBatchCollectionIsNull_ThrowArgumentNullException()
        {
            // Arrange
            IEnumerable<BatchMetadata> nullBatchesCollection = null;

            // Act
            Func<Task<IEnumerable<ExchangeDocument>>> func = async () => await _batchToExchangeDocumentConverter.ConvertToOrganisationExchangeDocuments(
                nullBatchesCollection,
                ExchangeDocumentDirection.SentByOrganisation);

            // Assert
            await func.Should().ThrowAsync<ArgumentNullException>();
        }

        [TestMethod, TestCategory("Unit")]
        public async Task ConvertToOrganisationExchangeDocuments_WhenBatchCollectionHasFiles_ReturnsExchangeDocumentsCollection()
        {
            // Arrange
            var batches = CreateTestBatches();
            var exchangeDocuments = CreateTestExchangeDocuments(string.Empty);

            SetupProductsLookup(string.Empty);
            SetupOrganisationLookup();
            SetupUserMapping();

            // Act
            var result = await _batchToExchangeDocumentConverter.ConvertToOrganisationExchangeDocuments(
                batches,
                ExchangeDocumentDirection.SentByOrganisation);

            // Assert
            result.Should().BeEquivalentTo(exchangeDocuments);
        }

        [TestMethod, TestCategory("Unit")]
        public async Task ConvertToOrganisationExchangeDocuments_WhenFileMetadataIsNull_ReturnsExchangeDocumentsCollection()
        {
            // Arrange
            var batch = CreateBatchMetadata(1, 12345678, 10002, 201819, 1);
            var batchFile = batch.Files.First();
            batchFile.Metadata = null;

            var exchangeMetadata = CreateExchangeDocument(1, 12345678, 10002, 201819, 1, string.Empty);
            exchangeMetadata.Year = -1;

            var batches = new[] { batch };
            var exchangeDocuments = new[] { exchangeMetadata };

            SetupProductsLookup(string.Empty);
            SetupOrganisationLookup();
            SetupUserMapping();

            // Act
            var result = await _batchToExchangeDocumentConverter.ConvertToOrganisationExchangeDocuments(
                batches,
                ExchangeDocumentDirection.SentByOrganisation);

            // Assert
            result.Should().BeEquivalentTo(exchangeDocuments);
        }

        [TestMethod, TestCategory("Unit")]
        public async Task ConvertToOrganisationExchangeDocuments_WhenFileDoesntContainYear_ReturnsExchangeDocumentsCollection()
        {
            // Arrange
            var batch = CreateBatchMetadata(1, 12345678, 10002, 201819, 1);
            var batchFile = batch.Files.First();
            batchFile.Metadata = new Dictionary<string, string>
            {
                ["some-key"] = "some-value"
            };

            var exchangeMetadata = CreateExchangeDocument(1, 12345678, 10002, 201819, 1, string.Empty);
            exchangeMetadata.Year = -1;

            var batches = new[] { batch };
            var exchangeDocuments = new[] { exchangeMetadata };

            SetupProductsLookup(string.Empty);
            SetupOrganisationLookup();
            SetupUserMapping();

            // Act
            var result = await _batchToExchangeDocumentConverter.ConvertToOrganisationExchangeDocuments(
                batches,
                ExchangeDocumentDirection.SentByOrganisation);

            // Assert
            result.Should().BeEquivalentTo(exchangeDocuments);
        }

        [TestMethod, TestCategory("Unit")]
        [DataRow(null)]
        [DataRow("")]
        [DataRow(" ")]
        [DataRow("not-a-number")]
        public async Task ConvertToOrganisationExchangeDocuments_WhenFileHasNonNumericYear_ReturnsExchangeDocumentsCollection(string year)
        {
            // Arrange
            var batch = CreateBatchMetadata(1, 12345678, 10002, 201819, 1);
            var batchFile = batch.Files.First();
            batchFile.Metadata = new Dictionary<string, string>
            {
                ["Year"] = year
            };

            var exchangeMetadata = CreateExchangeDocument(1, 12345678, 10002, 201819, 1, string.Empty);
            exchangeMetadata.Year = -1;

            var batches = new[] { batch };
            var exchangeDocuments = new[] { exchangeMetadata };

            SetupProductsLookup(string.Empty);
            SetupOrganisationLookup();
            SetupUserMapping();

            // Act
            var result = await _batchToExchangeDocumentConverter.ConvertToOrganisationExchangeDocuments(
                batches,
                ExchangeDocumentDirection.SentByOrganisation);

            // Assert
            result.Should().BeEquivalentTo(exchangeDocuments);
        }

        [TestMethod, TestCategory("Unit")]
        [DataRow("201920")]
        [DataRow("202021")]
        [DataRow("350001")]
        public async Task ConvertToOrganisationExchangeDocuments_WhenFileHasNumericYear_ReturnsExchangeDocumentsCollection(string year)
        {
            // Arrange
            var batch = CreateBatchMetadata(1, 12345678, 10002, 201819, 1);
            var batchFile = batch.Files.First();
            batchFile.Metadata = new Dictionary<string, string>
            {
                ["Year"] = year
            };

            var exchangeMetadata = CreateExchangeDocument(1, 12345678, 10002, 201819, 1, string.Empty);
            exchangeMetadata.Year = int.Parse(year);

            var batches = new[] { batch };
            var exchangeDocuments = new[] { exchangeMetadata };

            SetupProductsLookup(string.Empty);
            SetupOrganisationLookup();
            SetupUserMapping();

            // Act
            var result = await _batchToExchangeDocumentConverter.ConvertToOrganisationExchangeDocuments(
                batches,
                ExchangeDocumentDirection.SentByOrganisation);

            // Assert
            result.Should().BeEquivalentTo(exchangeDocuments);
        }

        #endregion

        private IEnumerable<BatchMetadata> CreateTestBatches()
        {
            return new[]
            {
                CreateBatchMetadata(1, 12345678, 10002, 201819, 1),

                CreateBatchMetadata(2, 87654321, 10002, 201920, 2),
                CreateBatchMetadata(3, 87654321, 10002, 201920, 1),

                CreateBatchMetadata(4, 11223344, 10005, 201920, 3),
                CreateBatchMetadata(5, 11223344, 10005, 201920, 2),
                CreateBatchMetadata(6, 11223344, 10005, 201920, 1),

                CreateBatchMetadata(7, 99999999, 10002, 202021, 4),
                CreateBatchMetadata(8, 99999999, 10002, 202021, 3),
                CreateBatchMetadata(9, 99999999, 10002, 202021, 2),
                CreateBatchMetadata(10, 99999999, 10002, 202021, 1)
            };
        }

        private BatchMetadata CreateBatchMetadata(int number, int fromUkprn, int productId, int year, int version)
            => new BatchMetadata
            {
                Id = $"batch-id-{number}",
                ParentBatchIdentifier = $"parent-batch-id-{number}",
                Files = new[] { CreateFileMetadata(number, fromUkprn, productId, year, version) }
            };

        private FileMetadata CreateFileMetadata(int number, int fromUkprn, int productId, int year, int version)
            => new FileMetadata
            {
                FileName = $"file-{number}.pdf",
                ProductIdentifier = productId.ToString(),
                FromUkprn = fromUkprn,
                Metadata = new Dictionary<string, string> { ["Year"] = year.ToString() },
                Version = version,
                History = new[]
                {
                    CreateFileMetadataHistory("user-1", FileAction.UploadedExternal, new DateTime(2019, 1, 1)),
                    CreateFileMetadataHistory("user-2", FileAction.ViewedByReceiver, new DateTime(2019, 6, 1))
                }
            };

        private IEnumerable<ExchangeDocument> CreateTestExchangeDocuments(string team)
        {
            var exchangeDoc1 = CreateExchangeDocument(1, 12345678, 10002, 201819, 1, team);

            var exchangeDoc2 = CreateExchangeDocument(2, 87654321, 10002, 201920, 2, team);
            exchangeDoc2.PreviousVersions = new[]
            {
                CreateExchangeDocument(3, 87654321, 10002, 201920, 1, team)
            };

            var exchangeDoc4 = CreateExchangeDocument(4, 11223344, 10005, 201920, 3, team);
            exchangeDoc4.PreviousVersions = new[]
            {
                CreateExchangeDocument(5, 11223344, 10005, 201920, 2, team),
                CreateExchangeDocument(6, 11223344, 10005, 201920, 1, team)
            };

            var exchangeDoc7 = CreateExchangeDocument(7, 99999999, 10002, 202021, 4, team);
            exchangeDoc7.PreviousVersions = new[]
            {
                CreateExchangeDocument(8, 99999999, 10002, 202021, 3, team),
                CreateExchangeDocument(9, 99999999, 10002, 202021, 2, team),
                CreateExchangeDocument(10, 99999999, 10002, 202021, 1, team)
            };

            return new[]
            {
                exchangeDoc1,
                exchangeDoc2,
                exchangeDoc4,
                exchangeDoc7
            };
        }

        private ExchangeDocument CreateExchangeDocument(int number, int fromUkprn, int productId, int year, int version, string team)
            => new ExchangeDocument
            {
                DocumentReference = CreateDocumentReference($"file-{number}.pdf", $"batch-id-{number}", $"parent-batch-id-{number}"),
                AgencyTeam = team,
                ExchangeDirection = ExchangeDocumentDirection.SentByOrganisation,
                EventHistory = new List<ExchangeDocumentEvent>
                     {
                         CreateExchangeDocumentEvent("user-1", ExchangeDocumentEventType.SentByOrganisation, new DateTime(2019, 1, 1)),
                         CreateExchangeDocumentEvent("user-2", ExchangeDocumentEventType.DownloadedByReceiver, new DateTime(2019, 6, 1))
                     },
                Product = CreateProduct(productId, team),
                OrganisationInfo = new OrganisationInfo
                {
                    OrganisationIdentifier = CreateOrganisationIdentifier(fromUkprn.ToString()),
                    Name = $"test-organisation-{fromUkprn}",
                },
                Organisation = new Organisation
                {
                    Name = $"test-organisation-{fromUkprn}",
                    Identifiers = new[] { new OrganisationIdentifier() { Type = OrganisationIdentifierType.Ukprn, Value = fromUkprn.ToString() } }
                },
                Year = year,
                Version = version,
                PreviousVersions = Enumerable.Empty<ExchangeDocument>()
            };

        private Product CreateProduct(int productIdentifier, string team)
            => new Product
            {
                Identifier = productIdentifier,
                AgencyTeams = new[] { team },
                Name = $"Product-{productIdentifier}",
                PluralName = $"Products-{productIdentifier}",
                CanOrganisationsUpload = false
            };

        private FileMetadataHistory CreateFileMetadataHistory(string userPrincipal, FileAction action, DateTime dateTime)
            => new FileMetadataHistory
            {
                Action = action,
                ActionDateTimeUtc = dateTime,
                User = CreateFileMetadataUser(userPrincipal)
            };

        private FileMetadataUser CreateFileMetadataUser(string userPrincipal)
            => new FileMetadataUser
            {
                Principal = userPrincipal,
                FullName = userPrincipal,
                EmailAddress = $"{userPrincipal}@education.gov.uk",
                IsEncrypted = false
            };

        private DocumentReference CreateDocumentReference(string fileName, string batchIdentifier, string parentBatchIdentifier)
            => new DocumentReference
            {
                FileName = fileName,
                BatchIdentifier = batchIdentifier,
                ParentBatchIdentifier = parentBatchIdentifier
            };

        private ExchangeDocumentEvent CreateExchangeDocumentEvent(string userPrincipal, ExchangeDocumentEventType eventType, DateTime dateTime)
            => new ExchangeDocumentEvent
            {
                EventType = eventType,
                EventDateTime = dateTime,
                UserInfo = CreateUserInfo(userPrincipal)
            };

        private UserInfo CreateUserInfo(string principal)
            => new UserInfo
            {
                Principal = principal,
                FullName = principal,
                EmailAddress = $"{principal}@education.gov.uk"
            };

        private OrganisationIdentifier CreateOrganisationIdentifier(string value)
            => new OrganisationIdentifier
            {
                Type = OrganisationIdentifierType.Ukprn,
                Value = value
            };

        private void SetupProductsLookup(string team)
        {
            _mockProductsLookup.Setup(conf => conf.Get(It.IsAny<string>()))
                 .ReturnsAsync((string productId) => CreateProduct(int.Parse(productId), team));
        }

        private void SetupOrganisationLookup()
        {
            _mockOrganisationsLookup
                  .Setup(lookup => lookup.Get(It.IsAny<OrganisationIdentifier>()))
                  .ReturnsAsync((OrganisationIdentifier organisationId) =>
                      new Organisation
                      {
                          Name = $"test-organisation-{organisationId.Value}",
                          Identifiers = new[] { organisationId }
                      });

            _mockOrganisationsLookup
                  .Setup(lookup => lookup.GetAllOrganisations())
                  .ReturnsAsync(new Dictionary<string, Organisation>()
                    {
                       {
                          "12345678",
                          new Organisation
                          {
                              Name = $"test-organisation-12345678",
                              Identifiers = new[] { new OrganisationIdentifier() { Type = OrganisationIdentifierType.Ukprn, Value = "12345678" } }
                          }
                       },
                       {
                            "87654321",
                            new Organisation
                            {
                                Name = $"test-organisation-87654321",
                                Identifiers = new[] { new OrganisationIdentifier() { Type = OrganisationIdentifierType.Ukprn, Value = "87654321" } }
                            }
                       },
                       {
                            "11223344",
                            new Organisation
                            {
                                Name = $"test-organisation-11223344",
                                Identifiers = new[] { new OrganisationIdentifier() { Type = OrganisationIdentifierType.Ukprn, Value = "11223344" } }
                            }
                       },
                       {
                            "99999999",
                            new Organisation
                            {
                                Name = $"test-organisation-99999999",
                                Identifiers = new[] { new OrganisationIdentifier() { Type = OrganisationIdentifierType.Ukprn, Value = "99999999" } }
                            }
                       }
                    });
        }

        private void SetupUserMapping()
        {
            _mockFileMetadataUserEncryptor
                .Setup(s => s.Decrypt(It.IsAny<FileMetadataUser>()))
                .Returns((FileMetadataUser user) => user);

            _mockMapper
                .Setup(m => m.Map<FileMetadataUser, UserInfo>(It.IsAny<FileMetadataUser>()))
                .Returns((FileMetadataUser user) => CreateUserInfo(user.Principal));
        }
    }
}