using Pds.Core.Common.Organisation.Enums;
using Pds.DocumentExchange.Data.Services.DTOs;
using Pds.DocumentExchange.Data.Services.Enums;
using Pds.DocumentExchange.Data.Services.Extensions;
using Pds.DocumentExchange.Data.Services.Interfaces.Filters;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Implementations.Filters
{
    /// <summary>
    /// The exchange document UKPRN filter.
    /// </summary>
    public class UkprnFilter : IMatchingTextBoxValueFilter<ExchangeDocument>
    {
        /// <inheritdoc/>
        public string FilterTitle
            => "Search by UKPRN";

        /// <inheritdoc/>
        public string FilterKey
            => Enums.FilterKey.Ukprn.ToString();

        /// <inheritdoc/>
        public FilterType FilterType
            => FilterType.TextBoxFilter;

        /// <inheritdoc/>
        public string TextBoxHint
            => "Search by UK Provider Reference Number. Must be an 8 digit number";

        /// <inheritdoc/>
        public string ValidationErrorMessage
            => "Please enter a valid UKPRN";

        /// <inheritdoc/>
        public string Regex
            => "^[1][0-9]{7}$";

        /// <inheritdoc/>
        public Task<bool> DoesElementMatchValue(ExchangeDocument element, string value)
        {
            var organisationIdentifier = element?.OrganisationInfo?.OrganisationIdentifier;

            if (organisationIdentifier == null
                || (organisationIdentifier.Type != OrganisationIdentifierType.Ukprn
                    || string.IsNullOrWhiteSpace(organisationIdentifier.Value)))
            {
                return Task.FromResult(false);
            }

            return Task.FromResult(organisationIdentifier.Value.IsEqualToIgnoreCase(value));
        }
    }
}