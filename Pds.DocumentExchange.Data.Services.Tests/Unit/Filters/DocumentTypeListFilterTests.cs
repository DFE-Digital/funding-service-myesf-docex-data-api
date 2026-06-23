using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pds.DocumentExchange.Data.Services.DTOs;
using Pds.DocumentExchange.Data.Services.Enums;
using Pds.DocumentExchange.Data.Services.Implementations.Filters;
using Pds.DocumentExchange.Data.Services.Interfaces.Lookup;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Tests.Unit.Filters
{
    [TestClass]
    public class DocumentTypeListFilterTests
    {
        private readonly Mock<IProductsLookup> _productsLookup = new Mock<IProductsLookup>(MockBehavior.Strict);

        private readonly DocumentTypeListFilter<Document> _filter;
        private readonly IEnumerable<Document> _files;

        public DocumentTypeListFilterTests()
        {
            _files = GetTestFiles();
            _filter = new DocumentTypeListFilter<Document>(_files, _productsLookup.Object);
        }

        [TestMethod, TestCategory("Unit")]
        public async Task GetFilterValues_ReturnsFilterValues()
        {
            // Arrange
            var expectedResult = new[]
            {
                "1",
                "2",
                "3"
            };

            // Act
            var result = await _filter.GetFilterValues();

            // Assert
            result.Should().BeEquivalentTo(expectedResult);
        }

        [TestMethod, TestCategory("Unit")]
        [DataRow("1", "Product_01")]
        [DataRow("2", "Product_02")]
        [DataRow("3", "Product_03")]
        [DataRow("not-numeric", "[Unknown document types]")]
        public async Task GetFilterTitleFromValue_ReturnsExpectedTitle(string value, string title)
        {
            // Arrange
            _productsLookup
                .Setup(p => p.Get(It.IsAny<string>()))
                .ReturnsAsync(new Product
                {
                    PluralName = title
                });

            // Act
            var result = await _filter.GetFilterTitleFromValue(value);

            // Assert
            result.Should().Be(title);
        }

        [TestMethod, TestCategory("Unit")]
        public async Task GetFilterTitleFromValue_WhenProductIsNotFound_ReturnsExpectedTitle()
        {
            // Arrange
            string nonExistingProductId = "99";
            var expectedTitle = "[Unknown products]";

            _productsLookup
                .Setup(p => p.Get(It.IsAny<string>()))
                .ReturnsAsync(new UnknownProduct());

            // Act
            var result = await _filter.GetFilterTitleFromValue(nonExistingProductId);

            // Assert
            result.Should().Be(expectedTitle);
        }

        [TestMethod, TestCategory("Unit")]
        [DataRow("1")]
        [DataRow("2")]
        [DataRow("3")]
        public async Task GetElementsByFilterValue_WhenValueExists_ReturnsElements(string value)
        {
            // Arrange
            var expectedFiles = _files.Where(file => file.Product.Identifier.ToString() == value);

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
        [DataRow("1,2")]
        [DataRow("2,3")]
        [DataRow("3,1")]
        public async Task GetElementsByFilterValues_WhenValueExists_ReturnsElements(string values)
        {
            // Arrange
            var valuesSplit = values.Split(',');
            var expectedFiles = _files.Where(file => valuesSplit.Contains(file.Product.Identifier.ToString()));

            // Act
            var result = await _filter.GetElementsByFilterValue(valuesSplit);

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
        [DataRow("01", "")]
        [DataRow("02", "")]
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
            _filter.FilterTitle.Should().Be("Filter by document type");
        }

        [TestMethod, TestCategory("Unit")]
        public void FilterKey_ReturnsExpectedFilterKey()
        {
            // Assert
            _filter.FilterKey.Should().Be("ProductIdList");
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

        private IEnumerable<Document> GetTestFiles()
        {
            return new[]
            {
                CreateExchangeDocument(1),
                CreateExchangeDocument(2),
                CreateExchangeDocument(3),
                CreateExchangeDocument(1),
                CreateExchangeDocument(1)
            };
        }

        private ExchangeDocument CreateExchangeDocument(int productIdentifier)
            => new ExchangeDocument
            {
                Product = new Product
                {
                    Identifier = productIdentifier
                }
            };
    }
}