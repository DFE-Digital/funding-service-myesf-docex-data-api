using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pds.Core.Common.Organisation.Enums;
using Pds.Core.Common.Organisation.Models;
using Pds.DocumentExchange.Data.Services.DTOs;
using Pds.DocumentExchange.Data.Services.DTOs.User;
using Pds.DocumentExchange.Data.Services.Enums;
using Pds.DocumentExchange.Data.Services.Implementations.Filters;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Tests.Unit.Filters
{
    [TestClass, TestCategory("Unit")]
    public class UkprnFilterTests
    {
        private readonly UkprnFilter _filter = new UkprnFilter();

        [TestMethod, TestCategory("Unit")]
        public void FilterTitle_ReturnsExpectedTitle()
        {
            // Assert
            _filter.FilterTitle.Should().Be("Search by UKPRN");
        }

        [TestMethod, TestCategory("Unit")]
        public void FilterKey_ReturnsExpectedFilterKey()
        {
            // Assert
            _filter.FilterKey.Should().Be("Ukprn");
        }

        [TestMethod, TestCategory("Unit")]
        public void FilterType_ReturnsDefaultFilterType()
        {
            // Assert
            _filter.FilterType.Should().Be(FilterType.TextBoxFilter);
        }

        [TestMethod, TestCategory("Unit")]
        public void TextBoxHint_ReturnsExpected()
        {
            // Assert
            _filter.TextBoxHint.Should().Be("Search by UK Provider Reference Number. Must be an 8 digit number");
        }

        [TestMethod, TestCategory("Unit")]
        public void ValidationErrorMessage_ReturnsExpected()
        {
            // Assert
            _filter.ValidationErrorMessage.Should().Be("Please enter a valid UKPRN");
        }

        [TestMethod, TestCategory("Unit")]
        public void Regex_ReturnsExpected()
        {
            // Assert
            _filter.Regex.Should().Be("^[1][0-9]{7}$");
        }

        [TestMethod, TestCategory("Unit")]
        [DataRow("12345678", "12345678", true)]
        [DataRow("the_ukprn", "THE_UKPRN", true)]
        [DataRow("the_ukprn", "ThE_uKpRn", true)]
        [DataRow("12345678", "87654321", false)]
        [DataRow("12345678", "1234567", false)]
        [DataRow("12345678", "123456789", false)]
        public async Task DoesElementMatchValue_ReturnsExpected(string exchangeDocumentUkprn, string ukprnValue, bool expected)
        {
            // Arrange
            var exchangeDocument = new ExchangeDocument
            {
                OrganisationInfo = new OrganisationInfo
                {
                    OrganisationIdentifier = new OrganisationIdentifier
                    {
                        Type = OrganisationIdentifierType.Ukprn,
                        Value = exchangeDocumentUkprn
                    }
                }
            };

            // Act
            var result = await _filter.DoesElementMatchValue(exchangeDocument, ukprnValue);

            // Assert
            result.Should().Be(expected);
        }

        [TestMethod, TestCategory("Unit")]
        public async Task DoesElementMatchValue_WhenExchangeDocumentIsNull_ReturnsFalse()
        {
            // Act
            var result = await _filter.DoesElementMatchValue(null, "any-ukprn-value");

            // Assert
            result.Should().BeFalse();
        }

        [TestMethod, TestCategory("Unit")]
        public async Task DoesElementMatchValue_WhenOrganisationInfoIsNull_ReturnsFalse()
        {
            // Arrange
            var exchangeDocument = new ExchangeDocument
            {
                OrganisationInfo = null
            };

            // Act
            var result = await _filter.DoesElementMatchValue(exchangeDocument, "any-ukprn-value");

            // Assert
            result.Should().BeFalse();
        }

        [TestMethod, TestCategory("Unit")]
        public async Task DoesElementMatchValue_WhenOrganisationIdentifierIsNull_ReturnsFalse()
        {
            // Arrange
            var exchangeDocument = new ExchangeDocument
            {
                OrganisationInfo = new OrganisationInfo
                {
                    OrganisationIdentifier = null
                }
            };

            // Act
            var result = await _filter.DoesElementMatchValue(exchangeDocument, "any-ukprn-value");

            // Assert
            result.Should().BeFalse();
        }

        [TestMethod, TestCategory("Unit")]
        public async Task DoesElementMatchValue_WhenOrganisationIdentifierIsNotUkprn_ReturnsFalse()
        {
            // Arrange
            var exchangeDocument = new ExchangeDocument
            {
                OrganisationInfo = new OrganisationInfo
                {
                    OrganisationIdentifier = new OrganisationIdentifier
                    {
                        Type = OrganisationIdentifierType.CompanyRegistrationNumber,
                        Value = "company-registration-value"
                    }
                }
            };

            // Act
            var result = await _filter.DoesElementMatchValue(exchangeDocument, "any-ukprn-value");

            // Assert
            result.Should().BeFalse();
        }

        [TestMethod, TestCategory("Unit")]
        [DataRow(null)]
        [DataRow("")]
        [DataRow(" ")]
        public async Task DoesElementMatchValue_WhenUkprnIsNullOrEmpty_ReturnsFalse(string ukprn)
        {
            // Arrange
            var exchangeDocument = new ExchangeDocument
            {
                OrganisationInfo = new OrganisationInfo
                {
                    OrganisationIdentifier = new OrganisationIdentifier
                    {
                        Type = OrganisationIdentifierType.Ukprn,
                        Value = ukprn
                    }
                }
            };

            // Act
            var result = await _filter.DoesElementMatchValue(exchangeDocument, "any-ukprn-value");

            // Assert
            result.Should().BeFalse();
        }
    }
}