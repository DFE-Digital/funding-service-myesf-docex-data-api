using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Interfaces
{
    /// <summary>
    /// A representation of a service bus queue.
    /// </summary>
    public interface IServiceBusQueue
    {
        /// <summary>
        /// Pushes a message onto the queue.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns>An awaitable task.</returns>
        Task PushMessage(string message);
    }
}