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
    public class StatusTypeFilterTests
    {
        private readonly StatusTypeFilter _filter;
        private readonly IEnumerable<ExchangeDocument> _files;

        public StatusTypeFilterTests()
        {
            _files = GetTestFiles();
            _filter = new StatusTypeFilter(_files);
        }

        [TestMethod, TestCategory("Unit")]
        public async Task GetFilterValues_ReturnsFilterValues()
        {
            // Arrange
            var expectedResult = new[]
            {
                "New",
                "Downloaded"
            };

            // Act
            var result = await _filter.GetFilterValues();

            // Assert
            result.Should().BeEquivalentTo(expectedResult);
        }

        [TestMethod, TestCategory("Unit")]
        [DataRow("New", "New")]
        [DataRow("Downloaded", "Downloaded")]
        public async Task GetFilterTitleFromValue_ReturnsExpectedTitle(string value, string title)
        {
            // Act
            var result = await _filter.GetFilterTitleFromValue(value);

            // Assert
            result.Should().Be(title);
        }

        [TestMethod, TestCategory("Unit")]
        [DataRow("New", "File_New_01.pdf,File_New_02.pdf,File_New_03.pdf")]
        [DataRow("Downloaded", "File_Downloaded_01.pdf,File_Downloaded_02.pdf")]
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
        [DataRow("New,Downloaded", "File_New_01.pdf,File_New_02.pdf,File_New_03.pdf,File_Downloaded_01.pdf,File_Downloaded_02.pdf")]
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
        [DataRow("New", "")]
        [DataRow("Downloaded", "")]
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
            _filter.FilterTitle.Should().Be("Filter by status");
        }

        [TestMethod, TestCategory("Unit")]
        public void FilterKey_ReturnsExpectedFilterKey()
        {
            // Assert
            _filter.FilterKey.Should().Be("Status");
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
                CreateExchangeDocument("File_New_01.pdf", null),
                CreateExchangeDocument("File_New_02.pdf", ExchangeDocumentEventType.PublishedByAgency),
                CreateExchangeDocument("File_New_03.pdf", ExchangeDocumentEventType.PublishedByAgency),
                CreateExchangeDocument("File_Downloaded_01.pdf", ExchangeDocumentEventType.DownloadedByReceiver),
                CreateExchangeDocument("File_Downloaded_02.pdf", ExchangeDocumentEventType.DownloadedByReceiver),
            };
        }

        private ExchangeDocument CreateExchangeDocument(string fileName, ExchangeDocumentEventType? eventType)
            => new ExchangeDocument
            {
                DocumentReference = new DocumentReference
                {
                    FileName = fileName
                },
                EventHistory = eventType.HasValue
                ? new[]
                {
                    new ExchangeDocumentEvent
                    {
                        EventType = eventType.Value
                    }
                }
                : Enumerable.Empty<ExchangeDocumentEvent>()
            };
    }
}