using Pds.DocumentExchange.Data.Services.DTOs.Configuration;
using Pds.DocumentExchange.Data.Services.Extensions;
using Pds.DocumentExchange.Data.Services.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace Pds.DocumentExchange.Data.Services.Implementations
{
    /// <summary>
    /// Represents an Azure service queue manager.
    /// </summary>
    public class AzureServiceBusQueueManager : IServiceBusQueueManager
    {
        private readonly Dictionary<string, AzureServiceBusQueue> _queuesDictionary;

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureServiceBusQueueManager"/> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        public AzureServiceBusQueueManager(ServiceBusQueueManagerConfiguration configuration)
        {
            var queueNames = configuration.QueueNames.SplitAndTrim(',');

            _queuesDictionary = queueNames.ToDictionary(
                queueName => queueName,
                queueName => new AzureServiceBusQueue(configuration.ConnectionString, queueName));
        }

        /// <inheritdoc/>
        public IJsonMessageServiceBusQueue GetQueue(string queueName) => _queuesDictionary[queueName];
    }
}