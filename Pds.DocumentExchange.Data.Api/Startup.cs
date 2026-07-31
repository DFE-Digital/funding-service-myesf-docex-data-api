using Mapster;
using MapsterMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Pds.Core.ApiAuthentication;
using Pds.Core.BulkJobs;
using Pds.Core.BulkJobs.Redis;
using Pds.Core.Caching;
using Pds.Core.Caching.Models;
using Pds.Core.DistributedLocks;
using Pds.Core.Logging;
using Pds.Core.Notification.Registration;
using Pds.Core.Telemetry.ApplicationInsights;
using Pds.Core.Utils;
using Pds.DocumentExchange.Data.Api.Extensions;
using Pds.DocumentExchange.Data.Api.Mapster;
using Pds.DocumentExchange.Data.Api.MvcConfiguration;
using Pds.DocumentExchange.Data.Api.Validations;
using Pds.DocumentExchange.Data.Repository.DependencyInjection;
using Pds.DocumentExchange.Data.Repository.DTOs.Configuration;
using Pds.DocumentExchange.Data.Services.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Api
{
    /// <summary>
    /// The startup class.
    /// </summary>
    public class Startup
    {
        private const string CurrentApiVersion = "v1.0.0";

        /// <summary>
        /// Gets the application configuration.
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// Gets the environment.
        /// </summary>
        public IWebHostEnvironment Environment { get; }

        /// <summary>
        /// The assembly name (field).
        /// </summary>
        private string _assemblyName;

        /// <summary>
        /// Gets the assembly name.
        /// </summary>
        public string AssemblyName =>
            _assemblyName ??= GetType().Assembly.GetName().Name;

        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        /// <param name="configuration">The application configuration.</param>
        /// <param name="environment">The hosting environment the application is running in.</param>
        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            Configuration = configuration;
            Environment = environment;
        }

        /// <summary>
        /// Configures the services for the container.
        /// </summary>
        /// <param name="services">The service collection.</param>
        public void ConfigureServices(IServiceCollection services)
        {
            var azureCosmosDbConfig = Configuration.LoadSection<AzureCosmosDbRepositoryConfiguration>("AzureCosmosDb");
            Action<RedisConfiguration> bindRedisConfig = c => Configuration.Bind("Cache:Redis", c);

            services
                .AddHttpClient()
                .AddRedisAndMemoryCache(bindRedisConfig)
                .AddNotificationClient(Configuration)
                .AddCacheConfiguration(Configuration)
                .AddApiControllers()
                .AddDocumentExchangeServiceConfiguration(Configuration)
                .AddAzureADAuthentication(Configuration)
                .AddDocumentExchangeRepositories(azureCosmosDbConfig)

                .AddSingleton(new TypeAdapterConfig().Configure())
                .AddSingleton<IMapper, ServiceMapper>()
                .AddDocumentExchangeServices(Configuration)
                .AddValidations()
                .AddLoggerAdapter()
                .AddPdsApplicationInsightsTelemetry(BuildAppInsightsConfiguration)
                .AddPdsUtils()
                .AddDistributedLocksService(bindRedisConfig)
                .AddBulkJobManager(
                    s =>
                        s.AddRedisBulkJobStorage(
                            options => options.RedisConnectionString = Configuration.GetValue<string>("Cache:Redis:ConnectionString")))
                .AddHealthChecks();

            services
                .AddControllers()
                .AddNewtonsoftJson();

            if (Environment.IsDevelopment())
            {
                // Register the Swagger generator, defining 1 or more Swagger documents
                services
                    .DisableAuthentication(AssemblyName)
                    .AddSwaggerGen(c =>
                    {
                        c.SwaggerDoc(CurrentApiVersion, new OpenApiInfo
                        {
                            Title = AssemblyName,
                            Version = CurrentApiVersion
                        });

                        c.CustomSchemaIds(x => x.FullName);
                    });
            }

            Task cosmosDbTask = new Task(async () => await InitializeCosmosClientInstanceAsync(azureCosmosDbConfig));
            cosmosDbTask.RunSynchronously();
        }

        /// <summary>
        /// Configures the HTTP request pipeline.
        /// </summary>
        /// <param name="app">The application builder.</param>
        /// <param name="env">The web hosting environment.</param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();

                // Enable middleware to serve generated Swagger as a JSON endpoint.
                app.UseSwagger();

                // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
                // specifying the Swagger JSON endpoint.
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint($"/swagger/{CurrentApiVersion}/swagger.json", AssemblyName);
                });
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHealthChecks("/ping");
            });
        }

        private void BuildAppInsightsConfiguration(PdsApplicationInsightsConfiguration options)
        {
            Configuration.Bind("PdsApplicationInsights", options);
            options.Component = AssemblyName;
        }

        private async Task InitializeCosmosClientInstanceAsync(AzureCosmosDbRepositoryConfiguration azureCosmosDbConfig)
        {
            var clientBuilder = new CosmosClientBuilder(azureCosmosDbConfig.ServiceEndpoint, azureCosmosDbConfig.AuthKeyOrResourceToken);

            var client = clientBuilder
                .WithConnectionModeDirect()
                .Build();

            var databaseResponse = await client.CreateDatabaseIfNotExistsAsync(azureCosmosDbConfig.DatabaseName);

            await CreateContainerIfNotExistsAsync(databaseResponse.Database, azureCosmosDbConfig.CollectionName);
        }

        private Task CreateContainerIfNotExistsAsync(Database database, string collectionName)
             => database.CreateContainerIfNotExistsAsync(collectionName, "/partitionKey");
    }
}