using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pds.Core.Common.Organisation.Enums;
using Pds.Core.Common.Organisation.Models;
using Pds.DocumentExchange.Data.Services.DTOs.User;
using Pds.DocumentExchange.Data.Services.Implementations.Lookup;
using Pds.DocumentExchange.Data.Services.Tests.Unit.Caching;
using System;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Tests.Unit.Lookup
{
    [TestClass, TestCategory("Unit")]
    public class OrganisationSubtypesLookupTests : BaseCacheTests
    {
        private readonly OrganisationSubtypesLookup _organisationSubTypesLookup;

        public OrganisationSubtypesLookupTests()
        {
            _organisationSubTypesLookup = new OrganisationSubtypesLookup(CacheManager);
        }

        #region Exists

        [TestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow(" ")]
        public void Exists_WhenIdentifierIsNullOrEmpty_Throws(string identifier)
        {
            // Act
            Func<Task<bool>> func = () => _organisationSubTypesLookup.Exists(identifier);

            // Assert
            func.Should().ThrowAsync<ArgumentNullException>();
        }

        [TestMethod]
        [DataRow("non-existing-identifier")]
        [DataRow("Organisation subtype 0")]
        [DataRow("Organisation subtype 15")]
        [DataRow("Organisation subtype -99")]
        public async Task Exists_WhenIdentifierDoesntExist_ReturnsFalse(string identifier)
        {
            // Arrange
            SetupCacheGetMocks<DisplayValues>(
                cacheOptionsProvider => cacheOptionsProvider.OrganisationTypes,
                cacheKeyBuilder => cacheKeyBuilder.BuildOrganisationSubtypeDisplayKey(identifier));

            // Act
            var result = await _organisationSubTypesLookup.Exists(identifier);

            // Assert
            result.Should().BeFalse();
            VerifyCacheMocks();
        }

        [TestMethod]
        [DataRow(1)]
        [DataRow(2)]
        [DataRow(5)]
        [DataRow(10)]
        public async Task Exists_WhenIdentifierExists_ReturnsTrue(int number)
        {
            // Arrange
            var identifier = $"Organisation subtype {number}";

            SetupCacheGetMock(
                cacheOptionsProvider => cacheOptionsProvider.OrganisationTypes,
                cacheKeyBuilder => cacheKeyBuilder.BuildOrganisationSubtypeDisplayKey(identifier),
                new DisplayValues
                {
                    Singular = "Existing type display singular",
                    Plural = "Existing type display plural"
                });

            // Act
            var result = await _organisationSubTypesLookup.Exists(identifier);

            // Assert
            result.Should().BeTrue();
            VerifyCacheMocks();
        }

        #endregion


        #region Get

        [TestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow(" ")]
        public void Get_WhenIdentifierIsNullOrEmpty_Throws(string identifier)
        {
            // Act
            Func<Task<DisplayValues>> func = () => _organisationSubTypesLookup.Get(identifier);

            // Assert
            func.Should().ThrowAsync<ArgumentNullException>();
        }

        [TestMethod]
        [DataRow("non-existing-identifier")]
        [DataRow("Organisation subtype 0")]
        [DataRow("Organisation subtype 15")]
        [DataRow("Organisation subtype -99")]
        public async Task Get_WhenIdentifierDoesntExist_ReturnsUnknownOrganisationTypeDisplay(string identifier)
        {
            // Arrange
            SetupCacheGetMocks<DisplayValues>(
                cacheOptionsProvider => cacheOptionsProvider.OrganisationTypes,
                cacheKeyBuilder => cacheKeyBuilder.BuildOrganisationSubtypeDisplayKey(identifier));

            // Act
            var result = await _organisationSubTypesLookup.Get(identifier);

            // Assert
            result.Should().BeOfType<UnknownOrganisationTypeDisplay>();
            VerifyCacheMocks();
        }

        [TestMethod]
        [DataRow(1)]
        [DataRow(2)]
        [DataRow(5)]
        [DataRow(10)]
        public async Task Get_WhenIdentifierExists_ReturnsOrganisationTypeDisplay(int number)
        {
            // Arrange
            var identifier = $"Organisation subtype {number}";

            var expectedResult = new DisplayValues
            {
                Singular = $"Organisation subtype {number} singular display",
                Plural = $"Organisation subtype {number} plural display"
            };

            SetupCacheGetMock(
                cacheOptionsProvider => cacheOptionsProvider.OrganisationTypes,
                cacheKeyBuilder => cacheKeyBuilder.BuildOrganisationSubtypeDisplayKey(identifier),
                expectedResult);

            // Act
            var result = await _organisationSubTypesLookup.Get(identifier);

            // Assert
            result.Should().BeEquivalentTo(expectedResult);
            VerifyCacheMocks();
        }

        #endregion


        #region GetOrganisationTypeDisplay

        [TestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow(" ")]
        public void GetOrganisationTypeDisplay_WhenIdentifierIsNullOrEmpty_Throws(string identifier)
        {
            // Act
            Func<Task<DisplayValues>> func = () => _organisationSubTypesLookup.GetOrganisationTypeDisplay(identifier);

            // Assert
            func.Should().ThrowAsync<ArgumentNullException>();
        }

        [TestMethod]
        [DataRow("non-existing-identifier")]
        [DataRow("Organisation subtype 0")]
        [DataRow("Organisation subtype 15")]
        [DataRow("Organisation subtype -99")]
        public async Task GetOrganisationTypeDisplay_WhenIdentifierDoesntExist_ReturnsUnknownOrganisationTypeDisplay(string identifier)
        {
            // Arrange
            SetupCacheGetMocks<DisplayValues>(
                cacheOptionsProvider => cacheOptionsProvider.OrganisationTypes,
                cacheKeyBuilder => cacheKeyBuilder.BuildOrganisationTypeDisplayBySubtypeKey(identifier));

            // Act
            var result = await _organisationSubTypesLookup.GetOrganisationTypeDisplay(identifier);

            // Assert
            result.Should().BeOfType<UnknownOrganisationTypeDisplay>();
            VerifyCacheMocks();
        }

        [TestMethod]
        [DataRow(1)]
        [DataRow(2)]
        [DataRow(5)]
        [DataRow(10)]
        public async Task GetOrganisationTypeDisplay_WhenIdentifierExists_ReturnsOrganisationTypeDisplay(int number)
        {
            // Arrange
            var identifier = $"Organisation subtype {number}";

            var expectedResult = new DisplayValues
            {
                Singular = $"Organisation type {number} singular display",
                Plural = $"Organisation type {number} plural display"
            };

            SetupCacheGetMock(
                cacheOptionsProvider => cacheOptionsProvider.OrganisationTypes,
                cacheKeyBuilder => cacheKeyBuilder.BuildOrganisationTypeDisplayBySubtypeKey(identifier),
                expectedResult);

            // Act
            var result = await _organisationSubTypesLookup.GetOrganisationTypeDisplay(identifier);

            // Assert
            result.Should().BeEquivalentTo(expectedResult);
            VerifyCacheMocks();
        }

        #endregion


        #region LoadOrganisationDisplays

        [TestMethod]
        public void LoadOrganisationDisplays_WhenOrganisationIsNull_Throws()
        {
            // Act
            Func<Task> func = () => _organisationSubTypesLookup.LoadOrganisationDisplays(null);

            // Assert
            func.Should().ThrowAsync<ArgumentNullException>();
        }

        [TestMethod]
        [DataRow(1)]
        [DataRow(2)]
        [DataRow(5)]
        [DataRow(10)]
        public async Task LoadOrganisationDisplays_OrganisationTypeDisplayGetsLoaded(int number)
        {
            // Arrange
            var organisation = CreateOrganisation(number);

            SetupCacheSetMocks<DisplayValues>(
                  cacheOptionsProvider => cacheOptionsProvider.OrganisationTypes,
                  cacheKeyBuilder => cacheKeyBuilder.BuildOrganisationSubtypeDisplayKey(organisation.OrganisationSubType),
                  cacheKeyBuilder => cacheKeyBuilder.BuildOrganisationTypeDisplayBySubtypeKey(organisation.OrganisationSubType));

            // Act
            await _organisationSubTypesLookup.LoadOrganisationDisplays(organisation);

            // Assert
            VerifyCacheMocks();
        }

        #endregion

        private Organisation CreateOrganisation(int number)
            => new Organisation
            {
                Identifiers = new[] { new OrganisationIdentifier { Type = OrganisationIdentifierType.Ukprn, Value = number.ToString() } },
                Name = $"Organisation {number}",
                OrganisationType = $"Organisation type {number}",
                OrganisationTypeDisplay = new DisplayValues
                {
                    Singular = $"Organisation type {number} singular display",
                    Plural = $"Organisation type {number} plural display"
                },
                OrganisationSubType = $"Organisation subtype {number}",
                OrganisationSubTypeDisplay = new DisplayValues
                {
                    Singular = $"Organisation subtype {number} singular display",
                    Plural = $"Organisation subtype {number} plural display"
                }
            };
    }
}