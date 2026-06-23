using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pds.DocumentExchange.Data.Services.DTOs;
using Pds.DocumentExchange.Data.Services.Enums;
using Pds.DocumentExchange.Data.Services.Implementations.Filters;
using Pds.DocumentExchange.Data.Services.Interfaces.Filters;
using Pds.DocumentExchange.Data.Services.Interfaces.Lookup;
using Pds.DocumentExchange.Data.Services.Interfaces.Settings;
using Pds.Services.Common.Helpers;
using System;
using System.Collections.Generic;

namespace Pds.DocumentExchange.Data.Services.Tests.Unit.Filters
{
    [TestClass]
    public class ExchangeDocumentFiltersFactoryTests
    {
        private readonly Mock<IProductsLookup> _productsLookup = new Mock<IProductsLookup>(MockBehavior.Strict);
        private readonly Mock<IConfigurationDataService> _configurationDataService = new Mock<IConfigurationDataService>(MockBehavior.Strict);
        private readonly Mock<IOrganisationSubtypesLookup> _organisationSubtypesLookup = new Mock<IOrganisationSubtypesLookup>(MockBehavior.Strict);

        private readonly ExchangeDocumentFiltersFactory _filtersFactory;

        public ExchangeDocumentFiltersFactoryTests()
        {
            _filtersFactory = new ExchangeDocumentFiltersFactory(
                _productsLookup.Object,
                _configurationDataService.Object,
                _organisationSubtypesLookup.Object);
        }

        [TestMethod, TestCategory("Unit")]
        public void GetFilters_WhenFilterKeyIsNotValid_ThrowsArgumentException()
        {
            // Arrange
            var elements = Mock.Of<IEnumerable<ExchangeDocument>>();
            var invalidFilterKey = FilterKey.DocumentNameError;

            // Act
            Func<IEnumerable<IFilter<ExchangeDocument>>> func = () => _filtersFactory.GetFilters(elements, new[] { invalidFilterKey });

            // Assert
            func.Should().Throw<ArgumentException>();
        }

        [TestMethod, TestCategory("Unit")]
        public void GetFilters_WhenFilterKeyIsValid_ReturnsFilter()
        {
            // Act
            var result = _filtersFactory.GetFilters(Collection.Empty<ExchangeDocument>(), new[] { FilterKey.ProductIdList });

            // Assert
            result.Should().ContainSingle();
            result.Should().AllBeAssignableTo<IFilter<ExchangeDocument>>();
        }

        [TestMethod, TestCategory("Unit")]
        public void GetFilters_WhenFilterKeysAreValid_ReturnsFilters()
        {
            // Arrange
            var filterKeys = new[]
            {
                FilterKey.Status,
                FilterKey.Organisation,
                FilterKey.ProductIdList,
                FilterKey.AcademicYear,
                FilterKey.Team,
                FilterKey.ProviderType,
                FilterKey.UploadDate,
                FilterKey.Ukprn
            };

            // Act
            var result = _filtersFactory.GetFilters(Collection.Empty<ExchangeDocument>(), filterKeys);

            // Assert
            result.Should().HaveCount(8);
            result.Should().AllBeAssignableTo<IFilter<ExchangeDocument>>();
        }
    }
}