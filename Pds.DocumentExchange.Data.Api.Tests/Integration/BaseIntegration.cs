using Mapster;
using MapsterMapper;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Pds.Core.BulkJobs.Interfaces;
using Pds.Core.Caching.Interfaces;
using Pds.Core.Caching.Models;
using Pds.Core.Caching.Services;
using Pds.Core.DistributedLocks.Services;
using Pds.Core.Logging;
using Pds.Core.Utils;
using Pds.Core.Utils.Interfaces;
using Pds.DocumentExchange.Data.Api.Mapster;
using Pds.DocumentExchange.Data.Api.Mapster.Converters;
using Pds.DocumentExchange.Data.Api.Validations;
using Pds.DocumentExchange.Data.Populator.Scenarios;
using Pds.DocumentExchange.Data.Populator.Storage;
using Pds.DocumentExchange.Data.Repository.DTOs.Configuration;
using Pds.DocumentExchange.Data.Repository.Implementations;
using Pds.DocumentExchange.Data.Repository.Interfaces;
using Pds.DocumentExchange.Data.Services.DTOs.Configuration;
using Pds.DocumentExchange.Data.Services.Implementations;
using Pds.DocumentExchange.Data.Services.Implementations.Caching;
using Pds.DocumentExchange.Data.Services.Implementations.CosmosDb;
using Pds.DocumentExchange.Data.Services.Implementations.Filters;
using Pds.DocumentExchange.Data.Services.Implementations.Lookup;
using Pds.DocumentExchange.Data.Services.Implementations.Settings;
using Pds.DocumentExchange.Data.Services.Implementations.Storage;
using Pds.DocumentExchange.Data.Services.Implementations.VirusScan;
using Pds.DocumentExchange.Data.Services.Interfaces;
using Pds.DocumentExchange.Data.Services.Interfaces.Caching;
using Pds.DocumentExchange.Data.Services.Interfaces.FDS;
using Pds.DocumentExchange.Data.Services.Interfaces.Filters;
using Pds.DocumentExchange.Data.Services.Interfaces.Lookup;
using Pds.DocumentExchange.Data.Services.Interfaces.Storage;
using System;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Api.Tests.Integration
{
    public abstract class BaseIntegration
    {
        protected const string Team1 = "DocumentExchangeAdministratorFundingCentre";
        protected const string Team2 = "DocumentExchangeAdministratorRiskAssurance";

        /// <summary>
        /// Builds the configuration hierarchy.
        /// This optionally adds the appsettings.json file if present (for local development use)
        /// and also adds the environment variables (for the hosted environments).
        /// </summary>
        /// <returns>The root of the configuration hierarchy.</returns>
        private static IConfigurationRoot BuildConfiguration()
            => new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true)
                .AddEnvironmentVariables()
                .Build();

        private readonly IDateTimeProvider _dateTimeProvider = Mock.Of<IDateTimeProvider>(MockBehavior.Strict);

        private readonly IGuidProvider _guidProvider = Mock.Of<IGuidProvider>(MockBehavior.Strict);

        private readonly IOrganisationService _organisationService
            = Mock.Of<IOrganisationService>();

        protected IConfigurationRoot Configuration { get; }

        protected IConfigurationRoot MockConfig { get; } = Mock.Of<IConfigurationRoot>(MockBehavior.Strict);

        protected IDateTimeProvider DateTimeProvider { get => _dateTimeProvider; }

        protected IMemoryCacheWrapper MockMemoryCache { get; } = Mock.Of<IMemoryCacheWrapper>(MockBehavior.Strict);

        protected IDistributedCacheWrapper MockDistributedCache { get; } = Mock.Of<IDistributedCacheWrapper>(MockBehavior.Strict);

        protected IOrganisationService OrganisationService { get; } = Mock.Of<IOrganisationService>();

        protected IOrganisationsLookup OrganisationsLookup { get; } = Mock.Of<IOrganisationsLookup>(MockBehavior.Strict);

        protected IServiceBusQueueManager QueueManager { get; } = Mock.Of<IServiceBusQueueManager>(MockBehavior.Strict);

        protected ISystemProvider SystemProvider { get; } = Mock.Of<ISystemProvider>(MockBehavior.Strict);

        protected IAntivirus MockAntivirus { get; } = Mock.Of<IAntivirus>(MockBehavior.Strict);

        protected IMapper Mapper { get; } = new Mapper(new TypeAdapterConfig().Configure());

        protected DocumentsPublishedByAgencyScenario DocumentsPublishedByAgencyPopulator { get; } = new DocumentsPublishedByAgencyScenario();

        protected DocumentsUploadedFromExternalScenario DocumentsUploadedByExternalPopulator { get; } = new DocumentsUploadedFromExternalScenario();

        protected BaseIntegration()
        {
            Configuration = BuildConfiguration();
        }

        protected ILoggerAdapter<T> CreateMockLoggerAdapter<T>()
            => Mock.Of<ILoggerAdapter<T>>(MockBehavior.Loose);

        protected IBulkJobManager CreateMockBulkJobManager()
            => Mock.Of<IBulkJobManager>(MockBehavior.Strict);

        protected ICosmosDbService GetCosmosDbService()
        {
            var fileRepository = new FileRepository();
            var mockLogger = CreateMockLoggerAdapter<CosmosDbServiceBase>();
            var configuration = new AzureCosmosDbRepositoryConfiguration
            {
                ServiceEndpoint = MockConfig["CosmosDb:ServiceEndpoint"],
                AuthKeyOrResourceToken = MockConfig["CosmosDb:AuthKeyOrResourceToken"]
            };

            ICosmosClientFactory cosmosClientFactory = GetCosmosClientFactory();

            var documentsRepository = new DocumentsRepository(
                cosmosClientFactory,
                configuration,
                CreateMockLoggerAdapter<AzureCosmosDbRepository>());

            return new CosmosDbService(
                documentsRepository,
                fileRepository,
                mockLogger);
        }

        protected ICacheManager GetCacheManager()
        {
            var cacheService = GetCacheService();
            var cacheKeyBuilder = new CacheKeyBuilder();
            var cacheConfiguration = new CacheConfiguration();
            var cacheOptionsProvider = new CacheOptionsProvider(cacheConfiguration);

            return new CacheManager(
                cacheService,
                cacheKeyBuilder,
                cacheOptionsProvider);
        }

        protected CacheService GetCacheService()
        {
            return new CacheService(
                MockMemoryCache,
                MockDistributedCache,
                CreateMockLoggerAdapter<CacheService>());
        }

        protected IOrganisationsLookup GetOrganisationsLookup()
        {
            return new OrganisationsLookup(
                OrganisationService,
                CreateMockLoggerAdapter<OrganisationsLookup>(),
                GetCacheManager());
        }

        protected IFileMetadataUserEncryptor GetFileMetadataUserEncryptor()
        {
            return new FileMetadataUserEncryptor(
                new EncryptionService(),
                GetCosmosDbConfiguration());
        }

        protected CosmosDbConfiguration GetCosmosDbConfiguration()
        {
            return new CosmosDbConfiguration
            {
                DataEncryptionKey = MockConfig["DocumentExchangeServices:CosmosDb:DataEncryptionKey"]
            };
        }

        protected IFiltersExecutionManager<TDocument> GetFiltersManager<TDocument>(IFiltersFactory<TDocument> filtersFactory)
        {
            var dateRangeFilterService = new DateRangeFilterService<TDocument>();
            var listFilterService = new ListFilterService<TDocument>();
            var radioFilterService = new RadioFilterService<TDocument>(Mapper);
            var textBoxFilterService = new TextBoxFilterService<TDocument>();

            return new FiltersExecutionManager<TDocument>(
                filtersFactory,
                dateRangeFilterService,
                listFilterService,
                radioFilterService,
                textBoxFilterService);
        }

        protected IVirusScanResultProcessor GetVirusScanResultProcessor()
        {
            var cosmosDbService = GetCosmosDbService();
            var directoriesManager = GetDirectoriesManager();
            var cacheManager = GetCacheManager();
            var distributedLocksService = new DistributedLocksService(MockDistributedCache);
            var configuration = new VirusScanResultProcessorConfiguration();

            return new VirusScanResultProcessor(
                cosmosDbService,
                directoriesManager,
                QueueManager,
                SystemProvider,
                cacheManager,
                distributedLocksService,
                configuration,
                CreateMockLoggerAdapter<VirusScanResultProcessor>());
        }

        protected IDirectoriesManager GetDirectoriesManager()
        {
            var fileNameProvider = new FileNameProvider();
            var cosmosDbService = GetCosmosDbService();
            var cacheManager = GetCacheManager();
            var configurationDataService = new ConfigurationDataService(cosmosDbService, cacheManager, CreateMockLoggerAdapter<ConfigurationDataService>());

            return new DirectoriesManager(
                fileNameProvider,
                configurationDataService,
                GetDirectoriesManagerConfiguration(),
                CreateMockLoggerAdapter<Services.Implementations.Storage.AzureBlobContainer>(),
                CreateMockLoggerAdapter<Services.Implementations.Storage.AzureFileShareDirectory>());
        }

        protected DirectoriesManagerConfiguration GetDirectoriesManagerConfiguration()
        {
            return new DirectoriesManagerConfiguration
            {
                BlobContainers = new AzureBlobContainerConfiguration
                {
                    ConnectionString = MockConfig["BlobContainers:ConnectionString"],
                    Containers = MockConfig["BlobContainers:ContainerName"]
                },

                TeamsFileShareConnectionString = MockConfig["TeamsFileShareConnectionString"],
                FileShareDirectories = new AzureFileShareDirectoriesConfiguration
                {
                    ConnectionString = MockConfig["ExternalFileShareConnectionString"],
                    FileShareDirectoryPairs = @"unscanned,
                                            unscanned-processing,
                                            unscanned-processed"
                }
            };
        }

        protected IDocumentDownloader GetDocumentDownloader()
        {
            var cosmosDbService = GetCosmosDbService();
            var directoriesManager = GetDirectoriesManager();
            var fileMetadataUserEncryptor = GetFileMetadataUserEncryptor();
            var configuration = new DocumentDownloaderConfiguration()
            {
                DownloadDirectoryKey = MockConfig["BlobContainers:ContainerName"]
            };
            var zipService = new ZipService();

            return new DocumentDownloader(
                cosmosDbService,
                directoriesManager,
                fileMetadataUserEncryptor,
                SystemProvider,
                Mapper,
                zipService,
                configuration,
                CreateMockLoggerAdapter<DocumentDownloader>());
        }

        protected IValidationService GetValidationService()
        {
            var cosmosDbService = GetCosmosDbService();
            var cacheManager = GetCacheManager();
            var configurationDataService = new ConfigurationDataService(cosmosDbService, cacheManager, CreateMockLoggerAdapter<ConfigurationDataService>());

            return new ValidationService(
                new ValidatorFactory(CreateServiceProvider()),
                new TeamsLookup(configurationDataService),
                new ProductsLookup(configurationDataService));
        }

        protected void SetUpConfig()
        {
            Mock.Get(MockConfig)
                .Setup(x => x["CosmosDb:ServiceEndpoint"])
                .Returns(Configuration["CosmosDb:ServiceEndpoint"]);

            Mock.Get(MockConfig)
                .Setup(x => x["CosmosDb:AuthKeyOrResourceToken"])
                .Returns(Configuration["CosmosDb:AuthKeyOrResourceToken"]);

            Mock.Get(MockConfig)
                .Setup(x => x["DocumentExchangeServices:CosmosDb:DataEncryptionKey"])
                .Returns(Configuration["DocumentExchangeServices:CosmosDb:DataEncryptionKey"]);

            Mock.Get(MockConfig)
                .Setup(x => x["BlobContainers:ConnectionString"])
                .Returns(Configuration["BlobContainers:ConnectionString"]);

            Mock.Get(MockConfig)
                .Setup(x => x["BlobContainers:ContainerName"])
                .Returns(Configuration["BlobContainers:ContainerName"]);

            Mock.Get(MockConfig)
                .Setup(x => x["TeamsFileShareConnectionString"])
                .Returns(Configuration["TeamsFileShareConnectionString"]);

            Mock.Get(MockConfig)
                .Setup(x => x["ExternalFileShareConnectionString"])
                .Returns(Configuration["ExternalFileShareConnectionString"]);

            Mock.Get(MockConfig)
                .Setup(x => x["ServiceBusConnectionString"])
                .Returns(Configuration["ServiceBusConnectionString"]);
        }

        protected void SetUpAllCaches<T>()
        {
            SetUpCache<T>(MockMemoryCache);
            SetUpCache<T>(MockDistributedCache);
        }

        protected void SetUpMemoryCache<T>(T item)
        {
            var cachedItem = new CacheItem<T>(item);

            Mock.Get(MockMemoryCache)
              .Setup(dc => dc.Get<T>(It.IsAny<string>(), It.IsAny<bool>()))
              .ReturnsAsync(cachedItem);
        }

        protected void SetUpCache<T>(ICacheWrapper wrapper)
        {
            Mock.Get(wrapper)
              .Setup(dc => dc.Get<T>(It.IsAny<string>(), It.IsAny<bool>()))
              .ReturnsAsync(null as CacheItem<T>);

            Mock.Get(wrapper)
              .Setup(dc => dc.Set(
                  It.IsAny<string>(),
                  It.IsAny<CacheItem<T>>(),
                  It.IsAny<TimeSpan>(),
                  It.IsAny<bool>()))
              .Returns(Task.CompletedTask);
        }

        protected void SetupSystemProvider()
        {
            Mock.Get(SystemProvider)
                    .SetupGet(p => p.DateTime)
                    .Returns(_dateTimeProvider);

            Mock.Get(SystemProvider)
                    .SetupGet(p => p.Guid)
                    .Returns(_guidProvider);
        }

        protected IVirusScanProcessor GetVirusScanProcessor()
        {
            var cosmosDbService = GetCosmosDbService();
            var virusScanner = GetVirusScanner();
            var pagingService = new PagingService();
            var configuration = new VirusScanProcessorConfiguration();
            var mockLogger = CreateMockLoggerAdapter<VirusScanProcessor>();

            return new VirusScanProcessor(
                cosmosDbService,
                virusScanner,
                QueueManager,
                pagingService,
                configuration,
                mockLogger);
        }

        protected DocumentsUploadedScenario.Configuration GetFilesInDocumentsUploadedConfiguration()
        {
            return new DocumentsUploadedScenario.Configuration
            {
                CosmosDb = GetAzureCosmosDbConfiguration(),
                BlobContainer = new Populator.Storage.AzureBlobContainer.Configuration
                {
                    ConnectionString = MockConfig["BlobContainers:ConnectionString"],
                    ContainerName = MockConfig["BlobContainers:ContainerName"]
                }
            };
        }

        protected DocumentsCosmosDb.Configuration GetAzureCosmosDbConfiguration()
        {
            return new DocumentsCosmosDb.Configuration
            {
                EndPoint = MockConfig["CosmosDb:ServiceEndpoint"],
                AuthKeyOrResourceToken = MockConfig["CosmosDb:AuthKeyOrResourceToken"],
                DatabaseName = "docx",
                CollectionName = "collection1"
            };
        }

        protected DocumentsCosmosDb.Configuration GetCacheWarmUpCosmosDbConfiguration()
        {
            return new DocumentsCosmosDb.Configuration
            {
                EndPoint = MockConfig["CosmosDb:ServiceEndpoint"],
                AuthKeyOrResourceToken = MockConfig["CosmosDb:AuthKeyOrResourceToken"],
                DatabaseName = "docx",
                CollectionName = "org-cache-warm-up"
            };
        }

        protected ICosmosClientFactory GetCosmosClientFactory()
            => new CosmosClientFactory(new[]
            {
                CreateCosmosClient(false),
                CreateCosmosClient(true)
            });

        private static PublishedBatchConverter CreatePublishedBatchConverter()
        {
            var fileMetadataUserEncryptor = new FileMetadataUserEncryptor(new EncryptionService(), new CosmosDbConfiguration());
            return new PublishedBatchConverter(fileMetadataUserEncryptor);
        }

        private CosmosClient CreateCosmosClient(bool allowBulkExecution = false)
            => new CosmosClient(
            MockConfig["CosmosDb:ServiceEndpoint"],
            MockConfig["CosmosDb:AuthKeyOrResourceToken"],
            new CosmosClientOptions
            {
                AllowBulkExecution = allowBulkExecution,
                ConnectionMode = ConnectionMode.Gateway
            });

        private IVirusScanner GetVirusScanner()
        {
            var directoriesManager = GetDirectoriesManager();
            var cosmosDbService = GetCosmosDbService();
            var configuration = new VirusScannerConfiguration();
            var mockLogger = CreateMockLoggerAdapter<VirusScanner>();

            return new VirusScanner(
                directoriesManager,
                MockAntivirus,
                cosmosDbService,
                SystemProvider,
                configuration,
                mockLogger);
        }

        private ServiceProvider CreateServiceProvider()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddValidations();
            return serviceCollection.BuildServiceProvider();
        }
    }
}