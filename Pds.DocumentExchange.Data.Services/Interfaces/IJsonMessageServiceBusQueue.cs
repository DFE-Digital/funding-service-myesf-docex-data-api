using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Interfaces
{
    /// <summary>
    /// A representation of a json message service bus queue.
    /// </summary>
    public interface IJsonMessageServiceBusQueue :
        IServiceBusQueue
    {
        /// <summary>
        /// Gets the queue name.
        /// </summary>
        string QueueName { get; }

        /// <summary>
        /// Pushes a message on to the queue.
        /// </summary>
        /// <typeparam name="T">The message type.</typeparam>
        /// <param name="message">The message.</param>
        /// <returns>An awaitable task.</returns>
        Task PushMessage<T>(T message);
    }
}