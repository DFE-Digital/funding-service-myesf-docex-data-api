using Pds.Core.Utils;
using Pds.DocumentExchange.Data.Services.DTOs;
using Pds.DocumentExchange.Data.Services.DTOs.BatchAnalysis;
using Pds.DocumentExchange.Data.Services.DTOs.Notification;
using Pds.DocumentExchange.Data.Services.Interfaces;
using Pds.DocumentExchange.Data.Services.Interfaces.Builders;
using Pds.DocumentExchange.Data.Services.Interfaces.Settings;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Implementations.Builders
{
    /// <inheritdoc cref="INotificationSummaryBuilder"/>
    public sealed class NotificationSummaryBuilder : INotificationSummaryBuilder
    {
        private readonly ICosmosDbService _cosmosDbService;

        private readonly IConfigurationDataService _configurationDataService;

        private readonly ISystemProvider _systemProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationSummaryBuilder"/> class.
        /// </summary>
        /// <param name="cosmosDbService">The document store.</param>
        /// <param name="configurationDataService">The configuration data service.</param>
        /// <param name="systemProvider">The system provider.</param>
        public NotificationSummaryBuilder(
            ICosmosDbService cosmosDbService,
            IConfigurationDataService configurationDataService,
            ISystemProvider systemProvider)
        {
            _cosmosDbService = cosmosDbService
                ?? throw new ArgumentNullException(nameof(cosmosDbService));

            _configurationDataService = configurationDataService
                ?? throw new ArgumentNullException(nameof(configurationDataService));

            _systemProvider = systemProvider
                ?? throw new ArgumentNullException(nameof(systemProvider));
        }

        /// <inheritdoc/>
        public async Task<INotificationSummary> BuildSummary(NotificationType notificationType, int ukprn, string notes, string recipient)
        {
            var serviceNoReplyEmail = await _configurationDataService.GetServiceNoReplyEmail();

            return new NotificationSummary
            {
                NotificationType = notificationType,
                Ukprn = ukprn,
                From = new[] { serviceNoReplyEmail },
                To = new[] { recipient },
                Notes = notes
            };
        }

        /// <inheritdoc/>
        public async Task<INotificationSummary> BuildSummary(NotificationType notificationType, int ukprn, string notes, IReadOnlyCollection<string> recipients)
        {
            var serviceNoReplyEmail = await _configurationDataService.GetServiceNoReplyEmail();

            return new NotificationSummary
            {
                NotificationType = notificationType,
                Ukprn = ukprn,
                From = new[] { serviceNoReplyEmail },
                To = recipients,
                Notes = notes
            };
        }

        /// <inheritdoc/>
        public async Task SaveSummary(
            BatchNotificationSummary batchNotificationSummary,
            IBatchMetadataAnalysis batchAnalysis)
        {
            batchNotificationSummary.Id = _systemProvider.Guid.NewGuid().ToString();
            await _cosmosDbService.AddBatchNotificationSummary(batchNotificationSummary);

            foreach (var batch in batchAnalysis.Batches)
            {
                foreach (var file in batch.Files)
                {
                    file.History = new[]
                    {
                        new FileMetadataHistory
                        {
                            Action = Enums.FileAction.EmailSent,
                            ActionDateTimeUtc = _systemProvider.DateTime.UtcNow()
                        }
                    };

                    await _cosmosDbService.AddHistoryToFileMetadata(batch.Id, file);
                }
            }
        }
    }
}