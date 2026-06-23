using Pds.Core.Utils.Helpers;
using Pds.DocumentExchange.Data.Services.DTOs;
using Pds.DocumentExchange.Data.Services.Extensions;
using Pds.DocumentExchange.Data.Services.Interfaces.Lookup;
using Pds.DocumentExchange.Data.Services.Interfaces.Settings;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Implementations.Lookup
{
    /// <summary>
    /// The teams lookup.
    /// </summary>
    public class TeamsLookup : ITeamsLookup
    {
        private readonly IConfigurationDataService _configurationDataService;

        /// <summary>
        /// Initializes a new instance of the <see cref="TeamsLookup"/> class.
        /// </summary>
        /// <param name="configurationDataService">The configuration data service.</param>
        public TeamsLookup(IConfigurationDataService configurationDataService)
        {
            _configurationDataService = configurationDataService;
        }

        /// <inheritdoc/>
        public async Task<bool> Exists(string identifier)
            => !(await Get(identifier) is UnknownAgencyTeam);

        /// <inheritdoc/>
        public async Task<AgencyTeam> Get(string identifier)
        {
            It.IsEmpty(identifier)
               .AsGuard<ArgumentNullException>(nameof(identifier));

            var teams = await _configurationDataService.GetTeams();
            return teams.SingleOrDefault(team => team.Identifier.IsEqualToIgnoreCase(identifier)) ?? new UnknownAgencyTeam();
        }
    }
}