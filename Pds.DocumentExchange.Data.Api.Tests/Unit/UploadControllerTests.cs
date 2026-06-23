using AutoMapper;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pds.Core.Logging;
using Pds.DocumentExchange.Data.Api.Controllers;
using Pds.DocumentExchange.Data.Api.Validations;
using Pds.DocumentExchange.Data.Services.DTOs;
using Pds.DocumentExchange.Data.Services.Interfaces;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Api.Tests.Unit
{
    [TestClass]
    public class UploadControllerTests
    {
        private readonly Mock<IVirusScanProcessor> _virusScanProcessor = new Mock<IVirusScanProcessor>(MockBehavior.Strict);
        private readonly Mock<IVirusScanResultProcessor> _virusScanResultProcessor = new Mock<IVirusScanResultProcessor>(MockBehavior.Strict);
        private readonly Mock<IDocumentUploader> _documentUploader = new Mock<IDocumentUploader>(MockBehavior.Strict);
        private readonly Mock<IMapper> _mapper = new Mock<IMapper>(MockBehavior.Strict);
        private readonly Mock<IValidationService> _validationService = new Mock<IValidationService>(MockBehavior.Strict);
        private readonly Mock<ILoggerAdapter<UploadController>> _logger = new Mock<ILoggerAdapter<UploadController>>();

        private readonly UploadController _uploadController;

        public UploadControllerTests()
        {
            _uploadController = new UploadController(
                _virusScanProcessor.Object,
                _virusScanResultProcessor.Object,
                _documentUploader.Object,
                _mapper.Object,
                _validationService.Object,
                _logger.Object);
        }

        #region Document

        [TestMethod, TestCategory("Unit")]
        public async Task Document_WhenUploadDocumentRequestIsInvalid_ReturnsUnprocessableEntityObjectResult()
        {
            // Arrange
            var uploadDocumentRequest = new Models.UploadDocumentRequest();

            _validationService
                .Setup(validation => validation.Validate(uploadDocumentRequest, _uploadController.AddToModelState))
                .Returns(false);

            // Act
            var result = await _uploadController.Document(uploadDocumentRequest);

            // Assert
            result.Should().BeOfType<UnprocessableEntityObjectResult>();
        }

        [TestMethod, TestCategory("Unit")]
        public async Task Document_WhenUploadDocumentRequestIsValid_ReturnsOkResult()
        {
            // Arrange
            var uploadDocumentRequest = new Models.UploadDocumentRequest();

            var mappedUploadDocumentRequest = new Services.DTOs.UploadDocumentRequest();

            _validationService
                .Setup(validation => validation.Validate(uploadDocumentRequest, _uploadController.AddToModelState))
                .Returns(true);

            _mapper
                .Setup(mapper => mapper.Map<Models.UploadDocumentRequest, Services.DTOs.UploadDocumentRequest>(uploadDocumentRequest))
                .Returns(mappedUploadDocumentRequest);

            _documentUploader
                .Setup(documentUploader => documentUploader.UploadDocument(mappedUploadDocumentRequest))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _uploadController.Document(uploadDocumentRequest);

            // Assert
            result.Should().BeOfType<OkResult>();
        }

        #endregion


        #region PerformVirusScan

        [TestMethod, TestCategory("Unit")]
        [DataRow(null)]
        [DataRow("")]
        [DataRow(" ")]
        public async Task PerformVirusScan_WhenBatchIdentifierIsEmpty_ReturnsUnprocessableEntityObjectResult(string batchIdentifier)
        {
            // Act
            var result = await _uploadController.PerformVirusScan(batchIdentifier);

            // Assert
            result.Should().BeOfType<UnprocessableEntityObjectResult>();
        }

        [TestMethod, TestCategory("Unit")]
        public async Task PerformVirusScan_WhenBatchIdentifierHasValue_PerformsVirusScan()
        {
            // Arrange
            var batchIdentifier = "batch-identifier";

            _virusScanProcessor
               .Setup(scan => scan.RunVirusScanOrganisation(batchIdentifier))
               .Returns(Task.CompletedTask);

            // Act
            var result = await _uploadController.PerformVirusScan(batchIdentifier);

            // Assert
            result.Should().BeOfType<OkResult>();
        }


        #endregion


        #region VirusScanFail

        [TestMethod, TestCategory("Unit")]
        public async Task VirusScanFail_WhenDocumentReferenceIsInvalid_ReturnsUnprocessableEntityObjectResult()
        {
            // Arrange
            var documentReference = new Models.DocumentReference();

            _validationService
                .Setup(validation => validation.Validate(documentReference, _uploadController.AddToModelState))
                .Returns(false);

            // Act
            var result = await _uploadController.VirusScanFail(documentReference);

            // Assert
            result.Should().BeOfType<UnprocessableEntityObjectResult>();
        }

        [TestMethod, TestCategory("Unit")]
        public async Task VirusScanFail_WhenDocumentReferenceIsValid_ReturnsOkObjectResult()
        {
            // Arrange
            var documentReference = new Models.DocumentReference();

            var mappedDocumentReference = new DocumentReference();
            var fileMetadata = new FileMetadata();

            _validationService
                .Setup(validation => validation.Validate(documentReference, _uploadController.AddToModelState))
                .Returns(true);

            _mapper
                .Setup(mapper => mapper.Map<Models.DocumentReference, DocumentReference>(documentReference))
                .Returns(mappedDocumentReference);

            _virusScanResultProcessor
                .Setup(virusScanResultProcessor => virusScanResultProcessor.ProcessOrganisationFileResultVirusFound(mappedDocumentReference))
                .ReturnsAsync(fileMetadata);

            // Act
            var result = await _uploadController.VirusScanFail(documentReference);

            // Assert
            result.Should().BeOfType<OkResult>();
        }

        #endregion
    }
}