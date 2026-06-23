namespace Pds.DocumentExchange.Data.Services.Interfaces.Providers
{
    /// <summary>
    /// i provide push notification queue configuration (contract).
    /// </summary>
    public interface IProvidePushNotificationQueueConfiguration
    {
        /// <summary>
        /// Gets the queue name.
        /// </summary>
        string QueueName { get; }

        /// <summary>
        /// Gets the connection string.
        /// </summary>
        string ConnectionString { get; }
    }
}