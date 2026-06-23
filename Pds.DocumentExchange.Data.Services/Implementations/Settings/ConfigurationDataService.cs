using Pds.Core.Logging;
using Pds.DocumentExchange.Data.Services.DTOs;
using Pds.DocumentExchange.Data.Services.Enums;
using Pds.DocumentExchange.Data.Services.Interfaces;
using Pds.DocumentExchange.Data.Services.Interfaces.Caching;
using Pds.DocumentExchange.Data.Services.Interfaces.Settings;
using Pds.Services.Common.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Implementations.Settings
{
    /// <summary>
    /// Class exposing methods to return the Document Exchange configuration data.
    /// </summary>
    public class ConfigurationDataService : IConfigurationDataService
    {
        private readonly ICosmosDbService _cosmosDbService;
        private readonly ICacheManager _cacheManager;
        private readonly ILoggerAdapter<ConfigurationDataService> _logger;

        private readonly SemaphoreSlim _configSemaphore
            = new SemaphoreSlim(1, 1);

        private readonly SemaphoreSlim _listConfigSemaphore
            = new SemaphoreSlim(1, 1);

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationDataService"/> class.
        /// </summary>
        /// <param name="cosmosDbService">An instance of an <see cref="ICosmosDbService"/>.</param>
        /// <param name="cacheManager">The cache manager.</param>
        /// <param name="logger">The logger.</param>
        public ConfigurationDataService(
            ICosmosDbService cosmosDbService,
            ICacheManager cacheManager,
            ILoggerAdapter<ConfigurationDataService> logger)
        {
            It.IsNull(cosmosDbService)
               .AsGuard<ArgumentNullException>();

            It.IsNull(cacheManager)
               .AsGuard<ArgumentNullException>();

            It.IsNull(logger)
              .AsGuard<ArgumentNullException>();

            _cosmosDbService = cosmosDbService;
            _cacheManager = cacheManager;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyCollection<Product>> GetProducts()
        {
            _logger.LogTrace("Configuration data service getting all products from configuration...");

            var products = await GetListConfiguration<Product, int>(
                ConfigurationSection.Products,
                product => product.Identifier);

            _logger.LogTrace("Configuration data service getting all products from configuration... Done.");

            return products.AsSafeReadOnlyList();
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyCollection<AgencyTeam>> GetTeams()
        {
            _logger.LogTrace("Configuration data service getting all teams from configuration...");

            var teams = await GetListConfiguration<AgencyTeam, string>(
                ConfigurationSection.Teams,
                team => team.Identifier);

            _logger.LogTrace("Configuration data service getting all teams from configuration... Done.");

            return teams.AsSafeReadOnlyList();
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyCollection<FileExtensionInfo>> GetFileExtensions()
        {
            _logger.LogTrace("Configuration data service getting all allowed file extensions from configuration...");

            var fileExtensions = await GetListConfiguration<FileExtensionInfo, string>(
                ConfigurationSection.AllowedFileExtensions,
                extension => extension.Identifier);

            _logger.LogTrace("Configuration data service getting all allowed file extensions from configuration... Done.");

            return fileExtensions.AsSafeReadOnlyList();
        }

        /// <inheritdoc/>
        public async Task<bool> IsDocumentExchangeEnabled()
        {
            _logger.LogTrace("Configuration data service getting document exchange enabled from configuration...");

            var documentExchangeEnabled = await GetConfiguration<bool>(ConfigurationSection.DocumentExchangeEnabled);

            _logger.LogTrace("Configuration data service getting document exchange enabled from configuration... Done.");

            return documentExchangeEnabled;
        }

        /// <inheritdoc/>
        public async Task<bool> IsOrganisationUploadEnabled()
        {
            _logger.LogTrace("Configuration data service getting document exchange organisation upload enabled from configuration...");

            var uploadEnabled = await GetConfiguration<bool>(ConfigurationSection.DocumentExchangeOrganisationUploadEnabled);

            _logger.LogTrace("Configuration data service getting document exchange organisation upload enabled from configuration... Done.");

            return uploadEnabled;
        }

        /// <inheritdoc/>
        public async Task<string> GetServiceReplyEmail()
        {
            _logger.LogTrace("Configuration data service getting service reply email from configuration...");

            var serviceReplyEmail = await GetConfiguration<string>(ConfigurationSection.ServiceReplyEmail);

            _logger.LogTrace("Configuration data service getting service reply email from configuration... Done.");

            return serviceReplyEmail;
        }

        /// <inheritdoc/>
        public async Task<string> GetServiceNoReplyEmail()
        {
            _logger.LogTrace("Configuration data service getting service no reply email from configuration...");

            var serviceNoReplyEmail = await GetConfiguration<string>(ConfigurationSection.ServiceNoReplyEmail);

            _logger.LogTrace("Configuration data service getting service no reply email from configuration... Done.");

            return serviceNoReplyEmail;
        }

        /// <inheritdoc/>
        public async Task<bool> IsAgencyPublishCompleteProviderNotificationEnabled()
        {
            _logger.LogTrace("Configuration data service getting agency publish complete provider notification from configuration...");

            var agencyPublishNotificationEnabled = await GetConfiguration<bool>(ConfigurationSection.AgencyPublishCompleteProviderNotificationEnabled);

            _logger.LogTrace("Configuration data service getting agency publish complete provider notification from configuration... Done.");

            return agencyPublishNotificationEnabled;
        }

        /// <inheritdoc/>
        public async Task<int> GetMaxFileUploadSize()
        {
            _logger.LogTrace("Configuration data service getting maximum file size for upload from configuration...");

            var maxFileUploadSizeInBytes = await GetConfiguration<int>(ConfigurationSection.MaxFileUploadSize);

            _logger.LogTrace("Configuration data service getting maximum file size for upload from configuration... Done.");

            return maxFileUploadSizeInBytes;
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyCollection<EmailSetting>> GetEmailSettings()
        {
            _logger.LogTrace("Configuration data service getting email settings configuration...");

            var emailSettings = await GetListConfiguration<EmailSetting, string>(
                ConfigurationSection.EmailSettings,
                setting => setting.EmailMessageType);

            _logger.LogTrace("Configuration data service getting email settings configuration... Done.");

            return emailSettings.AsSafeReadOnlyList();
        }

        private async Task<T> GetConfiguration<T>(ConfigurationSection section)
        {
            var cacheService = _cacheManager.CacheService;
            var cacheKey = _cacheManager.CacheKeyBuilder.BuildConfigurationKey(section);
            var cacheOptions = _cacheManager.CacheOptionsProvider.Configuration;

            await _configSemaphore.WaitAsync();

            var result = await cacheService.Get(
                            cacheKey,
                            () => _cosmosDbService.GetConfiguration<T>(section),
                            cacheOptions);

            _configSemaphore.Release();

            return result;
        }

        private async Task<IEnumerable<T>> GetListConfiguration<T, TProperty>(ConfigurationSection section, Func<T, TProperty> distinctKeySelector)
        {
            var cacheService = _cacheManager.CacheService;
            var cacheKey = _cacheManager.CacheKeyBuilder.BuildListConfigurationKey(section);
            var cacheOptions = _cacheManager.CacheOptionsProvider.Configuration;

            await _listConfigSemaphore.WaitAsync();

            var result = await cacheService.Get(
                            cacheKey,
                            async () =>
                            {
                                var listConfig = await _cosmosDbService.GetListConfiguration<T>(section);

                                return It.IsNull(listConfig)
                                            ? Collection.Empty<T>()
                                            : listConfig.GroupBy(distinctKeySelector).Select(group => group.First());
                            },
                            cacheOptions);

            _listConfigSemaphore.Release();

            return result;
        }
    }
}