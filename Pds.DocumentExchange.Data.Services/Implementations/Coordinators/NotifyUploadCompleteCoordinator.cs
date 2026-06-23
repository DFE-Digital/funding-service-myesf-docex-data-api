using Pds.Core.Logging;
using Pds.DocumentExchange.Data.Services.DTOs.BatchAnalysis;
using Pds.DocumentExchange.Data.Services.DTOs.Notification;
using Pds.DocumentExchange.Data.Services.Interfaces;
using Pds.DocumentExchange.Data.Services.Interfaces.Builders;
using Pds.DocumentExchange.Data.Services.Interfaces.Coordinators;
using Pds.DocumentExchange.Data.Services.Interfaces.Factories;
using Pds.Services.Common.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Implementations.Coordinators
{
    /// <inheritdoc cref="INotifyUploadCompleteCoordinator"/>
    public sealed class NotifyUploadCompleteCoordinator : INotifyUploadCompleteCoordinator
    {
        private readonly IBatchMetadataAnalysisFactory _analyser;

        private readonly INotificationMessageBuilder _messageBuilder;

        private readonly INotificationSummaryBuilder _notificationSummaryBuilder;

        private readonly INotifyEmailService _notifyEmailService;

        private readonly ILoggerAdapter<NotifyUploadCompleteCoordinator> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotifyUploadCompleteCoordinator"/> class.
        /// </summary>
        /// <param name="batchAnalysisFactory">The batch metadata analysis factory.</param>
        /// <param name="messageBuilder">The message builder.</param>
        /// <param name="notificationSummaryBuilder">The notification summary builder.</param>
        /// <param name="notifyEmailService">The notify email service.</param>
        /// <param name="logger">The logger.</param>
        public NotifyUploadCompleteCoordinator(
            IBatchMetadataAnalysisFactory batchAnalysisFactory,
            INotificationMessageBuilder messageBuilder,
            INotificationSummaryBuilder notificationSummaryBuilder,
            INotifyEmailService notifyEmailService,
            ILoggerAdapter<NotifyUploadCompleteCoordinator> logger)
        {
            It.IsNull(batchAnalysisFactory)
                .AsGuard<ArgumentNullException>();
            It.IsNull(messageBuilder)
                .AsGuard<ArgumentNullException>();
            It.IsNull(notificationSummaryBuilder)
                .AsGuard<ArgumentNullException>();

            _analyser = batchAnalysisFactory;
            _messageBuilder = messageBuilder;
            _notificationSummaryBuilder = notificationSummaryBuilder;
            _notifyEmailService = notifyEmailService;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<BatchNotificationSummary> NotifyUsers(string parentBatchId)
        {
            try
            {
                _logger.LogInformation($"Started {nameof(NotifyUsers)} for parentBatchId: {parentBatchId}");

                It.IsNull(parentBatchId)
                .AsGuard<ArgumentNullException>(nameof(parentBatchId));

                It.IsEmpty(parentBatchId).AsGuard<ArgumentException>();

                var batchAnalysis = await _analyser.AnalyseBatch(parentBatchId);

                _logger.LogInformation($"Notifying DocumentSender for parentBatchId: {parentBatchId}");
                await NotifyDocumentSender(batchAnalysis);

                var senderSummary = await GetSenderSummary(batchAnalysis);

                _logger.LogInformation($"Notifying AgencyTeams for parentBatchId: {parentBatchId}");
                var recipientSummaries = await NotifyDocumentRecipients(batchAnalysis);

                var summary = new BatchNotificationSummary
                {
                    ParentBatchIdentifier = parentBatchId,
                    SenderNotificationSummary = senderSummary,
                    RecipientNotificationSummaries = recipientSummaries
                };

                await _notificationSummaryBuilder.SaveSummary(summary, batchAnalysis);

                _logger.LogInformation($"Finished {nameof(NotifyUsers)} for parentBatchId: {parentBatchId}");

                return summary;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred in {nameof(NotifyUsers)} for parentBatchId: {parentBatchId}");
                throw;
            }
        }

        /// <summary>
        /// Build and send notification using...
        /// </summary>
        /// <param name="batchAnalysis">The batch analysis.</param>
        /// <returns>The currently running task.</returns>
        internal async Task NotifyDocumentSender(IBatchMetadataAnalysis batchAnalysis)
        {
            var notificationType = batchAnalysis.InfectedFiles.Any()
                ? NotificationType.ProviderUploadInfected
                : NotificationType.ProviderUploadClear;

            var subject = (notificationType == NotificationType.ProviderUploadInfected)
                ? "There is a problem with your document"
                : "We've received your document";

            var message = (notificationType == NotificationType.ProviderUploadInfected)
                ? await _messageBuilder.GetInfectedFileMessage(subject, batchAnalysis, batchAnalysis.IssuingEmail, batchAnalysis.IssuingPerson)
                : await _messageBuilder.GetCleanFileMessage(subject, batchAnalysis, batchAnalysis.IssuingEmail, batchAnalysis.IssuingPerson);

            if (!string.IsNullOrEmpty(message.EmailMessageType))
            {
                _logger.LogInformation($"Using Notify. NotifyDocumentSender for EmailMessageType:{message.EmailMessageType}, notificationType:{notificationType}, for parent batch id:{batchAnalysis.ParentBatchID}");
                await _notifyEmailService.Push(message);
            }
        }

        /// <summary>
        /// Build send summary from...
        /// </summary>
        /// <param name="batchAnalysis">The batch analysis.</param>
        /// <returns>The summarised notification collection.</returns>
        internal async Task<INotificationSummary> GetSenderSummary(IBatchMetadataAnalysis batchAnalysis)
        {
            var notificationType = batchAnalysis.InfectedFiles.Any()
                ? NotificationType.ProviderUploadInfected
                : NotificationType.ProviderUploadClear;

            var notes = $"Uploaded by (mail address) {batchAnalysis.IssuingEmail}. {batchAnalysis.InfectedFiles.Count} viruses. {batchAnalysis.ClearFiles.Count} okay";
            var summary = await _notificationSummaryBuilder.BuildSummary(notificationType, batchAnalysis.IssuingOrganisation, notes, batchAnalysis.IssuingEmail);

            return summary;
        }

        /// <summary>
        /// Notify managing teams using...
        /// </summary>
        /// <param name="batchAnalysis">The batch analysis.</param>
        /// <returns>The summarised notification collection.</returns>
        internal async Task<ICollection<INotificationSummary>> NotifyDocumentRecipients(IBatchMetadataAnalysis batchAnalysis)
        {
            var summaryList = Collection.Empty<INotificationSummary>();

            _logger.LogInformation($"Notifying {batchAnalysis.AgencyTeams.Count} AgencyTeams for parentBatchId: {batchAnalysis?.ParentBatchID}");

            foreach (var agencyTeam in batchAnalysis.AgencyTeams)
            {
                var summary = await NotifyAgencyTeam(agencyTeam, batchAnalysis);

                if (summary != null)
                {
                    summaryList.Add(summary);
                }
            }

            _logger.LogInformation($"Notified {summaryList.Count} AgencyTeams for parentBatchId: {batchAnalysis?.ParentBatchID}");

            return summaryList;
        }

        /// <summary>
        /// Notify managing team.
        /// </summary>
        /// <param name="agencyTeam">The agency team.</param>
        /// <param name="batchAnalysis">The batch analysis.</param>
        /// <returns>A summary of the notification.</returns>
        internal async Task<INotificationSummary> NotifyAgencyTeam(IBatchAnalysisTeam agencyTeam, IBatchMetadataAnalysis batchAnalysis)
        {
            try
            {
                var notes = $"Team: {agencyTeam.Name}. {batchAnalysis.ClearFiles.Count} okay";
                var summary = await _notificationSummaryBuilder.BuildSummary(NotificationType.ProviderUploadReceived, batchAnalysis.IssuingOrganisation, notes, agencyTeam.EmailAddress);

                var subject = batchAnalysis.ClearFiles.Count > 1
                    ? "You have new documents to review"
                    : "You have a new document to review";
                var message = await _messageBuilder.GetAgencyTeamMessage(subject, batchAnalysis, agencyTeam.EmailAddress, agencyTeam.Name);

                if (!string.IsNullOrEmpty(message.EmailMessageType))
                {
                    _logger.LogInformation($"Using Notify. NotifyAgencyTeam for EmailMessageType:{message.EmailMessageType}, for parent batch id:{batchAnalysis.ParentBatchID}");
                    await _notifyEmailService.Push(message);
                }

                return summary;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred in {nameof(NotifyAgencyTeam)} for the AgencyTeam: {agencyTeam?.Name} and parentBatchId: {batchAnalysis?.ParentBatchID}");
                return null;
            }
        }
    }
}