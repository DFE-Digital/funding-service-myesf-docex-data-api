using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Pds.Core.Logging;
using Pds.DocumentExchange.Data.Api.Models;
using Pds.DocumentExchange.Data.Api.Validations;
using Pds.DocumentExchange.Data.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using DocumentReference = Pds.DocumentExchange.Data.Api.Models.DocumentReference;
using Product = Pds.DocumentExchange.Data.Api.Models.Product;

namespace Pds.DocumentExchange.Data.Api.Controllers
{
    /// <summary>
    /// The Agency controller - Exposes API methods related to actions performed by the Agency.
    /// </summary>
    [ApiController]
    [Route("/api/[controller]")]
    public class AgencyController : BaseApiController
    {
        private readonly IVirusScanProcessor _virusScanProcessor;
        private readonly IVirusScanResultProcessor _virusScanResultProcessor;
        private readonly IDocumentDownloader _documentDownloader;
        private readonly IDocumentManager _documentManager;
        private readonly IAgencyService _agencyService;
        private readonly IDocumentPublisher _documentPublisher;
        private readonly IMapper _mapper;
        private readonly IValidationService _validationService;
        private readonly ILoggerAdapter<AgencyController> _logger;
        private readonly IExchangeDocumentSelector _exchangeDocumentSelector;

        /// <summary>
        /// Initializes a new instance of the <see cref="AgencyController"/> class.
        /// </summary>
        /// <param name="virusScanProcessor">The virus scan processor.</param>
        /// <param name="virusScanResultProcessor">The virus scan result processor.</param>
        /// <param name="documentDownloader">The document downloader provider.</param>
        /// <param name="documentManager">The document manager provider.</param>
        /// <param name="agencyService">The agency service.</param>
        /// <param name="documentPublisher">The document publisher.</param>
        /// <param name="mapper">The type mapper.</param>
        /// <param name="validationService">The validation service.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="exchangeDocumentSelector">The exchange document selector.</param>
        public AgencyController(
            IVirusScanProcessor virusScanProcessor,
            IVirusScanResultProcessor virusScanResultProcessor,
            IDocumentDownloader documentDownloader,
            IDocumentManager documentManager,
            IAgencyService agencyService,
            IDocumentPublisher documentPublisher,
            IMapper mapper,
            IValidationService validationService,
            ILoggerAdapter<AgencyController> logger,
            IExchangeDocumentSelector exchangeDocumentSelector)
        {
            _virusScanProcessor = virusScanProcessor;
            _virusScanResultProcessor = virusScanResultProcessor;
            _documentDownloader = documentDownloader;
            _documentManager = documentManager;
            _agencyService = agencyService;
            _documentPublisher = documentPublisher;
            _validationService = validationService;
            _mapper = mapper;
            _logger = logger;
            _exchangeDocumentSelector = exchangeDocumentSelector;
        }

        /// <summary>
        /// Gets a summary of the agency files contained within the file share for the given teams.
        /// </summary>
        /// <param name="teams">The csv list of agency teams for which file share summary has to be retrieved.</param>
        /// <returns>The action result containing, if successful,
        /// a summary of the agency team's files.</returns>
        [HttpGet("teams/{teams}/[action]")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<FileShareSummary>> Summary(string teams)
        {
            try
            {
                _logger.LogInformation($"Started action: {nameof(Summary)} for teams: {teams}");

                var teamsList = GetParameterValuesFromCsvString(teams).ToList();

                if (!await _validationService.ValidateTeams(teamsList, ModelState.AddModelError))
                {
                    return new UnprocessableEntityObjectResult(ModelState);
                }

                _logger.LogInformation($"Getting file share summary for the teams: {teams}");
                var fileShareSummary = await _agencyService.Summary(teamsList);
                _logger.LogInformation($"Received file share summary for the teams: {teams}");

                var result = _mapper.Map<Services.DTOs.FileShareSummary, FileShareSummary>(fileShareSummary);

                _logger.LogInformation($"Finished action: {nameof(Summary)}");

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred in action: {nameof(Summary)} for the teams: {teams}");
                throw;
            }
        }

        /// <summary>
        /// Gets the file share documents for the given team, filtered and paginated by the given options.
        /// </summary>
        /// <param name="teams">The agency teams.</param>
        /// <param name="options">The options for filters and pagination.</param>
        /// <returns>The action result containing, if successful,
        /// the documents' metadata and filter/pagination data.</returns>
        [HttpPost("teams/{teams}/[action]")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ListResult<AgencyDocument>>> Documents(string teams, AgencyListDocumentOptions options)
        {
            try
            {
                _logger.LogInformation($"Started action: {nameof(Documents)} for teams: {teams}");

                var teamsList = GetParameterValuesFromCsvString(teams);

                if (!_validationService.Validate(options, AddToModelState)
                    || !await _validationService.ValidateTeams(teamsList, ModelState.AddModelError))
                {
                    return new UnprocessableEntityObjectResult(ModelState);
                }

                _logger.LogInformation($"Agency documents process started for team {teams}.");

                var serviceOptions = _mapper.Map<AgencyListDocumentOptions,
                            Services.DTOs.AgencyListDocumentOptions>(options);

                var listResult = await _agencyService.GetDocuments(teamsList, serviceOptions);

                var result = _mapper.Map<Services.DTOs.ListResult<Services.DTOs.AgencyDocument>,
                        ListResult<AgencyDocument>>(listResult);

                _logger.LogInformation($"Agency documents process finished for team {teams}.");

                _logger.LogInformation($"Finished action: {nameof(Documents)}");

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred in action: {nameof(Documents)} for the teams: {teams}, options: {JsonSerializer.Serialize(options)} ");
                throw;
            }
        }

        /// <summary>
        /// Downloads the specified document for the given team.
        /// </summary>
        /// <param name="team">The agency team.</param>
        /// <param name="fileName">The file name.</param>
        /// <returns>The action result containing, if successful,
        /// the byte array representing the content of the document.</returns>
        [HttpGet("teams/{team}/documents/{fileName}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<byte[]>> Download(string team, string fileName)
        {
            try
            {
                _logger.LogInformation($"Started action: {nameof(Download)} for team: {team}, fileName: {fileName}");

                if (!await _validationService.ValidateTeam(team, ModelState.AddModelError))
                {
                    return new UnprocessableEntityObjectResult(ModelState);
                }

                if (string.IsNullOrWhiteSpace(fileName))
                {
                    ModelState.AddModelError(nameof(fileName), "The file name cannot be empty.");
                    return new UnprocessableEntityObjectResult(ModelState);
                }

                _logger.LogInformation($"Downloading file {fileName} for team {team}.");

                return await _documentDownloader.DownloadAgencyDocument(team, fileName);
            }
            catch (FileNotFoundException ex)
            {
                _logger.LogError(ex, $"The document {fileName} for team {team} was not found.");
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred in action: {nameof(Download)} for the team: {team}, fileName: {fileName}");
                throw;
            }
        }

        /// <summary>
        /// Deletes the specified documents from the file share of the given team.
        /// </summary>
        /// <param name="team">The agency team.</param>
        /// <param name="fileNames">The list of file names to delete.</param>
        /// <returns>The action result containing, if successful,
        /// the status of the action.</returns>
        [HttpPost("teams/{team}/documents/[action]")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> Remove(string team, IEnumerable<string> fileNames)
        {
            try
            {
                _logger.LogInformation($"Started action: {nameof(Remove)} for  team: {team} and {fileNames?.Count()} files.");

                if (!await _validationService.ValidateTeam(team, ModelState.AddModelError))
                {
                    return new UnprocessableEntityObjectResult(ModelState);
                }

                if (fileNames?.Any() != true)
                {
                    ModelState.AddModelError(nameof(fileNames), "File names to remove were not provided.");
                    return new UnprocessableEntityObjectResult(ModelState);
                }

                _logger.LogInformation($"Removing fileNames: {string.Join(", ", fileNames)} for  team: {team}");
                await _documentManager.RemoveAgencyDocuments(team, fileNames);

                _logger.LogInformation($"Finished action: {nameof(Remove)}");

                return Ok();
            }
            catch (FileNotFoundException ex)
            {
                _logger.LogError(ex, $"The document for team {team} was not found.");
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred in action: {nameof(Remove)} for the team: {team}, fileNames: {string.Join(", ", fileNames)}");
                throw;
            }
        }

        /// <summary>
        /// Publishes the specified documents from the file share of the given team.
        /// </summary>
        /// <param name="team">The agency team.</param>
        /// <param name="agencyPublishRequest">Request object containing the files to publish
        /// and the user information.</param>
        /// <returns>The action result containing, if successful,
        /// the list of counts of each product that was published.</returns>
        [HttpPost("teams/{team}/documents/[action]")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<KeyValuePair<Product, int>>> Publish(string team, AgencyPublishRequest agencyPublishRequest)
        {
            try
            {
                _logger.LogInformation($"Started action: {nameof(Publish)} for team: {team}");

                if (!_validationService.Validate(agencyPublishRequest, AddToModelState)
                              || !await _validationService.ValidateTeam(team, ModelState.AddModelError)
                              || !await _validationService.ValidateProductIdentifier(agencyPublishRequest.ProductId, ModelState.AddModelError))
                {
                    return new UnprocessableEntityObjectResult(ModelState);
                }

                _logger.LogInformation($"Document publish started for agency team {team} and productId {agencyPublishRequest?.ProductId}.");

                var result = await _documentPublisher.PublishDocuments(
                    team,
                    _mapper.Map<Services.DTOs.AgencyPublishRequest>(agencyPublishRequest));

                _logger.LogInformation($"Finished action: {nameof(Publish)}");

                return Ok(new KeyValuePair<Product, int>(_mapper.Map<Product>(result.Key), result.Value));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred in action: {nameof(Publish)} for the team: {team}, agencyPublishRequest: {JsonSerializer.Serialize(agencyPublishRequest)}");
                throw;
            }
        }

        /// <summary>
        /// Sends a batch of files to be scanned by the antivirus.
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
                _logger.LogInformation($"Started action: {nameof(PerformVirusScan)} for the batchIdentifier: {batchIdentifier}");

                if (string.IsNullOrWhiteSpace(batchIdentifier))
                {
                    ModelState.AddModelError(nameof(batchIdentifier), "The batch identifier cannot be empty.");
                    return new UnprocessableEntityObjectResult(ModelState);
                }

                _logger.LogInformation($"Virus scan process started for agency's batch {batchIdentifier}.");

                await _virusScanProcessor.RunVirusScanAgency(batchIdentifier);

                _logger.LogInformation($"Finished action: {nameof(PerformVirusScan)}");

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred in action: {nameof(PerformVirusScan)} for the batchIdentifier: {batchIdentifier}");
                throw;
            }
        }

        /// <summary>
        /// Performs the required actions after the antivirus finds a threat in one of
        /// the files uploaded by the agency.
        /// </summary>
        /// <param name="documentReference">The reference information of a file which failed to pass the virus scan.</param>
        /// <returns>The action result.</returns>
        [HttpPost("batches/[action]")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> VirusScanFail(DocumentReference documentReference)
        {
            try
            {
                _logger.LogInformation($"Started action: {nameof(VirusScanFail)}");

                if (!_validationService.Validate(documentReference, AddToModelState))
                {
                    return new UnprocessableEntityObjectResult(ModelState);
                }

                _logger.LogInformation($"Virus scan fail result process started for document {documentReference?.FileName}.");

                await _virusScanResultProcessor.ProcessAgencyFileResultVirusFound(
                    _mapper.Map<DocumentReference, Services.DTOs.DocumentReference>(documentReference));

                _logger.LogInformation($"Finished action: {nameof(VirusScanFail)}");

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred in action: {nameof(VirusScanFail)} for the DocumentReference: {JsonSerializer.Serialize(documentReference)}");
                throw;
            }
        }

        /// <summary>
        /// Gets list of document references associated with previous versions excluding the document references passed in.
        /// </summary>
        /// <param name="team">The agency team.</param>
        /// <param name="documentReferenceList">List of all document References for which previous document references need to be extracted.</param>
        /// <returns>The action result.</returns>
        [HttpPost("teams/{team}/documents/[action]")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> PreviousDocumentReferences(string team, IEnumerable<DocumentReference> documentReferenceList)
        {
            try
            {
                _logger.LogInformation($"Started action: {nameof(PreviousDocumentReferences)} for team : {team} and {documentReferenceList?.Count()} document references.");

                if (documentReferenceList?.Any() != true)
                {
                    ModelState.AddModelError(nameof(documentReferenceList), "Document references were not provided.");
                    return new UnprocessableEntityObjectResult(ModelState);
                }

                var previousDocumentReferenceList = new List<DocumentReference>();

                var dtodocumentReferences = new List<Services.DTOs.DocumentReference>();
                foreach (var item in documentReferenceList)
                {
                    dtodocumentReferences.Add(_mapper.Map<Services.DTOs.DocumentReference>(item));
                }

                _logger.LogInformation($"Running GetAgencyTeamExchangeDocuments for specified {documentReferenceList?.Count()} document references.");

                var result = await _exchangeDocumentSelector.GetAgencyTeamExchangeDocuments(new List<string> { team }, dtodocumentReferences);

                _logger.LogInformation($"Finished running GetAgencyTeamExchangeDocuments. Returned {result?.Count()} documents");

                var previousVersions = result.SelectMany(document => document.PreviousVersions);

                var finalListExcludingPreviousVersionsThatHaveAlreadyBeenDownloaded = (previousVersions ?? Enumerable.Empty<Services.DTOs.ExchangeDocument>())
                    .Where(item => item.EventHistory != null
                    && !item.EventHistory.Any(eventHistory => eventHistory.EventType == Services.Enums.ExchangeDocumentEventType.DownloadedByReceiver))
                    .ToList();

                previousDocumentReferenceList.AddRange(finalListExcludingPreviousVersionsThatHaveAlreadyBeenDownloaded.Select(item => new DocumentReference
                {
                    BatchIdentifier = item.DocumentReference.BatchIdentifier,
                    FileName = item.DocumentReference.FileName,
                    ParentBatchIdentifier = item.DocumentReference.ParentBatchIdentifier,
                }));

                _logger.LogInformation($"Finished action: {nameof(PreviousDocumentReferences)} with previousVersions:{previousVersions?.Count()}" +
                    $", finalListExcludingPreviousVersionsThatHaveAlreadyBeenDownloaded : {finalListExcludingPreviousVersionsThatHaveAlreadyBeenDownloaded?.Count}" +
                    $", previousDocumentReferenceList: {previousDocumentReferenceList?.Count} ");

                return Ok(previousDocumentReferenceList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred in action: {nameof(PreviousDocumentReferences)} for team:{team},  documentReferenceList: {JsonSerializer.Serialize(documentReferenceList)}");
                throw;
            }
        }
    }
}