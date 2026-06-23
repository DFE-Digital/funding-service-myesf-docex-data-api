using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pds.Core.Utils.Helpers;
using Pds.DocumentExchange.Data.Services.DTOs;
using Pds.DocumentExchange.Data.Services.Implementations.Lookup;
using Pds.DocumentExchange.Data.Services.Interfaces.Settings;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Tests.Unit.Lookup
{
    [TestClass, TestCategory("Unit")]
    public class ProductsLookupTests
    {
        private readonly Mock<IConfigurationDataService> _configurationDataService = new Mock<IConfigurationDataService>(MockBehavior.Strict);

        private readonly ProductsLookup _productsLookup;

        public ProductsLookupTests()
        {
            var products = GetTestsProducts();

            _configurationDataService
                .Setup(config => config.GetProducts())
                .ReturnsAsync(products.AsSafeReadOnlyList());

            _productsLookup = new ProductsLookup(_configurationDataService.Object);
        }

        #region Product identifier as string

        [TestMethod, TestCategory("Unit")]
        [DataRow("0")]
        [DataRow("50")]
        [DataRow("-99")]
        public async Task Exists_WhenProductDoesntExist_ReturnsFalse(string productId)
        {
            // Act
            var result = await _productsLookup.Exists(productId);

            // Assert
            result.Should().BeFalse();
        }

        [TestMethod, TestCategory("Unit")]
        [DataRow("1")]
        [DataRow("2")]
        [DataRow("5")]
        [DataRow("10")]
        public async Task Exists_WhenProductExists_ReturnsTrue(string productId)
        {
            // Act
            var result = await _productsLookup.Exists(productId);

            // Assert
            result.Should().BeTrue();
        }

        [TestMethod, TestCategory("Unit")]
        [DataRow(null)]
        [DataRow("")]
        [DataRow(" ")]
        public async Task Exists_WhenProductIsNullOrEmpty_ReturnsFalse(string productId)
        {
            // Act
            var result = await _productsLookup.Exists(productId);

            // Assert
            result.Should().BeFalse();
        }

        [TestMethod, TestCategory("Unit")]
        [DataRow("non-numeric-id")]
        [DataRow("12345X")]
        [DataRow("L45Y980")]
        public async Task Exists_WhenProductIsNotNumeric_ReturnsFalse(string productId)
        {
            // Act
            var result = await _productsLookup.Exists(productId);

            // Assert
            result.Should().BeFalse();
        }

        [TestMethod, TestCategory("Unit")]
        [DataRow("0")]
        [DataRow("50")]
        [DataRow("-99")]
        public async Task Get_WhenProductDoesntExist_ReturnsUnknownProduct(string productId)
        {
            // Act
            var result = await _productsLookup.Get(productId);

            // Assert
            result.Should().BeOfType<UnknownProduct>();
        }

        [TestMethod, TestCategory("Unit")]
        [DataRow("1")]
        [DataRow("2")]
        [DataRow("5")]
        [DataRow("10")]
        public async Task Get_WhenProductExists_ReturnsProduct(string productId)
        {
            // Arrange
            var expectedProduct = CreateProduct(int.Parse(productId));

            // Act
            var result = await _productsLookup.Get(productId);

            // Assert
            result.Should().BeEquivalentTo(expectedProduct);
        }

        [TestMethod, TestCategory("Unit")]
        [DataRow(null)]
        [DataRow("")]
        [DataRow(" ")]
        public async Task Get_WhenProductIsNullOrEmpty_ReturnsUnknownProduct(string productId)
        {
            // Act
            var result = await _productsLookup.Get(productId);

            // Assert
            result.Should().BeOfType<UnknownProduct>();
        }

        [TestMethod, TestCategory("Unit")]
        [DataRow("non-numeric-id")]
        [DataRow("12345X")]
        [DataRow("L45Y980")]
        public async Task Get_WhenProductIsNotNumeric_ReturnsUnknownProduct(string productId)
        {
            // Act
            var result = await _productsLookup.Get(productId);

            // Assert
            result.Should().BeOfType<UnknownProduct>();
        }

        #endregion


        #region Product identifier as int

        [TestMethod, TestCategory("Unit")]
        [DataRow(0)]
        [DataRow(50)]
        [DataRow(-99)]
        public async Task Exists_WhenProductDoesntExist_ReturnsFalse(int productId)
        {
            // Act
            var result = await _productsLookup.Exists(productId);

            // Assert
            result.Should().BeFalse();
        }

        [TestMethod, TestCategory("Unit")]
        [DataRow(1)]
        [DataRow(2)]
        [DataRow(5)]
        [DataRow(10)]
        public async Task Exists_WhenProductExists_ReturnsTrue(int productId)
        {
            // Act
            var result = await _productsLookup.Exists(productId);

            // Assert
            result.Should().BeTrue();
        }

        [TestMethod, TestCategory("Unit")]
        [DataRow(0)]
        [DataRow(50)]
        [DataRow(-99)]
        public async Task Get_WhenProductDoesntExist_ReturnsUnknownProduct(int productId)
        {
            // Act
            var result = await _productsLookup.Get(productId);

            // Assert
            result.Should().BeOfType<UnknownProduct>();
        }

        [TestMethod, TestCategory("Unit")]
        [DataRow(1)]
        [DataRow(2)]
        [DataRow(5)]
        [DataRow(10)]
        public async Task Get_WhenProductExists_ReturnsProduct(int productId)
        {
            // Arrange
            var expectedProduct = CreateProduct(productId);

            // Act
            var result = await _productsLookup.Get(productId);

            // Assert
            result.Should().BeEquivalentTo(expectedProduct);
        }

        #endregion

        private IEnumerable<Product> GetTestsProducts()
            => Enumerable.Range(1, 10)
                .Select(number => CreateProduct(number));

        private Product CreateProduct(int number)
            => new Product
            {
                Identifier = number,
                Name = $"Product {number} name",
                PluralName = $"Product {number} plural name",
                AgencyTeams = new[] { $"Product {number} team" }
            };
    }
}