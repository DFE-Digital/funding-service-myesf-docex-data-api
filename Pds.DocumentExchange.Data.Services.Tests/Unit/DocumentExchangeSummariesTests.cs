using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pds.Core.Common.Organisation.Enums;
using Pds.Core.Common.Organisation.Models;
using Pds.DocumentExchange.Data.Services.DTOs;
using Pds.DocumentExchange.Data.Services.DTOs.User;
using Pds.DocumentExchange.Data.Services.Enums;
using Pds.DocumentExchange.Data.Services.Implementations;
using Pds.DocumentExchange.Data.Services.Interfaces;
using Pds.DocumentExchange.Data.Services.Interfaces.Lookup;
using Pds.DocumentExchange.Data.Services.Interfaces.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Tests.Unit
{
    [TestClass]
    public class DocumentExchangeSummariesTests
    {
        private readonly Mock<IAgencyExchangeDocumentsService> _agencyExchangeDocumentsService = new Mock<IAgencyExchangeDocumentsService>(MockBehavior.Strict);
        private readonly Mock<IOrganisationExchangeDocumentsService> _organisationExchangeDocumentsService = new Mock<IOrganisationExchangeDocumentsService>(MockBehavior.Strict);
        private readonly Mock<ITeamsLookup> _teamsLookup = new Mock<ITeamsLookup>(MockBehavior.Strict);
        private readonly Mock<IConfigurationDataService> _configurationDataService = new Mock<IConfigurationDataService>(MockBehavior.Strict);

        private readonly DocumentExchangeSummaries _documentExchangeSummaries;

        public DocumentExchangeSummariesTests()
        {
            _documentExchangeSummaries = new DocumentExchangeSummaries(
                _agencyExchangeDocumentsService.Object,
                _organisationExchangeDocumentsService.Object,
                _teamsLookup.Object,
                _configurationDataService.Object);
        }

        #region GetTeamsSummary

        [TestMethod, TestCategory("Unit")]
        [DataRow(false)]
        [DataRow(true)]
        public async Task GetTeamsSummary_WhenTeamsListIsNullOrEmpty_ThrowsArgumentNullException(bool isNull)
        {
            IEnumerable<string> teams = isNull ? null : Enumerable.Empty<string>();

            // Act
            Func<Task<Summary>> func = () => _documentExchangeSummaries.GetTeamsSummary(teams);

            // Assert
            await func.Should().ThrowAsync<ArgumentNullException>();
        }

        [TestMethod, TestCategory("Unit")]
        public async Task GetTeamsSummary_WhenTeamIsInvalid_ThrowsArgumentException()
        {
            // Arrange
            var validTeam = "valid-team";
            var invalidTeam = "invalid-team";

            _teamsLookup
                .Setup(teamValidator => teamValidator.Exists(validTeam))
                .ReturnsAsync(true);

            _teamsLookup
                .Setup(teamValidator => teamValidator.Exists(invalidTeam))
                .ReturnsAsync(false);

            // Act
            Func<Task<Summary>> func = () => _documentExchangeSummaries.GetTeamsSummary(new[] { validTeam, invalidTeam });

            // Assert
            await func.Should().ThrowAsync<ArgumentException>();
        }

        [TestMethod, TestCategory("Unit")]
        public async Task GetTeamsSummary_WhenTeamsAreValid_ReturnsSummary()
        {
            // Arrange
            var teams = new[] { "valid-team-01", "valid-team-02", "valid-team-03" };
            var newDocuments = new List<DocumentWithViewedStatus>
            {
                new DocumentWithViewedStatus { FileType = "fileType", FromUKPRN = 1, Version = 1, Viewed = true, Year = "202020" },

                new DocumentWithViewedStatus { FileType = "fileType", FromUKPRN = 1, Version = 1, Viewed = false, Year = "202021" },
                new DocumentWithViewedStatus { FileType = "fileType", FromUKPRN = 1, Version = 2, Viewed = true, Year = "202021" },

                new DocumentWithViewedStatus { FileType = "fileType2", FromUKPRN = 2, Version = 1, Viewed = false, Year = "202021" },
                new DocumentWithViewedStatus { FileType = "fileType2", FromUKPRN = 2, Version = 2, Viewed = false, Year = "202021" },
                new DocumentWithViewedStatus { FileType = "fileType2", FromUKPRN = 2, Version = 3, Viewed = false, Year = "202021" }
            };

            _teamsLookup
                .Setup(teamValidator => teamValidator.Exists(It.IsAny<string>()))
                .ReturnsAsync(true);

            _agencyExchangeDocumentsService
                .Setup(agencyService => agencyService.GetReceivedFilesForAgencyWithSeenHistory(teams))
                .ReturnsAsync(newDocuments);

            _configurationDataService
                .Setup(config => config.IsDocumentExchangeEnabled())
                .ReturnsAsync(true);

            var expectedResult = new Summary
            {
                CountOfNewDocuments = 1,
                DocumentExchangeEnabled = true
            };

            // Act
            var result = await _documentExchangeSummaries.GetTeamsSummary(teams);

            // Assert
            result.Should().BeEquivalentTo(expectedResult);
            Mock.VerifyAll(_teamsLookup, _agencyExchangeDocumentsService, _configurationDataService);
        }

        #endregion


        #region GetOrganisationUserSummary

        [TestMethod, TestCategory("Unit")]
        public async Task GetOrganisationUserSummary_WhenUserInfoIsNull_ThrowsArgumentNullException()
        {
            // Act
            Func<Task<Summary>> func = () => _documentExchangeSummaries.GetOrganisationUserSummary(null);

            // Assert
            await func.Should().ThrowAsync<ArgumentNullException>();
        }

        [TestMethod, TestCategory("Unit")]
        public async Task GetOrganisationUserSummary_WhenUserInfoOrganisationIdentifierIsNull_ThrowsArgumentException()
        {
            // Arrange
            var userInfo = new UserInfo
            {
                Principal = "principal",
                FullName = "full-name",
                EmailAddress = "email-address@education.gov.uk",
                OrganisationInfo = new OrganisationInfo
                {
                    Name = "organisation-name",
                    OrganisationIdentifier = null
                }
            };

            // Act
            Func<Task<Summary>> func = () => _documentExchangeSummaries.GetOrganisationUserSummary(userInfo);

            // Assert
            await func.Should().ThrowAsync<ArgumentException>();
        }

        [TestMethod, TestCategory("Unit")]
        [DataRow(null)]
        [DataRow("")]
        [DataRow(" ")]
        public async Task GetOrganisationUserSummary_WhenUserInfoPrincipalIsNullOrEmpty_ThrowsArgumentException(string principal)
        {
            // Arrange
            var userInfo = new UserInfo
            {
                Principal = principal,
                FullName = "full-name",
                EmailAddress = "email-address@education.gov.uk",
                OrganisationInfo = new OrganisationInfo
                {
                    Name = "organisation-name",
                    OrganisationIdentifier = new OrganisationIdentifier
                    {
                        Type = OrganisationIdentifierType.Ukprn,
                        Value = "12345678"
                    }
                }
            };

            // Act
            Func<Task<Summary>> func = () => _documentExchangeSummaries.GetOrganisationUserSummary(userInfo);

            // Assert
            await func.Should().ThrowAsync<ArgumentException>();
        }

        [TestMethod, TestCategory("Unit")]
        public async Task GetOrganisationUserSummary_WhenUserInfoIsValid_ReturnsSummary()
        {
            // Arrange
            var organisationIdentifier = new OrganisationIdentifier
            {
                Type = OrganisationIdentifierType.Ukprn,
                Value = "12345678"
            };

            var userInfo = new UserInfo
            {
                Principal = "principal",
                FullName = "full-name",
                EmailAddress = "email-address@education.gov.uk",
                OrganisationInfo = new OrganisationInfo
                {
                    Name = "organisation-name",
                    OrganisationIdentifier = organisationIdentifier
                }
            };

            var exchangeDocuments = CreateExchangeDocuments(200);

            _organisationExchangeDocumentsService
                .Setup(organisationService => organisationService.GetExchangeDocuments(organisationIdentifier, It.IsAny<ExchangeDocumentDirection>()))
                .ReturnsAsync(exchangeDocuments);

            _configurationDataService
                .Setup(config => config.IsDocumentExchangeEnabled())
                .ReturnsAsync(true);

            var expectedResult = new Summary
            {
                CountOfNewDocuments = 100,
                DocumentExchangeEnabled = true
            };

            // Act
            var result = await _documentExchangeSummaries.GetOrganisationUserSummary(userInfo);

            // Assert
            result.Should().BeEquivalentTo(expectedResult);
            Mock.VerifyAll(_organisationExchangeDocumentsService, _configurationDataService);
        }

        #endregion

        private IEnumerable<ExchangeDocument> CreateExchangeDocuments(int numberOfDocuments)
           => Enumerable.Range(1, numberOfDocuments)
               .Select(i => CreateExchangeDocument(i))
               .ToList();

        private ExchangeDocument CreateExchangeDocument(int number)
           => new ExchangeDocument
           {
               DocumentReference = new DocumentReference
               {
                   BatchIdentifier = $"batch-id-{number}",
                   ParentBatchIdentifier = $"parent-batch-id-{number}",
                   FileName = $"file-{number}.pdf"
               },
               EventHistory = new[]
               {
                   new ExchangeDocumentEvent
                   {
                       EventType = number % 2 == 0
                        ? ExchangeDocumentEventType.SentByOrganisation
                        : ExchangeDocumentEventType.DownloadedByReceiver
                   }
               }
           };
    }
}