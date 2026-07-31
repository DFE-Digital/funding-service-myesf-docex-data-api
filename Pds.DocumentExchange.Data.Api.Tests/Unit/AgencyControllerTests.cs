using FluentAssertions;
using FluentValidation.Results;
using MapsterMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pds.Core.Logging;
using Pds.DocumentExchange.Data.Api.Controllers;
using Pds.DocumentExchange.Data.Api.Models;
using Pds.DocumentExchange.Data.Api.Validations;
using Pds.DocumentExchange.Data.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Api.Tests.Unit
{
    [TestClass]
    public class AgencyControllerTests
    {
        private readonly Mock<IVirusScanProcessor> _mockVirusScanProcessor =
            new Mock<IVirusScanProcessor>(MockBehavior.Strict);

        private readonly Mock<IVirusScanResultProcessor> _mockVirusScanResultProcessor =
            new Mock<IVirusScanResultProcessor>(MockBehavior.Strict);

        private readonly Mock<IDocumentDownloader> _mockDocumentDownloader =
            new Mock<IDocumentDownloader>(MockBehavior.Strict);

        private readonly Mock<IDocumentPublisher> _mockDocumentPublisher =
            new Mock<IDocumentPublisher>(MockBehavior.Strict);

        private readonly Mock<IDocumentManager> _mockDocumentManager = new Mock<IDocumentManager>(MockBehavior.Strict);
        private readonly Mock<IAgencyService> _mockAgencyService = new Mock<IAgencyService>(MockBehavior.Strict);

        private readonly Mock<IValidationService> _mockValidationService =
            new Mock<IValidationService>(MockBehavior.Strict);

        private readonly Mock<IMapper> _mockMapper = new Mock<IMapper>();

        private readonly Mock<ILoggerAdapter<AgencyController>> _mockLogger =
            new Mock<ILoggerAdapter<AgencyController>>();

        private readonly Mock<IExchangeDocumentSelector> _mockExchangeSelector =
            new Mock<IExchangeDocumentSelector>();

        private readonly AgencyController _agencyController;

        public AgencyControllerTests()
        {
            _agencyController = new AgencyController(
                _mockVirusScanProcessor.Object,
                _mockVirusScanResultProcessor.Object,
                _mockDocumentDownloader.Object,
                _mockDocumentManager.Object,
                _mockAgencyService.Object,
                _mockDocumentPublisher.Object,
                _mockMapper.Object,
                _mockValidationService.Object,
                _mockLogger.Object,
                _mockExchangeSelector.Object);
        }


        #region Summary

        [TestMethod]
        [TestCategory("Unit")]
        public async Task Summary_WhenTeamIsInvalid_ReturnsUnprocessableEntityObjectResult()
        {
            // Arrange
            var team = "invalid-team-name";

            _mockValidationService
                .Setup(
                    validation => validation.ValidateTeams(
                        new List<string> { team },
                        It.IsAny<Action<string, string>>()))
                .ReturnsAsync(false);

            // Act
            var result = await _agencyController.Summary(team);

            // Assert
            result.Result.Should().BeOfType<UnprocessableEntityObjectResult>();
        }

        [TestMethod]
        [TestCategory("Unit")]
        public async Task Summary_WhenTeamIsValid_ReturnsFileShareSummary()
        {
            // Arrange
            var team = "valid-team-name";
            var fileShareSummary = new Services.DTOs.FileShareSummary();
            var mappedFileShareSummary = new FileShareSummary();

            _mockValidationService
                .Setup(
                    validation => validation.ValidateTeams(
                        new List<string> { team },
                        It.IsAny<Action<string, string>>()))
                .ReturnsAsync(true);

            _mockAgencyService
                .Setup(agencyService => agencyService.Summary(new List<string> { team }))
                .ReturnsAsync(fileShareSummary);

            _mockMapper
                .Setup(mapper => mapper.Map<Services.DTOs.FileShareSummary, FileShareSummary>(fileShareSummary))
                .Returns(mappedFileShareSummary);

            // Act
            var result = await _agencyController.Summary(team);

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>();
            ((ObjectResult)result.Result).Value.Should().BeEquivalentTo(mappedFileShareSummary);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public async Task Summary_WhenMultipleTeamsValid_ReturnsFileShareSummary()
        {
            // Arrange
            var teamsParam = "valid-team-name1,valid-team-name2,";

            var teamsList = new List<string>
            {
                "valid-team-name1",
                "valid-team-name2"
            };

            var fileShareSummary = new Services.DTOs.FileShareSummary();
            var mappedFileShareSummary = new FileShareSummary();

            _mockValidationService
                .Setup(validation => validation.ValidateTeams(teamsList, It.IsAny<Action<string, string>>()))
                .ReturnsAsync(true);

            _mockAgencyService
                .Setup(agencyService => agencyService.Summary(teamsList))
                .ReturnsAsync(fileShareSummary);

            _mockMapper
                .Setup(mapper => mapper.Map<Services.DTOs.FileShareSummary, FileShareSummary>(fileShareSummary))
                .Returns(mappedFileShareSummary);

            // Act
            var result = await _agencyController.Summary(teamsParam);

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>();
            ((ObjectResult)result.Result).Value.Should().BeEquivalentTo(mappedFileShareSummary);
        }

        #endregion


        #region Documents

        [TestMethod]
        [TestCategory("Unit")]
        public async Task Documents_WhenTeamIsInvalid_ReturnsUnprocessableEntityObjectResult()
        {
            // Arrange
            var team = "invalid-team-name";
            var agencyListDocumentOption = new AgencyListDocumentOptions();

            _mockValidationService
                .Setup(
                    validation => validation.Validate(agencyListDocumentOption, It.IsAny<Action<ValidationResult>>()))
                .Returns(true);

            _mockValidationService
                .Setup(
                    validation => validation.ValidateTeams(
                        It.Is<IEnumerable<string>>(teams => teams.Contains(team)),
                        It.IsAny<Action<string, string>>()))
                .ReturnsAsync(false);

            // Act
            var result = await _agencyController.Documents(team, agencyListDocumentOption);

            // Assert
            result.Result.Should().BeOfType<UnprocessableEntityObjectResult>();
        }

        [TestMethod]
        [TestCategory("Unit")]
        public async Task Documents_WhenAgencyListDocumentOptionsIsInvalid_ReturnsUnprocessableEntityObjectResult()
        {
            // Arrange
            var team = "valid-team-name";
            var agencyListDocumentOption = new AgencyListDocumentOptions();

            _mockValidationService
                .Setup(validation => validation.ValidateTeam(team, It.IsAny<Action<string, string>>()))
                .ReturnsAsync(true);

            _mockValidationService
                .Setup(validation => validation.Validate(agencyListDocumentOption, _agencyController.AddToModelState))
                .Returns(false);

            // Act
            var result = await _agencyController.Documents(team, agencyListDocumentOption);

            // Assert
            result.Result.Should().BeOfType<UnprocessableEntityObjectResult>();
        }

        [TestMethod]
        [TestCategory("Unit")]
        public async Task Documents_WhenTeamAndAgencyListDocumentOptionsAreValid_ReturnsAgencyDocumentListResult()
        {
            // Arrange
            var team = "valid-team-name";
            var agencyListDocumentOption = new AgencyListDocumentOptions();
            var mappedAgencyListDocumentOption = new Services.DTOs.AgencyListDocumentOptions();
            var expectedResult = new ListResult<AgencyDocument>();

            _mockValidationService
                .Setup(
                    validation => validation.ValidateTeams(
                        It.Is<IEnumerable<string>>(teams => teams.Contains(team)),
                        It.IsAny<Action<string, string>>()))
                .ReturnsAsync(true);

            _mockValidationService
                .Setup(validation => validation.Validate(agencyListDocumentOption, _agencyController.AddToModelState))
                .Returns(true);

            _mockMapper
                .Setup(
                    mapper => mapper.Map<AgencyListDocumentOptions, Services.DTOs.AgencyListDocumentOptions>(
                        agencyListDocumentOption))
                .Returns(mappedAgencyListDocumentOption);

            _mockAgencyService
                .Setup(
                    s => s.GetDocuments(
                        It.Is<IEnumerable<string>>(teams => teams.Contains(team)),
                        mappedAgencyListDocumentOption))
                .ReturnsAsync(new Services.DTOs.ListResult<Services.DTOs.AgencyDocument>());

            _mockMapper
                .Setup(
                    mapper => mapper
                        .Map<Services.DTOs.ListResult<Services.DTOs.AgencyDocument>, ListResult<AgencyDocument>>(
                            It.IsAny<Services.DTOs.ListResult<Services.DTOs.AgencyDocument>>()))
                .Returns(expectedResult);

            // Act
            var result = await _agencyController.Documents(team, agencyListDocumentOption);

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>();
            ((ObjectResult)result.Result).Value.Should().Be(expectedResult);
        }

        #endregion


        #region Download

        [TestMethod]
        [TestCategory("Unit")]
        public async Task Download_WhenTeamIsInvalid_ReturnsUnprocessableEntityObjectResult()
        {
            // Arrange
            var team = "invalid-team-name";
            var fileName = "file-name.pdf";

            _mockValidationService
                .Setup(validation => validation.ValidateTeam(team, It.IsAny<Action<string, string>>()))
                .ReturnsAsync(false);

            // Act
            var result = await _agencyController.Download(team, fileName);

            // Assert
            result.Result.Should().BeOfType<UnprocessableEntityObjectResult>();
        }

        [TestMethod]
        [TestCategory("Unit")]
        [DataRow(null)]
        [DataRow("")]
        [DataRow(" ")]
        public async Task Download_WhenFilenameIsNullOrEmpty_ReturnsUnprocessableEntityObjectResult(string fileName)
        {
            // Arrange
            var team = "valid-team-name";

            _mockValidationService
                .Setup(validation => validation.ValidateTeam(team, It.IsAny<Action<string, string>>()))
                .ReturnsAsync(true);

            // Act
            var result = await _agencyController.Download(team, fileName);

            // Assert
            result.Result.Should().BeOfType<UnprocessableEntityObjectResult>();
        }

        [TestMethod]
        [TestCategory("Unit")]
        public async Task Download_WhenFileExists_ReturnsFileContents()
        {
            // Arrange
            var team = "valid-team-name";
            var fileName = "file-name.pdf";
            var fileContent = new byte[] { 1, 2, 3, 4, 5 };

            _mockValidationService
                .Setup(validation => validation.ValidateTeam(team, It.IsAny<Action<string, string>>()))
                .ReturnsAsync(true);

            _mockDocumentDownloader
                .Setup(provider => provider.DownloadAgencyDocument(team, fileName))
                .ReturnsAsync(fileContent);

            // Act
            var result = await _agencyController.Download(team, fileName);

            // Assert
            result.Value.Should().BeEquivalentTo(fileContent);

            _mockDocumentDownloader
                .Verify(
                    service => service.DownloadAgencyDocument(team, fileName),
                    Times.Once);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public async Task Download_WhenFileDoesntExist_ReturnsNotFoundResult()
        {
            // Arrange
            var team = "valid-team-name";
            var fileName = "non-existing-file-name.pdf";

            _mockValidationService
                .Setup(validation => validation.ValidateTeam(team, It.IsAny<Action<string, string>>()))
                .ReturnsAsync(true);

            _mockDocumentDownloader
                .Setup(provider => provider.DownloadAgencyDocument(team, fileName))
                .ThrowsAsync(new FileNotFoundException());

            // Act
            var response = await _agencyController.Download(team, fileName);

            // Assert
            _mockDocumentDownloader
                .Verify(
                    service => service.DownloadAgencyDocument(team, fileName),
                    Times.Once);

            response.Result.Should().BeOfType(typeof(NotFoundResult));
        }

        #endregion


        #region Remove

        [TestMethod]
        [TestCategory("Unit")]
        public async Task Remove_WhenTeamIsInvalid_ReturnsUnprocessableEntityObjectResult()
        {
            // Arrange
            var team = "invalid-team-name";
            var fileNames = new[] { "file-01.pdf", "file-02.pdf" };

            _mockValidationService
                .Setup(validation => validation.ValidateTeam(team, It.IsAny<Action<string, string>>()))
                .ReturnsAsync(false);

            // Act
            var result = await _agencyController.Remove(team, fileNames);

            // Assert
            result.Should().BeOfType<UnprocessableEntityObjectResult>();
        }

        [TestMethod]
        [TestCategory("Unit")]
        public async Task Remove_WhenFileNamesIsNull_ReturnsUnprocessableEntityObjectResult()
        {
            // Arrange
            var team = "valid-team-name";

            _mockValidationService
                .Setup(validation => validation.ValidateTeam(team, It.IsAny<Action<string, string>>()))
                .ReturnsAsync(true);

            // Act
            var result = await _agencyController.Remove(team, null);

            // Assert
            result.Should().BeOfType<UnprocessableEntityObjectResult>();
        }

        [TestMethod]
        [TestCategory("Unit")]
        public async Task Remove_WhenFileExists_Removes()
        {
            // Arrange
            var team = "valid-team-name";
            var fileNames = new[] { "file-01.pdf", "file-02.pdf" };

            _mockValidationService
                .Setup(validation => validation.ValidateTeam(team, It.IsAny<Action<string, string>>()))
                .ReturnsAsync(true);

            _mockDocumentManager
                .Setup(provider => provider.RemoveAgencyDocuments(team, fileNames))
                .Returns(Task.CompletedTask);

            // Act
            var response = await _agencyController.Remove(team, fileNames);

            // Assert
            response.Should().BeOfType(typeof(OkResult));

            _mockDocumentManager
                .Verify(
                    service => service.RemoveAgencyDocuments(team, fileNames),
                    Times.Once);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public async Task Remove_WhenFileDoesntExist_ReturnsNotFoundResult()
        {
            // Arrange
            var team = "valid-team-name";
            var fileNames = new[] { "file-01.pdf", "file-02.pdf" };

            _mockValidationService
                .Setup(validation => validation.ValidateTeam(team, It.IsAny<Action<string, string>>()))
                .ReturnsAsync(true);

            _mockDocumentManager
                .Setup(provider => provider.RemoveAgencyDocuments(team, fileNames))
                .ThrowsAsync(new FileNotFoundException());

            // Act
            var response = await _agencyController.Remove(team, fileNames);

            // Assert
            _mockDocumentManager
                .Verify(
                    service => service.RemoveAgencyDocuments(team, fileNames),
                    Times.Once);

            response.Should().BeOfType(typeof(NotFoundResult));
        }

        #endregion


        #region Publish

        [TestMethod]
        [TestCategory("Unit")]
        public async Task Publish_WhenTeamIsInvalid_ReturnsUnprocessableEntityObjectResult()
        {
            // Arrange
            var team = "invalid-team-name";

            var agencyPublishRequest = new AgencyPublishRequest
            {
                ProductId = 1,
                UserInfo = new UserInfo
                {
                    Principal = "user-principal"
                }
            };

            _mockValidationService
                .Setup(validation => validation.ValidateTeam(team, It.IsAny<Action<string, string>>()))
                .ReturnsAsync(false);

            _mockValidationService
                .Setup(validation => validation.Validate(agencyPublishRequest, _agencyController.AddToModelState))
                .Returns(true);

            // Act
            var result = await _agencyController.Publish(team, agencyPublishRequest);

            // Assert
            result.Result.Should().BeOfType<UnprocessableEntityObjectResult>();
        }

        [TestMethod]
        [TestCategory("Unit")]
        public async Task Publish_WhenAgencyPublishRequestIsInvalid_ReturnsUnprocessableEntityObjectResult()
        {
            // Arrange
            var team = "valid-team-name";

            var agencyPublishRequest = new AgencyPublishRequest
            {
                ProductId = 0,
                UserInfo = new UserInfo
                {
                    Principal = "user-principal"
                }
            };

            _mockValidationService
                .Setup(validation => validation.ValidateTeam(team, It.IsAny<Action<string, string>>()))
                .ReturnsAsync(true);

            _mockValidationService
                .Setup(validation => validation.Validate(agencyPublishRequest, _agencyController.AddToModelState))
                .Returns(false);

            // Act
            var result = await _agencyController.Publish(team, agencyPublishRequest);

            // Assert
            result.Result.Should().BeOfType<UnprocessableEntityObjectResult>();
        }

        [TestMethod]
        [TestCategory("Unit")]
        public async Task Publish_WhenAgencyPublishRequestIsValid_ReturnsProductDetails()
        {
            // Arrange
            var team = "valid-team-name";

            var agencyPublishRequest = new AgencyPublishRequest
            {
                UserInfo = new UserInfo
                {
                    Principal = "user-principal"
                },
                ProductId = 1002
            };

            var mappedAgencyPublishRequest = new Services.DTOs.AgencyPublishRequest
            {
                UserInfo = new Services.DTOs.User.UserInfo
                {
                    Principal = "user-principal"
                },
                ProductId = 1002
            };

            var productDetails = new KeyValuePair<Services.DTOs.Product, int>(
                new Services.DTOs.Product { Identifier = 1002, Name = "product 1", PluralName = "products 1" },
                2);

            var expected =
                new KeyValuePair<Product, int>(new Product { Identifier = 1002 }, 2);

            _mockValidationService
                .Setup(validation => validation.ValidateTeam(team, It.IsAny<Action<string, string>>()))
                .ReturnsAsync(true);

            _mockValidationService
                .Setup(validation => validation.Validate(agencyPublishRequest, _agencyController.AddToModelState))
                .Returns(true);

            _mockValidationService
                .Setup(
                    validation => validation.ValidateProductIdentifier(
                        agencyPublishRequest.ProductId,
                        It.IsAny<Action<string, string>>()))
                .ReturnsAsync(true);

            _mockMapper
                .Setup(mapper => mapper.Map<Services.DTOs.AgencyPublishRequest>(agencyPublishRequest))
                .Returns(mappedAgencyPublishRequest);

            _mockMapper
                .Setup(m => m.Map<Product>(productDetails.Key))
                .Returns(expected.Key);

            _mockDocumentPublisher
                .Setup(provider => provider.PublishDocuments(team, mappedAgencyPublishRequest))
                .ReturnsAsync(productDetails);

            // Act
            var result = await _agencyController.Publish(team, agencyPublishRequest);

            // Assert
            result.Result.Should()
                .BeOfType<OkObjectResult>()
                .Which.Value.Should()
                .Be(expected);

            _mockDocumentPublisher
                .Verify(
                    service => service.PublishDocuments(team, mappedAgencyPublishRequest),
                    Times.Once);
        }

        #endregion


        #region PerformVirusScan

        [TestMethod]
        [TestCategory("Unit")]
        [DataRow(null)]
        [DataRow("")]
        [DataRow(" ")]
        public async Task PerformVirusScan_WhenBatchIdentifierIsEmpty_ReturnsUnprocessableEntityObjectResult(
            string batchIdentifier)
        {
            // Act
            var result = await _agencyController.PerformVirusScan(batchIdentifier);

            // Assert
            result.Should().BeOfType<UnprocessableEntityObjectResult>();
        }

        [TestMethod]
        [TestCategory("Unit")]
        public async Task PerformVirusScan_WhenBatchIdentifierHasValue_PerformsVirusScan()
        {
            // Arrange
            var batchIdentifier = "batch-identifier";

            _mockVirusScanProcessor
                .Setup(scan => scan.RunVirusScanAgency(batchIdentifier))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _agencyController.PerformVirusScan(batchIdentifier);

            // Assert
            _mockVirusScanProcessor
                .Verify(scan => scan.RunVirusScanAgency(batchIdentifier), Times.Once);

            result.Should().BeOfType<OkResult>();
        }

        #endregion


        #region VirusScanFail

        [TestMethod]
        [TestCategory("Unit")]
        public async Task VirusScanFail_WhenDocumentReferenceIsInvalid_ReturnsUnprocessableEntityObjectResult()
        {
            // Arrange
            var documentReference = new DocumentReference
            {
                BatchIdentifier = "batch-identifier",
                FileName = string.Empty
            };

            _mockValidationService
                .Setup(validation => validation.Validate(documentReference, _agencyController.AddToModelState))
                .Returns(false);

            // Act
            var result = await _agencyController.VirusScanFail(documentReference);

            // Assert
            result.Should().BeOfType<UnprocessableEntityObjectResult>();
        }

        [TestMethod]
        [TestCategory("Unit")]
        public async Task VirusScanFail_WhenDocumentReferenceIsValid_RunsVirusScanFail()
        {
            // Arrange
            var documentReference = new DocumentReference
            {
                BatchIdentifier = "batch-identifier",
                ParentBatchIdentifier = "parent-batch-identifier",
                FileName = "file-01.pdf"
            };

            var mappedDocumentReference = new Services.DTOs.DocumentReference
            {
                BatchIdentifier = documentReference.BatchIdentifier,
                ParentBatchIdentifier = documentReference.ParentBatchIdentifier,
                FileName = documentReference.FileName
            };

            _mockValidationService
                .Setup(validation => validation.Validate(documentReference, _agencyController.AddToModelState))
                .Returns(true);

            _mockMapper
                .Setup(mapper => mapper.Map<DocumentReference, Services.DTOs.DocumentReference>(documentReference))
                .Returns(mappedDocumentReference);

            _mockVirusScanResultProcessor
                .Setup(p => p.ProcessAgencyFileResultVirusFound(mappedDocumentReference))
                .ReturnsAsync(new Services.DTOs.FileMetadata());

            // Act
            var result = await _agencyController.VirusScanFail(documentReference);

            // Assert
            _mockVirusScanResultProcessor
                .Verify(
                    scanResult => scanResult.ProcessAgencyFileResultVirusFound(mappedDocumentReference),
                    Times.Once);

            result.Should().BeOfType<OkResult>();
        }

        #endregion


        #region Document References

        [TestMethod]
        [TestCategory("Unit")]
        public async Task PreviousDocumentReferences_ReturnsExpectedResponse_WhenValidInputSpecified()
        {
            // Arrange
            var team = "valid-team-name";

            _mockValidationService
                .Setup(validation => validation.ValidateTeam(team, It.IsAny<Action<string, string>>()))
                .ReturnsAsync(true);

            var documentReference = new DocumentReference
            {
                BatchIdentifier = "batch-identifier-2",
                FileName = string.Empty
            };

            _mockValidationService
                .Setup(validation => validation.Validate(documentReference, _agencyController.AddToModelState))
                .Returns(false);

            var expectedPreviousDocumentReference = new DocumentReference
            {
                BatchIdentifier = "batch-identifier-1",
                FileName = string.Empty
            };

            List<Services.DTOs.ExchangeDocument> fakeExchangeDocuments = CreateFakeExchangeDocuments(Services.Enums.ExchangeDocumentEventType.SentByOrganisation);

            _mockExchangeSelector
                .Setup(selector => selector.GetAgencyTeamExchangeDocuments(It.IsAny<IEnumerable<string>>(), It.IsAny<IEnumerable<Services.DTOs.DocumentReference>>()))
                .ReturnsAsync(fakeExchangeDocuments);

            // Act
            var result = await _agencyController.PreviousDocumentReferences(
                team,
                new List<DocumentReference> { documentReference });

            // Assert
            var model = (result as OkObjectResult).Value as List<DocumentReference>;
            result.Should().BeOfType<OkObjectResult>();
            model.Should().BeEquivalentTo(new List<DocumentReference> { expectedPreviousDocumentReference });
        }

        [TestMethod]
        [TestCategory("Unit")]
        public async Task PreviousDocumentReferences_ReturnsExpectedResponse_WhenPreviousVersionAlreadyDownloaded()
        {
            // Arrange
            var team = "valid-team-name";

            _mockValidationService
                .Setup(validation => validation.ValidateTeam(team, It.IsAny<Action<string, string>>()))
                .ReturnsAsync(true);

            var documentReference = new DocumentReference
            {
                BatchIdentifier = "batch-identifier-2",
                FileName = string.Empty
            };

            _mockValidationService
                .Setup(validation => validation.Validate(documentReference, _agencyController.AddToModelState))
                .Returns(false);

            List<Services.DTOs.ExchangeDocument> fakeExchangeDocuments = CreateFakeExchangeDocuments(Services.Enums.ExchangeDocumentEventType.DownloadedByReceiver);

            _mockExchangeSelector
                .Setup(selector => selector.GetAgencyTeamExchangeDocuments(It.IsAny<IEnumerable<string>>(), It.IsAny<IEnumerable<Services.DTOs.DocumentReference>>()))
                .ReturnsAsync(fakeExchangeDocuments);

            // Act
            var result = await _agencyController.PreviousDocumentReferences(
                team,
                new List<DocumentReference> { documentReference });

            // Assert
            var model = (result as OkObjectResult).Value as List<DocumentReference>;
            result.Should().BeOfType<OkObjectResult>();
            model.Should().HaveCount(0);
        }

        private static List<Services.DTOs.ExchangeDocument> CreateFakeExchangeDocuments(Services.Enums.ExchangeDocumentEventType exchangeDocumentEvent)
        {
            return new List<Services.DTOs.ExchangeDocument>
            {
                new Services.DTOs.ExchangeDocument
                {
                    DocumentReference = new Services.DTOs.DocumentReference
                    {
                        BatchIdentifier = "batch-identifier-2",
                        FileName = string.Empty,
                    },
                    PreviousVersions = new List<Services.DTOs.ExchangeDocument>
                    {
                        new Services.DTOs.ExchangeDocument
                        {
                            DocumentReference = new Services.DTOs.DocumentReference
                            {
                                BatchIdentifier = "batch-identifier-1",
                                FileName = string.Empty
                            },
                            Version = 1,
                            EventHistory = new List<Services.DTOs.ExchangeDocumentEvent>
                            {
                                new Services.DTOs.ExchangeDocumentEvent { EventDateTime = DateTime.Now, EventType = exchangeDocumentEvent, UserInfo = null }
                            }
                        }
                    },
                    Version = 2
                }
            };
        }

        #endregion
    }
}