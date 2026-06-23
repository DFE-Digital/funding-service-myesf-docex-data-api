using Pds.DocumentExchange.Data.Services.Interfaces.Providers;

namespace Pds.DocumentExchange.Data.Services.Implementations.Providers
{
    /// <summary>
    /// The push notification configuration provider (implementation).
    /// </summary>
    public sealed class PushNotificationQueueConfigurationProvider :
        IProvidePushNotificationQueueConfiguration
    {
        /// <summary>
        /// Gets or sets the queue name.
        /// </summary>
        public string QueueName { get; set; }

        /// <summary>
        /// Gets or sets connection string.
        /// </summary>
        public string ConnectionString { get; set;  }
    }
}