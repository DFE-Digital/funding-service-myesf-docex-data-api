using Pds.DocumentExchange.Data.Services.DTOs;
using Pds.DocumentExchange.Data.Services.Enums;
using Pds.DocumentExchange.Data.Services.Interfaces.Converters;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Implementations.Filters
{
    /// <summary>
    /// The document error filter.
    /// </summary>
    public class DocumentErrorTypeFilter : ListFilterBase<AgencyDocument>
    {
        private readonly IConvertAgencyDocumentErrorTypes _converter;

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentErrorTypeFilter"/> class.
        /// </summary>
        /// <param name="elements">The collection of elements to be filtered.</param>
        /// <param name="converter">The error type to string converter.</param>
        public DocumentErrorTypeFilter(
            IEnumerable<AgencyDocument> elements,
            IConvertAgencyDocumentErrorTypes converter)
            : base(elements)
        {
            _converter = converter;
        }

        /// <inheritdoc/>
        public override string FilterTitle
            => "Filter by document name error";

        /// <inheritdoc/>
        public override string FilterKey
            => Enums.FilterKey.DocumentNameError.ToString();

        /// <inheritdoc/>
        public override Func<AgencyDocument, Task<string>> GetFilterValueFromElement
            => agencyDocument => Task.FromResult(agencyDocument.ErrorType.ToString());

        /// <inheritdoc/>
        public override Func<string, Task<string>> GetFilterTitleFromValue
            => (value) => Task.FromResult(GetTitleFromValue(value));

        private string GetTitleFromValue(string value)
            => Enum.TryParse(value, out AgencyDocumentErrorType errorType)
            ? _converter.Convert(errorType)
            : "Unknown error type";
    }
}