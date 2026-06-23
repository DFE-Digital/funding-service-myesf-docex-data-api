using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pds.Core.Logging;
using Pds.DocumentExchange.Data.Services.DTOs;
using Pds.DocumentExchange.Data.Services.Enums;
using Pds.DocumentExchange.Data.Services.Implementations.Settings;
using Pds.DocumentExchange.Data.Services.Interfaces;
using Pds.DocumentExchange.Data.Services.Tests.Unit.Caching;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Tests.Unit.Settings
{
    [TestClass, TestCategory("Unit")]
    public class ConfigurationDataServiceTests : BaseCacheTests
    {
        private readonly ICosmosDbService _cosmosDbService = Mock.Of<ICosmosDbService>(MockBehavior.Strict);
        private readonly ILoggerAdapter<ConfigurationDataService> _logger = Mock.Of<ILoggerAdapter<ConfigurationDataService>>(MockBehavior.Loose);

        private Mock<ICosmosDbService> MockCosmos => Mock.Get(_cosmosDbService);

        [TestMethod]
        [DataRow(0)]
        [DataRow(1)]
        [DataRow(100)]
        [DataRow(1000)]
        public async Task GetProducts_ReturnsDataFromCosmosDb(int numberOfProducts)
        {
            // Arrange
            var expected = Enumerable
                .Range(0, numberOfProducts)
                .Select(i => new Product
                {
                    AgencyTeams = Enumerable
                        .Range(0, i % 5)
                        .Select(j => $"team{j}"),
                    CanOrganisationsUpload = true,
                    Identifier = i,
                    Name = $"product{i}",
                    PluralName = $"products{i}"
                });

            MockCosmos
                .Setup(c => c.GetListConfiguration<Product>(ConfigurationSection.Products))
                .ReturnsAsync(expected);

            SetupCacheMocksForListConfig<IEnumerable<Product>>(ConfigurationSection.Products);

            var configurationDataService = GetTestService();

            // Act
            var actual = await configurationDataService.GetProducts();

            // Assert
            actual.Should().BeEquivalentTo(expected);

            MockCosmos.Verify();
            VerifyCacheMocks();
        }

        [TestMethod]
        [DataRow(0)]
        [DataRow(1)]
        [DataRow(100)]
        [DataRow(1000)]
        public async Task GetTeams_ReturnsDataFromCosmosDb(int numberOfTeams)
        {
            // Arrange
            var expected = Enumerable
                .Range(0, numberOfTeams)
                .Select(i => new AgencyTeam
                {
                    Identifier = i.ToString(),
                    EmailAddress = $"team{i}@teams.com",
                    Name = $"team{i}"
                });

            MockCosmos
                .Setup(c => c.GetListConfiguration<AgencyTeam>(ConfigurationSection.Teams))
                .ReturnsAsync(expected);

            SetupCacheMocksForListConfig<IEnumerable<AgencyTeam>>(ConfigurationSection.Teams);

            var configurationDataService = GetTestService();

            // Act
            var actual = await configurationDataService.GetTeams();

            // Assert
            actual.Should().BeEquivalentTo(expected);

            MockCosmos.Verify();
            VerifyCacheMocks();
        }

        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public async Task IsDocumentExchangeEnabled_ReturnsValueFromCosmosDb(bool expected)
        {
            // Arrange
            MockCosmos
                .Setup(c => c.GetConfiguration<bool>(ConfigurationSection.DocumentExchangeEnabled))
                .ReturnsAsync(expected);

            SetupCacheGetMocks<bool>(ConfigurationSection.DocumentExchangeEnabled);

            var configurationDataService = GetTestService();

            // Act
            var actual = await configurationDataService.IsDocumentExchangeEnabled();

            // Assert
            actual.Should().Be(expected);

            MockCosmos.Verify();
            VerifyCacheMocks();
        }

        [TestMethod]
        public async Task GetFileExtensions_ReturnsDataFromCosmosDb()
        {
            // Arrange
            var configurationDataService = GetTestService();

            var expectedExtensions = new[]
            {
                new FileExtensionInfo
                {
                    Identifier = "doc",
                    Extension = "doc",
                    ProductIdentifiers = new[] { 1000, 1001 }
                },
                new FileExtensionInfo
                {
                    Identifier = "xls",
                    Extension = "xls",
                    ProductIdentifiers = new[] { 1000 }
                },
                new FileExtensionInfo
                {
                    Identifier = "pdf",
                    Extension = "pdf",
                    ProductIdentifiers = Enumerable.Empty<int>()
                }
            };

            MockCosmos
                .Setup(db => db.GetListConfiguration<FileExtensionInfo>(ConfigurationSection.AllowedFileExtensions))
                .ReturnsAsync(expectedExtensions);

            SetupCacheMocksForListConfig<IEnumerable<FileExtensionInfo>>(ConfigurationSection.AllowedFileExtensions);

            // Act
            var result = await configurationDataService.GetFileExtensions();

            // Assert
            result.Should().BeEquivalentTo(expectedExtensions);
            MockCosmos.Verify();
            VerifyCacheMocks();
        }

        [TestMethod]
        public async Task GetServiceReplyEmail_ReturnsValueFromCosmosDb()
        {
            // Arrange
            var configurationDataService = GetTestService();

            var expectedServiceReplyEmail = "service.reply.email@education.gov.uk";

            MockCosmos
                .Setup(c => c.GetConfiguration<string>(ConfigurationSection.ServiceReplyEmail))
                .ReturnsAsync(expectedServiceReplyEmail);

            SetupCacheGetMocks<string>(ConfigurationSection.ServiceReplyEmail);

            // Act
            var actual = await configurationDataService.GetServiceReplyEmail();

            // Assert
            actual.Should().Be(expectedServiceReplyEmail);

            MockCosmos.Verify();
            VerifyCacheMocks();
        }

        [TestMethod]
        public async Task GetServiceNoReplyEmail_ReturnsValueFromCosmosDb()
        {
            // Arrange
            var configurationDataService = GetTestService();

            var expectedServiceNoReplyEmail = "service.no-reply.email@education.gov.uk";

            MockCosmos
                .Setup(c => c.GetConfiguration<string>(ConfigurationSection.ServiceNoReplyEmail))
                .ReturnsAsync(expectedServiceNoReplyEmail);

            SetupCacheGetMocks<string>(ConfigurationSection.ServiceNoReplyEmail);

            // Act
            var actual = await configurationDataService.GetServiceNoReplyEmail();

            // Assert
            actual.Should().Be(expectedServiceNoReplyEmail);

            MockCosmos.Verify();
            VerifyCacheMocks();
        }

        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public async Task IsAgencyPublishCompleteProviderNotificationEnabled_ReturnsValueFromCosmosDb(bool expected)
        {
            // Arrange
            MockCosmos
                .Setup(c => c.GetConfiguration<bool>(ConfigurationSection.AgencyPublishCompleteProviderNotificationEnabled))
                .ReturnsAsync(expected);

            SetupCacheGetMocks<bool>(ConfigurationSection.AgencyPublishCompleteProviderNotificationEnabled);

            var configurationDataService = GetTestService();

            // Act
            var actual = await configurationDataService.IsAgencyPublishCompleteProviderNotificationEnabled();

            // Assert
            actual.Should().Be(expected);

            MockCosmos.Verify();
            VerifyCacheMocks();
        }

        [TestMethod]
        [DataRow(1)]
        [DataRow(100)]
        public async Task GetMaxFileUploadSize_ReturnsValueFromCosmosDb(int expected)
        {
            // Arrange
            MockCosmos
                .Setup(c => c.GetConfiguration<int>(ConfigurationSection.MaxFileUploadSize))
                .ReturnsAsync(expected);

            SetupCacheGetMocks<int>(ConfigurationSection.MaxFileUploadSize);

            var configurationDataService = GetTestService();

            // Act
            var actual = await configurationDataService.GetMaxFileUploadSize();

            // Assert
            actual.Should().Be(expected);

            MockCosmos.Verify();
            VerifyCacheMocks();
        }

        private ConfigurationDataService GetTestService()
        {
            return new ConfigurationDataService(_cosmosDbService, CacheManager, _logger);
        }

        private void SetupCacheGetMocks<TConfig>(ConfigurationSection section)
        {
            SetupCacheGetMocks<TConfig>(
                cacheOptionsProvider => cacheOptionsProvider.Configuration,
                cacheKeyBuilder => cacheKeyBuilder.BuildConfigurationKey(section));
        }

        private void SetupCacheMocksForListConfig<TConfig>(ConfigurationSection section)
        {
            SetupCacheGetMocks<TConfig>(
                cacheOptionsProvider => cacheOptionsProvider.Configuration,
                cacheKeyBuilder => cacheKeyBuilder.BuildListConfigurationKey(section));
        }
    }
}