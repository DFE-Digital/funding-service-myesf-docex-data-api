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
    public class TeamsLookupTests
    {
        private readonly Mock<IConfigurationDataService> _configurationDataService = new Mock<IConfigurationDataService>(MockBehavior.Strict);

        private readonly TeamsLookup _teamsLookup;

        public TeamsLookupTests()
        {
            _configurationDataService
               .Setup(config => config.GetTeams())
               .ReturnsAsync(GetTestAgencyTeams());

            _teamsLookup = new TeamsLookup(_configurationDataService.Object);
        }

        [TestMethod]
        [DataRow("team-99")]
        [DataRow("team-11")]
        [DataRow("non-existing-team")]
        public async Task Exists_WhenTeamDoesntExist_ReturnsFalse(string teamIdentifier)
        {
            // Act
            var result = await _teamsLookup.Exists(teamIdentifier);

            // Assert
            result.Should().BeFalse();
        }

        [TestMethod]
        [DataRow("team-1")]
        [DataRow("TEAM-2")]
        [DataRow("TeAm-10")]
        public async Task Exists_WhenTeamExists_ReturnsTrue(string teamIdentifier)
        {
            // Act
            var result = await _teamsLookup.Exists(teamIdentifier);

            // Assert
            result.Should().BeTrue();
        }

        [TestMethod]
        [DataRow("team-99")]
        [DataRow("team-11")]
        [DataRow("non-existing-team")]
        public async Task Get_WhenTeamDoesntExist_ReturnsUnknownTeam(string teamIdentifier)
        {
            // Act
            var result = await _teamsLookup.Get(teamIdentifier);

            // Assert
            result.Should().BeOfType<UnknownAgencyTeam>();
        }

        [TestMethod]
        [DataRow("team-1", 1)]
        [DataRow("TEAM-2", 2)]
        [DataRow("TeAm-10", 10)]
        public async Task Get_WhenTeamExists_ReturnsTeam(string teamIdentifier, int number)
        {
            // Arrange
            var expectedTeam = CreateTestAgencyTeam(number);

            // Act
            var result = await _teamsLookup.Get(teamIdentifier);

            // Assert
            result.Should().BeEquivalentTo(expectedTeam);
        }

        private IReadOnlyCollection<AgencyTeam> GetTestAgencyTeams()
            => Enumerable.Range(1, 10)
                .Select(number => CreateTestAgencyTeam(number))
                .AsSafeReadOnlyList();

        private AgencyTeam CreateTestAgencyTeam(int number)
            => new AgencyTeam
            {
                Identifier = $"team-{number}",
                Name = $"Team {number}"
            };
    }
}