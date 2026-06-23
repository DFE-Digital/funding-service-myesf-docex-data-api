using Pds.DocumentExchange.Data.Services.DTOs.BatchAnalysis;
using Pds.DocumentExchange.Data.Services.DTOs.Notification;
using Pds.DocumentExchange.Data.Services.Enums;
using Pds.DocumentExchange.Data.Services.Interfaces.Builders;
using Pds.DocumentExchange.Data.Services.Interfaces.Factories;
using Pds.DocumentExchange.Data.Services.Interfaces.Providers;
using Pds.DocumentExchange.Data.Services.Interfaces.Settings;
using Pds.Services.Common.Helpers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Implementations.Builders
{
    /// <summary>
    /// The notification message builder (implementation).
    /// </summary>
    public sealed class NotificationMessageBuilder :
        INotificationMessageBuilder
    {
        /// <summary>
        /// Gets the notification message body template provider.
        /// </summary>
        internal IProvideNotificationMessageBodyTemplates TemplateProvider { get; }

        /// <summary>
        /// Gets the message body (factory).
        /// </summary>
        internal ICreateNotificationMessageBodies MessageBody { get; }

        /// <summary>
        /// Gets the message header (factory).
        /// </summary>
        internal ICreateNotificationMessageHeaders MessageHeader { get; }

        /// <summary>
        /// Gets the notification message factory.
        /// </summary>
        internal ICreateNotificationMessages Message { get; }

        /// <summary>
        /// Gets the configuration data service.
        /// </summary>
        internal IConfigurationDataService ConfigurationDataService { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationMessageBuilder"/> class.
        /// </summary>
        /// <param name="templateProvider">The message body template provider.</param>
        /// <param name="messageHeaderFactory">The message header factory.</param>
        /// <param name="messageBodyFactory">The message body factory.</param>
        /// <param name="messageFactory">The message factory.</param>
        /// <param name="configurationDataService">The configuration data service.</param>
        /// <param name="agencyServiceConfigurationProvider">The agency configuration setting provider.</param>
        public NotificationMessageBuilder(
            IProvideNotificationMessageBodyTemplates templateProvider,
            ICreateNotificationMessageHeaders messageHeaderFactory,
            ICreateNotificationMessageBodies messageBodyFactory,
            ICreateNotificationMessages messageFactory,
            IConfigurationDataService configurationDataService)
        {
            It.IsNull(messageHeaderFactory)
                .AsGuard<ArgumentNullException>();
            It.IsNull(templateProvider)
                .AsGuard<ArgumentNullException>();
            It.IsNull(messageBodyFactory)
                .AsGuard<ArgumentNullException>();
            It.IsNull(messageFactory)
                .AsGuard<ArgumentNullException>();
            It.IsNull(configurationDataService)
               .AsGuard<ArgumentNullException>();

            MessageHeader = messageHeaderFactory;
            TemplateProvider = templateProvider;
            MessageBody = messageBodyFactory;
            Message = messageFactory;
            ConfigurationDataService = configurationDataService;
        }

        /// <inheritdoc/>
        public async Task<INotificationMessage> GetInfectedFileMessage(
            string subject,
            IBatchMetadataAnalysis batchAnalysis,
            string recipientEmail,
            string userName)
        {
            var serviceNoReplyEmail = await ConfigurationDataService.GetServiceNoReplyEmail();
            var header = await MessageHeader.Create(subject, serviceNoReplyEmail, recipientEmail, userName);

            var emailMessageType = GetInfectedFileEmailMessageTypeFor(batchAnalysis);
            Dictionary<string, dynamic> emailPersonalisation = null;

            string body = string.Empty;

            if (string.IsNullOrEmpty(emailMessageType))
            {
                var bodyTemplate = await TemplateProvider.GetInfectedFileTemplateFor(batchAnalysis);
                body = await MessageBody.BuildMessageContentFrom(bodyTemplate, batchAnalysis);
            }
            else
            {
                emailPersonalisation = await MessageBody.BuildEmailPersonalisationFrom(batchAnalysis, true, userName);
            }

            return await Message.Create(header, body, batchAnalysis.IsInternal, batchAnalysis.ParentBatchID, batchAnalysis.IssuingOrganisation, emailMessageType, emailPersonalisation); // <= We use the standard template if notifying an agency team.
        }

        /// <inheritdoc/>
        public async Task<INotificationMessage> GetCleanFileMessage(
            string subject,
            IBatchMetadataAnalysis batchAnalysis,
            string recipientEmail,
            string userName)
        {
            var serviceNoReplyEmail = await ConfigurationDataService.GetServiceNoReplyEmail();
            var header = await MessageHeader.Create(subject, serviceNoReplyEmail, recipientEmail, userName);

            var emailMessageType = GetCleanFileEmailMessageTypeFor(batchAnalysis);
            Dictionary<string, dynamic> emailPersonalisation = null;

            string body = string.Empty;

            if (string.IsNullOrEmpty(emailMessageType))
            {
                var bodyTemplate = await TemplateProvider.GetCleanFileTemplateFor(batchAnalysis);
                body = await MessageBody.BuildMessageContentFrom(bodyTemplate, batchAnalysis);
            }
            else
            {
                emailPersonalisation = await MessageBody.BuildEmailPersonalisationFrom(batchAnalysis, false, userName);
            }

            return await Message.Create(header, body, batchAnalysis.IsInternal, batchAnalysis.ParentBatchID, batchAnalysis.IssuingOrganisation, emailMessageType, emailPersonalisation); // <= We use the standard template if notifying an agency team.
        }

        /// <inheritdoc/>
        public async Task<INotificationMessage> GetAgencyTeamMessage(
            string subject,
            IBatchMetadataAnalysis batchAnalysis,
            string teamEmail,
            string teamName)
        {
            var serviceNoReplyEmail = await ConfigurationDataService.GetServiceNoReplyEmail();
            var header = await MessageHeader.Create(subject, serviceNoReplyEmail, teamEmail, teamName);

            var emailMessageType = GetAgencyTeamEmailMessageTypeFor(batchAnalysis);
            Dictionary<string, dynamic> emailPersonalisation = null;

            string body = string.Empty;
            if (string.IsNullOrEmpty(emailMessageType))
            {
                var bodyTemplate = await TemplateProvider.GetTeamNotificationTemplateFor(batchAnalysis);
                body = await MessageBody.BuildMessageContentFrom(bodyTemplate, batchAnalysis);
            }
            else
            {
                emailPersonalisation = await MessageBody.BuildEmailPersonalisationFrom(batchAnalysis, false, teamName);
            }

            return await Message.Create(header, body, true, batchAnalysis.ParentBatchID, batchAnalysis.IssuingOrganisation, emailMessageType, emailPersonalisation); // <= We use the standard template when notifying an agency team.
        }

        /// <inheritdoc/>
        public async Task<INotificationMessage> GetDocumentUserMessage(
            string subject,
            IReadOnlyCollection<KeyValuePair<IBatchAnalysisProduct, int>> productGroups,
            IReadOnlyCollection<string> recipientEmails,
            IReadOnlyCollection<string> emailNames,
            string parentBatchId,
            int ukprn)
        {
            var serviceNoReplyEmail = await ConfigurationDataService.GetServiceNoReplyEmail();
            var header = await MessageHeader.Create(subject, serviceNoReplyEmail, recipientEmails, emailNames);

            var emailMessageType = GetDocumentUserEmailMessageTypeFor(productGroups);
            Dictionary<string, dynamic> emailPersonalisation = null;

            string body = string.Empty;

            if (string.IsNullOrEmpty(emailMessageType))
            {
                var bodyTemplate = await TemplateProvider.GetDocumentUserMessageTemplateFor(productGroups);
                body = await MessageBody.BuildMessageContentFrom(bodyTemplate, productGroups);
            }
            else
            {
                emailPersonalisation = await MessageBody.BuildEmailPersonalisationFrom(productGroups);
            }

            return await Message.Create(header, body, false, parentBatchId, ukprn, emailMessageType, emailPersonalisation); // <= We don't use the standard template when notifiying out.
        }

        private string GetInfectedFileEmailMessageTypeFor(IBatchMetadataAnalysis batchAnalysis)
        {
            It.IsEmpty(batchAnalysis.InfectedFiles)
                .AsGuard<ArgumentNullException>(nameof(batchAnalysis.InfectedFiles));

            var templateName = Convert.ToString(batchAnalysis.IsInternal
                    ? MessageBodyTemplateName.ESFAPublicationInfected
                    : MessageBodyTemplateName.ExternalUploadInfected);

            return templateName;
        }

        private string GetCleanFileEmailMessageTypeFor(IBatchMetadataAnalysis batchAnalysis)
        {
            It.IsEmpty(batchAnalysis.ClearFiles)
                .AsGuard<ArgumentNullException>(nameof(batchAnalysis.ClearFiles));

            string templateName = Convert.ToString(MessageBodyTemplateName.ExternalUploadCompleted);

            if (batchAnalysis.IsInternal)
            {
                templateName = Convert.ToString(batchAnalysis.ClearFiles.Count > 1
                   ? MessageBodyTemplateName.ESFAPublishedMultiple
                   : MessageBodyTemplateName.ESFAPublishedSingle);
            }

            return templateName;
        }

        private string GetAgencyTeamEmailMessageTypeFor(IBatchMetadataAnalysis batchAnalysis)
        {
            It.IsEmpty(batchAnalysis.ClearFiles)
               .AsGuard<ArgumentNullException>(nameof(batchAnalysis.ClearFiles));

            string templateName = Convert.ToString(batchAnalysis.ClearFiles.Count > 1
                ? MessageBodyTemplateName.ESFAReceivedMultiple
                : MessageBodyTemplateName.ESFAReceivedSingle);

            return templateName;
        }

        private string GetDocumentUserEmailMessageTypeFor(IReadOnlyCollection<KeyValuePair<IBatchAnalysisProduct, int>> productGroups)
        {
            It.IsEmpty(productGroups)
                   .AsGuard<ArgumentNullException>(nameof(productGroups));

            var templateName = Convert.ToString(MessageBodyTemplateName.ProviderReceivedMultipleTypes);

            if (productGroups.Count == 1)
            {
                templateName = Convert.ToString(MessageBodyTemplateName.ProviderReceivedSingleType);
            }

            if (productGroups.Count == 2)
            {
                templateName = Convert.ToString(MessageBodyTemplateName.ProviderReceivedTwoTypes);
            }

            return templateName;
        }
    }
}
