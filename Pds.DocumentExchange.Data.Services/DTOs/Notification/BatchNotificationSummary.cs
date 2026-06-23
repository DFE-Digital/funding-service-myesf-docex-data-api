using Newtonsoft.Json;
using Pds.DocumentExchange.Data.Repository.DTOs;
using Pds.DocumentExchange.Data.Services.JsonConverters;
using System.Collections.Generic;

namespace Pds.DocumentExchange.Data.Services.DTOs.Notification
{
    /// <summary>
    /// Represents the notification summary data for a batch.
    /// </summary>
    public class BatchNotificationSummary : CosmosDbDocument
    {
        /// <summary>
        /// Gets or sets the cosmos document type.
        /// </summary>
        [JsonProperty(PropertyName = "documentType")]
        public string DocumentType { get; set; } = nameof(BatchNotificationSummary);

        /// <summary>
        /// Gets or sets the parent batch identifier.
        /// </summary>
        public string ParentBatchIdentifier { get; set; }

        /// <summary>
        /// Gets or sets the summary of the notification sent to the document sender.
        /// </summary>
        [JsonConverter(typeof(ConcreteConverter<NotificationSummary>))]
        public INotificationSummary SenderNotificationSummary { get; set; }

        /// <summary>
        /// Gets or sets the summaries of the notifications sent to the document recipients.
        /// </summary>
        [JsonConverter(typeof(ConcreteConverter<IEnumerable<NotificationSummary>>))]
        public IEnumerable<INotificationSummary> RecipientNotificationSummaries { get; set; }
    }
}