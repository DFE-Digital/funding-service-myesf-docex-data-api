using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Pds.Core.BulkJobs.Interfaces;
using Pds.Core.BulkJobs.Models;
using Pds.Core.Logging;
using Pds.DocumentExchange.Data.Services.DTOs.Notification;
using Pds.DocumentExchange.Data.Services.Interfaces.Coordinators;
using System;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Api.Controllers
{
    /// <summary>
    /// The Notifications controller - Responsible for actions involving notifications.
    /// </summary>
    [ApiController]
    public class NotificationController : BaseApiController
    {
        private readonly INotifyPublishCompleteCoordinator _notifyPublishCompleteCoordinator;
        private readonly INotifyUploadCompleteCoordinator _notifyUploadCompleteCoordinator;
        private readonly IBulkJobManager _bulkJobManager;
        private readonly ILoggerAdapter<NotificationController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationController"/> class.
        /// </summary>
        /// <param name="notifyPublishCompleteCoordinator">the notify publish complete coordinator.</param>
        /// <param name="notifyUploadCompleteCoordinator">the notify upload complete coordinator.</param>
        /// <param name="bulkJobManager">The bulk job manager.</param>
        /// <param name="logger">The logger.</param>
        public NotificationController(
            INotifyPublishCompleteCoordinator notifyPublishCompleteCoordinator,
            INotifyUploadCompleteCoordinator notifyUploadCompleteCoordinator,
            IBulkJobManager bulkJobManager,
            ILoggerAdapter<NotificationController> logger)
        {
            _notifyPublishCompleteCoordinator = notifyPublishCompleteCoordinator;
            _notifyUploadCompleteCoordinator = notifyUploadCompleteCoordinator;
            _bulkJobManager = bulkJobManager;
            _logger = logger;
        }

        /// <summary>
        /// Sends notifications after files have been published by the agency.
        /// </summary>
        /// <param name="parentBatchIdentifier">The parent batch identifier.</param>
        /// <returns>The action result.</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> AgencyPublishComplete([FromBody] string parentBatchIdentifier)
        {
            try
            {
                _logger.LogInformation($"Started action: {nameof(AgencyPublishComplete)} with parentBatchIdentifier: {parentBatchIdentifier}");

                var job = new Job<string, BatchNotificationSummary>
                {
                    Parameters = parentBatchIdentifier
                };

                var bulkJobId = await _bulkJobManager.CreateBulkJob(
                   new[] { job },
                   parentBatchId => _notifyPublishCompleteCoordinator.NotifyUsers(parentBatchIdentifier));

                _logger.LogInformation($"Job started: {nameof(AgencyPublishComplete)} with bulk job ID : {bulkJobId}");

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred in action: {nameof(AgencyPublishComplete)} for parentBatchIdentifier: {parentBatchIdentifier}");
                throw;
            }
        }

        /// <summary>
        /// Sends notifications after files have been uploaded by an organisation.
        /// </summary>
        /// <param name="parentBatchIdentifier">The parent batch identifier.</param>
        /// <returns>The action result.</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> OrganisationUploadComplete([FromBody] string parentBatchIdentifier)
        {
            try
            {
                _logger.LogInformation($"Started action: {nameof(OrganisationUploadComplete)} with parentBatchIdentifier: {parentBatchIdentifier}");

                await _notifyUploadCompleteCoordinator.NotifyUsers(parentBatchIdentifier);

                _logger.LogInformation($"Finished: {nameof(OrganisationUploadComplete)}");

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred in action: {nameof(OrganisationUploadComplete)} for parentBatchIdentifier: {parentBatchIdentifier}");
                throw;
            }
        }
    }
}