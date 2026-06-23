using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pds.Core.Caching.Interfaces;
using Pds.DocumentExchange.Data.Services.Implementations.Caching;
using Pds.DocumentExchange.Data.Services.Interfaces.Caching;
using System;

namespace Pds.DocumentExchange.Data.Services.Tests.Unit.Caching
{
    [TestClass]
    public class CacheManagerTests
    {
        [TestMethod]
        [DataRow(false, false, false)]
        [DataRow(true, false, false)]
        [DataRow(false, true, false)]
        [DataRow(false, false, true)]
        public void Constructor_PerformsNullChecks(bool cacheServiceNull, bool cacheKeyBuilderNull, bool cacheOptionsProviderNull)
        {
            // Arrange
            var cacheService = cacheServiceNull ? null : Mock.Of<ICacheService>();
            var cacheKeyBuilder = cacheKeyBuilderNull ? null : Mock.Of<ICacheKeyBuilder>();
            var cacheOptionsProvider = cacheOptionsProviderNull ? null : Mock.Of<ICacheOptionsProvider>();

            // Act
            Action act = () => new CacheManager(cacheService, cacheKeyBuilder, cacheOptionsProvider);

            // Assert
            if (cacheServiceNull || cacheKeyBuilderNull || cacheOptionsProviderNull)
            {
                act.Should().Throw<ArgumentNullException>();
            }
            else
            {
                act.Should().NotThrow();
            }
        }

        [TestMethod]
        public void Constructor_InitialisesProperties()
        {
            // Arrange
            var cacheService = Mock.Of<ICacheService>();
            var cacheKeyBuilder = Mock.Of<ICacheKeyBuilder>();
            var cacheOptionsProvider = Mock.Of<ICacheOptionsProvider>();

            // Act
            var cacheManager = new CacheManager(cacheService, cacheKeyBuilder, cacheOptionsProvider);

            // Assert
            cacheManager.CacheService.Should().Be(cacheService);
            cacheManager.CacheKeyBuilder.Should().Be(cacheKeyBuilder);
            cacheManager.CacheOptionsProvider.Should().Be(cacheOptionsProvider);
        }
    }
}
