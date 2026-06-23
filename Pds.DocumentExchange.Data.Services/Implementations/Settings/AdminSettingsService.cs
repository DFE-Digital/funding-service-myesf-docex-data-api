using Pds.Core.Logging;
using Pds.Core.Utils.Helpers;
using Pds.Core.Utils.Interfaces;
using Pds.DocumentExchange.Data.Services.DTOs;
using Pds.DocumentExchange.Data.Services.Enums;
using Pds.DocumentExchange.Data.Services.Interfaces;
using Pds.DocumentExchange.Data.Services.Interfaces.Caching;
using Pds.DocumentExchange.Data.Services.Interfaces.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Implementations.Settings
{
    /// <summary>
    /// Class exposing methods to query and update the Document Exchange configuration data.
    /// </summary>
    public class AdminSettingsService : IAdminSettingsService
    {
        private readonly ICosmosDbService _cosmosDbService;
        private readonly ICacheManager _cacheManager;
        private readonly IDateTimeProvider _dateTime;
        private readonly ILoggerAdapter<AdminSettingsService> _logger;

        private readonly SemaphoreSlim _configSemaphore = new SemaphoreSlim(1, 1);
        private readonly SemaphoreSlim _listConfigSemaphore = new SemaphoreSlim(1, 1);

        /// <summary>
        /// Initializes a new instance of the <see cref="AdminSettingsService"/> class.
        /// </summary>
        /// <param name="cosmosDbService">The Cosmos DB service.</param>
        /// <param name="cacheManager">The cache manager.</param>
        /// <param name="dateTime">The date & time provider.</param>
        /// <param name="logger">The logger.</param>
        public AdminSettingsService(
            ICosmosDbService cosmosDbService,
            ICacheManager cacheManager,
            IDateTimeProvider dateTime,
            ILoggerAdapter<AdminSettingsService> logger)
        {
            It.IsNull(cosmosDbService)
                .AsGuard<ArgumentNullException>();

            It.IsNull(cacheManager)
                .AsGuard<ArgumentNullException>();

            It.IsNull(logger)
                .AsGuard<ArgumentNullException>();

            _cosmosDbService = cosmosDbService;
            _cacheManager = cacheManager;
            _dateTime = dateTime;
            _logger = logger;
        }

        #region Products

        /// <inheritdoc/>
        public async Task<IReadOnlyCollection<Product>> GetProducts()
        {
            _logger.LogTrace("Admin settings service getting all products from configuration...");

            var products = await GetProductsInternal();

            _logger.LogTrace("Admin settings service getting all products from configuration... Done.");

            return products.AsSafeReadOnlyList();
        }

        /// <inheritdoc/>
        public async Task<Product> GetProduct(int identifier)
        {
            _logger.LogTrace($"Admin settings service getting the product {identifier} from configuration...");

            var products = await GetProductsInternal();
            var product = products.FirstOrDefault(p => p.Identifier == identifier) ?? new UnknownProduct();

            _logger.LogTrace($"Admin settings service getting the product {identifier} from configuration... Done.");

            return product;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Product>> GetProductsThatOrganisationsCanUpload()
        {
            _logger.LogTrace("Admin settings service getting products that organisations can upload from configuration...");

            var products = await GetProductsInternal();
            var canOrganisationUploadProducts = products.Where(product => product.CanOrganisationsUpload).AsSafeReadOnlyList();

            _logger.LogTrace("Admin settings service getting products that organisations can upload from configuration... Done.");

            return canOrganisationUploadProducts;
        }

        /// <inheritdoc/>
        public async Task<Product> AddOrUpdateProduct(int oldIdentifier, Product newProductValue)
        {
            _logger.LogTrace($"Admin settings service adding or updating product {oldIdentifier} in configuration...");

            newProductValue.LastUpdated = _dateTime.UtcNow();
            var product = await AddOrUpdateListConfiguration(ConfigurationSection.Products, oldIdentifier, newProductValue);

            _logger.LogTrace($"Admin settings service adding or updating product {oldIdentifier} in configuration... Done.");

            return product;
        }

        #endregion


        #region Teams

        /// <inheritdoc/>
        public async Task<IReadOnlyCollection<AgencyTeam>> GetTeams()
        {
            _logger.LogTrace("Admin settings service getting all teams from configuration...");

            var teams = await GetListConfiguration<AgencyTeam, string>(ConfigurationSection.Teams, team => team.Identifier);

            _logger.LogTrace("Admin settings service getting all teams from configuration... Done.");

            return teams.AsSafeReadOnlyList();
        }

        /// <inheritdoc/>
        public async Task<AgencyTeam> AddOrUpdateTeam(string oldIdentifier, AgencyTeam newTeamValue)
        {
            _logger.LogTrace($"Admin settings service adding or updating team {oldIdentifier} in configuration...");

            var team = await AddOrUpdateListConfiguration(ConfigurationSection.Teams, oldIdentifier, newTeamValue);

            _logger.LogTrace($"Admin settings service adding or updating team {oldIdentifier} in configuration... Done.");

            return team;
        }

        #endregion


        #region File extensions

        /// <inheritdoc/>
        public async Task<IReadOnlyCollection<FileExtensionInfo>> GetFileExtensions()
        {
            _logger.LogTrace("Admin settings service getting all allowed file extensions from configuration...");

            var fileExtensions = await GetListConfiguration<FileExtensionInfo, string>(
                ConfigurationSection.AllowedFileExtensions,
                extension => extension.Identifier);

            _logger.LogTrace("Admin settings service getting all allowed file extensions from configuration... Done.");

            return fileExtensions.AsSafeReadOnlyList();
        }

        /// <inheritdoc/>
        public Task<FileExtensionInfo> AddOrUpdateFileExtension(string oldIdentifier, FileExtensionInfo newFileExtensionValue)
        {
            _logger.LogTrace($"Admin settings service adding or updating file extension {oldIdentifier} in configuration...");

            var fileExtension = AddOrUpdateListConfiguration(ConfigurationSection.AllowedFileExtensions, oldIdentifier, newFileExtensionValue);

            _logger.LogTrace($"Admin settings service adding or updating file extension {oldIdentifier} in configuration... Done.");

            return fileExtension;
        }

        #endregion


        #region Emails

        /// <inheritdoc/>
        public async Task<string> GetServiceReplyEmail()
        {
            _logger.LogTrace("Admin settings service getting service reply email from configuration...");

            var serviceReplyEmail = await _cosmosDbService.GetConfiguration<string>(ConfigurationSection.ServiceReplyEmail);

            _logger.LogTrace("Admin settings service getting service reply email from configuration... Done.");

            return serviceReplyEmail;
        }

        /// <inheritdoc/>
        public async Task<string> SetServiceReplyEmail(string email)
        {
            _logger.LogTrace($"Admin settings service setting service reply email to {email} in configuration...");

            var serviceReplyEmail = await UpdateConfiguration(ConfigurationSection.ServiceReplyEmail, email);

            _logger.LogTrace($"Admin settings service setting service reply email to {email} in configuration... Done.");

            return serviceReplyEmail;
        }

        /// <inheritdoc/>
        public async Task<string> GetServiceNoReplyEmail()
        {
            _logger.LogTrace("Admin settings service getting service no reply email from configuration...");

            var serviceNoReplyEmail = await _cosmosDbService.GetConfiguration<string>(ConfigurationSection.ServiceNoReplyEmail);

            _logger.LogTrace("Admin settings service getting service no reply email from configuration... Done.");

            return serviceNoReplyEmail;
        }

        /// <inheritdoc/>
        public async Task<string> SetServiceNoReplyEmail(string email)
        {
            _logger.LogTrace($"Admin settings service setting service no reply email to {email} in configuration...");

            var serviceNoReplyEmail = await UpdateConfiguration(ConfigurationSection.ServiceNoReplyEmail, email);

            _logger.LogTrace($"Admin settings service setting service no reply email to {email} in configuration... Done.");

            return serviceNoReplyEmail;
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyCollection<EmailSetting>> GetEmailSettings()
        {
            _logger.LogTrace("Admin settings  service getting email settings configuration...");

            var emailSettings = await GetListConfiguration<EmailSetting, string>(
                ConfigurationSection.EmailSettings,
                setting => setting.EmailMessageType);

            _logger.LogTrace("Admin settings  service getting email settings configuration... Done.");

            return emailSettings.AsSafeReadOnlyList();
        }

        /// <inheritdoc/>
        public async Task<EmailSetting> SetEmailSetting(string oldEmailType, EmailSetting newEmailSetting)
        {
            _logger.LogTrace($"Admin settings service adding or updating email setting {oldEmailType} in configuration...");

            newEmailSetting.LastUpdated = _dateTime.UtcNow();
            var setting = await AddOrUpdateListConfiguration(ConfigurationSection.EmailSettings, oldEmailType, newEmailSetting);

            _logger.LogTrace($"Admin settings service adding or updating email setting {oldEmailType} in configuration... Done.");

            return setting;
        }

        #endregion

        /// <inheritdoc/>
        public async Task<bool> IsDocumentExchangeEnabled()
        {
            _logger.LogTrace("Admin settings service getting document exchange enabled from configuration...");

            var documentExchangeEnabled = await _cosmosDbService.GetConfiguration<bool>(ConfigurationSection.DocumentExchangeEnabled);

            _logger.LogTrace("Admin settings service getting document exchange enabled from configuration... Done.");

            return documentExchangeEnabled;
        }

        /// <inheritdoc/>
        public async Task<bool> SetDocumentExchangeEnabledStatus(bool isEnabled)
        {
            _logger.LogTrace($"Admin settings service setting document exchange enabled to {isEnabled} in configuration...");

            var documentExchangeEnabled = await UpdateConfiguration(ConfigurationSection.DocumentExchangeEnabled, isEnabled);

            _logger.LogTrace($"Admin settings service setting document exchange enabled to {isEnabled} in configuration... Done.");

            return documentExchangeEnabled;
        }

        /// <inheritdoc/>
        public async Task<bool> IsOrganisationUploadEnabled()
        {
            _logger.LogTrace("Admin settings service getting document exchange organisation upload enabled from configuration...");

            var uploadEnabled = await _cosmosDbService.GetConfiguration<bool>(ConfigurationSection.DocumentExchangeOrganisationUploadEnabled);

            _logger.LogTrace("Admin settings service getting document exchange organisation upload enabled from configuration... Done.");

            return uploadEnabled;
        }

        /// <inheritdoc/>
        public async Task<bool> SetOrganisationUploadEnabledStatus(bool isEnabled)
        {
            _logger.LogTrace($"Admin settings service setting document exchange organisation upload enabled to {isEnabled} in configuration...");

            var uploadEnabled = await UpdateConfiguration(ConfigurationSection.DocumentExchangeOrganisationUploadEnabled, isEnabled);

            _logger.LogTrace($"Admin settings service setting document exchange organisation upload enabled to {isEnabled} in configuration... Done.");

            return uploadEnabled;
        }

        /// <inheritdoc/>
        public async Task<bool> IsAgencyPublishCompleteProviderNotificationEnabled()
        {
            _logger.LogTrace("Admin settings service getting agency publish complete provider notification from configuration...");

            var agencyPublishCompleteProviderNotificationEnabled = await _cosmosDbService.GetConfiguration<bool>(ConfigurationSection.AgencyPublishCompleteProviderNotificationEnabled);

            _logger.LogTrace("Admin settings service getting agency publish complete provider notification from configuration... Done.");

            return agencyPublishCompleteProviderNotificationEnabled;
        }

        /// <inheritdoc/>
        public async Task<bool> SetAgencyPublishCompleteNotificationsStatus(bool isEnabled)
        {
            _logger.LogTrace($"Admin settings service setting agency publish complete provider notification to {isEnabled} in configuration...");

            var agencyPublishCompleteProviderNotificationEnabled = await UpdateConfiguration(ConfigurationSection.AgencyPublishCompleteProviderNotificationEnabled, isEnabled);

            _logger.LogTrace($"Admin settings service setting agency publish complete provider notification to {isEnabled} in configuration... Done.");

            return agencyPublishCompleteProviderNotificationEnabled;
        }

        /// <inheritdoc/>
        public async Task<int> GetMaxFileUploadSize()
        {
            _logger.LogTrace("Admin settings service getting maximum file size for upload from configuration...");

            var maxFileUploadSizeInKiloBytes = await _cosmosDbService.GetConfiguration<int>(ConfigurationSection.MaxFileUploadSize);

            _logger.LogTrace("Admin settings service getting maximum file size for upload from configuration... Done.");

            return maxFileUploadSizeInKiloBytes;
        }

        /// <inheritdoc/>
        public async Task<long> SetMaxFileUploadSize(long fileUploadSize)
        {
            _logger.LogTrace($"Admin settings service setting maximum file size for upload to {fileUploadSize} in configuration...");

            var maxFileUploadSize = await UpdateConfiguration(ConfigurationSection.MaxFileUploadSize, fileUploadSize);

            _logger.LogTrace($"Admin settings service setting maximum file size for upload to {fileUploadSize} in configuration... Done.");

            return maxFileUploadSize;
        }

        /// <inheritdoc/>
        public async Task<bool> GetCacheWarmUpRejectSpiInvalidData()
        {
            _logger.LogTrace("Admin settings service getting cache warm-up reject SPI invalid data from configuration...");

            var rejectInvalidData = await _cosmosDbService.GetConfiguration<bool>(ConfigurationSection.CacheWarmUpRejectSpiInvalidData);

            _logger.LogTrace("Admin settings service getting cache warm-up reject SPI invalid data from configuration... Done.");

            return rejectInvalidData;
        }

        private Task<IEnumerable<Product>> GetProductsInternal()
            => GetListConfiguration<Product, int>(ConfigurationSection.Products, product => product.Identifier);

        private async Task<IEnumerable<T>> GetListConfiguration<T, TProperty>(ConfigurationSection section, Func<T, TProperty> distinctKeySelector)
        {
            var listConfig = await _cosmosDbService.GetListConfiguration<T>(section);

            return It.IsNull(listConfig)
                ? Collection.Empty<T>()
                : listConfig.GroupBy(distinctKeySelector).Select(group => group.First()).ToList();
        }

        private async Task<T> UpdateConfiguration<T>(ConfigurationSection section, T newValue)
        {
            await _configSemaphore.WaitAsync();

            var result = await _cosmosDbService.UpdateConfiguration(section, newValue);

            var cacheService = _cacheManager.CacheService;
            var cacheKey = _cacheManager.CacheKeyBuilder.BuildConfigurationKey(section);
            var cacheOptions = _cacheManager.CacheOptionsProvider.Configuration;

            await cacheService.Remove(cacheKey, cacheOptions);

            _configSemaphore.Release();

            return result;
        }

        private async Task<T> AddOrUpdateListConfiguration<TIdentifier, T>(ConfigurationSection section, TIdentifier oldIdentifier, T newValue)
        {
            await _listConfigSemaphore.WaitAsync();

            var result = await _cosmosDbService.AddOrUpdateListConfiguration(section, oldIdentifier, newValue);

            var cacheService = _cacheManager.CacheService;
            var cacheKey = _cacheManager.CacheKeyBuilder.BuildListConfigurationKey(section);
            var cacheOptions = _cacheManager.CacheOptionsProvider.Configuration;

            await cacheService.Remove(cacheKey, cacheOptions);

            _listConfigSemaphore.Release();

            return result;
        }
    }
}