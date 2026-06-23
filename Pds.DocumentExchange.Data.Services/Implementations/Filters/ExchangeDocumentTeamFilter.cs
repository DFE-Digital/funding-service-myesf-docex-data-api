using Pds.DocumentExchange.Data.Services.DTOs;
using Pds.DocumentExchange.Data.Services.Extensions;
using Pds.DocumentExchange.Data.Services.Interfaces.Settings;
using System.Linq;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Implementations.Filters
{
    /// <summary>
    /// The exchange document team filter.
    /// </summary>
    /// <typeparam name="TElement">The element type to be filtered.</typeparam>
    public class ExchangeDocumentTeamFilter : TeamsMatchingValueFilter<ExchangeDocument>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExchangeDocumentTeamFilter"/> class.
        /// </summary>
        /// <param name="configurationDataService">The configuration data service.</param>
        public ExchangeDocumentTeamFilter(IConfigurationDataService configurationDataService)
            : base(configurationDataService)
        {
        }

        /// <inheritdoc/>
        public override Task<bool> DoesElementMatchValue(ExchangeDocument element, string value)
            => Task.FromResult(element.Product.AgencyTeams.Any(team => team.IsEqualToIgnoreCase(value)));
    }
}