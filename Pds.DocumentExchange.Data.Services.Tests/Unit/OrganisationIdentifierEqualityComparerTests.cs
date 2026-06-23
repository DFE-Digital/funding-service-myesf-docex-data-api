using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pds.Core.Common.Organisation.Enums;
using Pds.Core.Common.Organisation.Models;
using Pds.DocumentExchange.Data.Services.Comparers;

namespace Pds.DocumentExchange.Data.Services.Tests.Unit
{
    [TestClass]
    public class OrganisationIdentifierEqualityComparerTests
    {
        private OrganisationIdentifierEqualityComparer _comparer = new OrganisationIdentifierEqualityComparer();

        [TestMethod, TestCategory("Unit")]
        [DataRow(OrganisationIdentifierType.Ukprn, "12345678")]
        [DataRow(OrganisationIdentifierType.CompanyRegistrationNumber, "12345ABC")]
        public void Equals_WhenOrganisationsAreEqual_ReturnTrue(OrganisationIdentifierType type, string value)
        {
            // Arrange
            var firstIdentifier = new OrganisationIdentifier { Type = type, Value = value };
            var secondIdentifier = new OrganisationIdentifier { Type = type, Value = value };

            // Act
            var areEqual = _comparer.Equals(firstIdentifier, secondIdentifier);

            // Assert
            areEqual.Should().BeTrue();
        }

        [TestMethod, TestCategory("Unit")]
        [DataRow(OrganisationIdentifierType.Ukprn, "12345678", OrganisationIdentifierType.CompanyRegistrationNumber, "12345678")]
        [DataRow(OrganisationIdentifierType.Ukprn, "12345678", OrganisationIdentifierType.Ukprn, "87654321")]
        public void Equals_WhenOrganisationsAreDifferent_ReturnFalse(
            OrganisationIdentifierType firstType,
            string firstValue,
            OrganisationIdentifierType secondType,
            string secondValue)
        {
            // Arrange
            var firstIdentifier = new OrganisationIdentifier { Type = firstType, Value = firstValue };
            var secondIdentifier = new OrganisationIdentifier { Type = secondType, Value = secondValue };

            // Act
            var areEqual = _comparer.Equals(firstIdentifier, secondIdentifier);

            // Assert
            areEqual.Should().BeFalse();
        }

        [TestMethod, TestCategory("Unit")]
        [DataRow(OrganisationIdentifierType.Ukprn, "12345678")]
        [DataRow(OrganisationIdentifierType.CompanyRegistrationNumber, "12345ABC")]
        public void GetHashCode_WhenOrganisationsAreEqual_ReturnSameHasCode(OrganisationIdentifierType type, string value)
        {
            // Arrange
            var firstIdentifier = new OrganisationIdentifier { Type = type, Value = value };
            var secondIdentifier = new OrganisationIdentifier { Type = type, Value = value };

            // Act
            var firstsHashCode = _comparer.GetHashCode(firstIdentifier);
            var secondHashCode = _comparer.GetHashCode(secondIdentifier);

            // Assert
            firstsHashCode.Should().Be(secondHashCode);
        }

        [TestMethod, TestCategory("Unit")]
        [DataRow(OrganisationIdentifierType.Ukprn, "12345678", OrganisationIdentifierType.CompanyRegistrationNumber, "12345678")]
        [DataRow(OrganisationIdentifierType.Ukprn, "12345678", OrganisationIdentifierType.Ukprn, "87654321")]
        public void GetHashCode_WhenOrganisationsAreDifferent_ReturnDifferentHasCodes(
            OrganisationIdentifierType firstType,
            string firstValue,
            OrganisationIdentifierType secondType,
            string secondValue)
        {
            // Arrange
            var firstIdentifier = new OrganisationIdentifier { Type = firstType, Value = firstValue };
            var secondIdentifier = new OrganisationIdentifier { Type = secondType, Value = secondValue };

            // Act
            var firstsHashCode = _comparer.GetHashCode(firstIdentifier);
            var secondHashCode = _comparer.GetHashCode(secondIdentifier);

            // Assert
            firstsHashCode.Should().NotBe(secondHashCode);
        }
    }
}