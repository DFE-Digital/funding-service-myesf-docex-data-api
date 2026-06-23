using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Pds.Core.Logging;
using Pds.DocumentExchange.Data.Api.Models;
using Pds.DocumentExchange.Data.Api.Validations;
using Pds.DocumentExchange.Data.Services.Interfaces;
using System;
using System.Text.Json;
using System.Threading.Tasks;
using DocumentReference = Pds.DocumentExchange.Data.Api.Models.DocumentReference;

namespace Pds.DocumentExchange.Data.Api.Controllers
{
    /// <summary>
    /// The Upload controller: Responsible for actions involving the documents that are being uploaded by an organisation user.
    /// </summary>
    [ApiController]
    public class UploadController : BaseApiController
    {
        private readonly IVirusScanProcessor _virusScanProcessor;
        private readonly IVirusScanResultProcessor _virusScanResultProcessor;
        private readonly IDocumentUploader _documentUploader;
        private readonly IMapper _mapper;
        private readonly IValidationService _validationService;
        private readonly ILoggerAdapter<UploadController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="UploadController"/> class.
        /// </summary>
        /// <param name="virusScanProcessor">The virus scan processor.</param>
        /// <param name="virusScanResultProcessor">The virus scan result processor.</param>
        /// <param name="documentUploader">The document uploading service.</param>
        /// <param name="mapper">The mapper.</param>
        /// <param name="validationService">The validation service.</param>
        /// <param name="logger">The logger.</param>
        public UploadController(
            IVirusScanProcessor virusScanProcessor,
            IVirusScanResultProcessor virusScanResultProcessor,
            IDocumentUploader documentUploader,
            IMapper mapper,
            IValidationService validationService,
            ILoggerAdapter<UploadController> logger)
        {
            _virusScanProcessor = virusScanProcessor;
            _virusScanResultProcessor = virusScanResultProcessor;
            _documentUploader = documentUploader;
            _mapper = mapper;
            _validationService = validationService;
            _logger = logger;
        }

        /// <summary>Uploads the document.</summary>
        /// <param name="request">Request object containing information about the document being uploaded.</param>
        /// <returns>The action result.</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> Document(UploadDocumentRequest request)
        {
            try
            {
                if (!_validationService.Validate(request, AddToModelState))
                {
                    return new UnprocessableEntityObjectResult(ModelState);
                }

                _logger.LogInformation($"Document upload started for organisation's file [{request.FileName}]");

                var serviceUploadRequest = _mapper.Map<UploadDocumentRequest, Services.DTOs.UploadDocumentRequest>(request);

                await _documentUploader.UploadDocument(serviceUploadRequest);

                _logger.LogInformation($"Document upload finished for organisation's file [{request.FileName}]");

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred when uploading the organisation's file with request: {JsonSerializer.Serialize(request)}");
                throw;
            }
        }

        /// <summary>
        /// Sends a batch of files to be scanned by the anti-virus.
        /// </summary>
        /// <param name="batchIdentifier">The batch identifier.</param>
        /// <returns>The action result.</returns>
        [HttpPost("batches/{batchIdentifier}/[action]")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> PerformVirusScan(string batchIdentifier)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(batchIdentifier))
                {
                    ModelState.AddModelError(nameof(batchIdentifier), "The batch identifier cannot be empty.");
                    return new UnprocessableEntityObjectResult(ModelState);
                }

                _logger.LogInformation($"Virus scan process started for organisation's batch {batchIdentifier}.");

                await _virusScanProcessor.RunVirusScanOrganisation(batchIdentifier);

                _logger.LogInformation($"Virus scan process finished for organisation's batch {batchIdentifier}.");

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred when performing the virus scan with batchIdentifier: {batchIdentifier}");
                throw;
            }
        }

        /// <summary>
        /// Performs the required actions after the anti-virus finds a threat in one of the files uploaded by the an external user.
        /// </summary>
        /// <param name="documentReference">The virus scan info.</param>
        /// <returns>The action result.</returns>
        [HttpPost("batches/[action]")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> VirusScanFail(DocumentReference documentReference)
        {
            try
            {
                if (!_validationService.Validate(documentReference, AddToModelState))
                {
                    return new UnprocessableEntityObjectResult(ModelState);
                }

                _logger.LogInformation($"Virus scan fail result process started for organisation's batch {documentReference.BatchIdentifier}.");

                await _virusScanResultProcessor.ProcessOrganisationFileResultVirusFound(
                    _mapper.Map<DocumentReference, Services.DTOs.DocumentReference>(documentReference));

                _logger.LogInformation($"Virus scan fail result process finished for organisation's batch {documentReference.BatchIdentifier}.");

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred when performing the virus scan fail result process for organisation's batch with documentReference: {JsonSerializer.Serialize(documentReference)}.");
                throw;
            }
        }
    }
}