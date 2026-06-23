using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pds.DocumentExchange.Data.Services.DTOs;
using Pds.DocumentExchange.Data.Services.Enums;
using Pds.DocumentExchange.Data.Services.Implementations.Filters;
using Pds.DocumentExchange.Data.Services.Interfaces.Converters;
using Pds.DocumentExchange.Data.Services.Interfaces.Filters;
using Pds.DocumentExchange.Data.Services.Interfaces.Lookup;
using Pds.DocumentExchange.Data.Services.Interfaces.Settings;
using Pds.Services.Common.Helpers;
using System;
using System.Collections.Generic;

namespace Pds.DocumentExchange.Data.Services.Tests.Unit.Filters
{
    [TestClass]
    public class AgencyDocumentFiltersFactoryTests
    {
        private readonly Mock<IProductsLookup> _productsLookup = new Mock<IProductsLookup>(MockBehavior.Strict);
        private readonly Mock<IConfigurationDataService> _configurationDataService = new Mock<IConfigurationDataService>(MockBehavior.Strict);
        private readonly Mock<IConvertAgencyDocumentErrorTypes> _converter = new Mock<IConvertAgencyDocumentErrorTypes>(MockBehavior.Strict);

        private readonly AgencyDocumentFiltersFactory _agencyFiltersFactory;

        public AgencyDocumentFiltersFactoryTests()
        {
            _agencyFiltersFactory = new AgencyDocumentFiltersFactory(
                _productsLookup.Object,
                _configurationDataService.Object,
                _converter.Object);
        }

        [TestMethod, TestCategory("Unit")]
        public void GetFilters_WhenFilterKeyIsNotValid_ThrowsArgumentException()
        {
            // Arrange
            var elements = Mock.Of<IEnumerable<AgencyDocument>>();
            var invalidFilterKey = FilterKey.AcademicYear;

            // Act
            Func<IEnumerable<IFilter<AgencyDocument>>> func = () => _agencyFiltersFactory.GetFilters(elements, new[] { invalidFilterKey });

            // Assert
            func.Should().Throw<ArgumentException>();
        }

        [TestMethod, TestCategory("Unit")]
        public void GetFilters_WhenFilterKeyIsValid_ReturnsFilter()
        {
            // Act
            var result = _agencyFiltersFactory.GetFilters(Collection.Empty<AgencyDocument>(), new[] { FilterKey.ProductIdRadio });

            // Assert
            result.Should().ContainSingle();
            result.Should().AllBeAssignableTo<IFilter<AgencyDocument>>();
        }

        [TestMethod, TestCategory("Unit")]
        public void GetFilters_WhenFilterKeysAreValid_ReturnsFilters()
        {
            // Act
            var result = _agencyFiltersFactory.GetFilters(Collection.Empty<AgencyDocument>(), new[] { FilterKey.ProductIdList, FilterKey.DocumentNameError, FilterKey.Team });

            // Assert
            result.Should().HaveCount(3);
            result.Should().AllBeAssignableTo<IFilter<AgencyDocument>>();
        }
    }
}