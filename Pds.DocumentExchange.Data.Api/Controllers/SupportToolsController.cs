using MapsterMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Pds.Core.Logging;
using Pds.DocumentExchange.Data.Api.Enums;
using Pds.DocumentExchange.Data.Api.Models;
using Pds.DocumentExchange.Data.Api.Models.SupportTools;
using Pds.DocumentExchange.Data.Services.DTOs.SupportTools;
using Pds.DocumentExchange.Data.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using ExchangeDocument = Pds.DocumentExchange.Data.Api.Models.ExchangeDocument;
using PublishedBatchApi = Pds.DocumentExchange.Data.Api.Models.SupportTools.PublishedBatch;

namespace Pds.DocumentExchange.Data.Api.Controllers
{
    /// <summary>
    /// The support tools controller: Responsible for actions involving information used for support.
    /// </summary>
    [ApiController]
    public class SupportToolsController : BaseApiController
    {
        private readonly ICosmosDbService _cosmosDbService;
        private readonly IReportService _reportService;
        private readonly IMapper _mapper;
        private readonly ICsvWriter _csvWriter;
        private readonly IAgencyExchangeDocumentsService _agencyExchangeDocumentsService;
        private readonly ILoggerAdapter<SupportToolsController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="SupportToolsController"/> class.
        /// </summary>
        /// <param name="cosmosDbService">The Cosmos DB service.</param>
        /// <param name="reportService">The report service.</param>
        /// <param name="mapper">The mapper.</param>
        /// <param name="csvWriter">The CSV writer.</param>
        /// <param name="agencyExchangeDocumentsService">The agency exchange documents service.</param>
        /// <param name="logger">The logger.</param>
        public SupportToolsController(
            ICosmosDbService cosmosDbService,
            IReportService reportService,
            IMapper mapper,
            ICsvWriter csvWriter,
            IAgencyExchangeDocumentsService agencyExchangeDocumentsService,
            ILoggerAdapter<SupportToolsController> logger)
        {
            _cosmosDbService = cosmosDbService;
            _reportService = reportService;
            _mapper = mapper;
            _csvWriter = csvWriter;
            _agencyExchangeDocumentsService = agencyExchangeDocumentsService;
            _logger = logger;
        }

        /// <summary>
        /// Gets the documents published by ESFA.
        /// </summary>
        /// <param name="pageNumber">The page number.</param>
        /// <param name="pageSize">The page size.</param>
        /// <returns>A <see cref="Task"/> returning the documents published by ESFA.</returns>
        [HttpGet("/api/[controller]/[action]")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ListResult<PublishedBatchApi>>> DocumentsPublishedByESFA(int pageNumber, int pageSize)
        {
            _logger.LogInformation("[SupportToolsController]: Getting batches published by ESFA.");

            var publishedBatchesResult = await _cosmosDbService.GetPublishedBatches(pageNumber, pageSize);
            var mappedPublishedBatches = _mapper.Map<Services.DTOs.ListResult<Services.DTOs.SupportTools.PublishedBatch>, ListResult<PublishedBatchApi>>(publishedBatchesResult);

            _logger.LogInformation($"[SupportToolsController]: Returning batches published by ESFA (Count: {mappedPublishedBatches.Items.Count()}).");
            return Ok(mappedPublishedBatches);
        }

        /// <summary>
        /// Gets the documents published by the given parent batch identifier.
        /// </summary>
        /// <param name="parentBatchIdentifier">The parent batch identifier.</param>
        /// <returns>A <see cref="Task"/> returning the documents published by parent batch identifier.</returns>
        [HttpGet("/api/[controller]/[action]/{parentBatchIdentifier}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<PublishedDocument>>> DocumentsPublished(Guid parentBatchIdentifier)
        {
            _logger.LogInformation($"[SupportToolsController]: Getting the published documents for parent batch: {parentBatchIdentifier}.");

            var batches = await _cosmosDbService.GetBatchesByParent(parentBatchIdentifier.ToString());

            if (batches == null || !batches.Any())
            {
                _logger.LogInformation($"[SupportToolsController]: No batches found for parent batch: {parentBatchIdentifier}.");
                return NotFound();
            }

            var publishedDocuments = _mapper.Map<IEnumerable<Services.DTOs.BatchMetadata>, IEnumerable<PublishedDocument>>(batches);

            _logger.LogInformation($"[SupportToolsController]: Returning the published documents for parent batch: {parentBatchIdentifier} (Count: {publishedDocuments.Count()}).");
            return Ok(publishedDocuments);
        }

        /// <summary>
        /// Gets the CSV file containing the documents published by the given parent batch identifier.
        /// </summary>
        /// <param name="parentBatchIdentifier">The parent batch identifier.</param>
        /// <returns>A <see cref="Task"/> returning a CSV file containing the documents published by parent batch identifier.</returns>
        [HttpGet("/api/[controller]/[action]/{parentBatchIdentifier}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<byte[]>> DocumentsPublishedCsv(Guid parentBatchIdentifier)
        {
            _logger.LogInformation($"[SupportToolsController]: Getting the published documents for parent batch: {parentBatchIdentifier}.");

            var batches = await _cosmosDbService.GetBatchesByParent(parentBatchIdentifier.ToString());

            if (batches == null || !batches.Any())
            {
                _logger.LogInformation($"[SupportToolsController]: No batches found for parent batch: {parentBatchIdentifier}.");
                return NotFound();
            }

            var publishedDocuments = _mapper.Map<IEnumerable<Services.DTOs.BatchMetadata>, IEnumerable<PublishedDocument>>(batches);

            _logger.LogInformation($"[SupportToolsController]: Writing the published documents to a CSV file for parent batch: {parentBatchIdentifier} (Count: {publishedDocuments.Count()}).");

            var csvFile = _csvWriter.WriteCsvFile(publishedDocuments);
            return Ok(csvFile);
        }

        /// <summary>
        /// Gets the notification recipients by the given parent batch identifier.
        /// </summary>
        /// <param name="parentBatchIdentifier">The parent batch identifier.</param>
        /// <returns>A <see cref="Task"/> returning the documents published by parent batch identifier.</returns>
        [HttpGet("/api/[controller]/[action]/{parentBatchIdentifier}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<NotificationRecipient>>> NotificationRecipients(Guid parentBatchIdentifier)
        {
            _logger.LogInformation($"[SupportToolsController]: Getting the notification recipients for parent batch: {parentBatchIdentifier}.");

            var notificationRecipients = await _cosmosDbService.GetNotificationRecipientsByParentBatch(parentBatchIdentifier);

            _logger.LogInformation($"[SupportToolsController]: Returning the notification recipients for parent batch: {parentBatchIdentifier} (Count: {notificationRecipients.Count()}).");
            return Ok(notificationRecipients);
        }

        /// <summary>
        /// Gets the CSV file containing the notification recipients by the given parent batch identifier.
        /// </summary>
        /// <param name="parentBatchIdentifier">The parent batch identifier.</param>
        /// <returns>A <see cref="Task"/> returning a CSV file containing the documents published by parent batch identifier.</returns>
        [HttpGet("/api/[controller]/[action]/{parentBatchIdentifier}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<byte[]>> NotificationRecipientsCsv(Guid parentBatchIdentifier)
        {
            _logger.LogInformation($"[SupportToolsController]: Getting the notification recipients for parent batch: {parentBatchIdentifier}.");

            var notificationRecipients = await _cosmosDbService.GetNotificationRecipientsByParentBatch(parentBatchIdentifier);

            _logger.LogInformation($"[SupportToolsController]: Writing the the notification recipients to a CSV file for parent batch: {parentBatchIdentifier} (Count: {notificationRecipients.Count()}).");

            var csvFile = _csvWriter.WriteCsvFile(notificationRecipients);
            return Ok(csvFile);
        }

        /// <summary>
        /// Gets the ODS file containing the Management Information report between the given date range.
        /// </summary>
        /// <param name="from">from date (report will be populated from the start of the day).</param>
        /// <param name="to">to date (report will be populated till the end of the day).</param>
        /// <returns><see cref="Task"/> representing the asynchronous operation.</returns>
        [HttpGet("/api/[controller]/[action]")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<byte[]>> DownloadMIReport(DateTime from, DateTime to)
        {
            try
            {
                _logger.LogInformation($"[SupportToolsController]: DownloadMIReport is triggered for the given date range: {from} to {to}.");

                return Ok(await _reportService.GetMIReport(from, to));
            }
            catch (Exception ex)
            {
                _logger.LogError($"[SupportToolsController]: Error while building MI report for the given range: {from} to {to}. {ex.ToString()}");

                return StatusCode(500, ex.ToString());
            }
        }

        /// <summary>
        /// Gets the documents to be deleted.
        /// </summary>
        /// <param name="direction">The exchange document direction.</param>
        /// <param name="ukprn">The UKPRN.</param>
        /// <param name="fileType">The file type.</param>
        /// <param name="year">The academic year.</param>
        /// <returns>A <see cref="Task"/> returning the documents to delete info.</returns>
        [HttpGet("/api/[controller]/[action]")]

        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ListResult<ExchangeDocument>>> DeleteDocumentsSearch([FromQuery] ExchangeDocumentDirection direction, [FromQuery] int ukprn, [FromQuery] string fileType, [FromQuery] string year)
        {
            _logger.LogInformation($"[SupportToolsController]: Getting the delete documents info: {direction}, {ukprn}, {fileType}, {year}.");

            var directionMapped = _mapper.Map<ExchangeDocumentDirection,
                    Services.Enums.ExchangeDocumentDirection>(direction);

            var listResult = await _agencyExchangeDocumentsService.GetDeleteFiles(directionMapped, ukprn, fileType, year);

            if (listResult == null || (!listResult.Items?.Any() ?? true))
            {
                _logger.LogInformation($"[SupportToolsController]: No delete documents info found for {direction}, {ukprn}, {fileType}, {year}.");
                return NotFound();
            }

            var result = _mapper.Map<Services.DTOs.ListResult<Services.DTOs.ExchangeDocument>,
                   ListResult<ExchangeDocument>>(listResult);

            _logger.LogInformation($"[SupportToolsController]: Returning the delete documents info for {direction}, {ukprn}, {fileType}, {year} ");
            return Ok(result);
        }
    }
}