namespace Pds.DocumentExchange.Data.Services.Interfaces
{
    /// <summary>
    /// Represents a service bus queue manager.
    /// </summary>
    public interface IServiceBusQueueManager
    {
        /// <summary>
        /// Retrives the queue by the queue name provided.
        /// </summary>
        /// <param name="queueName">The queue name.</param>
        /// <returns>An IServiceBusQueue.</returns>
        IJsonMessageServiceBusQueue GetQueue(string queueName);
    }
}