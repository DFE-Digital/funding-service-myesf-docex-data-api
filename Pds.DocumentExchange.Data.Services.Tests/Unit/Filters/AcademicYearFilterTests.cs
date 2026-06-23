using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pds.DocumentExchange.Data.Services.DTOs;
using Pds.DocumentExchange.Data.Services.Enums;
using Pds.DocumentExchange.Data.Services.Implementations.Filters;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Tests.Unit.Filters
{
    [TestClass]
    public class AcademicYearFilterTests
    {
        private readonly AcademicYearFilter _filter;
        private readonly IEnumerable<ExchangeDocument> _files;

        public AcademicYearFilterTests()
        {
            _files = GetTestFiles();
            _filter = new AcademicYearFilter(_files);
        }

        [TestMethod, TestCategory("Unit")]
        public async Task GetFilterValues_ReturnsFilterValues()
        {
            // Arrange
            var expectedResult = new[]
            {
                "201819",
                "201920",
                "202021"
            };

            // Act
            var result = await _filter.GetFilterValues();

            // Assert
            result.Should().BeEquivalentTo(expectedResult);
        }

        [TestMethod, TestCategory("Unit")]
        [DataRow("201819", "2018 to 2019")]
        [DataRow("202021", "2020 to 2021")]
        [DataRow("355051", "3550 to 3551")]
        [DataRow("non-numeric", "[Unknown year]")]
        [DataRow("199", "[Unknown year]")]
        [DataRow("19999", "[Unknown year]")]
        public async Task GetFilterTitleFromValue_ReturnsExpectedTitle(string value, string title)
        {
            // Act
            var result = await _filter.GetFilterTitleFromValue(value);

            // Assert
            result.Should().Be(title);
        }

        [TestMethod, TestCategory("Unit")]
        [DataRow("201819", "File_201819_01.pdf,File_201819_02.pdf")]
        [DataRow("201920", "File_201920_01.pdf,File_201920_02.pdf")]
        [DataRow("202021", "File_202021_01.pdf")]
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
            var nonExistingValue = "205051";

            // Act
            var result = await _filter.GetElementsByFilterValue(nonExistingValue);

            // Assert
            result.Should().BeEmpty();
        }

        [TestMethod, TestCategory("Unit")]
        [DataRow("201819,201920", "File_201819_01.pdf,File_201819_02.pdf,File_201920_01.pdf,File_201920_02.pdf")]
        [DataRow("201920,202021", "File_201920_01.pdf,File_201920_02.pdf,File_202021_01.pdf")]
        [DataRow("201819,202021", "File_201819_01.pdf,File_201819_02.pdf,File_202021_01.pdf")]
        [DataRow("202021,205051", "File_202021_01.pdf")]
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
            var result = await _filter.GetElementsByFilterValue(new[] { "205051, whatever" });

            // Assert
            result.Should().BeEmpty();
        }

        [TestMethod, TestCategory("Unit")]
        [DataRow("201819", "")]
        [DataRow("201920", "")]
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
            _filter.FilterTitle.Should().Be("Filter by academic year");
        }

        [TestMethod, TestCategory("Unit")]
        public void FilterKey_ReturnsExpectedFilterKey()
        {
            // Assert
            _filter.FilterKey.Should().Be("AcademicYear");
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
                CreateExchangeDocument("File_201819_01.pdf", 201819),
                CreateExchangeDocument("File_201920_01.pdf", 201920),
                CreateExchangeDocument("File_202021_01.pdf", 202021),
                CreateExchangeDocument("File_201819_02.pdf", 201819),
                CreateExchangeDocument("File_201920_02.pdf", 201920)
            };
        }

        private ExchangeDocument CreateExchangeDocument(string fileName, int year)
            => new ExchangeDocument
            {
                DocumentReference = new DocumentReference
                {
                    FileName = fileName
                },
                Year = year
            };
    }
}