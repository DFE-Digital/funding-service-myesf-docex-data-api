using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pds.DocumentExchange.Data.Services.DTOs;
using Pds.DocumentExchange.Data.Services.Enums;
using Pds.DocumentExchange.Data.Services.Implementations.Filters;
using Pds.DocumentExchange.Data.Services.Interfaces.Converters;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Tests.Unit.Filters
{
    [TestClass]
    public class DocumentErrorTypeFilterTests
    {
        private readonly DocumentErrorTypeFilter _filter;
        private readonly IEnumerable<AgencyDocument> _files;

        private readonly Mock<IConvertAgencyDocumentErrorTypes> _converter = new Mock<IConvertAgencyDocumentErrorTypes>(MockBehavior.Strict);

        public DocumentErrorTypeFilterTests()
        {
            _files = GetTestFiles();
            _filter = new DocumentErrorTypeFilter(_files, _converter.Object);
        }

        [TestMethod, TestCategory("Unit")]
        public async Task GetFilterValues_ReturnsFilterValues()
        {
            // Arrange
            var expectedResult = new[]
            {
                "DocumentNameInvalidFormat",
                "DocumentNameContainsInvalidCharacters",
                "ProductIdentifierInvalidFormat",
                "AcademicYearInvalidFormat"
            };

            // Act
            var result = await _filter.GetFilterValues();

            // Assert
            result.Should().BeEquivalentTo(expectedResult);
        }

        [TestMethod, TestCategory("Unit")]
        [DataRow("NoError", "No error found")]
        [DataRow("DocumentNameInvalidFormat", "File name is not a valid format")]
        [DataRow("DocumentNameContainsInvalidCharacters", "The document name contains invalid characters")]
        [DataRow("ProductIdentifierInvalidFormat", "Document type code is not a valid format")]
        [DataRow("OrganisationIdentifierInvalidFormat", "UKPRN does not contain 8 numbers")]
        [DataRow("OrganisationIdentifierNotRecognised", "UKPRN is not recognised in the system")]
        [DataRow("AcademicYearInvalidFormat", "Academic year is not a valid format")]
        [DataRow("TeamNotAuthorisedToPublish", "Team is not authorised to publish")]
        [DataRow("non-existing-value", "Unknown error type")]
        public async Task GetFilterTitleFromValue_ReturnsExpectedTitle(string value, string title)
        {
            // Arrange
            _converter
                .Setup(converter => converter.Convert(It.IsAny<AgencyDocumentErrorType>()))
                .Returns(title);

            // Act
            var result = await _filter.GetFilterTitleFromValue(value);

            // Assert
            result.Should().Be(title);
        }

        [TestMethod, TestCategory("Unit")]
        [DataRow("DocumentNameInvalidFormat", "File_DocumentNameInvalidFormat_01.pdf")]
        [DataRow("DocumentNameContainsInvalidCharacters", "File_DocumentNameContainsInvalidCharacters_01.pdf")]
        [DataRow("ProductIdentifierInvalidFormat", "File_ProductIdentifierInvalidFormat_01.pdf,File_ProductIdentifierInvalidFormat_02.pdf")]
        [DataRow("AcademicYearInvalidFormat", "File_AcademicYearInvalidFormat_01.pdf")]
        public async Task GetElementsByFilterValue_WhenValueExists_ReturnsElements(string value, string fileNames)
        {
            // Arrange
            var fileNamesArray = fileNames.Split(',');
            var expectedFiles = _files.Where(file => fileNamesArray.Contains(file.FileName));

            // Act
            var result = await _filter.GetElementsByFilterValue(value);

            // Assert
            result.Should().BeEquivalentTo(expectedFiles);
        }

        [TestMethod, TestCategory("Unit")]
        public async Task GetElementsByFilterValue_WhenValueDoesntExist_ReturnsEmptyCollection()
        {
            // Arrange
            var nonExistingValue = "non-existing-error";

            // Act
            var result = await _filter.GetElementsByFilterValue(nonExistingValue);

            // Assert
            result.Should().BeEmpty();
        }

        [TestMethod, TestCategory("Unit")]
        [DataRow("DocumentNameInvalidFormat,DocumentNameContainsInvalidCharacters", "File_DocumentNameInvalidFormat_01.pdf,File_DocumentNameContainsInvalidCharacters_01.pdf")]
        [DataRow("ProductIdentifierInvalidFormat,AcademicYearInvalidFormat", "File_ProductIdentifierInvalidFormat_01.pdf,File_ProductIdentifierInvalidFormat_02.pdf,File_AcademicYearInvalidFormat_01.pdf")]
        public async Task GetElementsByFilterValues_WhenValueExists_ReturnsElements(string values, string fileNames)
        {
            // Arrange
            var fileNamesArray = fileNames.Split(',');
            var expectedFiles = _files.Where(file => fileNamesArray.Contains(file.FileName));

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
        [DataRow("DocumentNameInvalidFormat", "")]
        [DataRow("DocumentNameContainsInvalidCharacters", "")]
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
            _filter.FilterTitle.Should().Be("Filter by document name error");
        }

        [TestMethod, TestCategory("Unit")]
        public void FilterKey_ReturnsExpectedFilterKey()
        {
            // Assert
            _filter.FilterKey.Should().Be("DocumentNameError");
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

        private IEnumerable<AgencyDocument> GetTestFiles()
        {
            return new[]
            {
                CreateAgencyDocument("File_DocumentNameInvalidFormat_01.pdf", Enums.AgencyDocumentErrorType.DocumentNameInvalidFormat),
                CreateAgencyDocument("File_DocumentNameContainsInvalidCharacters_01.pdf", Enums.AgencyDocumentErrorType.DocumentNameContainsInvalidCharacters),
                CreateAgencyDocument("File_ProductIdentifierInvalidFormat_01.pdf", Enums.AgencyDocumentErrorType.ProductIdentifierInvalidFormat),
                CreateAgencyDocument("File_ProductIdentifierInvalidFormat_02.pdf", Enums.AgencyDocumentErrorType.ProductIdentifierInvalidFormat),
                CreateAgencyDocument("File_AcademicYearInvalidFormat_01.pdf", Enums.AgencyDocumentErrorType.AcademicYearInvalidFormat)
            };
        }

        private AgencyDocument CreateAgencyDocument(string fileName, Enums.AgencyDocumentErrorType error)
            => new AgencyDocument
            {
                FileName = fileName,
                IsValid = false,
                ErrorType = error
            };
    }
}