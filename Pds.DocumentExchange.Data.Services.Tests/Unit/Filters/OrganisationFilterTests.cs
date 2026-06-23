using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pds.Core.Common.Organisation.Enums;
using Pds.Core.Common.Organisation.Models;
using Pds.DocumentExchange.Data.Services.DTOs;
using Pds.DocumentExchange.Data.Services.DTOs.User;
using Pds.DocumentExchange.Data.Services.Enums;
using Pds.DocumentExchange.Data.Services.Implementations.Filters;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Tests.Unit.Filters
{
    [TestClass]
    public class OrganisationFilterTests
    {
        private const int InternalAgencyID = -999;

        private readonly OrganisationFilter _filter;
        private readonly IEnumerable<ExchangeDocument> _files;

        public OrganisationFilterTests()
        {
            _files = GetTestFiles();
            _filter = new OrganisationFilter(_files);
        }

        [TestMethod, TestCategory("Unit")]
        public async Task GetFilterValues_ReturnsFilterValues()
        {
            // Arrange
            var expectedResult = new[]
            {
                "Organisation_12345678",
                "Organisation_11111111",
                "Organisation_99999999"
            };

            // Act
            var result = await _filter.GetFilterValues();

            // Assert
            result.Should().BeEquivalentTo(expectedResult);
        }

        [TestMethod, TestCategory("Unit")]
        [DataRow("Organisation_12345678", "File_12345678_01.pdf,File_12345678_02.pdf")]
        [DataRow("Organisation_11111111", "File_11111111_01.pdf")]
        [DataRow("Organisation_99999999", "File_99999999_01.pdf")]
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
        [DataRow("Organisation_12345678,Organisation_11111111", "File_12345678_01.pdf,File_12345678_02.pdf,File_11111111_01.pdf")]
        [DataRow("Organisation_11111111,Organisation_99999999", "File_11111111_01.pdf,File_99999999_01.pdf")]
        [DataRow("Organisation_99999999,Organisation_12345678", "File_99999999_01.pdf,File_12345678_01.pdf,File_12345678_02.pdf")]
        [DataRow("Organisation_12345678,Organisation_11111111,Organisation_99999999", "File_12345678_01.pdf,File_12345678_02.pdf,File_11111111_01.pdf,File_99999999_01.pdf")]
        public async Task GetElementsByFilterValues_WhenValueExists_ReturnsElements(string values, string fileNames)
        {
            // Arrange
            var fileNamesArray = fileNames.Split(',');
            var expectedFiles = _files.Where(file => fileNamesArray.Contains(file.DocumentReference.FileName));

            // Act
            var result = await _filter.GetElementsByFilterValue(values.Split(','));

            // Assert
            result.Should().BeEquivalentTo(expectedFiles);
        }

        [TestMethod, TestCategory("Unit")]
        public async Task GetElementsByFilterValues_WhenValuesDontExist_ReturnsEmptyCollection()
        {
            // Act
            var result = await _filter.GetElementsByFilterValue(new[] { "non-existing-value, whatever" });

            // Assert
            result.Should().BeEmpty();
        }

        [TestMethod, TestCategory("Unit")]
        [DataRow("Organisation_12345678", "")]
        [DataRow("Organisation_11111111", "")]
        [DataRow("any-value", "")]
        public async Task GetFilterCategoryFromValue_ReturnsExpectedCategory(string value, string category)
        {
            // Act
            var result = await _filter.GetFilterCategoryFromValue(value);

            // Assert
            result.Should().Be(category);
        }

        [TestMethod, TestCategory("Unit")]
        public void FilterTitle_ReturnsExpectedTitle()
        {
            // Assert
            _filter.FilterTitle.Should().Be("Filter by organisation");
        }

        [TestMethod, TestCategory("Unit")]
        public void FilterKey_ReturnsExpectedFilterKey()
        {
            // Assert
            _filter.FilterKey.Should().Be("Organisation");
        }

        [TestMethod, TestCategory("Unit")]
        public void ListFilterType_ReturnsDefaultListFilterType()
        {
            // Assert
            _filter.ListFilterType.Should().Be(ListFilterType.List);
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
                CreateExchangeDocument("File_12345678_01.pdf", 12345678),
                CreateExchangeDocument("File_12345678_02.pdf", 12345678),
                CreateExchangeDocument("File_11111111_01.pdf", 11111111),
                CreateExchangeDocument("File_99999999_01.pdf", 99999999)
            };
        }

        private ExchangeDocument CreateExchangeDocument(string fileName, int ukprn)
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
                Organisation = new Organisation()
                {
                    Name = $"Organisation_{ukprn}"
                }
            };
    }
}