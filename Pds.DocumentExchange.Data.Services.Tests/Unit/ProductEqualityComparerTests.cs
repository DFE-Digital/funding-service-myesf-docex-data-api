using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pds.DocumentExchange.Data.Services.Comparers;
using Pds.DocumentExchange.Data.Services.DTOs;
using System.Linq;

namespace Pds.DocumentExchange.Data.Services.Tests.Unit
{
    [TestClass]
    public class ProductEqualityComparerTests
    {
        private ProductEqualityComparer _comparer = new ProductEqualityComparer();

        [TestMethod, TestCategory("Unit")]
        public void Equals_WhenProductssAreEqual_ReturnTrue()
        {
            // Arrange
            var firstIdentifier = new Product
            {
                Identifier = 12345,
                Name = "product-1",
                AgencyTeams = new[] { "team-1", "team-2" },
                CanOrganisationsUpload = true
            };

            var secondIdentifier = new Product
            {
                Identifier = 12345,
                Name = "product-1",
                AgencyTeams = new[] { "team-1", "team-2" },
                CanOrganisationsUpload = true
            };

            // Act
            var areEqual = _comparer.Equals(firstIdentifier, secondIdentifier);

            // Assert
            areEqual.Should().BeTrue();
        }

        [TestMethod, TestCategory("Unit")]
        public void Equals_WhenProductssAreDifferent_ReturnFalse()
        {
            // Arrange
            var firstIdentifier = new Product
            {
                Identifier = 12345,
                Name = "product-1",
                AgencyTeams = new[] { "team-1", "team-2" },
                CanOrganisationsUpload = true
            };

            var secondIdentifier = new Product
            {
                Identifier = 12345,
                Name = "product-2",
                AgencyTeams = new[] { "team-1" },
                CanOrganisationsUpload = false
            };

            var thirdIdentifier = new Product
            {
                Identifier = 112233,
                Name = "product-3",
                AgencyTeams = Enumerable.Empty<string>(),
                CanOrganisationsUpload = true
            };

            // Act
            var areDifferent = !_comparer.Equals(firstIdentifier, secondIdentifier)
                && !_comparer.Equals(firstIdentifier, thirdIdentifier)
                && !_comparer.Equals(secondIdentifier, thirdIdentifier);

            // Assert
            areDifferent.Should().BeTrue();
        }

        [TestMethod, TestCategory("Unit")]
        public void GetHashCode_WhenProductssAreEqual_ReturnSameHashCode()
        {
            // Arrange
            var firstIdentifier = new Product
            {
                Identifier = 12345,
                Name = "product-1",
                AgencyTeams = new[] { "team-1", "team-2" },
                CanOrganisationsUpload = true
            };

            var secondIdentifier = new Product
            {
                Identifier = 12345,
                Name = "product-1",
                AgencyTeams = new[] { "team-1", "team-2" },
                CanOrganisationsUpload = true
            };

            // Act
            var firstHashCode = _comparer.GetHashCode(firstIdentifier);
            var secondHashCode = _comparer.GetHashCode(secondIdentifier);

            // Assert
            firstHashCode.Should().Be(secondHashCode);
        }

        [TestMethod, TestCategory("Unit")]
        public void GetHashCode_WhenProductssAreEqualButAgencyTeamsOrderIsDifferent_ReturnSameHashCode()
        {
            // Arrange
            var firstIdentifier = new Product
            {
                Identifier = 12345,
                Name = "product-1",
                AgencyTeams = new[] { "team-1", "team-2" },
                CanOrganisationsUpload = true
            };

            var secondIdentifier = new Product
            {
                Identifier = 12345,
                Name = "product-1",
                AgencyTeams = new[] { "team-2", "team-1" },
                CanOrganisationsUpload = true
            };

            // Act
            var firstHashCode = _comparer.GetHashCode(firstIdentifier);
            var secondHashCode = _comparer.GetHashCode(secondIdentifier);

            // Assert
            firstHashCode.Should().Be(secondHashCode);
        }

        [TestMethod, TestCategory("Unit")]
        public void GetHashCode_WhenProductssAreDifferent_ReturnDifferentHashCodes()
        {
            // Arrange
            var firstIdentifier = new Product
            {
                Identifier = 12345,
                Name = "product-1",
                AgencyTeams = new[] { "team-1", "team-2" },
                CanOrganisationsUpload = true
            };

            var secondIdentifier = new Product
            {
                Identifier = 12345,
                Name = "product-2",
                AgencyTeams = new[] { "team-1" },
                CanOrganisationsUpload = false
            };

            var thirdIdentifier = new Product
            {
                Identifier = 112233,
                Name = "product-3",
                AgencyTeams = Enumerable.Empty<string>(),
                CanOrganisationsUpload = true
            };

            // Act
            var firstHashCode = _comparer.GetHashCode(firstIdentifier);
            var secondHashCode = _comparer.GetHashCode(secondIdentifier);
            var thirdHashCode = _comparer.GetHashCode(thirdIdentifier);

            // Assert
            firstHashCode.Should().NotBe(secondHashCode);
            firstHashCode.Should().NotBe(thirdHashCode);
            secondHashCode.Should().NotBe(thirdHashCode);
        }
    }
}