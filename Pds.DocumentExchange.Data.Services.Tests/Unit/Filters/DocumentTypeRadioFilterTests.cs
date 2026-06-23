using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pds.DocumentExchange.Data.Services.DTOs;
using Pds.DocumentExchange.Data.Services.Enums;
using Pds.DocumentExchange.Data.Services.Implementations.Filters;
using Pds.DocumentExchange.Data.Services.Interfaces.Settings;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Tests.Unit.Filters
{
    [TestClass]
    [TestCategory("Unit")]
    public class DocumentTypeRadioFilterTests
    {
        private readonly Mock<IConfigurationDataService> _configServiceMock
            = new Mock<IConfigurationDataService>(MockBehavior.Strict);

        private readonly DocumentTypeRadioFilter<Document> _filter;
        private readonly IEnumerable<Document> _files;

        public DocumentTypeRadioFilterTests()
        {
            _files = GetTestFiles();
            _filter = new DocumentTypeRadioFilter<Document>(_configServiceMock.Object, _files);
        }

        [TestMethod]
        public void FilterTitle_ReturnsExpectedTitle()
        {
            // Assert
            _filter.FilterTitle.Should().Be("Select a document type");
        }

        [TestMethod]
        public void FilterKey_ReturnsExpectedFilterKey()
        {
            // Assert
            _filter.FilterKey.Should().Be("ProductIdRadio");
        }

        [TestMethod]
        public void FilterType_ReturnsExpectedFilterType()
        {
            // Assert
            _filter.FilterType.Should().Be(FilterType.RadioFilter);
        }

        [TestMethod]
        [DataRow("-2", false)]
        [DataRow("-1", true)]
        [DataRow("0", false)]
        [DataRow("1", false)]
        [DataRow("cheese", false)]
        public async Task DoesElementMatchValue_ForNullProduct_ReturnsExpected(string value, bool expected)
        {
            // Act
            var actual = await _filter.DoesElementMatchValue(new AgencyDocument { Product = null }, value);

            // Assert
            actual.Should().Be(expected);
        }

        [TestMethod]
        [DataRow(1001, "1002", false)]
        [DataRow(1001, "1001", true)]
        [DataRow(1002, "1002", true)]
        [DataRow(1001, "1001.1", false)]
        [DataRow(1001, "cheese", false)]
        public async Task DoesElementMatchValue_ForNotNullProduct_ReturnsExpected(int id, string value, bool expected)
        {
            // Act
            var actual = await _filter.DoesElementMatchValue(
                new AgencyDocument { Product = new Product { Identifier = id } },
                value);

            // Assert
            actual.Should().Be(expected);
        }

        [TestMethod]
        public async Task GetAllTitleValuePairs_ReturnsExpectedPairs()
        {
            // Arrange
            _configServiceMock
                .Setup(config => config.GetProducts())
                .ReturnsAsync(
                    Enumerable
                        .Range(1, 10)
                        .Select(
                            id => new Product
                            {
                                Identifier = id,
                                PluralName = $"product {id}"
                            })
                        .ToList());

            var unknownProduct = new UnknownProduct();

            var expectedResult = new List<(string Title, string Value)>
            {
                (unknownProduct.PluralName, unknownProduct.Identifier.ToString()),
                ("product 1", "1"),
                ("product 2", "2"),
                ("product 5", "5")
            };

            // Act
            var actual = await _filter.GetAllTitleValuePairs();

            // Assert
            actual.Should().BeEquivalentTo(expectedResult);
        }

        private IEnumerable<Document> GetTestFiles()
        {
            return new[]
            {
                CreateExchangeDocument(1),
                CreateExchangeDocument(5),
                CreateExchangeDocument(5),
                CreateExchangeDocument(2),
                CreateExchangeDocument(1),
                CreateExchangeDocument(null),
                CreateExchangeDocument(100),
                CreateExchangeDocument(1),
                CreateExchangeDocument(2)
            };
        }

        private ExchangeDocument CreateExchangeDocument(int? productIdentifier)
        {
            return new ExchangeDocument
            {
                Product =
                    productIdentifier.HasValue
                        ? new Product
                        {
                            Identifier = productIdentifier.Value
                        }
                        : null
            };
        }
    }
}