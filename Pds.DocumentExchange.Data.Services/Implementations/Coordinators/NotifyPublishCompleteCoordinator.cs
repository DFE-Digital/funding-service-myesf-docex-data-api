using Pds.Core.Common.Identity.Enums;
using Pds.Core.DfESignIn.Extensions;
using Pds.Core.DfESignIn.Interfaces;
using Pds.Core.DfESignIn.Models;
using Pds.Core.Logging;
using Pds.DocumentExchange.Data.Services.DTOs;
using Pds.DocumentExchange.Data.Services.DTOs.BatchAnalysis;
using Pds.DocumentExchange.Data.Services.DTOs.Configuration;
using Pds.DocumentExchange.Data.Services.DTOs.Notification;
using Pds.DocumentExchange.Data.Services.Interfaces;
using Pds.DocumentExchange.Data.Services.Interfaces.Builders;
using Pds.DocumentExchange.Data.Services.Interfaces.Coordinators;
using Pds.DocumentExchange.Data.Services.Interfaces.Factories;
using Pds.DocumentExchange.Data.Services.Interfaces.Settings;
using Pds.Services.Common.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Implementations.Coordinators
{
    /// <inheritdoc cref="INotifyPublishCompleteCoordinator"/>
    public sealed class NotifyPublishCompleteCoordinator : INotifyPublishCompleteCoordinator
    {
        /// <summary>
        /// The role assigned to external document exchange users.
        /// </summary>
        private readonly IBatchMetadataAnalysisFactory _analyser;

        private readonly INotificationMessageBuilder _messageBuilder;

        private readonly INotificationSummaryBuilder _summaryBuilder;

        private readonly NotificationConfiguration _configuration;

        private readonly IDfESignInPublicApi _dfeSignInPublicApi;

        private readonly IAdminSettingsService _adminSettingsService;

        private readonly INotifyEmailService _notifyEmailService;

        private readonly ILoggerAdapter<NotifyPublishCompleteCoordinator> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotifyPublishCompleteCoordinator"/> class.
        /// </summary>
        /// <param name="batchAnalysisFactory">The batch metadata analysis factory.</param>
        /// <param name="messageBuilder">The message builder.</param>
        /// <param name="notificationSummaryBuilder">The notification summary builder.</param>
        /// <param name="configuration">The configuration options.</param>
        /// <param name="dfeSignInPublicApi">The dfe signin public api service.</param>
        /// <param name="adminSettingsService">The admin settings service.</param>
        /// <param name="notifyEmailService">The notify email service.</param>
        /// <param name="logger">The logger.</param>
        public NotifyPublishCompleteCoordinator(
            IBatchMetadataAnalysisFactory batchAnalysisFactory,
            INotificationMessageBuilder messageBuilder,
            INotificationSummaryBuilder notificationSummaryBuilder,
            NotificationConfiguration configuration,
            IDfESignInPublicApi dfeSignInPublicApi,
            IAdminSettingsService adminSettingsService,
            INotifyEmailService notifyEmailService,
            ILoggerAdapter<NotifyPublishCompleteCoordinator> logger)
        {
            It.IsNull(batchAnalysisFactory)
                .AsGuard<ArgumentNullException>();

            It.IsNull(messageBuilder)
                .AsGuard<ArgumentNullException>();

            It.IsNull(notificationSummaryBuilder)
                .AsGuard<ArgumentNullException>();

            It.IsNull(dfeSignInPublicApi)
                .AsGuard<ArgumentNullException>();

            _analyser = batchAnalysisFactory;
            _messageBuilder = messageBuilder;
            _summaryBuilder = notificationSummaryBuilder;
            _configuration = configuration;
            _dfeSignInPublicApi = dfeSignInPublicApi;
            _adminSettingsService = adminSettingsService;
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

                _logger.LogInformation($"Notifying DocumentRecipients for parentBatchId: {parentBatchId}");
                var recipientSummaries = await NotifyDocumentRecipients(batchAnalysis);

                var summary = new BatchNotificationSummary
                {
                    ParentBatchIdentifier = parentBatchId,
                    SenderNotificationSummary = senderSummary,
                    RecipientNotificationSummaries = recipientSummaries
                };

                await _summaryBuilder.SaveSummary(summary, batchAnalysis);

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
            string subject;
            INotificationMessage message;

            // Build and 'send' the notification(s)
            // Potentially there could be two, one for infected and certainly one for clear.
            if (batchAnalysis.InfectedFiles.Any())
            {
                subject = GetSubjectFor(NotificationType.AgencyPublishInfected, batchAnalysis.InfectedFiles.Count);

                message = await _messageBuilder.GetInfectedFileMessage(
                    subject,
                    batchAnalysis,
                    batchAnalysis.IssuingEmail,
                    batchAnalysis.IssuingPerson);

                if (!string.IsNullOrEmpty(message.EmailMessageType))
                {
                    _logger.LogInformation($"Using Notify. NotifyDocumentSender for EmailMessageType:{message.EmailMessageType}, for parent batch id:{batchAnalysis.ParentBatchID}");
                    await _notifyEmailService.Push(message);
                }
            }

            if (batchAnalysis.ClearFiles.Any())
            {
                subject = GetSubjectFor(NotificationType.AgencyPublishClear, batchAnalysis.ClearFiles.Count);

                message = await _messageBuilder.GetCleanFileMessage(
                    subject,
                    batchAnalysis,
                    batchAnalysis.IssuingEmail,
                    batchAnalysis.IssuingPerson);

                if (!string.IsNullOrEmpty(message.EmailMessageType))
                {
                    _logger.LogInformation($"Using Notify. NotifyDocumentSender for EmailMessageType:{message.EmailMessageType}, for parent batch id:{batchAnalysis.ParentBatchID}");
                    await _notifyEmailService.Push(message);
                }
            }
        }

        /// <summary>
        /// Get subject for...
        /// </summary>
        /// <param name="notification">The notification.</param>
        /// <param name="documentCount">The document count.</param>
        /// <returns>The subject.</returns>
        internal string GetSubjectFor(NotificationType notification, int documentCount)
        {
            (documentCount < 1)
                .AsGuard<ArgumentOutOfRangeException>();

            if (notification == NotificationType.AgencyPublishInfected)
            {
                if (documentCount > 1)
                {
                    return $"You have an issue with {documentCount} documents you tried to publish.";
                }

                return "You have an issue with 1 document you tried to publish.";
            }

            if (documentCount > 1)
            {
                return $"You published {documentCount} documents";
            }

            return "You published 1 document";
        }

        /// <summary>
        /// Build send summary from...
        /// </summary>
        /// <param name="batchAnalysis">The batch analysis.</param>
        /// <returns>The summarised notification collection.</returns>
        internal async Task<INotificationSummary> GetSenderSummary(
            IBatchMetadataAnalysis batchAnalysis)
        {
            // prepare summaries...
            var notificationType = batchAnalysis.InfectedFiles.Any()
                ? NotificationType.AgencyPublishInfected
                : NotificationType.AgencyPublishClear;

            var notes =
                $"Uploaded by {batchAnalysis.IssuingEmail}. {batchAnalysis.InfectedFiles.Count} viruses. {batchAnalysis.ClearFiles.Count} okay";

            var summary = await _summaryBuilder.BuildSummary(
                notificationType,
                batchAnalysis.IssuingOrganisation,
                notes,
                batchAnalysis.IssuingEmail);

            return summary;
        }

        /// <summary>
        /// Notify the consumers using...
        /// </summary>
        /// <param name="batchAnalysis">The batch analysis.</param>
        /// <returns>The summarised notification collection.</returns>
        internal async Task<ICollection<INotificationSummary>> NotifyDocumentRecipients(
            IBatchMetadataAnalysis batchAnalysis)
        {
            // notify the document consumers
            var summaryList = Collection.Empty<INotificationSummary>();

            _logger.LogInformation($"Notifying {batchAnalysis.RecipientOrganisations.Count} DocumentRecipientOrganisations for parentBatchId: {batchAnalysis?.ParentBatchID}");

            foreach (var ukprn in batchAnalysis.RecipientOrganisations)
            {
                var summary = await NotifyRecipientOrganisation(ukprn, batchAnalysis);

                if (summary != null)
                {
                    summaryList.Add(summary);
                }
            }

            _logger.LogInformation($"Notified {summaryList.Count} DocumentRecipientOrganisations for parentBatchId: {batchAnalysis?.ParentBatchID}");

            return summaryList;
        }

        /// <summary>
        /// Notify the consumer.
        /// </summary>
        /// <param name="ukprn">The provider id.</param>
        /// <param name="batchAnalysis">The batch analysis.</param>
        /// <returns>A summary of the notification.</returns>
        internal async Task<INotificationSummary> NotifyRecipientOrganisation(int ukprn, IBatchMetadataAnalysis batchAnalysis)
        {
            try
            {
                var documents = batchAnalysis.ClearFiles
              .Where(document => MatchingTargetProvider(document, ukprn))
              .AsSafeReadOnlyList();

                var productGroups = documents
                    .GroupBy(GetProductIdentifier)
                    .Select(group => GetGroupingCountsFor(group, documents, batchAnalysis.GetProductFor))
                    .AsSafeReadOnlyList();

                var docExUsers = await _dfeSignInPublicApi.GetUserContactsForOrganisation(ukprn, new[] { UserRole.DocumentExchangeUser.ToString() });

                var emails = docExUsers.EmailAddressOf(UserRole.DocumentExchangeUser).AsSafeReadOnlyList();

                var emailNames = docExUsers.FullNameOf(UserRole.DocumentExchangeUser).AsSafeReadOnlyList();

                // Create a summary of the notification.
                var notes =
                    $"DocEx Users: {docExUsers.Users?.Count() ?? 0}.{SupplementaryNote(docExUsers.Users ?? Enumerable.Empty<UserContact>())} {documents.Count} okay files for provider.";

                var summary = await _summaryBuilder.BuildSummary(NotificationType.AgencyPublishClear, ukprn, notes, emails);

                // As there seems to be a risk of not having any...
                if (emails.Any() && await _adminSettingsService.IsAgencyPublishCompleteProviderNotificationEnabled())
                {
                    // Create the message details and push onto the queue
                    var subject = "New documents to view in document exchange";
                    var message = await _messageBuilder.GetDocumentUserMessage(subject, productGroups, emails, emailNames, batchAnalysis.ParentBatchID, ukprn);

                    if (!string.IsNullOrEmpty(message.EmailMessageType))
                    {
                        _logger.LogInformation($"Using Notify. NotifyRecipientOrganisation for EmailMessageType:{message.EmailMessageType}, for parent batch id:{batchAnalysis.ParentBatchID}");
                        await _notifyEmailService.Push(message);
                    }
                }

                return summary;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred in {nameof(NotifyRecipientOrganisation)} for the UKPRN: {ukprn} and parentBatchId: {batchAnalysis?.ParentBatchID}");
                return null;
            }
        }

        /// <summary>
        /// Supplementary note.
        /// </summary>
        /// <param name="users">The users.</param>
        /// <returns>The supplementary note.</returns>
        internal string SupplementaryNote(IEnumerable<UserContact> users) =>
            users.Any() ? string.Empty : " No provider to send to.";

        /// <summary>
        /// Matching the target provider (id).
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="ukprn">The provider id.</param>
        /// <returns>True, based on a valid match.</returns>
        internal bool MatchingTargetProvider(FileMetadata file, int ukprn) =>
            It.Has(file) && file.ToUkprn == ukprn;

        /// <summary>
        /// Get grouping by product identifier.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <returns>The product identifier.</returns>
        internal string GetProductIdentifier(FileMetadata file) =>
            file.ProductIdentifier;

        /// <summary>
        /// Get the grouping counts for...
        /// </summary>
        /// <param name="grouping">The grouping.</param>
        /// <param name="documents">The documents.</param>
        /// <param name="getProductFor">Gets the product for (the group key).</param>
        /// <returns>The grouped details.</returns>
        internal KeyValuePair<IBatchAnalysisProduct, int> GetGroupingCountsFor(
            IGrouping<string, FileMetadata> grouping,
            IReadOnlyCollection<FileMetadata> documents,
            Func<string, IBatchAnalysisProduct> getProductFor) =>
            new KeyValuePair<IBatchAnalysisProduct, int>(
                getProductFor(grouping.Key),
                documents.Count(document => document.ProductIdentifier == grouping.Key));
    }
}