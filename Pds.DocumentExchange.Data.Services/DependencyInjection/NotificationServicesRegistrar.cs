using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Pds.Core.DfESignIn.Interfaces;
using Pds.Core.Logging;
using Pds.DocumentExchange.Data.Services.DTOs.Configuration;
using Pds.DocumentExchange.Data.Services.Implementations.Builders;
using Pds.DocumentExchange.Data.Services.Implementations.Coordinators;
using Pds.DocumentExchange.Data.Services.Implementations.Factories;
using Pds.DocumentExchange.Data.Services.Implementations.Providers;
using Pds.DocumentExchange.Data.Services.Interfaces;
using Pds.DocumentExchange.Data.Services.Interfaces.Builders;
using Pds.DocumentExchange.Data.Services.Interfaces.Coordinators;
using Pds.DocumentExchange.Data.Services.Interfaces.Factories;
using Pds.DocumentExchange.Data.Services.Interfaces.Providers;
using Pds.DocumentExchange.Data.Services.Interfaces.Settings;
using Pds.Services.Common.Implementations.Converters;
using Pds.Services.Common.Implementations.Factories;
using Pds.Services.Common.Implementations.Providers;
using Pds.Services.Common.Interfaces.Converters;
using Pds.Services.Common.Interfaces.Factories;
using Pds.Services.Common.Interfaces.Providers;

namespace Pds.DocumentExchange.Data.Services.DependencyInjection
{
    /// <summary>
    /// the notification services registrar.
    /// </summary>
    internal static class NotificationServicesRegistrar
    {
        /// <summary>
        /// adds (the notification services) to...
        /// </summary>
        /// <param name="serviceCollection">the service collection.</param>
        internal static void AddTo(IServiceCollection serviceCollection)
        {
            // builders
            serviceCollection.AddSingleton<INotificationSummaryBuilder, NotificationSummaryBuilder>();
            serviceCollection.AddSingleton<INotificationMessageBuilder, NotificationMessageBuilder>();

            // converters
            serviceCollection.AddSingleton<IConvertJsonTypes, JsonTypeConverter>();
            serviceCollection.AddSingleton<IConvertXmlTypes, XmlTypeConverter>();

            // coordinators
            serviceCollection.AddSingleton<INotifyUploadCompleteCoordinator, NotifyUploadCompleteCoordinator>();

            serviceCollection.AddSingleton<INotifyPublishCompleteCoordinator>(
                serviceProvider =>
                {
                    var createBatchMetadataAnalyses = serviceProvider.GetService<IBatchMetadataAnalysisFactory>();
                    var buildNotificationMessages = serviceProvider.GetService<INotificationMessageBuilder>();
                    var buildFileMetadataHistory = serviceProvider.GetService<INotificationSummaryBuilder>();
                    var adminSettingsService = serviceProvider.GetService<IAdminSettingsService>();
                    var dfeSignInPublicApi = serviceProvider.GetService<IDfESignInPublicApi>();
                    var notifyEmailService = serviceProvider.GetService<INotifyEmailService>();
                    var logger = serviceProvider.GetService<ILoggerAdapter<NotifyPublishCompleteCoordinator>>();

                    var serviceConfig = serviceProvider.GetService<IOptions<DocumentExchangeServicesConfiguration>>()
                        .Value;

                    return new NotifyPublishCompleteCoordinator(
                        createBatchMetadataAnalyses,
                        buildNotificationMessages,
                        buildFileMetadataHistory,
                        serviceConfig.Notification,
                        dfeSignInPublicApi,
                        adminSettingsService,
                        notifyEmailService,
                        logger);
                });

            // factories
            serviceCollection.AddSingleton<IBatchMetadataAnalysisFactory>(serviceProvider =>
            {
                var cosmosDbService = serviceProvider.GetService<ICosmosDbService>();
                var encryptionService = serviceProvider.GetService<IEncryptionService>();
                var configurationDataService = serviceProvider.GetService<IConfigurationDataService>();
                var serviceConfig = serviceProvider.GetService<IOptions<DocumentExchangeServicesConfiguration>>().Value;
                var logger = serviceProvider.GetService<ILoggerAdapter<BatchMetadataAnalysisFactory>>();
                return new BatchMetadataAnalysisFactory(cosmosDbService, encryptionService, configurationDataService, serviceConfig.CosmosDb, logger);
            });

            serviceCollection.AddSingleton<ICreateNotificationMessageHeaders, NotificationMessageHeaderFactory>();
            serviceCollection.AddSingleton<ICreateLoggingContexts, LoggingContextFactory>();
            serviceCollection.AddSingleton<ICreateNotificationMessageBodies, NotificationMessageBodyFactory>();
            serviceCollection.AddSingleton<ICreateNotificationMessages, NotificationMessageFactory>();

            // providers
            serviceCollection.AddSingleton<IProvideAssets, AssetProvider>();
            serviceCollection.AddSingleton<IProvideNotificationMessageBodyTemplates, NotificationMessageBodyTemplateProvider>();
            serviceCollection.AddSingleton<IProvidePresentationFormatting, PresentationFormattingProvider>();
            serviceCollection.AddSingleton<IProvideSafeOperations, SafeOperationsProvider>();
        }
    }
}