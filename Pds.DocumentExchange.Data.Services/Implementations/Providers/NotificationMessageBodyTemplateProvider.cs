using Pds.DocumentExchange.Data.Services.DTOs.BatchAnalysis;
using Pds.DocumentExchange.Data.Services.Enums;
using Pds.DocumentExchange.Data.Services.Exceptions;
using Pds.DocumentExchange.Data.Services.Interfaces.Providers;
using Pds.Services.Common.Helpers;
using Pds.Services.Common.Interfaces.Providers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Implementations.Providers
{
    /// <summary>
    /// the notiification message body template provider.
    /// </summary>
    public sealed class NotificationMessageBodyTemplateProvider :
        IProvideNotificationMessageBodyTemplates
    {
        /// <summary>
        /// Gets the asset provider.
        /// </summary>
        public IProvideAssets Assets { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationMessageBodyTemplateProvider"/> class.
        /// </summary>
        /// <param name="assetProvider">the asset provider.</param>
        public NotificationMessageBodyTemplateProvider(IProvideAssets assetProvider)
        {
            It.IsNull(assetProvider)
                .AsGuard<ArgumentNullException>();

            Assets = assetProvider;
        }

        /// <inheritdoc/>
        public async Task<string> GetCleanFileTemplateFor(IBatchMetadataAnalysis batchAnalysis)
        {
            It.IsEmpty(batchAnalysis.ClearFiles)
                .AsGuard<MessageBodyTemplateProviderCardinalityException>(nameof(batchAnalysis.ClearFiles));

            if (batchAnalysis.IsInternal)
            {
                var templateName = batchAnalysis.ClearFiles.Count > 1
                    ? MessageBodyTemplateName.ESFAPublishedMultiple
                    : MessageBodyTemplateName.ESFAPublishedSingle;

                return await GetBodyFor(templateName);
            }

            return await GetBodyFor(MessageBodyTemplateName.ExternalUploadCompleted);
        }

        /// <inheritdoc/>
        public async Task<string> GetInfectedFileTemplateFor(IBatchMetadataAnalysis batchAnalysis)
        {
            It.IsEmpty(batchAnalysis.InfectedFiles)
                .AsGuard<MessageBodyTemplateProviderCardinalityException>(nameof(batchAnalysis.InfectedFiles));

            var templateName = batchAnalysis.IsInternal
                    ? MessageBodyTemplateName.ESFAPublicationInfected
                    : MessageBodyTemplateName.ExternalUploadInfected;

            return await GetBodyFor(templateName);
        }

        /// <inheritdoc/>
        public async Task<string> GetTeamNotificationTemplateFor(IBatchMetadataAnalysis batchAnalysis)
        {
            It.IsEmpty(batchAnalysis.ClearFiles)
                .AsGuard<MessageBodyTemplateProviderCardinalityException>(nameof(batchAnalysis.ClearFiles));

            var templateName = batchAnalysis.ClearFiles.Count > 1
                ? MessageBodyTemplateName.ESFAReceivedMultiple
                : MessageBodyTemplateName.ESFAReceivedSingle;

            return await GetBodyFor(templateName);
        }

        /// <inheritdoc/>
        public async Task<string> GetDocumentUserMessageTemplateFor(IReadOnlyCollection<KeyValuePair<IBatchAnalysisProduct, int>> productGroups)
        {
            It.IsEmpty(productGroups)
                .AsGuard<MessageBodyTemplateProviderCardinalityException>(nameof(productGroups));

            var templateName = MessageBodyTemplateName.ProviderReceivedMultipleTypes;

            if (productGroups.Count == 1)
            {
                templateName = MessageBodyTemplateName.ProviderReceivedSingleType;
            }

            if (productGroups.Count == 2)
            {
                templateName = MessageBodyTemplateName.ProviderReceivedTwoTypes;
            }

            return await GetBodyFor(templateName);
        }

        /// <summary>
        /// Gets the body for...
        /// </summary>
        /// <param name="templateName">The template name.</param>
        /// <returns>The message body text.</returns>
        internal async Task<string> GetBodyFor(MessageBodyTemplateName templateName) =>
            await Assets.GetTextAsset($"{templateName}.txt");
    }
}