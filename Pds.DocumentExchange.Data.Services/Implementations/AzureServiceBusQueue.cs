using Microsoft.Azure.ServiceBus;
using Newtonsoft.Json;
using Pds.DocumentExchange.Data.Services.Interfaces;
using System.Text;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Implementations
{
    /// <summary>
    /// A class for managing an Azure Service Bus Queue.
    /// </summary>
    public class AzureServiceBusQueue : IJsonMessageServiceBusQueue
    {
        private readonly QueueClient _queueClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureServiceBusQueue"/> class.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="queueName">The queue name.</param>
        public AzureServiceBusQueue(string connectionString, string queueName)
            => _queueClient = new QueueClient(connectionString, queueName);

        /// <inheritdoc/>
        public string QueueName => _queueClient.Path;

        /// <inheritdoc/>
        public async Task PushMessage<T>(T message) =>
            await PushMessage(JsonConvert.SerializeObject(message));

        /// <inheritdoc/>
        public async Task PushMessage(string message)
        {
            var queueMessage = new Message(Encoding.UTF8.GetBytes(message));

            await _queueClient.SendAsync(queueMessage);
        }
    }
}