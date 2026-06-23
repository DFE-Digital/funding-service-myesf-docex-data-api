using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Pds.Core.Logging;
using Pds.DocumentExchange.Data.Api.Models;
using Pds.DocumentExchange.Data.Api.Validations;
using Pds.DocumentExchange.Data.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using DocumentReference = Pds.DocumentExchange.Data.Api.Models.DocumentReference;
using ExchangeDocument = Pds.DocumentExchange.Data.Api.Models.ExchangeDocument;
using ExchangeListOrganisationDocumentOptions = Pds.DocumentExchange.Data.Api.Models.ExchangeListOrganisationDocumentOptions;
using OrganisationIdentifier = Pds.DocumentExchange.Data.Api.Models.OrganisationIdentifier;
using UserInfo = Pds.DocumentExchange.Data.Api.Models.UserInfo;

namespace Pds.DocumentExchange.Data.Api.Controllers
{
    /// <summary>The Exchange Controller - exposes api methods to provide information about exchanging documents.</summary>
    [ApiController]
    public class ExchangeController : BaseApiController
    {
        private readonly IExchangeDocumentSelector _exchangeDocumentSelector;
        private readonly IVirusScanResultProcessor _virusScanResultProcessor;
        private readonly IDocumentDownloader _documentDownloader;
        private readonly IDocumentExchangeSummaries _documentExchangeSummaries;
        private readonly IProductVersionService _productVersionService;
        private readonly IMapper _mapper;
        private readonly IValidationService _validationService;
        private readonly IDocumentDeletion _documentDeletion;
        private readonly ILoggerAdapter<ExchangeController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExchangeController"/> class.
        /// </summary>
        /// <param name="exchangeDocumentSelector">The exchange document selector.</param>
        /// <param name="virusScanResultProcessor">The virus scan result processor.</param>
        /// <param name="cosmosDbService">The cosmos db service.</param>
        /// <param name="documentDownloader">The document downloaded class.</param>
        /// <param name="academicYearCalculator">The academic year calculator.</param>
        /// <param name="documentExchangeSummaries">The document exchange summaries service.</param>
        /// <param name="productVersionService">The product version service.</param>
        /// <param name="mapper">The type mapper.</param>
        /// <param name="validationService">The validation service.</param>
        /// <param name="documentDeletion">The document deletion service.</param>
        /// <param name="logger">The logger.</param>
        public ExchangeController(
            IExchangeDocumentSelector exchangeDocumentSelector,
            IVirusScanResultProcessor virusScanResultProcessor,
            IDocumentDownloader documentDownloader,
            IDocumentExchangeSummaries documentExchangeSummaries,
            IProductVersionService productVersionService,
            IMapper mapper,
            IValidationService validationService,
            IDocumentDeletion documentDeletion,
            ILoggerAdapter<ExchangeController> logger)
        {
            _exchangeDocumentSelector = exchangeDocumentSelector;
            _virusScanResultProcessor = virusScanResultProcessor;
            _documentDownloader = documentDownloader;
            _documentExchangeSummaries = documentExchangeSummaries;
            _productVersionService = productVersionService;
            _mapper = mapper;
            _validationService = validationService;
            _documentDeletion = documentDeletion;
            _logger = logger;
        }

        /// <summary>
        /// Gets the summary for an organisation user.
        /// </summary>
        /// <param name="userInfo">The user information.</param>
        /// <exception cref="ArgumentNullException">Argument cannot be null.</exception>
        /// <returns>The action result containing, if successful, the summary response.</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Summary>> Summary(UserInfo userInfo)
        {
            try
            {
                _logger.LogInformation($"Started action: {nameof(Summary)} for user");

                if (!_validationService.Validate(userInfo, AddToModelState))
                {
                    return new UnprocessableEntityObjectResult(ModelState);
                }

                _logger.LogInformation($"Running {nameof(_documentExchangeSummaries.GetOrganisationUserSummary)} for user");

                var summary = await _documentExchangeSummaries.GetOrganisationUserSummary(_mapper.Map<UserInfo, Services.DTOs.User.UserInfo>(userInfo));

                _logger.LogInformation($"Finished action: {nameof(Summary)} and running {nameof(_documentExchangeSummaries.GetOrganisationUserSummary)} with CountOfNewDocuments : {summary?.CountOfNewDocuments}" +
                    $", DocumentExchangeEnabled : {summary?.DocumentExchangeEnabled}");

                var result = _mapper.Map<Services.DTOs.Summary, Summary>(summary);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred in action: {nameof(Summary)} for the user's OrganisationIdentifier : {userInfo?.OrganisationInfo?.OrganisationIdentifier}");
                throw;
            }
        }

        /// <summary>
        /// Gets the summary for a list of agency teams.
        /// </summary>
        /// <param name="teams">The team names.</param>
        /// <returns>The action result containing, if successful, the summary response.</returns>
        [HttpGet("/api/[controller]/teams/{teams}/[action]")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Summary>> TeamSummary(string teams)
        {
            try
            {
                _logger.LogInformation($"Started action: {nameof(TeamSummary)} for teams : {teams}");

                var teamsList = GetParameterValuesFromCsvString(teams).ToList();

                if (!await _validationService.ValidateTeams(teamsList, ModelState.AddModelError))
                {
                    return new UnprocessableEntityObjectResult(ModelState);
                }

                _logger.LogInformation($"Running {nameof(_documentExchangeSummaries.GetTeamsSummary)} for teams : {string.Join(", ", teamsList)}");

                var summary = await _documentExchangeSummaries.GetTeamsSummary(teamsList);

                _logger.LogInformation($"Finished running {nameof(_documentExchangeSummaries.GetTeamsSummary)} for teams : {string.Join(", ", teamsList)} with CountOfNewDocuments : {summary?.CountOfNewDocuments}" +
                    $", DocumentExchangeEnabled : {summary?.DocumentExchangeEnabled}");

                var result = _mapper.Map<Services.DTOs.Summary, Summary>(summary);

                _logger.LogInformation($"Finished action: {nameof(TeamSummary)}");

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred in action: {nameof(TeamSummary)} for teams: {teams}");
                throw;
            }
        }

        /// <summary>
        /// Gets the exchanged documents for the given team, filtered and paginated by the given options.
        /// </summary>
        /// <param name="teams">The csv list of agency teams.</param>
        /// <param name="options">The options for filters and pagination.</param>
        /// <returns>The action result containing, if successful,
        /// the documents' metadata and filter/pagination data.</returns>
        [HttpPost("/api/[controller]/teams/{teams}/[action]")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ListResult<ExchangeDocument>>> Documents(
            string teams,
            ExchangeListDocumentOptions options)
        {
            try
            {
                var teamsList = GetParameterValuesFromCsvString(teams);

                _logger.LogInformation($"Started action: {nameof(Documents)} for teams : {teams}");

                if (!_validationService.Validate(options, AddToModelState)
                    || !await _validationService.ValidateTeams(teamsList, ModelState.AddModelError))
                {
                    return new UnprocessableEntityObjectResult(ModelState);
                }

                var serviceOptions = _mapper.Map<ExchangeListDocumentOptions,
                    Services.DTOs.ExchangeListDocumentOptions>(options);

                _logger.LogInformation($"Running {nameof(_exchangeDocumentSelector.GetAgencyTeamFiles)} for teams : {string.Join(", ", teamsList)}");

                var listResult = await _exchangeDocumentSelector.GetAgencyTeamFiles(
                    teamsList,
                    serviceOptions);

                _logger.LogInformation($"Finished running {nameof(_exchangeDocumentSelector.GetAgencyTeamFiles)} for teams : {string.Join(", ", teamsList)} with: {listResult?.Items?.Count()} documents");

                var result = _mapper.Map<Services.DTOs.ListResult<Services.DTOs.ExchangeDocument>,
                    ListResult<ExchangeDocument>>(listResult);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred in action: {nameof(Documents)} for teams: {teams}, options:{JsonSerializer.Serialize(options)}");
                throw;
            }
        }

        /// <summary>
        /// Gets the organisation received and sent files.
        /// </summary>
        /// <param name="options">The options for user, filters and pagination.</param>
        /// <returns>The action result containing, if successful,
        /// the documents' metadata and filter/pagination data.</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ListResult<ExchangeDocument>>> Documents(ExchangeListOrganisationDocumentOptions options)
        {
            try
            {
                _logger.LogInformation($"Started action: {nameof(Documents)}");

                if (!_validationService.Validate(options, AddToModelState))
                {
                    return new UnprocessableEntityObjectResult(ModelState);
                }

                var serviceOptions = _mapper.Map<ExchangeListOrganisationDocumentOptions,
                    Services.DTOs.ExchangeListOrganisationDocumentOptions>(options);

                _logger.LogInformation($"Running {nameof(_exchangeDocumentSelector.GetOrganisationFiles)}");

                var listResult = await _exchangeDocumentSelector.GetOrganisationFiles(serviceOptions);

                _logger.LogInformation($"Finished running {nameof(_exchangeDocumentSelector.GetOrganisationFiles)} with: {listResult?.Items?.Count()} documents");

                var result = _mapper.Map<Services.DTOs.ListResult<Services.DTOs.ExchangeDocument>,
                     ListResult<ExchangeDocument>>(listResult);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred in action: {nameof(Documents)} for options:{JsonSerializer.Serialize(options)}");
                throw;
            }
        }

        /// <summary>
        /// Downloads the specified documents. Marks the documents as viewed by the specified user.
        /// </summary>
        /// <param name="downloadRequest">Request object containing
        /// the user information and the documents to download.</param>
        /// <returns>The action result containing, if successful, a byte array representing a file
        /// that is either the single document if only one was requested,
        /// or a zip file including all of the requested documents.</returns>
        [HttpPost("/api/[controller]/documents/[action]")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<byte[]>> Download(ExchangeDocumentDownloadRequest downloadRequest)
        {
            try
            {
                if (!_validationService.Validate(downloadRequest, AddToModelState))
                {
                    return new UnprocessableEntityObjectResult(ModelState);
                }

                var serviceDownloadRequest = _mapper.Map<Services.DTOs.ExchangeDocumentDownloadRequest>(downloadRequest);
                return await _documentDownloader.DownloadExchangeDocument(serviceDownloadRequest);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred in action: {nameof(Download)} for ListOptions:{JsonSerializer.Serialize(downloadRequest?.ListOptions)}");
                throw;
            }
        }

        /// <summary>
        /// Downloads the specified documents. Marks the documents as viewed by the specified user.
        /// </summary>
        /// <param name="teams">The team names.</param>
        /// <param name="downloadRequest">Request object containing
        /// the user information and the documents to download.</param>
        /// <returns>The action result containing, if successful, a byte array representing a file
        /// that is either the single document if only one was requested,
        /// or a zip file including all of the requested documents.</returns>
        [HttpPost("/api/[controller]/teams/{teams}/documents/[action]")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<byte[]>> Download(string teams, ExchangeDocumentDownloadRequest downloadRequest)
        {
            try
            {
                _logger.LogInformation($"Started action: {nameof(Download)} for teams : {teams}");

                var teamsList = GetParameterValuesFromCsvString(teams).ToList();

                if (!_validationService.Validate(downloadRequest, AddToModelState)
                    || !await _validationService.ValidateTeams(teamsList, ModelState.AddModelError))
                {
                    return new UnprocessableEntityObjectResult(ModelState);
                }

                var serviceDownloadRequest = _mapper.Map<Services.DTOs.ExchangeDocumentDownloadRequest>(downloadRequest);

                _logger.LogInformation($"Running {nameof(_exchangeDocumentSelector.GetAgencyTeamFiles)} for teams : {teams}");

                var listResult = await _exchangeDocumentSelector.GetAgencyTeamFiles(
                    teamsList,
                    serviceDownloadRequest.ListOptions);

                _logger.LogInformation($"Finished running {nameof(_exchangeDocumentSelector.GetAgencyTeamFiles)} with: {listResult?.Items?.Count()} documents");

                serviceDownloadRequest.ListOptions.DocumentReferences = listResult?.Items.Select(doc => doc.DocumentReference);

                var fileContent = await _documentDownloader.DownloadExchangeDocument(serviceDownloadRequest, true);

                return Ok(fileContent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred in action: {nameof(Download)}  for teams : {teams} for ListOptions:{JsonSerializer.Serialize(downloadRequest?.ListOptions)}");
                throw;
            }
        }

        /// <summary>The current product version for organisation.</summary>
        /// <param name="organisationIdentifier">The organisation identifier.</param>
        /// <param name="productIdentifier">The product identifier.</param>
        /// <returns>The action result containing, if successful, the current version number of the product,
        /// for the given organisation and for the current year.</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<int>> CurrentProductVersionForOrganisation(OrganisationIdentifier organisationIdentifier, int productIdentifier)
        {
            try
            {
                _logger.LogInformation($"Started action: {nameof(CurrentProductVersionForOrganisation)} for organisationIdentifier : {JsonSerializer.Serialize(organisationIdentifier)} and productIdentifier:{productIdentifier}");

                if (!_validationService.Validate(organisationIdentifier, AddToModelState)
               || !await _validationService.ValidateProductIdentifier(productIdentifier, ModelState.AddModelError))
                {
                    return new UnprocessableEntityObjectResult(ModelState);
                }

                var mappedOrg = _mapper.Map<OrganisationIdentifier, Pds.Core.Common.Organisation.Models.OrganisationIdentifier>(organisationIdentifier);
                var productVersion = await _productVersionService.GetCurrentProductVersion(mappedOrg, productIdentifier);

                _logger.LogInformation($"Finished action {nameof(CurrentProductVersionForOrganisation)}");

                return Ok(productVersion);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred in action: {nameof(CurrentProductVersionForOrganisation)} for organisationIdentifier : {JsonSerializer.Serialize(organisationIdentifier)} and productIdentifier:{productIdentifier}");
                throw;
            }
        }

        /// <summary>
        /// Performs the required actions after an external user's document have been successfully scanned by the antivirus.
        /// </summary>
        /// <param name="documentReference">The document reference.</param>
        /// <returns>The action result.</returns>
        [HttpPost("/api/[controller]/batches/[action]")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> OrganisationFilesVirusScanSuccessful(DocumentReference documentReference)
        {
            try
            {
                if (!_validationService.Validate(documentReference, AddToModelState))
                {
                    return new UnprocessableEntityObjectResult(ModelState);
                }

                _logger.LogInformation($"Virus scan ok result process started for organisation's batch {documentReference.BatchIdentifier}.");

                await _virusScanResultProcessor.ProcessOrganisationFileResultOk(
                    _mapper.Map<DocumentReference, Services.DTOs.DocumentReference>(documentReference));

                _logger.LogInformation($"Virus scan ok result process finished for organisation's batch {documentReference.BatchIdentifier}.");

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred in action: {nameof(OrganisationFilesVirusScanSuccessful)}  for organisation's batch {documentReference?.BatchIdentifier}.");
                throw;
            }
        }

        /// <summary>
        /// Performs the required actions after an agency's document have been successfully scanned by the antivirus.
        /// </summary>
        /// <param name="documentReference">The document reference.</param>
        /// <returns>The action result.</returns>
        [HttpPost("/api/[controller]/batches/[action]")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> AgencyFilesVirusScanSuccessful(DocumentReference documentReference)
        {
            try
            {
                if (!_validationService.Validate(documentReference, AddToModelState))
                {
                    return new UnprocessableEntityObjectResult(ModelState);
                }

                _logger.LogInformation($"Virus scan ok result process started for agency's batch {documentReference.BatchIdentifier}.");

                await _virusScanResultProcessor.ProcessAgencyFileResultOk(
                    _mapper.Map<DocumentReference, Services.DTOs.DocumentReference>(documentReference));

                _logger.LogInformation($"Virus scan ok result process finished for agency's batch {documentReference.BatchIdentifier}.");

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred in action: {nameof(AgencyFilesVirusScanSuccessful)}  for agency's batch {documentReference?.BatchIdentifier}.");
                throw;
            }
        }

        /// <summary>
        /// Deletes the specified documents. Marks the documents metadata as deleted.
        /// </summary>
        /// <param name="deleteRequest">Request object containing
        /// the user information and the documents to delete.</param>
        /// <returns>The action result.</returns>
        [HttpPost("/api/[controller]/documents/[action]")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<IEnumerable<ExchangeDocument>>> Delete(ExchangeDocumentDeleteRequest deleteRequest)
        {
            try
            {
                _logger.LogInformation($"Started action: {nameof(Delete)}");

                if (!_validationService.Validate(deleteRequest, AddToModelState))
                {
                    return new UnprocessableEntityObjectResult(ModelState);
                }

                _logger.LogInformation($"Running {nameof(_documentDeletion.DeleteDocuments)} for {deleteRequest?.DocumentReferences?.Count()} documents");

                var documents = await _documentDeletion.DeleteDocuments(
                    _mapper.Map<Services.DTOs.ExchangeDocumentDeleteRequest>(deleteRequest));

                _logger.LogInformation($"Finished running {nameof(_documentDeletion.DeleteDocuments)} with: {documents?.Count()} documents");

                var result = _mapper.Map<IEnumerable<ExchangeDocument>>(documents);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred in action: {nameof(Delete)}  for deleteRequest {JsonSerializer.Serialize(deleteRequest)}");
                throw;
            }
        }

        /// <summary>
        /// Deletes the specified document. Marks the document metadata as deleted.
        /// </summary>
        /// <param name="deleteRequest">Request object containing
        /// the user information and the document to delete.</param>
        /// <returns>The action result.</returns>
        [HttpPost("/api/[controller]/documents/[action]")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<ExchangeDocument>> DeleteSingle(ExchangeDocumentDeleteRequest deleteRequest)
        {
            try
            {
                _logger.LogInformation($"Started action: {nameof(DeleteSingle)}");

                if (!_validationService.Validate(deleteRequest, AddToModelState))
                {
                    return new UnprocessableEntityObjectResult(ModelState);
                }

                _logger.LogInformation($"Running {nameof(_documentDeletion.DeleteDocument)} for {deleteRequest?.DocumentReferences?.Count()} documents");

                var document = await _documentDeletion.DeleteDocument(
                    _mapper.Map<Services.DTOs.ExchangeDocumentDeleteRequest>(deleteRequest));

                _logger.LogInformation($"Finished running {nameof(_documentDeletion.DeleteDocument)} with  document: {document?.DocumentReference?.FileName}");

                var result = _mapper.Map<ExchangeDocument>(document);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred in action: {nameof(DeleteSingle)}  for deleteRequest {JsonSerializer.Serialize(deleteRequest)}");
                throw;
            }
        }
    }
}