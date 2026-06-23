using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pds.Core.Common.Organisation.Enums;
using Pds.Core.Common.Organisation.Models;
using Pds.DocumentExchange.Data.Services.Enums;
using Pds.DocumentExchange.Data.Services.Implementations.Caching;
using System.Collections.Generic;

namespace Pds.DocumentExchange.Data.Services.Tests.Unit.Caching
{
    [TestClass]
    public class CacheKeyBuilderTests
    {
        [DataTestMethod]
        [DynamicData(nameof(BuildAgencyTeamFileShareKeyTestData))]
        public void BuildAgencyTeamFileShareKey_ReturnsExpectedKey(string team, string expectedKey)
        {
            // Arrange
            var builder = GetCacheKeyBuilder();

            // Act
            var actual = builder.BuildAgencyTeamFileShareKey(team);

            // Assert
            actual.Should().Be(expectedKey);
        }

        [DataTestMethod]
        [DynamicData(nameof(BuildAgencyTeamExchangedDocumentsKeyTestData))]
        public void BuildAgencyTeamExchangedDocumentsKey_ReturnsExpectedKey(string team, ExchangeDocumentDirection direction, string expectedKey)
        {
            // Arrange
            var builder = GetCacheKeyBuilder();

            // Act
            var actual = builder.BuildAgencyTeamExchangedDocumentsKey(team, direction);

            // Assert
            actual.Should().Be(expectedKey);
        }

        [DataTestMethod]
        [DynamicData(nameof(BuildAgencyDocumentValidationKeyTestData))]
        public void BuildAgencyDocumentValidationKey_ReturnsExpectedKey(string team, string fileName, string expectedKey)
        {
            // Arrange
            var builder = GetCacheKeyBuilder();

            // Act
            var actual = builder.BuildAgencyDocumentValidationKey(team, fileName);

            // Assert
            actual.Should().Be(expectedKey);
        }

        [DataTestMethod]
        [DynamicData(nameof(BuildOrganisationExchangedDocumentsKeyTestData))]
        public void BuildOrganisationExchangedDocumentsKey_ReturnsExpectedKey(OrganisationIdentifier organisationIdentifier, ExchangeDocumentDirection direction, string expectedKey)
        {
            // Arrange
            var builder = GetCacheKeyBuilder();

            // Act
            var actual = builder.BuildOrganisationExchangedDocumentsKey(organisationIdentifier, direction);

            // Assert
            actual.Should().Be(expectedKey);
        }

        [DataTestMethod]
        [DynamicData(nameof(BuildConfigurationKeyTestData))]
        public void BuildConfigurationKey_ReturnsExpectedKey(ConfigurationSection section, string expectedKey)
        {
            // Arrange
            var builder = GetCacheKeyBuilder();

            // Act
            var actual = builder.BuildConfigurationKey(section);

            // Assert
            actual.Should().Be(expectedKey);
        }

        [DataTestMethod]
        [DynamicData(nameof(BuildListConfigurationKeyTestData))]
        public void BuildListConfigurationKey_ReturnsExpectedKey(ConfigurationSection section, string expectedKey)
        {
            // Arrange
            var builder = GetCacheKeyBuilder();

            // Act
            var actual = builder.BuildListConfigurationKey(section);

            // Assert
            actual.Should().Be(expectedKey);
        }

        [DataTestMethod]
        [DynamicData(nameof(BuildEmailResourceLockKeyTestData))]
        public void BuildEmailResourceLockKey_ReturnsExpectedKey(string parentBatchId, string expectedKey)
        {
            // Arrange
            var builder = GetCacheKeyBuilder();

            // Act
            var actual = builder.BuildEmailResourceLockKey(parentBatchId);

            // Assert
            actual.Should().Be(expectedKey);
        }

        private CacheKeyBuilder GetCacheKeyBuilder()
        {
            return new CacheKeyBuilder();
        }

        private static IEnumerable<object[]> BuildAgencyTeamFileShareKeyTestData
        {
            get
            {
                yield return new object[] { "x", "DocEx:AgencyTeam:x:FileShareItems" };
                yield return new object[] { "X", "DocEx:AgencyTeam:X:FileShareItems" };
                yield return new object[] { "1", "DocEx:AgencyTeam:1:FileShareItems" };
                yield return new object[] { "the_team_name", "DocEx:AgencyTeam:the_team_name:FileShareItems" };
                yield return new object[] { "The_Team_Name", "DocEx:AgencyTeam:The_Team_Name:FileShareItems" };
            }
        }

        private static IEnumerable<object[]> BuildAgencyTeamExchangedDocumentsKeyTestData
        {
            get
            {
                yield return new object[] { "x", ExchangeDocumentDirection.PublishedByAgency, "DocEx:AgencyTeam:x:ExchangedDocuments:PublishedByAgency" };
                yield return new object[] { "X", ExchangeDocumentDirection.PublishedByAgency, "DocEx:AgencyTeam:X:ExchangedDocuments:PublishedByAgency" };
                yield return new object[] { "1", ExchangeDocumentDirection.PublishedByAgency, "DocEx:AgencyTeam:1:ExchangedDocuments:PublishedByAgency" };
                yield return new object[] { "the_team_name", ExchangeDocumentDirection.PublishedByAgency, "DocEx:AgencyTeam:the_team_name:ExchangedDocuments:PublishedByAgency" };
                yield return new object[] { "The_Team_Name", ExchangeDocumentDirection.PublishedByAgency, "DocEx:AgencyTeam:The_Team_Name:ExchangedDocuments:PublishedByAgency" };
                yield return new object[] { "x", ExchangeDocumentDirection.SentByOrganisation, "DocEx:AgencyTeam:x:ExchangedDocuments:SentByOrganisation" };
                yield return new object[] { "X", ExchangeDocumentDirection.SentByOrganisation, "DocEx:AgencyTeam:X:ExchangedDocuments:SentByOrganisation" };
                yield return new object[] { "1", ExchangeDocumentDirection.SentByOrganisation, "DocEx:AgencyTeam:1:ExchangedDocuments:SentByOrganisation" };
                yield return new object[] { "the_team_name", ExchangeDocumentDirection.SentByOrganisation, "DocEx:AgencyTeam:the_team_name:ExchangedDocuments:SentByOrganisation" };
                yield return new object[] { "The_Team_Name", ExchangeDocumentDirection.SentByOrganisation, "DocEx:AgencyTeam:The_Team_Name:ExchangedDocuments:SentByOrganisation" };
            }
        }

        private static IEnumerable<object[]> BuildAgencyDocumentValidationKeyTestData
        {
            get
            {
                yield return new object[] { "x", "filename", "DocEx:AgencyTeam:x:FileShareItems:filename:ValidationResult" };
                yield return new object[] { "X", "filename", "DocEx:AgencyTeam:X:FileShareItems:filename:ValidationResult" };
                yield return new object[] { "1", "filename", "DocEx:AgencyTeam:1:FileShareItems:filename:ValidationResult" };
                yield return new object[] { "the_team_name", "filename", "DocEx:AgencyTeam:the_team_name:FileShareItems:filename:ValidationResult" };
                yield return new object[] { "The_Team_Name", "filename", "DocEx:AgencyTeam:The_Team_Name:FileShareItems:filename:ValidationResult" };

                yield return new object[] { "x", "aBcDE.doc", "DocEx:AgencyTeam:x:FileShareItems:aBcDE.doc:ValidationResult" };
                yield return new object[] { "X", "aBcDE.doc", "DocEx:AgencyTeam:X:FileShareItems:aBcDE.doc:ValidationResult" };
                yield return new object[] { "1", "aBcDE.doc", "DocEx:AgencyTeam:1:FileShareItems:aBcDE.doc:ValidationResult" };
                yield return new object[] { "the_team_name", "aBcDE.doc", "DocEx:AgencyTeam:the_team_name:FileShareItems:aBcDE.doc:ValidationResult" };
                yield return new object[] { "The_Team_Name", "aBcDE.doc", "DocEx:AgencyTeam:The_Team_Name:FileShareItems:aBcDE.doc:ValidationResult" };
            }
        }

        private static IEnumerable<object[]> BuildOrganisationExchangedDocumentsKeyTestData
        {
            get
            {
                yield return new object[]
                {
                    new OrganisationIdentifier { Type = OrganisationIdentifierType.Ukprn, Value = "12345678" },
                    ExchangeDocumentDirection.PublishedByAgency,
                    "DocEx:Organisation:Ukprn_12345678:ExchangedDocuments:PublishedByAgency"
                };
                yield return new object[]
                {
                    new OrganisationIdentifier { Type = OrganisationIdentifierType.Ukprn, Value = "12345678" },
                    ExchangeDocumentDirection.SentByOrganisation,
                    "DocEx:Organisation:Ukprn_12345678:ExchangedDocuments:SentByOrganisation"
                };
                yield return new object[]
                {
                    new OrganisationIdentifier { Type = OrganisationIdentifierType.Ukprn, Value = "10000055" },
                    ExchangeDocumentDirection.PublishedByAgency,
                    "DocEx:Organisation:Ukprn_10000055:ExchangedDocuments:PublishedByAgency"
                };
                yield return new object[]
                {
                    new OrganisationIdentifier { Type = OrganisationIdentifierType.Ukprn, Value = "10000055" },
                    ExchangeDocumentDirection.SentByOrganisation,
                    "DocEx:Organisation:Ukprn_10000055:ExchangedDocuments:SentByOrganisation"
                };
                yield return new object[]
                {
                    new OrganisationIdentifier { Type = OrganisationIdentifierType.CompanyRegistrationNumber, Value = "07674473" },
                    ExchangeDocumentDirection.PublishedByAgency,
                    "DocEx:Organisation:CompanyRegistrationNumber_07674473:ExchangedDocuments:PublishedByAgency"
                };
                yield return new object[]
                {
                    new OrganisationIdentifier { Type = OrganisationIdentifierType.CompanyRegistrationNumber, Value = "07674473" },
                    ExchangeDocumentDirection.SentByOrganisation,
                    "DocEx:Organisation:CompanyRegistrationNumber_07674473:ExchangedDocuments:SentByOrganisation"
                };
            }
        }

        private static IEnumerable<object[]> BuildConfigurationKeyTestData
        {
            get
            {
                yield return new object[] { ConfigurationSection.DocumentExchangeEnabled, "DocEx:Configuration:DocumentExchangeEnabled" };
                yield return new object[] { ConfigurationSection.ServiceNoReplyEmail, "DocEx:Configuration:ServiceNoReplyEmail" };
                yield return new object[] { ConfigurationSection.ServiceReplyEmail, "DocEx:Configuration:ServiceReplyEmail" };
                yield return new object[] { ConfigurationSection.AgencyPublishCompleteProviderNotificationEnabled, "DocEx:Configuration:AgencyPublishCompleteProviderNotificationEnabled" };
            }
        }

        private static IEnumerable<object[]> BuildListConfigurationKeyTestData
        {
            get
            {
                yield return new object[] { ConfigurationSection.Products, "DocEx:ListConfiguration:Products" };
                yield return new object[] { ConfigurationSection.Teams, "DocEx:ListConfiguration:Teams" };
                yield return new object[] { ConfigurationSection.AllowedFileExtensions, "DocEx:ListConfiguration:AllowedFileExtensions" };
            }
        }

        private static IEnumerable<object[]> BuildEmailResourceLockKeyTestData
        {
            get
            {
                yield return new object[] { "parent-batch-identifier-1", "DocEx:EmailLock:parent-batch-identifier-1" };
                yield return new object[] { "parent-batch-identifier-2", "DocEx:EmailLock:parent-batch-identifier-2" };
                yield return new object[] { "parent-batch-identifier-3", "DocEx:EmailLock:parent-batch-identifier-3" };
            }
        }
    }
}
