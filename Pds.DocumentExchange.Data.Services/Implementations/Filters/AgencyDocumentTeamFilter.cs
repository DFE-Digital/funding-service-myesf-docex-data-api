using Pds.DocumentExchange.Data.Services.DTOs;
using Pds.DocumentExchange.Data.Services.Extensions;
using Pds.DocumentExchange.Data.Services.Interfaces.Settings;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Implementations.Filters
{
    /// <summary>
    /// The team filter.
    /// </summary>
    /// <typeparam name="TElement">The element type to be filtered.</typeparam>
    public class AgencyDocumentTeamFilter : TeamsMatchingValueFilter<AgencyDocument>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AgencyDocumentTeamFilter"/> class.
        /// </summary>
        /// <param name="configurationDataService">The configuration data service.</param>
        public AgencyDocumentTeamFilter(IConfigurationDataService configurationDataService)
            : base(configurationDataService)
        {
        }

        /// <inheritdoc/>
        public override Task<bool> DoesElementMatchValue(AgencyDocument element, string value)
            => Task.FromResult(element.Team.IsEqualToIgnoreCase(value));
    }
}