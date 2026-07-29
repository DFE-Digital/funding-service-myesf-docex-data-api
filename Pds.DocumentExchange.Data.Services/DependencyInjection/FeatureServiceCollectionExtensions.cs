using Mapster;
using MapsterMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Pds.Core.BulkJobs.Interfaces;
using Pds.Core.Common.Organisation.Models;
using Pds.Core.DfESignIn.Interfaces;
using Pds.Core.DfESignIn.Models;
using Pds.Core.DfESignIn.Services;
using Pds.Core.Logging;
using Pds.Core.Utils;
using Pds.Core.Utils.Interfaces;
using Pds.DocumentExchange.Data.Services.Comparers;
using Pds.DocumentExchange.Data.Services.DTOs;
using Pds.DocumentExchange.Data.Services.DTOs.Configuration;
using Pds.DocumentExchange.Data.Services.Implementations;
using Pds.DocumentExchange.Data.Services.Implementations.Caching;
using Pds.DocumentExchange.Data.Services.Implementations.Converters;
using Pds.DocumentExchange.Data.Services.Implementations.CosmosDb;
using Pds.DocumentExchange.Data.Services.Implementations.FDS;
using Pds.DocumentExchange.Data.Services.Implementations.Filters;
using Pds.DocumentExchange.Data.Services.Implementations.Lookup;
using Pds.DocumentExchange.Data.Services.Implementations.Settings;
using Pds.DocumentExchange.Data.Services.Implementations.Storage;
using Pds.DocumentExchange.Data.Services.Implementations.VirusScan;
using Pds.DocumentExchange.Data.Services.Interfaces;
using Pds.DocumentExchange.Data.Services.Interfaces.Caching;
using Pds.DocumentExchange.Data.Services.Interfaces.Converters;
using Pds.DocumentExchange.Data.Services.Interfaces.FDS;
using Pds.DocumentExchange.Data.Services.Interfaces.Filters;
using Pds.DocumentExchange.Data.Services.Interfaces.Lookup;
using Pds.DocumentExchange.Data.Services.Interfaces.Settings;
using Pds.DocumentExchange.Data.Services.Interfaces.Storage;
using System.Collections.Generic;

namespace Pds.DocumentExchange.Data.Services.DependencyInjection
{
    /// <summary>
    /// Extensions class for <see cref="IServiceCollection"/> for registering the feature's services.
    /// </summary>
    public static class FeatureServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the services.
        /// </summary>
        /// <param name="services">The services collection.</param>
        /// <param name="configuration">The configuration.</param>
        /// <returns>The updated services collection.</returns>
        public static IServiceCollection AddDocumentExchangeServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddTransient<IEncryptionService, EncryptionService>();

            services.AddTransient<IFileMetadataUserEncryptor>(
                services =>
                {
                    IEncryptionService encryptionService = services.GetService<IEncryptionService>();
                    var configuration = services.GetService<IOptions<DocumentExchangeServicesConfiguration>>().Value;

                    return new FileMetadataUserEncryptor(encryptionService, configuration.CosmosDb);
                });

            services.AddSingleton<ICosmosDbService, CosmosDbService>();
            services.Configure<PublicApiSettings>(configuration.GetSection("DfESignin:PublicApi"));
            services.AddSingleton(resolver =>
            {
                return resolver.GetService<IOptions<PublicApiSettings>>().Value;
            });

            services.AddHttpClient<IDfESignInPublicApi, DfESignInPublicApi>();

            services.AddTransient<IFileNameProvider, FileNameProvider>();

            services.AddSingleton<IServiceBusQueueManager>(
                service =>
                {
                    var serviceConfig = service.GetService<IOptions<DocumentExchangeServicesConfiguration>>().Value;
                    return new AzureServiceBusQueueManager(serviceConfig.ServiceBusQueueManager);
                });

            services.AddSingleton<IDirectoriesManager>(
                service =>
                {
                    var fileNameProvider = service.GetService<IFileNameProvider>();
                    var configurationDataService = service.GetService<IConfigurationDataService>();
                    var serviceConfig = service.GetService<IOptions<DocumentExchangeServicesConfiguration>>().Value;
                    var azureBlobContainerlogger = service.GetService<ILoggerAdapter<AzureBlobContainer>>();
                    var azureFileShareDirectorylogger = service.GetService<ILoggerAdapter<AzureFileShareDirectory>>();
                    return new DirectoriesManager(fileNameProvider, configurationDataService, serviceConfig.DirectoriesManager, azureBlobContainerlogger, azureFileShareDirectorylogger);
                });

            AddDocumentDownloader(services);
            AddDocumentManager(services);
            AddDocumentUploader(services);
            AddDocumentPublisher(services);
            AddVirusScanProcessorService(services, configuration);
            AddVirusScanResultProcessorService(services);
            AddFdsApiClientService(services, configuration);
            NotificationServicesRegistrar.AddTo(services);

            services.AddSingleton<IBatchesService, BatchesService>();
            services.AddSingleton<IBatchToExchangeDocumentConverter, BatchToExchangeDocumentConverter>();

            services.AddTransient<IPagingService, PagingService>();
            services.AddSingleton<IFilterToListResultConverter<ExchangeDocument>, FilterToListResultConverter<ExchangeDocument>>();
            services.AddSingleton<IFilterToListResultConverter<AgencyDocument>, FilterToListResultConverter<AgencyDocument>>();

            services.AddSingleton<IAgencyExchangeDocumentsService, AgencyExchangeDocumentsService>();
            services.AddSingleton<IOrganisationExchangeDocumentsService, OrganisationExchangeDocumentsService>();
            services.AddSingleton<IExchangeDocumentSelector, ExchangeDocumentSelector>();

            services.AddSingleton<IListFilterService<AgencyDocument>, ListFilterService<AgencyDocument>>();
            services.AddSingleton<IListFilterService<ExchangeDocument>, ListFilterService<ExchangeDocument>>();

            services.AddSingleton<IDateRangeFilterService<AgencyDocument>, DateRangeFilterService<AgencyDocument>>();
            services.AddSingleton<IDateRangeFilterService<ExchangeDocument>, DateRangeFilterService<ExchangeDocument>>();

            services.AddSingleton<IRadioFilterService<AgencyDocument>, RadioFilterService<AgencyDocument>>();
            services.AddSingleton<IRadioFilterService<ExchangeDocument>, RadioFilterService<ExchangeDocument>>();

            services.AddSingleton<ITextBoxFilterService<AgencyDocument>, TextBoxFilterService<AgencyDocument>>();
            services.AddSingleton<ITextBoxFilterService<ExchangeDocument>, TextBoxFilterService<ExchangeDocument>>();

            services.AddSingleton<IFiltersExecutionManager<ExchangeDocument>, FiltersExecutionManager<ExchangeDocument>>();
            services.AddSingleton<IFiltersExecutionManager<AgencyDocument>, FiltersExecutionManager<AgencyDocument>>();

            services.AddSingleton<IFiltersFactory<ExchangeDocument>, ExchangeDocumentFiltersFactory>();
            services.AddSingleton<IFiltersFactory<AgencyDocument>, AgencyDocumentFiltersFactory>();

            services.AddSingleton<IConfigurationDataService, ConfigurationDataService>();
            services.AddSingleton<IAdminSettingsService, AdminSettingsService>();

            services.AddSingleton<IAgencyDocumentValidator, AgencyDocumentValidator>();
            services.AddSingleton<IAgencyService, AgencyService>();

            services.AddSingleton<IEqualityComparer<Product>, ProductEqualityComparer>();
            services.AddSingleton<IEqualityComparer<OrganisationIdentifier>, OrganisationIdentifierEqualityComparer>();

            services.AddSingleton<IProductsLookup, ProductsLookup>();
            services.AddSingleton<IOrganisationsLookup, OrganisationsLookup>();
            services.AddSingleton<ITeamsLookup, TeamsLookup>();
            services.AddSingleton<IOrganisationSubtypesLookup, OrganisationSubtypesLookup>();

            services.AddSingleton<IZipService, ZipService>();
            services.AddSingleton<IAcademicYearCalculator, AcademicYearCalculator>();
            services.AddSingleton<IDocumentExchangeSummaries, DocumentExchangeSummaries>();
            services.AddSingleton<IProductVersionService, ProductVersionService>();

            services.AddSingleton<IReportService>(
                serviceProvider =>
                {
                    var dateTimeProvider = serviceProvider.GetService<IDateTimeProvider>();
                    var cosmosDbService = serviceProvider.GetService<ICosmosDbService>();
                    var loggerService = serviceProvider.GetService<ILoggerAdapter<ReportService>>();
                    var organisationsLookupService = serviceProvider.GetService<IOrganisationsLookup>();
                    var configurationDataService = serviceProvider.GetService<IConfigurationDataService>();
                    IEncryptionService encryptionService = serviceProvider.GetService<IEncryptionService>();
                    var configuration = serviceProvider.GetService<IOptions<DocumentExchangeServicesConfiguration>>().Value;

                    return new ReportService(dateTimeProvider, cosmosDbService, encryptionService, loggerService, organisationsLookupService, configurationDataService, configuration.CosmosDb);
                });

            services.AddSingleton<IConvertAgencyDocumentErrorTypes, AgencyDocumentErrorTypeConverter>();

            services.AddSingleton<ICacheManager, CacheManager>();
            services.AddSingleton<ICacheKeyBuilder, CacheKeyBuilder>();
            services.AddSingleton<ICacheOptionsProvider, CacheOptionsProvider>();

            services.AddSingleton<IDocumentDeletion>(
                service =>
                {
                    var cosmosDbService = service.GetService<ICosmosDbService>();
                    var directoriesManager = service.GetService<IDirectoriesManager>();
                    var systemProvider = service.GetService<ISystemProvider>();
                    var fileMetadataUserEncryptor = service.GetService<IFileMetadataUserEncryptor>();
                    var batchToExchangeDocumentConverter = service.GetService<IBatchToExchangeDocumentConverter>();
                    var mapper = service.GetService<IMapper>();
                    var configuration = service.GetService<IOptions<DocumentExchangeServicesConfiguration>>().Value;
                    var logger = service.GetService<ILoggerAdapter<DocumentDeletion>>();

                    return new DocumentDeletion(
                        cosmosDbService,
                        directoriesManager,
                        systemProvider,
                        fileMetadataUserEncryptor,
                        batchToExchangeDocumentConverter,
                        mapper,
                        configuration.VirusScanResultProcessor,
                        logger);
                });

            services.AddSingleton<ICsvWriter, CsvWriter>();
            services.AddSingleton<INotifyEmailService, NotifyEmailService>();

            return services;
        }

        private static void AddDocumentUploader(IServiceCollection services)
        {
            services.AddTransient<IDocumentUploader>(serviceProvider =>
            {
                var directoriesManager = serviceProvider.GetService<IDirectoriesManager>();
                var queueManager = serviceProvider.GetService<IServiceBusQueueManager>();
                var cosmosDbService = serviceProvider.GetService<ICosmosDbService>();
                var fileNameProvider = serviceProvider.GetService<IFileNameProvider>();
                var fileMetadataUserEncryptor = serviceProvider.GetService<IFileMetadataUserEncryptor>();
                var academicYearCalculator = serviceProvider.GetService<IAcademicYearCalculator>();
                var systemProvider = serviceProvider.GetService<ISystemProvider>();
                var mapper = serviceProvider.GetService<IMapper>();
                var serviceConfig = serviceProvider.GetService<IOptions<DocumentExchangeServicesConfiguration>>().Value;
                var logger = serviceProvider.GetService<ILoggerAdapter<DocumentUploader>>();

                return new DocumentUploader(
                    directoriesManager,
                    queueManager,
                    cosmosDbService,
                    fileNameProvider,
                    fileMetadataUserEncryptor,
                    academicYearCalculator,
                    systemProvider,
                    mapper,
                    serviceConfig.DocumentUploader,
                    logger);
            });
        }

        private static void AddDocumentPublisher(IServiceCollection services)
        {
            services.AddTransient<IDocumentPublisher>(serviceProvider =>
            {
                var directoriesManager = serviceProvider.GetService<IDirectoriesManager>();
                var queueManager = serviceProvider.GetService<IServiceBusQueueManager>();
                var cosmosDbService = serviceProvider.GetService<ICosmosDbService>();
                var fileNameProvider = serviceProvider.GetService<IFileNameProvider>();
                var productsLookup = serviceProvider.GetService<IProductsLookup>();
                var fileMetadataUserEncryptor = serviceProvider.GetService<IFileMetadataUserEncryptor>();
                var systemProvider = serviceProvider.GetService<ISystemProvider>();
                var mapper = serviceProvider.GetService<IMapper>();
                var serviceConfig = serviceProvider.GetService<IOptions<DocumentExchangeServicesConfiguration>>().Value;
                var logger = serviceProvider.GetService<ILoggerAdapter<DocumentPublisher>>();
                var agencyService = serviceProvider.GetService<IAgencyService>();
                var bulkJobManager = serviceProvider.GetService<IBulkJobManager>();
                var retry = serviceProvider.GetService<IRetryMechanism>();

                return new DocumentPublisher(
                    directoriesManager,
                    queueManager,
                    cosmosDbService,
                    fileNameProvider,
                    productsLookup,
                    fileMetadataUserEncryptor,
                    systemProvider,
                    mapper,
                    serviceConfig.DocumentPublisher,
                    logger,
                    agencyService,
                    bulkJobManager,
                    retry);
            });
        }

        private static void AddVirusScanProcessorService(
            IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<WindowsDefenderAntivirusConfiguration>(configuration.GetSection("DocumentExchangeServices:WindowsDefenderAntivirus"));
            services.AddScoped(resolver =>
            {
                return resolver.GetService<IOptions<WindowsDefenderAntivirusConfiguration>>().Value;
            });
            services.AddTransient<IAntivirus, WindowsDefenderAntivirus>();
            services.AddTransient<VirusScannerConfiguration>();
            services.AddTransient<IVirusScanner, VirusScanner>();
            services.AddTransient<VirusScanProcessorConfiguration>();
            services.AddTransient<IVirusScanProcessor, VirusScanProcessor>();
        }

        private static void AddFdsApiClientService(
            IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<FdsApiClientConfiguration>(configuration.GetSection("FdsApiClientConfiguration"));
            services.AddSingleton(resolver =>
            {
                return resolver.GetService<IOptions<FdsApiClientConfiguration>>().Value;
            });
            services.AddHttpClient<IOrganisationService, OrganisationService>();
        }

        private static void AddVirusScanResultProcessorService(
            IServiceCollection services)
        {
            services.AddTransient<VirusScanResultProcessorConfiguration>();
            services.AddTransient<IVirusScanResultProcessor, VirusScanResultProcessor>();
        }

        private static void AddDocumentDownloader(IServiceCollection services)
        {
            services.AddTransient<IDocumentDownloader>(serviceProvider =>
            {
                var directoriesManager = serviceProvider.GetService<IDirectoriesManager>();
                var cosmosDbService = serviceProvider.GetService<ICosmosDbService>();
                var serviceConfig = serviceProvider.GetService<IOptions<DocumentExchangeServicesConfiguration>>().Value;
                var fileMetadataUserEncryptor = serviceProvider.GetService<IFileMetadataUserEncryptor>();
                var systemProvider = serviceProvider.GetService<ISystemProvider>();
                var mapper = serviceProvider.GetService<IMapper>();
                var zipService = serviceProvider.GetService<IZipService>();
                var logger = serviceProvider.GetService<ILoggerAdapter<DocumentDownloader>>();

                return new DocumentDownloader(
                    cosmosDbService,
                    directoriesManager,
                    fileMetadataUserEncryptor,
                    systemProvider,
                    mapper,
                    zipService,
                    serviceConfig.DocumentDownloader,
                    logger);
            });
        }

        private static void AddDocumentManager(IServiceCollection services)
        {
            services.AddTransient<IDocumentManager>(serviceProvider =>
            {
                var directoriesManager = serviceProvider.GetService<IDirectoriesManager>();
                var logger = serviceProvider.GetService<ILoggerAdapter<DocumentManager>>();

                return new DocumentManager(
                    directoriesManager,
                    logger);
            });
        }
    }
}