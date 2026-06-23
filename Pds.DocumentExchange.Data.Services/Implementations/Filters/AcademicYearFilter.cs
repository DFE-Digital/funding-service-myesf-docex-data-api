using Pds.DocumentExchange.Data.Services.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Implementations.Filters
{
    /// <summary>
    /// The academic year filter.
    /// </summary>
    public class AcademicYearFilter : ListFilterBase<ExchangeDocument>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AcademicYearFilter"/> class.
        /// </summary>
        /// <param name="elements">The collection of elements to be filtered.</param>
        public AcademicYearFilter(IEnumerable<ExchangeDocument> elements)
            : base(elements)
        {
        }

        /// <inheritdoc/>
        public override string FilterTitle
            => "Filter by academic year";

        /// <inheritdoc/>
        public override string FilterKey
            => Enums.FilterKey.AcademicYear.ToString();

        /// <inheritdoc/>
        public override Func<ExchangeDocument, Task<string>> GetFilterValueFromElement
            => (exchangeDocument) => Task.FromResult(exchangeDocument.Year.ToString());

        /// <inheritdoc/>
        public override Func<string, Task<string>> GetFilterTitleFromValue
            => (value) => Task.FromResult(GetYearTitleFromValue(value));

        private string GetYearTitleFromValue(string value)
        {
            bool isFormatValid = int.TryParse(value, out _) && value.Length == 6;

            if (!isFormatValid)
            {
                return "[Unknown year]";
            }

            var yearFirstTwoDigits = value.Substring(0, 2);
            var firstYearLastTwoDigits = value.Substring(2, 2);
            var secondYearLastTwoDigits = value.Substring(4, 2);

            var firstYear = $"{yearFirstTwoDigits}{firstYearLastTwoDigits}";
            var secondYear = $"{yearFirstTwoDigits}{secondYearLastTwoDigits}";

            return $"{firstYear} to {secondYear}";
        }
    }
}