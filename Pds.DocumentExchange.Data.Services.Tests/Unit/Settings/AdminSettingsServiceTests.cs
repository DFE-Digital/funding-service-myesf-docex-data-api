using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pds.Core.Logging;
using Pds.Core.Utils.Interfaces;
using Pds.DocumentExchange.Data.Services.DTOs;
using Pds.DocumentExchange.Data.Services.Enums;
using Pds.DocumentExchange.Data.Services.Implementations.Settings;
using Pds.DocumentExchange.Data.Services.Interfaces;
using Pds.DocumentExchange.Data.Services.Tests.Unit.Caching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Tests.Unit.Settings
{
    [TestClass, TestCategory("Unit")]
    public class AdminSettingsServiceTests : BaseCacheTests
    {
        private readonly Mock<ICosmosDbService> _cosmosDbService = new Mock<ICosmosDbService>(MockBehavior.Strict);
        private readonly Mock<IDateTimeProvider> _mockDateTime = new Mock<IDateTimeProvider>(MockBehavior.Strict);
        private readonly Mock<ILoggerAdapter<AdminSettingsService>> _logger = new Mock<ILoggerAdapter<AdminSettingsService>>(MockBehavior.Loose);

        private readonly AdminSettingsService _adminSettingsService;

        public AdminSettingsServiceTests()
        {
            _adminSettingsService = new AdminSettingsService(
                _cosmosDbService.Object,
                CacheManager,
                _mockDateTime.Object,
                logger: _logger.Object);
        }

        #region Products

        [TestMethod]
        public async Task GetProductsThatOrganisationsCanUpload_WhenCosmosDbReturnsNull_ReturnsEmptyList()
        {
            // Arrange
            _cosmosDbService
                .Setup(db => db.GetListConfiguration<Product>(ConfigurationSection.Products))
                .ReturnsAsync(null as IEnumerable<Product>);

            var expectedProducts = Enumerable.Empty<Product>();

            // Act
            var result = await _adminSettingsService.GetProductsThatOrganisationsCanUpload();

            // Assert
            result.Should().BeEquivalentTo(expectedProducts);
            VerifyMocks();
        }

        [TestMethod]
        public async Task GetProductsThatOrganisationsCanUpload_WhenCosmosDbReturnsEmpty_ReturnsEmptyList()
        {
            // Arrange
            _cosmosDbService
                .Setup(db => db.GetListConfiguration<Product>(ConfigurationSection.Products))
                .ReturnsAsync(Enumerable.Empty<Product>());

            var expectedProducts = Enumerable.Empty<Product>();

            // Act
            var result = await _adminSettingsService.GetProductsThatOrganisationsCanUpload();

            // Assert
            result.Should().BeEquivalentTo(expectedProducts);
            VerifyMocks();
        }

        [TestMethod]
        public async Task GetProductsThatOrganisationsCanUpload_WhenCosmosDbHasProducts_ReturnsProducts()
        {
            // Arrange
            var allProducts = GetProducts();

            _cosmosDbService
                .Setup(db => db.GetListConfiguration<Product>(ConfigurationSection.Products))
                .ReturnsAsync(allProducts);

            var expectedProducts = allProducts.Where(p => p.CanOrganisationsUpload);

            // Act
            var result = await _adminSettingsService.GetProductsThatOrganisationsCanUpload();

            // Assert
            result.Should().BeEquivalentTo(expectedProducts);
            VerifyMocks();
        }

        [TestMethod]
        public async Task GetProducts_WhenCosmosDbReturnsNull_ReturnsEmptyList()
        {
            // Arrange
            _cosmosDbService
                .Setup(db => db.GetListConfiguration<Product>(ConfigurationSection.Products))
                .ReturnsAsync(null as IEnumerable<Product>);

            var expectedProducts = Enumerable.Empty<Product>();

            // Act
            var result = await _adminSettingsService.GetProducts();

            // Assert
            result.Should().BeEquivalentTo(expectedProducts);
            VerifyMocks();
        }

        [TestMethod]
        public async Task GetProducts_WhenCosmosDbReturnsEmpty_ReturnsEmptyList()
        {
            // Arrange
            _cosmosDbService
                .Setup(db => db.GetListConfiguration<Product>(ConfigurationSection.Products))
                .ReturnsAsync(Enumerable.Empty<Product>());

            var expectedProducts = Enumerable.Empty<Product>();

            // Act
            var result = await _adminSettingsService.GetProducts();

            // Assert
            result.Should().BeEquivalentTo(expectedProducts);
            VerifyMocks();
        }

        [TestMethod]
        public async Task GetProducts_WhenCosmosDbHasProducts_ReturnsProducts()
        {
            // Arrange
            var allProducts = GetProducts();

            _cosmosDbService
                .Setup(db => db.GetListConfiguration<Product>(ConfigurationSection.Products))
                .ReturnsAsync(allProducts);

            // Act
            var result = await _adminSettingsService.GetProducts();

            // Assert
            result.Should().BeEquivalentTo(allProducts);
            VerifyMocks();
        }

        [TestMethod]
        [DataRow(1)]
        [DataRow(5)]
        [DataRow(10)]
        public async Task GetProduct_WhenProductExists_Returns(int productId)
        {
            // Arrange
            var allProducts = GetProducts();

            _cosmosDbService
                .Setup(db => db.GetListConfiguration<Product>(ConfigurationSection.Products))
                .ReturnsAsync(allProducts);

            var expectedProduct = allProducts.First(product => product.Identifier == productId);

            // Act
            var result = await _adminSettingsService.GetProduct(productId);

            // Assert
            result.Should().BeEquivalentTo(expectedProduct);
            VerifyMocks();
        }

        [TestMethod]
        [DataRow(99)]
        [DataRow(-1)]
        [DataRow(1000)]
        public async Task GetProduct_WhenProductDoesntExists_ReturnsUnknown(int productId)
        {
            // Arrange
            var allProducts = GetProducts();

            _cosmosDbService
                .Setup(db => db.GetListConfiguration<Product>(ConfigurationSection.Products))
                .ReturnsAsync(allProducts);

            var expectedProduct = new UnknownProduct();

            // Act
            var result = await _adminSettingsService.GetProduct(productId);

            // Assert
            result.Should().BeEquivalentTo(expectedProduct);
            VerifyMocks();
        }

        [TestMethod]
        public async Task AddOrUpdateProduct_UpdatesCosmosDb()
        {
            // Arrange
            var productId = 12345;

            var product = new Product
            {
                AgencyTeams = new[] { "team" },
                CanOrganisationsUpload = true,
                Identifier = productId,
                Name = $"the-new-product",
                PluralName = $"the-new-products"
            };

            var lastUpdated = new DateTime(2020, 1, 1, 12, 34, 56);

            _cosmosDbService
                .Setup(db => db.AddOrUpdateListConfiguration(ConfigurationSection.Products, productId, product))
                .ReturnsAsync(product);

            _mockDateTime
                .Setup(dt => dt.UtcNow())
                .Returns(lastUpdated);

            SetupCacheRemoveMocksForListConfig(ConfigurationSection.Products);

            // Act
            var result = await _adminSettingsService.AddOrUpdateProduct(productId, product);

            // Assert
            result.Should().BeEquivalentTo(product);
            product.LastUpdated.Should().Be(lastUpdated);
            VerifyMocks();
        }

        #endregion


        #region Teams

        [TestMethod]
        public async Task GetTeams_WhenCosmosDbReturnsNull_ReturnsEmptyList()
        {
            // Arrange
            _cosmosDbService
                .Setup(db => db.GetListConfiguration<AgencyTeam>(ConfigurationSection.Teams))
                .ReturnsAsync(null as IEnumerable<AgencyTeam>);

            var expectedTeams = Enumerable.Empty<AgencyTeam>();

            // Act
            var result = await _adminSettingsService.GetTeams();

            // Assert
            result.Should().BeEquivalentTo(expectedTeams);
            VerifyMocks();
        }

        [TestMethod]
        public async Task GetTeams_WhenCosmosDbReturnsEmpty_ReturnsEmptyList()
        {
            // Arrange
            _cosmosDbService
                .Setup(db => db.GetListConfiguration<AgencyTeam>(ConfigurationSection.Teams))
                .ReturnsAsync(Enumerable.Empty<AgencyTeam>());

            var expectedTeams = Enumerable.Empty<AgencyTeam>();

            // Act
            var result = await _adminSettingsService.GetTeams();

            // Assert
            result.Should().BeEquivalentTo(expectedTeams);
            VerifyMocks();
        }

        [TestMethod]
        public async Task GetTeams_WhenCosmosDbHasTeams_ReturnsTeams()
        {
            // Arrange
            var teams = Enumerable.Range(1, 5)
                .Select(i => new AgencyTeam
                {
                    Identifier = i.ToString(),
                    EmailAddress = $"team-{i}@education.gov.uk",
                    Name = $"team-{i}"
                });

            _cosmosDbService
                .Setup(db => db.GetListConfiguration<AgencyTeam>(ConfigurationSection.Teams))
                .ReturnsAsync(teams);

            // Act
            var result = await _adminSettingsService.GetTeams();

            // Assert
            result.Should().BeEquivalentTo(teams);
            VerifyMocks();
        }

        [TestMethod]
        public async Task AddOrUpdateTeam_UpdatesCosmosDb()
        {
            // Arrange
            var teamId = "identifier";

            var team = new AgencyTeam
            {
                Identifier = teamId,
                Name = $"the-team",
                EmailAddress = $"the-team@education.gov.uk"
            };

            _cosmosDbService
                .Setup(db => db.AddOrUpdateListConfiguration(ConfigurationSection.Teams, teamId, team))
                .ReturnsAsync(team);

            SetupCacheRemoveMocksForListConfig(ConfigurationSection.Teams);

            // Act
            var actual = await _adminSettingsService.AddOrUpdateTeam(teamId, team);

            // Assert
            actual.Should().BeEquivalentTo(team);
            VerifyMocks();
        }

        #endregion


        #region File extensions

        [TestMethod]
        public async Task GetFileExtensions_WhenCosmosDbReturnsNull_ReturnsEmptyList()
        {
            // Arrange
            _cosmosDbService
                .Setup(db => db.GetListConfiguration<FileExtensionInfo>(ConfigurationSection.AllowedFileExtensions))
                .ReturnsAsync(null as IEnumerable<FileExtensionInfo>);

            var expectedFileExtensions = Enumerable.Empty<FileExtensionInfo>();

            // Act
            var result = await _adminSettingsService.GetFileExtensions();

            // Assert
            result.Should().BeEquivalentTo(expectedFileExtensions);
            VerifyMocks();
        }

        [TestMethod]
        public async Task GetFileExtensions_WhenCosmosDbReturnsEmpty_ReturnsEmptyList()
        {
            // Arrange
            _cosmosDbService
                .Setup(db => db.GetListConfiguration<FileExtensionInfo>(ConfigurationSection.AllowedFileExtensions))
                .ReturnsAsync(Enumerable.Empty<FileExtensionInfo>());

            var expectedFileExtensions = Enumerable.Empty<FileExtensionInfo>();

            // Act
            var result = await _adminSettingsService.GetFileExtensions();

            // Assert
            result.Should().BeEquivalentTo(expectedFileExtensions);
            VerifyMocks();
        }

        [TestMethod]
        public async Task GetFileExtensions_WhenCosmosDbHasExtensions_ReturnsFileExtensions()
        {
            // Arrange
            var extensions = new[] { "doc", "docx", "xls", "xlsx", "pdf" }
                .Select(currentExtension => new FileExtensionInfo
                {
                    Identifier = currentExtension,
                    Extension = currentExtension
                });

            _cosmosDbService
                .Setup(db => db.GetListConfiguration<FileExtensionInfo>(ConfigurationSection.AllowedFileExtensions))
                .ReturnsAsync(extensions);

            // Act
            var result = await _adminSettingsService.GetFileExtensions();

            // Assert
            result.Should().BeEquivalentTo(extensions);
            VerifyMocks();
        }

        [TestMethod]
        public async Task AddOrUpdateFileExtension_UpdatesCosmosDb()
        {
            // Arrange
            var extensionId = "identifier";

            var newExtension = new FileExtensionInfo
            {
                Identifier = extensionId,
                Extension = "xyz",
                ProductIdentifiers = new[] { 12345, 67890 }
            };

            _cosmosDbService
                .Setup(db => db.AddOrUpdateListConfiguration(ConfigurationSection.AllowedFileExtensions, extensionId, newExtension))
                .ReturnsAsync(newExtension);

            SetupCacheRemoveMocksForListConfig(ConfigurationSection.AllowedFileExtensions);

            // Act
            var result = await _adminSettingsService.AddOrUpdateFileExtension(extensionId, newExtension);

            // Assert
            result.Should().BeEquivalentTo(newExtension);
            VerifyMocks();
        }

        #endregion


        #region Emails

        [TestMethod]
        public async Task GetServiceReplyEmail_ReturnsValueFromCosmosDb()
        {
            // Arrange
            var expectedServiceReplyEmail = "service.reply.email@education.gov.uk";

            _cosmosDbService
                .Setup(c => c.GetConfiguration<string>(ConfigurationSection.ServiceReplyEmail))
                .ReturnsAsync(expectedServiceReplyEmail);

            // Act
            var result = await _adminSettingsService.GetServiceReplyEmail();

            // Assert
            result.Should().Be(expectedServiceReplyEmail);
            VerifyMocks();
        }

        [TestMethod]
        public async Task SetServiceReplyEmail_UpdatesCosmosDb()
        {
            // Arrange
            var expectedServiceReplyEmail = "service.reply.email@education.gov.uk";

            _cosmosDbService
                .Setup(c => c.UpdateConfiguration(ConfigurationSection.ServiceReplyEmail, expectedServiceReplyEmail))
                .ReturnsAsync(expectedServiceReplyEmail);

            SetupCacheRemoveMocks(ConfigurationSection.ServiceReplyEmail);

            // Act
            var result = await _adminSettingsService.SetServiceReplyEmail(expectedServiceReplyEmail);

            // Assert
            result.Should().Be(expectedServiceReplyEmail);
            VerifyMocks();
        }

        [TestMethod]
        public async Task GetServiceNoReplyEmail_ReturnsValueFromCosmosDb()
        {
            // Arrange
            var expectedServiceNoReplyEmail = "service.no-reply.email@education.gov.uk";

            _cosmosDbService
                .Setup(c => c.GetConfiguration<string>(ConfigurationSection.ServiceNoReplyEmail))
                .ReturnsAsync(expectedServiceNoReplyEmail);

            // Act
            var result = await _adminSettingsService.GetServiceNoReplyEmail();

            // Assert
            result.Should().Be(expectedServiceNoReplyEmail);
            VerifyMocks();
        }

        [TestMethod]
        public async Task SetServiceNoReplyEmail_UpdatesCosmosDb()
        {
            // Arrange
            var expectedServiceNoReplyEmail = "service.no-reply.email@education.gov.uk";

            _cosmosDbService
                .Setup(c => c.UpdateConfiguration(ConfigurationSection.ServiceNoReplyEmail, expectedServiceNoReplyEmail))
                .ReturnsAsync(expectedServiceNoReplyEmail);

            SetupCacheRemoveMocks(ConfigurationSection.ServiceNoReplyEmail);

            // Act
            var result = await _adminSettingsService.SetServiceNoReplyEmail(expectedServiceNoReplyEmail);

            // Assert
            result.Should().Be(expectedServiceNoReplyEmail);
            VerifyMocks();
        }

        #endregion

        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public async Task IsDocumentExchangeEnabled_ReturnsValueFromCosmosDb(bool expected)
        {
            // Arrange
            _cosmosDbService
                .Setup(db => db.GetConfiguration<bool>(ConfigurationSection.DocumentExchangeEnabled))
                .ReturnsAsync(expected);

            // Act
            var result = await _adminSettingsService.IsDocumentExchangeEnabled();

            // Assert
            result.Should().Be(expected);
            VerifyMocks();
        }

        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public async Task SetDocumentExchangeEnabledStatus_UpdatesCosmosDb(bool expected)
        {
            // Arrange
            _cosmosDbService
                .Setup(db => db.UpdateConfiguration(ConfigurationSection.DocumentExchangeEnabled, expected))
                .ReturnsAsync(expected);

            SetupCacheRemoveMocks(ConfigurationSection.DocumentExchangeEnabled);

            // Act
            var result = await _adminSettingsService.SetDocumentExchangeEnabledStatus(expected);

            // Assert
            result.Should().Be(expected);
            VerifyMocks();
        }

        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public async Task IsOrganisationUploadEnabled_ReturnsValueFromCosmosDb(bool expected)
        {
            // Arrange
            _cosmosDbService
                .Setup(db => db.GetConfiguration<bool>(ConfigurationSection.DocumentExchangeOrganisationUploadEnabled))
                .ReturnsAsync(expected);

            // Act
            var result = await _adminSettingsService.IsOrganisationUploadEnabled();

            // Assert
            result.Should().Be(expected);
            VerifyMocks();
        }

        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public async Task SetOrganisationUploadEnabledStatus_UpdatesTheConfigurationAndClearsTheCache(bool expected)
        {
            // Arrange
            _cosmosDbService
                .Setup(db => db.UpdateConfiguration(ConfigurationSection.DocumentExchangeOrganisationUploadEnabled, expected))
                .ReturnsAsync(expected);

            SetupCacheRemoveMocks(ConfigurationSection.DocumentExchangeOrganisationUploadEnabled);

            // Act
            var result = await _adminSettingsService.SetOrganisationUploadEnabledStatus(expected);

            // Assert
            result.Should().Be(expected);
            VerifyMocks();
        }

        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public async Task IsAgencyPublishCompleteProviderNotificationEnabled_ReturnsValueFromCosmosDb(bool expected)
        {
            // Arrange
            _cosmosDbService
                .Setup(db => db.GetConfiguration<bool>(ConfigurationSection.AgencyPublishCompleteProviderNotificationEnabled))
                .ReturnsAsync(expected);

            // Act
            var result = await _adminSettingsService.IsAgencyPublishCompleteProviderNotificationEnabled();

            // Assert
            result.Should().Be(expected);
            VerifyMocks();
        }

        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public async Task SetAgencyPublishCompleteProviderNotificationEnabledStatus_UpdatesCosmosDb(bool expected)
        {
            // Arrange
            _cosmosDbService
                .Setup(db => db.UpdateConfiguration(ConfigurationSection.AgencyPublishCompleteProviderNotificationEnabled, expected))
                .ReturnsAsync(expected);

            SetupCacheRemoveMocks(ConfigurationSection.AgencyPublishCompleteProviderNotificationEnabled);

            // Act
            var result = await _adminSettingsService.SetAgencyPublishCompleteNotificationsStatus(expected);

            // Assert
            result.Should().Be(expected);
            VerifyMocks();
        }

        [TestMethod]
        [DataRow(1)]
        [DataRow(100)]
        public async Task GetMaxFileUploadSize_ReturnsValueFromCosmosDb(int expected)
        {
            // Arrange
            _cosmosDbService
                .Setup(db => db.GetConfiguration<int>(ConfigurationSection.MaxFileUploadSize))
                .ReturnsAsync(expected);

            // Act
            var result = await _adminSettingsService.GetMaxFileUploadSize();

            // Assert
            result.Should().Be(expected);
            VerifyMocks();
        }

        [TestMethod]
        [DataRow(1)]
        [DataRow(100)]
        public async Task SetMaxFileUploadSize_UpdatesCosmosDb(long expected)
        {
            // Arrange
            _cosmosDbService
                .Setup(db => db.UpdateConfiguration(ConfigurationSection.MaxFileUploadSize, expected))
                .ReturnsAsync(expected);

            SetupCacheRemoveMocks(ConfigurationSection.MaxFileUploadSize);

            // Act
            var result = await _adminSettingsService.SetMaxFileUploadSize(expected);

            // Assert
            result.Should().Be(expected);
            VerifyMocks();
        }

        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public async Task GetCacheWarmUpRejectSpiInvalidData_ReturnsTheSettingValue(bool expected)
        {
            // Arrange
            _cosmosDbService
                .Setup(db => db.GetConfiguration<bool>(ConfigurationSection.CacheWarmUpRejectSpiInvalidData))
                .ReturnsAsync(expected);

            // Act
            var result = await _adminSettingsService.GetCacheWarmUpRejectSpiInvalidData();

            // Assert
            result.Should().Be(expected);
            VerifyMocks();
        }

        private IEnumerable<Product> GetProducts()
            => Enumerable.Range(1, 10)
                .Select(i => new Product
                {
                    Identifier = i,
                    AgencyTeams = new[] { "team-1", "team-2" },
                    CanOrganisationsUpload = i % 2 == 0,
                    Name = $"product{i}",
                    PluralName = $"products{i}"
                });

        private void SetupCacheRemoveMocks(ConfigurationSection section)
        {
            SetupCacheRemoveMocks(
                cacheOptionsProvider => cacheOptionsProvider.Configuration,
                cacheKeyBuilder => cacheKeyBuilder.BuildConfigurationKey(section));
        }

        private void SetupCacheRemoveMocksForListConfig(ConfigurationSection section)
        {
            SetupCacheRemoveMocks(
                cacheOptionsProvider => cacheOptionsProvider.Configuration,
                cacheKeyBuilder => cacheKeyBuilder.BuildListConfigurationKey(section));
        }

        private void VerifyMocks()
        {
            VerifyCacheMocks();
            Mock.VerifyAll(_cosmosDbService, _mockDateTime, _logger);
        }
    }
}