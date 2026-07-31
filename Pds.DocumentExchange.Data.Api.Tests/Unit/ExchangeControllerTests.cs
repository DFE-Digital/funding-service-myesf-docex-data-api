using FluentAssertions;
using MapsterMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pds.Core.Common.Organisation.Models;
using Pds.Core.Logging;
using Pds.DocumentExchange.Data.Api.Controllers;
using Pds.DocumentExchange.Data.Api.Validations;
using Pds.DocumentExchange.Data.Services.DTOs;
using Pds.DocumentExchange.Data.Services.DTOs.Filters;
using Pds.DocumentExchange.Data.Services.DTOs.User;
using Pds.DocumentExchange.Data.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrganisationIdentifierType = Pds.DocumentExchange.Data.Api.Enums.OrganisationIdentifierType;
using UserInfo = Pds.DocumentExchange.Data.Api.Models.UserInfo;

namespace Pds.DocumentExchange.Data.Api.Tests.Unit
{
    [TestClass]
    [TestCategory("Unit")]
    public class ExchangeControllerTests
    {
        private readonly Mock<IExchangeDocumentSelector> _mockExchangeDocumentSelector = new Mock<IExchangeDocumentSelector>(MockBehavior.Strict);
        private readonly Mock<IVirusScanResultProcessor> _mockVirusScanResultProcessor = new Mock<IVirusScanResultProcessor>(MockBehavior.Strict);
        private readonly Mock<IDocumentDownloader> _mockDocumentDownloader = new Mock<IDocumentDownloader>(MockBehavior.Strict);
        private readonly Mock<IDocumentExchangeSummaries> _mockDocumentExchangeSummaries = new Mock<IDocumentExchangeSummaries>(MockBehavior.Strict);
        private readonly Mock<IProductVersionService> _mockProductVersionService = new Mock<IProductVersionService>(MockBehavior.Strict);
        private readonly Mock<IValidationService> _validationService = new Mock<IValidationService>(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new Mock<IMapper>(MockBehavior.Strict);
        private readonly Mock<IDocumentDeletion> _documentDeletion = new Mock<IDocumentDeletion>(MockBehavior.Strict);
        private readonly Mock<ILoggerAdapter<ExchangeController>> _mockLogger = new Mock<ILoggerAdapter<ExchangeController>>();

        private readonly ExchangeController _exchangeController;

        public ExchangeControllerTests()
        {
            _exchangeController = new ExchangeController(
                _mockExchangeDocumentSelector.Object,
                _mockVirusScanResultProcessor.Object,
                _mockDocumentDownloader.Object,
                _mockDocumentExchangeSummaries.Object,
                _mockProductVersionService.Object,
                _mockMapper.Object,
                _validationService.Object,
                _documentDeletion.Object,
                _mockLogger.Object);
        }

        #region Summary

        [TestMethod]
        public async Task Summary_WhenUserInfoIsInvalid_ReturnsUnprocessableEntityObjectResult()
        {
            // Arrange
            var invalidUserInfo = new UserInfo
            {
                Principal = string.Empty
            };

            _validationService
                .Setup(validation => validation.Validate(invalidUserInfo, _exchangeController.AddToModelState))
                .Returns(false);

            // Act
            var result = await _exchangeController.Summary(invalidUserInfo);

            // Assert
            result.Result.Should().BeOfType<UnprocessableEntityObjectResult>();
        }

        [TestMethod]
        public async Task Summary_WhenUserInfoIsValid_ReturnsSummary()
        {
            // Arrange
            var userInfo = new UserInfo
            {
                Principal = "principal",
                FullName = "full-name",
                EmailAddress = "email-address@education@gov.uk",
                OrganisationInfo = new Models.OrganisationInfo
                {
                    Name = "organisation-name",
                    OrganisationIdentifier = new Models.OrganisationIdentifier
                    {
                        Type = OrganisationIdentifierType.Ukprn,
                        Value = "12345678"
                    }
                }
            };

            var mappedUserInfo = new Services.DTOs.User.UserInfo
            {
                Principal = userInfo.Principal,
                FullName = userInfo.FullName,
                EmailAddress = userInfo.EmailAddress,
                OrganisationInfo = new OrganisationInfo
                {
                    Name = userInfo.OrganisationInfo.Name,
                    OrganisationIdentifier = new OrganisationIdentifier
                    {
                        Type = Pds.Core.Common.Organisation.Enums.OrganisationIdentifierType.Ukprn,
                        Value = userInfo.OrganisationInfo.OrganisationIdentifier.Value
                    }
                }
            };

            var summary = new Summary
            {
                CountOfNewDocuments = 15,
                DocumentExchangeEnabled = true
            };

            var mappedSummary = new Models.Summary
            {
                CountOfNewDocuments = summary.CountOfNewDocuments,
                DocumentExchangeEnabled = summary.DocumentExchangeEnabled
            };

            _validationService
                .Setup(validation => validation.Validate(userInfo, _exchangeController.AddToModelState))
                .Returns(true);

            _mockMapper
               .Setup(mapper => mapper.Map<UserInfo, Services.DTOs.User.UserInfo>(userInfo))
               .Returns(mappedUserInfo);

            _mockMapper
                .Setup(mapper => mapper.Map<Summary, Models.Summary>(summary))
                .Returns(mappedSummary);

            _mockDocumentExchangeSummaries
                .Setup(summaries => summaries.GetOrganisationUserSummary(mappedUserInfo))
                .ReturnsAsync(summary);

            // Act
            var result = await _exchangeController.Summary(userInfo);

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>();
            ((ObjectResult)result.Result).Value.Should().BeEquivalentTo(mappedSummary);
        }

        #endregion


        #region Team Summary

        [TestMethod]
        public async Task TeamSummary_WhenTeamIsInvalid_ReturnsUnprocessableEntityObjectResult()
        {
            // Arrange
            var team = "invalid-team";

            _validationService
                .Setup(validation => validation.ValidateTeams(new[] { team }, It.IsAny<Action<string, string>>()))
                .ReturnsAsync(false);

            // Act
            var result = await _exchangeController.TeamSummary(team);

            // Assert
            result.Result.Should().BeOfType<UnprocessableEntityObjectResult>();
        }

        [TestMethod]
        public async Task TeamSummary_WhenTeamIsValid_ReturnsSummary()
        {
            // Arrange
            var team = "test-team";

            var summary = new Summary
            {
                CountOfNewDocuments = 15,
                DocumentExchangeEnabled = true
            };

            var mappedSummary = new Models.Summary
            {
                CountOfNewDocuments = summary.CountOfNewDocuments,
                DocumentExchangeEnabled = summary.DocumentExchangeEnabled
            };

            _validationService
                .Setup(validation => validation.ValidateTeams(new[] { team }, It.IsAny<Action<string, string>>()))
                .ReturnsAsync(true);

            _mockMapper
                .Setup(mapper => mapper.Map<Summary, Models.Summary>(summary))
                .Returns(mappedSummary);

            _mockDocumentExchangeSummaries
                .Setup(summaries => summaries.GetTeamsSummary(new[] { team }))
                .ReturnsAsync(summary);

            // Act
            var result = await _exchangeController.TeamSummary(team);

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>();
            ((ObjectResult)result.Result).Value.Should().BeEquivalentTo(mappedSummary);
        }

        [TestMethod]
        public async Task TeamSummary_WhenMultipleValidTeams_ReturnsSummary()
        {
            // Arrange
            var team1 = "test-team1";
            var team2 = "test-team2";
            var teams = $"{team1},{team2}";

            var summary = new Summary
            {
                CountOfNewDocuments = 15,
                DocumentExchangeEnabled = true
            };

            var mappedSummary = new Models.Summary
            {
                CountOfNewDocuments = summary.CountOfNewDocuments,
                DocumentExchangeEnabled = summary.DocumentExchangeEnabled
            };

            _validationService
                .Setup(validation => validation.ValidateTeams(new[] { team1, team2 }, It.IsAny<Action<string, string>>()))
                .ReturnsAsync(true);

            _mockMapper
                .Setup(mapper => mapper.Map<Summary, Models.Summary>(summary))
                .Returns(mappedSummary);

            _mockDocumentExchangeSummaries
                .Setup(summaries => summaries.GetTeamsSummary(new[] { team1, team2 }))
                .ReturnsAsync(summary);

            // Act
            var result = await _exchangeController.TeamSummary(teams);

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>();
            ((ObjectResult)result.Result).Value.Should().BeEquivalentTo(mappedSummary);
        }

        #endregion


        #region CurrentProductVersionForOrganisation

        [TestMethod]
        public async Task CurrentProductVersionForOrganisation_WhenOrganisationIdentifierIsInvalid_ReturnsUnprocessableEntityObjectResult()
        {
            // Arrange
            var organisationIdentifier = new Models.OrganisationIdentifier
            {
                Type = OrganisationIdentifierType.Ukprn,
                Value = string.Empty
            };

            var productIdentifier = 1000;

            _validationService
                .Setup(validation => validation.Validate(organisationIdentifier, _exchangeController.AddToModelState))
                .Returns(false);

            // Act
            var result = await _exchangeController.CurrentProductVersionForOrganisation(organisationIdentifier, productIdentifier);

            // Assert
            result.Result.Should().BeOfType<UnprocessableEntityObjectResult>();
        }

        [TestMethod]
        public async Task CurrentProductVersionForOrganisation_WhenProductIdentifierIsInvalid_ReturnsUnprocessableEntityObjectResult()
        {
            // Arrange
            var organisationIdentifier = new Models.OrganisationIdentifier
            {
                Type = OrganisationIdentifierType.Ukprn,
                Value = string.Empty
            };

            var productIdentifier = 1000;

            _validationService
                .Setup(validation => validation.Validate(organisationIdentifier, _exchangeController.AddToModelState))
                .Returns(true);

            _validationService
                .Setup(
                    validation => validation.ValidateProductIdentifier(
                        productIdentifier,
                        _exchangeController.ModelState.AddModelError))
                .ReturnsAsync(
                    (int productId, Action<string, string> action) =>
                    {
                        action("test-key", "test-error");
                        return false;
                    });

            // Act
            var result = await _exchangeController.CurrentProductVersionForOrganisation(organisationIdentifier, productIdentifier);

            // Assert
            result.Result.Should().BeOfType<UnprocessableEntityObjectResult>();

            ((ObjectResult)result.Result).Value.Should()
                .BeOfType<SerializableError>()
                .Which.Should()
                .BeEquivalentTo(
                    new Dictionary<string, object>
                    {
                        { "test-key", new[] { "test-error" } }
                    });
        }

        [TestMethod]
        public async Task CurrentProductVersionForOrganisation_WhenOrganisationAndProductIdentifierAreValid_ReturnsProductVersionNumber()
        {
            // Arrange
            var organisationIdentifier = new Models.OrganisationIdentifier
            {
                Type = OrganisationIdentifierType.Ukprn,
                Value = "12345678"
            };

            var mappedOrganisationIdentifier = new OrganisationIdentifier
            {
                Type = Pds.Core.Common.Organisation.Enums.OrganisationIdentifierType.Ukprn,
                Value = organisationIdentifier.Value
            };

            var productIdentifier = 1000;

            var expectedProductVersion = 15;

            _validationService
                .Setup(validation => validation.Validate(organisationIdentifier, _exchangeController.AddToModelState))
                .Returns(true);

            _validationService
               .Setup(validation => validation.ValidateProductIdentifier(productIdentifier, It.IsAny<Action<string, string>>()))
               .ReturnsAsync(true);

            _mockMapper
                .Setup(mapper => mapper.Map<Models.OrganisationIdentifier, OrganisationIdentifier>(organisationIdentifier))
                .Returns(mappedOrganisationIdentifier);

            _mockProductVersionService
                .Setup(productVersionService => productVersionService.GetCurrentProductVersion(mappedOrganisationIdentifier, productIdentifier))
                .ReturnsAsync(expectedProductVersion);

            // Act
            var result = await _exchangeController.CurrentProductVersionForOrganisation(organisationIdentifier, productIdentifier);

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>();
        }

        #endregion


        #region Documents (Teams)

        [TestMethod]
        public async Task DocumentsTeam_WhenTeamIsInvalid_ReturnsUnprocessableEntity()
        {
            // Arrange
            var team = "invalid-team";
            var options = new Models.ExchangeListDocumentOptions();

            _validationService
                .Setup(validator => validator.ValidateTeams(
                    It.Is<IEnumerable<string>>(teams => teams.Contains(team)),
                    It.IsAny<Action<string, string>>()))
                .ReturnsAsync(false);

            _validationService
                .Setup(validator => validator.Validate(options, _exchangeController.AddToModelState))
                .Returns(true);

            // Act
            var result = await _exchangeController.Documents(team, options);

            // Assert
            result.Result.Should().BeOfType<UnprocessableEntityObjectResult>();
        }

        [TestMethod]
        public async Task DocumentsTeam_WhenExchangeListDocumentOptionsIsInvalid_ReturnsUnprocessableEntity()
        {
            // Arrange
            var options = new Models.ExchangeListDocumentOptions();

            _validationService
                .Setup(validator => validator.Validate(options, _exchangeController.AddToModelState))
                .Returns(false);

            // Act
            var result = await _exchangeController.Documents("testTeam", options);

            // Assert
            result.Result.Should().BeOfType<UnprocessableEntityObjectResult>();
        }

        [TestMethod]
        public async Task DocumentsTeam_WhenTeamAndExchangeListDocumentOptionsAreValid_ReturnsExchangeDocumentsListResult()
        {
            // Arrange
            var inputOptions = new Models.ExchangeListDocumentOptions
            {
                DocumentStatusOption = Enums.ExchangeDocumentDirection.SentByOrganisation,
                DocumentReferences = Enumerable.Empty<Models.DocumentReference>(),
                FilterOptions = Enumerable.Empty<Models.Filters.IFilterOption>(),
                PageSize = 25
            };

            var mappedOptions = new ExchangeListDocumentOptions
            {
                DocumentStatusOption = Services.Enums.ExchangeDocumentDirection.SentByOrganisation,
                DocumentReferences = Enumerable.Empty<DocumentReference>(),
                FilterOptions = Enumerable.Empty<IFilterOption>(),
                PageSize = 25
            };

            var team = "testTeam";

            var expectedResult = new Models.ListResult<Models.ExchangeDocument>();

            _validationService
               .Setup(validation => validation.ValidateTeams(
                   It.Is<IEnumerable<string>>(teams => teams.Contains(team)),
                   It.IsAny<Action<string, string>>()))
               .ReturnsAsync(true);

            _validationService
                .Setup(validation => validation.Validate(inputOptions, _exchangeController.AddToModelState))
                .Returns(true);

            _mockMapper
                .Setup(m => m.Map<Models.ExchangeListDocumentOptions, ExchangeListDocumentOptions>(It.IsAny<Models.ExchangeListDocumentOptions>()))
                .Returns(mappedOptions);

            _mockExchangeDocumentSelector
                .Setup(p => p.GetAgencyTeamFiles(
                    It.Is<IEnumerable<string>>(teams => teams.Contains(team)),
                    mappedOptions))
                .ReturnsAsync(new ListResult<ExchangeDocument>());

            _mockMapper
                .Setup(m => m.Map<ListResult<ExchangeDocument>, Models.ListResult<Models.ExchangeDocument>>(It.IsAny<ListResult<ExchangeDocument>>()))
                .Returns(expectedResult);

            // Act
            var result = await _exchangeController.Documents(team, inputOptions);

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>();
            ((ObjectResult)result.Result).Value.Should().Be(expectedResult);
        }

        #endregion


        #region Documents

        [TestMethod]
        public async Task Documents_WhenExchangeListDocumentOptionsIsInvalid_ReturnsUnprocessableEntity()
        {
            // Arrange
            var options = new Models.ExchangeListOrganisationDocumentOptions();

            _validationService
                .Setup(validator => validator.Validate(options, _exchangeController.AddToModelState))
                .Returns(false);

            // Act
            var result = await _exchangeController.Documents(options);

            // Assert
            result.Result.Should().BeOfType<UnprocessableEntityObjectResult>();
        }

        [TestMethod]
        public async Task Documents_WhenExchangeListDocumentOptionsIsValid_ReturnsExchangeDocumentsListResult()
        {
            // Arrange
            var inputOptions = new Models.ExchangeListOrganisationDocumentOptions
            {
                DocumentStatusOption = Enums.ExchangeDocumentDirection.SentByOrganisation,
                DocumentReferences = Enumerable.Empty<Models.DocumentReference>(),
                FilterOptions = Enumerable.Empty<Models.Filters.IFilterOption>(),
                PageSize = 25,
                OrganisationIdentifier = new Models.OrganisationIdentifier
                {
                    Type = OrganisationIdentifierType.CompanyRegistrationNumber,
                    Value = "11223344"
                }
            };

            var mappedOptions = new ExchangeListOrganisationDocumentOptions
            {
                DocumentStatusOption = Services.Enums.ExchangeDocumentDirection.SentByOrganisation,
                DocumentReferences = Enumerable.Empty<DocumentReference>(),
                FilterOptions = Enumerable.Empty<IFilterOption>(),
                PageSize = 25,
                OrganisationIdentifier = new OrganisationIdentifier
                {
                    Type = Pds.Core.Common.Organisation.Enums.OrganisationIdentifierType.CompanyRegistrationNumber,
                    Value = "11223344"
                }
            };

            var expectedResult = new Models.ListResult<Models.ExchangeDocument>();

            _validationService
                .Setup(validation => validation.Validate(inputOptions, _exchangeController.AddToModelState))
                .Returns(true);

            _mockMapper
                .Setup(m => m.Map<Models.ExchangeListDocumentOptions, ExchangeListDocumentOptions>(It.IsAny<Models.ExchangeListDocumentOptions>()))
                .Returns(mappedOptions);

            _mockExchangeDocumentSelector
                .Setup(p => p.GetOrganisationFiles(mappedOptions))
                .ReturnsAsync(new ListResult<ExchangeDocument>());

            _mockMapper
                .Setup(m => m.Map<ListResult<ExchangeDocument>, Models.ListResult<Models.ExchangeDocument>>(It.IsAny<ListResult<ExchangeDocument>>()))
                .Returns(expectedResult);

            // Act
            var result = await _exchangeController.Documents(inputOptions);

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>();
            ((ObjectResult)result.Result).Value.Should().Be(expectedResult);
        }

        #endregion


        #region Download (Teams)

        [TestMethod]
        public async Task Download_Teams_ForInvalidDownloadRequest_ReturnsUnprocessableEntityObjectResult()
        {
            // Arrange
            var request = new Models.ExchangeDocumentDownloadRequest();

            _validationService
                .Setup(validator => validator.Validate(request, _exchangeController.AddToModelState))
                .Returns(false);

            // Act
            var result = await _exchangeController.Download("team1,team2", request);

            // Assert
            result.Result.Should().BeOfType<UnprocessableEntityObjectResult>();
        }

        [TestMethod]
        public async Task Download_Teams_ForInvalidTeams_ReturnsUnprocessableEntityObjectResult()
        {
            // Arrange
            var teams = "team1,team2";
            var teamsList = teams.Split(',');
            var request = new Models.ExchangeDocumentDownloadRequest();

            _validationService
                .Setup(validator => validator.Validate(request, _exchangeController.AddToModelState))
                .Returns(true);

            _validationService
                .Setup(validator => validator.ValidateTeams(teamsList, _exchangeController.ModelState.AddModelError))
                .ReturnsAsync(false);

            // Act
            var result = await _exchangeController.Download(teams, request);

            // Assert
            result.Result.Should().BeOfType<UnprocessableEntityObjectResult>();
        }

        [TestMethod]
        public async Task Download_Teams_ForValidParams_ReturnsOkResultContainingFileContent()
        {
            // Arrange
            var teams = "team1,team2";
            var teamsList = teams.Split(',');
            var request = new Models.ExchangeDocumentDownloadRequest();

            _validationService
                .Setup(validator => validator.Validate(request, _exchangeController.AddToModelState))
                .Returns(true);

            _validationService
                .Setup(validator => validator.ValidateTeams(teamsList, _exchangeController.ModelState.AddModelError))
                .ReturnsAsync(true);

            var listOptions = new ExchangeListDocumentOptions();

            _mockMapper
                .Setup(m => m.Map<ExchangeDocumentDownloadRequest>(request))
                .Returns(new ExchangeDocumentDownloadRequest
                {
                    ListOptions = listOptions
                });

            _mockExchangeDocumentSelector
                .Setup(
                    eds => eds.GetAgencyTeamFiles(
                        It.Is<IEnumerable<string>>(
                            list => list.Count() == teamsList.Length && !list.Except(teamsList).Any()),
                        listOptions))
                .ReturnsAsync(
                    new ListResult<ExchangeDocument>
                    {
                        Items = Enumerable.Range(1, 10)
                            .Select(
                                id => new ExchangeDocument
                                { DocumentReference = new DocumentReference { FileName = $"file{id}.doc" } })
                    });

            ExchangeDocumentDownloadRequest actualRequest = null;
            var expectedResult = new byte[] { 1, 2, 3, 4, 5 };

            _mockDocumentDownloader
                .Setup(
                    dd => dd.DownloadExchangeDocument(
                        It.IsAny<ExchangeDocumentDownloadRequest>(),
                        true))
                .ReturnsAsync(
                    (ExchangeDocumentDownloadRequest capturedRequest, bool _) =>
                    {
                        actualRequest = capturedRequest;
                        return expectedResult;
                    });

            // Act
            var result = await _exchangeController.Download(teams, request);

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>().Which.Value.Should().BeEquivalentTo(expectedResult);

            actualRequest.ListOptions.DocumentReferences.Should()
                .BeEquivalentTo(
                    Enumerable.Range(1, 10).Select(id => new DocumentReference { FileName = $"file{id}.doc" }));
        }

        [TestMethod]
        public async Task Download_FoValidExchangeDocumentDownloadRequest_ReturnsDocumentBytes()
        {
            // Arrange
            var request = new Models.ExchangeDocumentDownloadRequest();

            _validationService
                .Setup(validator => validator.Validate(request, _exchangeController.AddToModelState))
                .Returns(true);

            var listOptions = new ExchangeListDocumentOptions();

            _mockMapper
                .Setup(m => m.Map<ExchangeDocumentDownloadRequest>(request))
                .Returns(new ExchangeDocumentDownloadRequest
                {
                    ListOptions = listOptions
                });

            ExchangeDocumentDownloadRequest actualRequest = null;
            var expectedResult = new byte[] { 1, 2, 3, 4, 5 };

            _mockDocumentDownloader
                .Setup(
                    dd => dd.DownloadExchangeDocument(It.IsAny<ExchangeDocumentDownloadRequest>(), false))
                    .ReturnsAsync(
                        (ExchangeDocumentDownloadRequest capturedRequest, bool _) =>
                        {
                            actualRequest = capturedRequest;
                            return expectedResult;
                        });

            // Act
            var result = await _exchangeController.Download(request);

            // Assert
            result.Value.Should().BeEquivalentTo(expectedResult);
        }

        #endregion


        #region OrganisationFilesVirusScanSuccessful

        [TestMethod]
        public async Task OrganisationFilesVirusScanSuccessful_WhenDocumentReferenceIsInvalid_ReturnsUnprocessableEntity()
        {
            // Arrange
            var documentReference = new Models.DocumentReference();

            _validationService
                .Setup(validationService => validationService.Validate(documentReference, _exchangeController.AddToModelState))
                .Returns(false);

            // Act
            var result = await _exchangeController.OrganisationFilesVirusScanSuccessful(documentReference);

            // Assert
            result.Should().BeOfType<UnprocessableEntityObjectResult>();
        }

        [TestMethod]
        public async Task OrganisationFilesVirusScanSuccessful_WhenDocumentReferenceIsValid_ProcessesFileAndReturnsOk()
        {
            // Arrange
            var documentReference = new Models.DocumentReference
            {
                FileName = "file-name.pdf",
                BatchIdentifier = "batch-id",
                ParentBatchIdentifier = "parent-batch-id"
            };

            var mappedDocumentReference = new DocumentReference
            {
                FileName = "file-name.pdf",
                BatchIdentifier = "batch-id",
                ParentBatchIdentifier = "parent-batch-id"
            };

            var fileMetadataResult = new FileMetadata
            {
                FileName = documentReference.FileName
            };

            _validationService
                .Setup(validationService => validationService.Validate(documentReference, _exchangeController.AddToModelState))
                .Returns(true);

            _mockMapper
                .Setup(mapper => mapper.Map<Models.DocumentReference, DocumentReference>(documentReference))
                .Returns(mappedDocumentReference);

            _mockVirusScanResultProcessor
                .Setup(virusResultProcessor => virusResultProcessor.ProcessOrganisationFileResultOk(mappedDocumentReference))
                .ReturnsAsync(fileMetadataResult);

            // Act
            var result = await _exchangeController.OrganisationFilesVirusScanSuccessful(documentReference);

            // Assert
            result.Should().BeOfType<OkResult>();

            _mockVirusScanResultProcessor
               .Verify(virusResultProcessor => virusResultProcessor.ProcessOrganisationFileResultOk(mappedDocumentReference), Times.Once);
        }

        #endregion


        #region AgencyFilesVirusScanSuccessful

        [TestMethod]
        public async Task AgencyFilesVirusScanSuccessful_WhenDocumentReferenceIsInvalid_ReturnsUnprocessableEntity()
        {
            // Arrange
            var documentReference = new Models.DocumentReference();

            _validationService
                .Setup(validationService => validationService.Validate(documentReference, _exchangeController.AddToModelState))
                .Returns(false);

            // Act
            var result = await _exchangeController.AgencyFilesVirusScanSuccessful(documentReference);

            // Assert
            result.Should().BeOfType<UnprocessableEntityObjectResult>();
        }

        [TestMethod]
        public async Task AgencyFilesVirusScanSuccessful_WhenDocumentReferenceIsValid_ProcessesFileAndReturnsOk()
        {
            // Arrange
            var documentReference = new Models.DocumentReference
            {
                FileName = "file-name.pdf",
                BatchIdentifier = "batch-id",
                ParentBatchIdentifier = "parent-batch-id"
            };

            var mappedDocumentReference = new DocumentReference
            {
                FileName = "file-name.pdf",
                BatchIdentifier = "batch-id",
                ParentBatchIdentifier = "parent-batch-id"
            };

            var fileMetadataResult = new FileMetadata
            {
                FileName = documentReference.FileName
            };

            _validationService
                .Setup(validationService => validationService.Validate(documentReference, _exchangeController.AddToModelState))
                .Returns(true);

            _mockMapper
                .Setup(mapper => mapper.Map<Models.DocumentReference, DocumentReference>(documentReference))
                .Returns(mappedDocumentReference);

            _mockVirusScanResultProcessor
                .Setup(virusResultProcessor => virusResultProcessor.ProcessAgencyFileResultOk(mappedDocumentReference))
                .ReturnsAsync(fileMetadataResult);

            // Act
            var result = await _exchangeController.AgencyFilesVirusScanSuccessful(documentReference);

            // Assert
            result.Should().BeOfType<OkResult>();

            _mockVirusScanResultProcessor
               .Verify(virusResultProcessor => virusResultProcessor.ProcessAgencyFileResultOk(mappedDocumentReference), Times.Once);
        }

        #endregion


        #region Delete

        [TestMethod]
        public async Task Delete_WhenExchangeDocumentDeleteRequestIsInvalid_ReturnsUnprocessableEntity()
        {
            // Arrange
            var deleteRequest = new Models.ExchangeDocumentDeleteRequest();

            _validationService
                .Setup(validator => validator.Validate(deleteRequest, _exchangeController.AddToModelState))
                .Returns(false);

            // Act
            var result = await _exchangeController.Delete(deleteRequest);

            // Assert
            result.Result.Should().BeOfType<UnprocessableEntityObjectResult>();

            Mock.VerifyAll(_validationService);
        }

        [TestMethod]
        public async Task Delete_WhenExchangeDocumentDeleteRequestIsValid_DeletesDocumentsAndReturnsOk()
        {
            // Arrange
            var documentReference = new Models.DocumentReferenceWithPreviousVersions
            {
                FileName = "file-name.pdf",
                BatchIdentifier = "batch-id",
                ParentBatchIdentifier = "parent-batch-id"
            };

            var deleteRequest = new Models.ExchangeDocumentDeleteRequest
            {
                UserInfo = new UserInfo
                {
                    Principal = "principal",
                    FullName = "full-name",
                    EmailAddress = "email-address@education.gov.uk"
                },
                DocumentReferences = new[] { documentReference }
            };

            var mappedDeleteRequest = new ExchangeDocumentDeleteRequest
            {
                UserInfo = new Services.DTOs.User.UserInfo
                {
                    Principal = deleteRequest.UserInfo.Principal,
                    FullName = deleteRequest.UserInfo.FullName,
                    EmailAddress = deleteRequest.UserInfo.EmailAddress
                },
                DocumentReferences = deleteRequest.DocumentReferences.Select(doc =>
                new DocumentReferenceWithPreviousVersions
                {
                    FileName = doc.FileName,
                    BatchIdentifier = doc.BatchIdentifier,
                    ParentBatchIdentifier = doc.ParentBatchIdentifier
                })
            };

            var serviceDocuments = new[]
            {
                new ExchangeDocument
                {
                    DocumentReference = mappedDeleteRequest.DocumentReferences.First()
                }
            };

            var expectedResult = new[]
            {
                new Models.ExchangeDocument
                {
                    DocumentReference = deleteRequest.DocumentReferences.First()
                }
            };

            _validationService
                .Setup(validator => validator.Validate(deleteRequest, _exchangeController.AddToModelState))
                .Returns(true);

            _mockMapper
                .Setup(mapper => mapper.Map<ExchangeDocumentDeleteRequest>(deleteRequest))
                .Returns(mappedDeleteRequest);

            _documentDeletion
                .Setup(documentDeletion => documentDeletion.DeleteDocuments(mappedDeleteRequest))
                .ReturnsAsync(serviceDocuments);

            _mockMapper
                .Setup(mapper => mapper.Map<IEnumerable<Models.ExchangeDocument>>(serviceDocuments))
                .Returns(expectedResult);

            // Act
            var result = await _exchangeController.Delete(deleteRequest);

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>();
            ((OkObjectResult)result.Result).Value.Should().BeEquivalentTo(expectedResult);

            Mock.VerifyAll(_validationService, _mockMapper, _documentDeletion);
        }

        [TestMethod]
        public async Task DeleteSingle_WhenExchangeDocumentDeleteRequestIsInvalid_ReturnsUnprocessableEntity()
        {
            // Arrange
            var deleteRequest = new Models.ExchangeDocumentDeleteRequest();

            _validationService
                .Setup(validator => validator.Validate(deleteRequest, _exchangeController.AddToModelState))
                .Returns(false);

            // Act
            var result = await _exchangeController.DeleteSingle(deleteRequest);

            // Assert
            result.Result.Should().BeOfType<UnprocessableEntityObjectResult>();

            Mock.VerifyAll(_validationService);
        }

        [TestMethod]
        public async Task DeleteSingle_WhenExchangeDocumentDeleteRequestIsValid_DeletesDocumentsAndReturnsOk()
        {
            // Arrange
            var documentReference = new Models.DocumentReferenceWithPreviousVersions
            {
                FileName = "file-name.pdf",
                BatchIdentifier = "batch-id",
                ParentBatchIdentifier = "parent-batch-id"
            };

            var deleteRequest = new Models.ExchangeDocumentDeleteRequest
            {
                UserInfo = new UserInfo
                {
                    Principal = "principal",
                    FullName = "full-name",
                    EmailAddress = "email-address@education.gov.uk"
                },
                DocumentReferences = new[] { documentReference }
            };

            var mappedDeleteRequest = new ExchangeDocumentDeleteRequest
            {
                UserInfo = new Services.DTOs.User.UserInfo
                {
                    Principal = deleteRequest.UserInfo.Principal,
                    FullName = deleteRequest.UserInfo.FullName,
                    EmailAddress = deleteRequest.UserInfo.EmailAddress
                },
                DocumentReferences = deleteRequest.DocumentReferences.Select(doc =>
                new DocumentReferenceWithPreviousVersions
                {
                    FileName = doc.FileName,
                    BatchIdentifier = doc.BatchIdentifier,
                    ParentBatchIdentifier = doc.ParentBatchIdentifier
                })
            };

            var serviceDocument = new ExchangeDocument
            {
                DocumentReference = mappedDeleteRequest.DocumentReferences.First()
            };

            var expectedResult = new Models.ExchangeDocument
            {
                DocumentReference = deleteRequest.DocumentReferences.First()
            };

            _validationService
                .Setup(validator => validator.Validate(deleteRequest, _exchangeController.AddToModelState))
                .Returns(true);

            _mockMapper
                .Setup(mapper => mapper.Map<ExchangeDocumentDeleteRequest>(deleteRequest))
                .Returns(mappedDeleteRequest);

            _documentDeletion
                .Setup(documentDeletion => documentDeletion.DeleteDocument(mappedDeleteRequest))
                .ReturnsAsync(serviceDocument);

            _mockMapper
                .Setup(mapper => mapper.Map<Models.ExchangeDocument>(serviceDocument))
                .Returns(expectedResult);

            // Act
            var result = await _exchangeController.DeleteSingle(deleteRequest);

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>();
            ((OkObjectResult)result.Result).Value.Should().BeEquivalentTo(expectedResult);

            Mock.VerifyAll(_validationService, _mockMapper, _documentDeletion);
        }

        #endregion
    }
}