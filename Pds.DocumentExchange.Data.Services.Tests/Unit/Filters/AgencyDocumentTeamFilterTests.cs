using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pds.DocumentExchange.Data.Services.DTOs;
using Pds.DocumentExchange.Data.Services.Enums;
using Pds.DocumentExchange.Data.Services.Implementations.Filters;
using Pds.DocumentExchange.Data.Services.Interfaces.Settings;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Tests.Unit.Filters
{
    [TestClass]
    public class AgencyDocumentTeamFilterTests
    {
        private readonly Mock<IConfigurationDataService> _configurationDataService = new Mock<IConfigurationDataService>(MockBehavior.Strict);

        private readonly AgencyDocumentTeamFilter _filter;

        public AgencyDocumentTeamFilterTests()
        {
            _filter = new AgencyDocumentTeamFilter(_configurationDataService.Object);
        }

        [TestMethod, TestCategory("Unit")]
        public void FilterTitle_ReturnsExpectedTitle()
        {
            // Assert
            _filter.FilterTitle.Should().Be("Select a team");
        }

        [TestMethod, TestCategory("Unit")]
        public void FilterKey_ReturnsExpectedFilterKey()
        {
            // Assert
            _filter.FilterKey.Should().Be("Team");
        }

        [TestMethod, TestCategory("Unit")]
        public void FilterType_ReturnsDefaultFilterType()
        {
            // Assert
            _filter.FilterType.Should().Be(FilterType.RadioFilter);
        }

        [TestMethod, TestCategory("Unit")]
        [DataRow("team_01", "team_01", true)]
        [DataRow("team_01", "TEAM_01", true)]
        [DataRow("team_01", "TeAm_01", true)]
        [DataRow("team_01", "team_99", false)]
        [DataRow("team_name", "other_team_name", false)]
        public async Task DoesElementMatchValue_ReturnsExpected(string agencyDocumentTeam, string teamValue, bool expected)
        {
            // Arrange
            var agencyDocument = new AgencyDocument
            {
                Team = agencyDocumentTeam
            };

            // Act
            var result = await _filter.DoesElementMatchValue(agencyDocument, teamValue);

            // Assert
            result.Should().Be(expected);
        }

        [TestMethod, TestCategory("Unit")]
        public async Task GetAllTitleValuePairs_ReturnsTeamsTitlesAndValues()
        {
            // Arrange
            var teams = GetTeams();

            var expectedResult = new[]
            {
                (Title: "Team 1", Value: "team1"),
                (Title: "Team 2", Value: "team2"),
                (Title: "Team 3", Value: "team3")
            };

            _configurationDataService
                .Setup(config => config.GetTeams())
                .ReturnsAsync(teams);

            // Act
            var result = await _filter.GetAllTitleValuePairs();

            // Assert
            result.Should().BeEquivalentTo(expectedResult);
        }

        private IReadOnlyCollection<AgencyTeam> GetTeams()
            => new[]
            {
                CreateAgencyTeam("Team 1", "team1"),
                CreateAgencyTeam("Team 2", "team2"),
                CreateAgencyTeam("Team 3", "team3")
            };

        private AgencyTeam CreateAgencyTeam(string name, string identifier)
            => new AgencyTeam
            {
                Name = name,
                Identifier = identifier
            };
    }
}