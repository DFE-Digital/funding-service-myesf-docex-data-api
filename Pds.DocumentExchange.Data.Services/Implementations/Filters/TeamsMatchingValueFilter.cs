using Pds.DocumentExchange.Data.Services.Enums;
using Pds.DocumentExchange.Data.Services.Interfaces.Filters;
using Pds.DocumentExchange.Data.Services.Interfaces.Settings;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Implementations.Filters
{
    /// <summary>
    /// The teams matching value filter.
    /// </summary>
    /// <typeparam name="TElement">The element type.</typeparam>
    public abstract class TeamsMatchingValueFilter<TElement> : IMatchingValueSelectionFilter<TElement>
    {
        private readonly IConfigurationDataService _configurationDataService;

        /// <summary>
        /// Initializes a new instance of the <see cref="TeamsMatchingValueFilter{TElement}"/> class.
        /// </summary>
        /// <param name="configurationDataService">The configuration data service.</param>
        protected TeamsMatchingValueFilter(IConfigurationDataService configurationDataService)
        {
            _configurationDataService = configurationDataService;
        }

        /// <inheritdoc/>
        public string FilterTitle
            => "Select a team";

        /// <inheritdoc/>
        public string FilterKey
            => Enums.FilterKey.Team.ToString();

        /// <inheritdoc/>
        public FilterType FilterType
            => FilterType.RadioFilter;

        /// <inheritdoc/>
        public async Task<IEnumerable<(string Title, string Value)>> GetAllTitleValuePairs()
        {
            var teams = await _configurationDataService.GetTeams();
            return teams.Select(team => (Title: team.Name, Value: team.Identifier))
                .OrderBy(t => t.Title);
        }

        /// <inheritdoc/>
        public abstract Task<bool> DoesElementMatchValue(TElement element, string value);
    }
}