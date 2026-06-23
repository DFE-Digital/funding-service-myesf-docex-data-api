using Pds.Core.Common.Organisation.Enums;
using Pds.Core.Common.Organisation.Models;
using Pds.DocumentExchange.Data.Services.DTOs;
using Pds.DocumentExchange.Data.Services.Interfaces.Lookup;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Implementations.Filters
{
    /// <summary>
    /// The organisation filter.
    /// </summary>
    public class OrganisationFilter : ListFilterBase<ExchangeDocument>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OrganisationFilter"/> class.
        /// </summary>
        /// <param name="elements">The collection of elements to be filtered.</param>
        public OrganisationFilter(IEnumerable<ExchangeDocument> elements)
            : base(elements)
        {
        }

        /// <inheritdoc/>
        public override string FilterTitle
            => "Filter by organisation";

        /// <inheritdoc/>
        public override string FilterKey
            => Enums.FilterKey.Organisation.ToString();

        /// <inheritdoc/>
        public override Func<ExchangeDocument, Task<string>> GetFilterValueFromElement
            => (exchangeDocument) => Task.FromResult(exchangeDocument.Organisation.Name);

        /// <inheritdoc/>
        public override Func<string, Task<string>> GetFilterTitleFromValue
            => (orgName) => Task.FromResult(orgName);
    }
}