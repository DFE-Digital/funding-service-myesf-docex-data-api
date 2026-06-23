using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection;
using Pds.Core.Logging;
using Pds.DocumentExchange.Data.Repository.DTOs.Configuration;
using Pds.DocumentExchange.Data.Repository.Implementations;
using Pds.DocumentExchange.Data.Repository.Interfaces;

namespace Pds.DocumentExchange.Data.Repository.DependencyInjection
{
    /// <summary>
    /// Extensions class for <see cref="IServiceCollection"/> for registering the feature's services.
    /// </summary>
    public static class RepositoryServiceCollectionExtensions
    {
        /// <summary>Adds repositories for Document Exchange to the specified <see cref="IServiceCollection"/>.</summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the feature's services to.</param>
        /// <param name="azureCosmosDbConfig">The azure CosmosDb configuration.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public static IServiceCollection AddDocumentExchangeRepositories(
            this IServiceCollection services,
            AzureCosmosDbRepositoryConfiguration azureCosmosDbConfig)
        {
            services.AddSingleton<ICosmosClientFactory, CosmosClientFactory>();
            services.AddSingleton(CreateCosmosClient(azureCosmosDbConfig));
            services.AddSingleton(CreateCosmosClient(azureCosmosDbConfig, allowBulkExecution: true));

            services.AddSingleton<IDocumentsRepository>(serviceProvider =>
            {
                var cosmosClientFactory = serviceProvider.GetService<ICosmosClientFactory>();
                var logger = serviceProvider.GetService<ILoggerAdapter<AzureCosmosDbRepository>>();
                return new DocumentsRepository(cosmosClientFactory, azureCosmosDbConfig, logger);
            });

            services.AddSingleton<IFileRepository, FileRepository>();

            return services;
        }

        private static CosmosClient CreateCosmosClient(AzureCosmosDbRepositoryConfiguration config, bool allowBulkExecution = false)
            => new CosmosClient(
                config.ServiceEndpoint,
                config.AuthKeyOrResourceToken,
                new CosmosClientOptions
                {
                    AllowBulkExecution = allowBulkExecution
                });
    }
}