using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pds.Core.Common.Organisation.Enums;
using Pds.Core.Common.Organisation.Models;
using Pds.DocumentExchange.Data.Services.DTOs;
using Pds.DocumentExchange.Data.Services.DTOs.User;
using Pds.DocumentExchange.Data.Services.Enums;
using Pds.DocumentExchange.Data.Services.Implementations.Filters;
using Pds.DocumentExchange.Data.Services.Interfaces.Lookup;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Tests.Unit.Filters
{
    [TestClass, TestCategory("Unit")]
    public class ProviderTypeFilterTests
    {
        private readonly Mock<IOrganisationSubtypesLookup> _organisationSubTypesLookup = new Mock<IOrganisationSubtypesLookup>(MockBehavior.Strict);

        private readonly ProviderTypeFilter _filter;
        private readonly IEnumerable<ExchangeDocument> _files;

        public ProviderTypeFilterTests()
        {
            _files = GetTestFiles();

            SetupLookups(ukprn: 12345678);
            SetupLookups(ukprn: 11111111);
            SetupLookups(ukprn: 99999999);

            _filter = new ProviderTypeFilter(
                _files,
                _organisationSubTypesLookup.Object);
        }

        [TestMethod, TestCategory("Unit")]
        [DataRow(12345678, "Organisation 12345678 subtype")]
        [DataRow(11111111, "Organisation 11111111 subtype")]
        [DataRow(99999999, "Organisation 99999999 subtype")]
        public async Task GetFilterValueFromElement_ReturnsExpectedValue(int ukprn, string expectedValue)
        {
            // Arrange
            var exchangeDocument = new ExchangeDocument
            {
                OrganisationInfo = new OrganisationInfo
                {
                    OrganisationIdentifier = new OrganisationIdentifier
                    {
                        Type = OrganisationIdentifierType.Ukprn,
                        Value = ukprn.ToString()
                    }
                },
                Organisation = SetupOrganisation(ukprn)
            };

            // Act
            var result = await _filter.GetFilterValueFromElement(exchangeDocument);

            // Assert
            result.Should().BeEquivalentTo(expectedValue);
        }

        [TestMethod, TestCategory("Unit")]
        public async Task GetFilterValues_ReturnsFilterValues()
        {
            // Arrange
            var expectedResult = new[]
            {
                "Organisation 12345678 subtype",
                "Organisation 11111111 subtype",
                "Organisation 99999999 subtype"
            };

            // Act
            var result = await _filter.GetFilterValues();

            // Assert
            result.Should().BeEquivalentTo(expectedResult);
        }

        [TestMethod, TestCategory("Unit")]
        [DataRow("Organisation 12345678 subtype", "Organisation 12345678 subtype display plural")]
        [DataRow("Organisation 11111111 subtype", "Organisation 11111111 subtype display plural")]
        [DataRow("Organisation 99999999 subtype", "Organisation 99999999 subtype display plural")]
        public async Task GetFilterTitleFromValue_ReturnsExpectedTitle(string value, string title)
        {
            // Act
            var result = await _filter.GetFilterTitleFromValue(value);

            // Assert
            result.Should().Be(title);
        }

        [TestMethod, TestCategory("Unit")]
        [DataRow("Organisation 12345678 subtype", "Organisation 12345678 type display plural")]
        [DataRow("Organisation 11111111 subtype", "Organisation 11111111 type display plural")]
        [DataRow("Organisation 99999999 subtype", "Organisation 99999999 type display plural")]
        public async Task GetFilterCategoryFromValue_ReturnsExpectedCategory(string value, string category)
        {
            // Act
            var result = await _filter.GetFilterCategoryFromValue(value);

            // Assert
            result.Should().Be(category);
        }

        [TestMethod, TestCategory("Unit")]
        [DataRow("Organisation 12345678 subtype", "File_12345678_01.pdf,File_12345678_02.pdf")]
        [DataRow("Organisation 11111111 subtype", "File_11111111_01.pdf")]
        [DataRow("Organisation 99999999 subtype", "File_99999999_01.pdf")]
        public async Task GetElementsByFilterValue_WhenValueExists_ReturnsElements(string value, string fileNames)
        {
            // Arrange
            var fileNamesArray = fileNames.Split(',');
            var expectedFiles = _files.Where(file => fileNamesArray.Contains(file.DocumentReference.FileName));

            // Act
            var result = await _filter.GetElementsByFilterValue(value);

            // Assert
            result.Should().BeEquivalentTo(expectedFiles);
        }

        [TestMethod, TestCategory("Unit")]
        public async Task GetElementsByFilterValue_WhenValueDoesntExist_ReturnsEmptyCollection()
        {
            // Arrange
            var nonExistingValue = "non-existing-value";

            // Act
            var result = await _filter.GetElementsByFilterValue(nonExistingValue);

            // Assert
            result.Should().BeEmpty();
        }

        [TestMethod, TestCategory("Unit")]
        public void FilterTitle_ReturnsExpectedTitle()
        {
            // Assert
            _filter.FilterTitle.Should().Be("Filter by provider type");
        }

        [TestMethod, TestCategory("Unit")]
        public void FilterKey_ReturnsExpectedFilterKey()
        {
            // Assert
            _filter.FilterKey.Should().Be("ProviderType");
        }

        [TestMethod, TestCategory("Unit")]
        public void ListFilterType_ReturnsDefaultListFilterType()
        {
            // Assert
            _filter.ListFilterType.Should().Be(ListFilterType.Group);
        }

        [TestMethod, TestCategory("Unit")]
        public void FilterType_ReturnsDefaultFilterType()
        {
            // Assert
            _filter.FilterType.Should().Be(FilterType.ListFilter);
        }

        private IEnumerable<ExchangeDocument> GetTestFiles()
        {
            return new[]
            {
                CreateExchangeDocument("File_12345678_01.pdf", 12345678, SetupOrganisation(12345678)),
                CreateExchangeDocument("File_12345678_02.pdf", 12345678, SetupOrganisation(12345678)),
                CreateExchangeDocument("File_11111111_01.pdf", 11111111, SetupOrganisation(11111111)),
                CreateExchangeDocument("File_99999999_01.pdf", 99999999, SetupOrganisation(99999999))
            };
        }

        private ExchangeDocument CreateExchangeDocument(string fileName, int ukprn, Organisation organisation)
            => new ExchangeDocument
            {
                DocumentReference = new DocumentReference
                {
                    FileName = fileName
                },
                OrganisationInfo = new OrganisationInfo
                {
                    OrganisationIdentifier = new OrganisationIdentifier
                    {
                        Type = OrganisationIdentifierType.Ukprn,
                        Value = ukprn.ToString()
                    }
                },
                Organisation = organisation
            };

        private void SetupLookups(int ukprn)
        {
            var organisation = SetupOrganisation(ukprn);

            _organisationSubTypesLookup
                .Setup(lookup => lookup.LoadOrganisationDisplays(It.IsAny<Organisation>()))
                .Returns(Task.CompletedTask);

            _organisationSubTypesLookup
                .Setup(lookup => lookup.Get(organisation.OrganisationSubType))
                .ReturnsAsync(organisation.OrganisationSubTypeDisplay);

            _organisationSubTypesLookup
                .Setup(lookup => lookup.GetOrganisationTypeDisplay(organisation.OrganisationSubType))
                .ReturnsAsync(organisation.OrganisationTypeDisplay);
        }

        private Organisation SetupOrganisation(int ukprn)
        {
            return new Organisation
            {
                Name = $"Organisation {ukprn}",
                OrganisationType = $"Organisation {ukprn} type",
                OrganisationTypeDisplay = new DisplayValues
                {
                    Singular = $"Organisation {ukprn} type display singular",
                    Plural = $"Organisation {ukprn} type display plural",
                },
                OrganisationSubType = $"Organisation {ukprn} subtype",
                OrganisationSubTypeDisplay = new DisplayValues
                {
                    Singular = $"Organisation {ukprn} subtype display singular",
                    Plural = $"Organisation {ukprn} subtype display plural",
                },
            };
        }
    }
}