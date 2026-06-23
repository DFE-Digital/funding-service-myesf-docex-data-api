using Pds.DocumentExchange.Data.Services.DTOs;
using Pds.DocumentExchange.Data.Services.Enums;
using Pds.Services.Common.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Implementations.Filters
{
    /// <summary>
    /// The status type filter.
    /// </summary>
    public class StatusTypeFilter : ListFilterBase<ExchangeDocument>
    {
        private const string New = "New";
        private const string Downloaded = "Downloaded";

        /// <summary>
        /// Initializes a new instance of the <see cref="StatusTypeFilter"/> class.
        /// </summary>
        /// <param name="elements">The collection of elements to be filtered.</param>
        public StatusTypeFilter(IEnumerable<ExchangeDocument> elements)
            : base(elements)
        {
        }

        /// <inheritdoc/>
        public override string FilterTitle
            => "Filter by status";

        /// <inheritdoc/>
        public override string FilterKey
            => Enums.FilterKey.Status.ToString();

        /// <inheritdoc/>
        public override Func<ExchangeDocument, Task<string>> GetFilterValueFromElement
            => (exchangeDocument) => Task.FromResult(GetStatusTypeFromFile(exchangeDocument));

        /// <inheritdoc/>
        public override Func<string, Task<string>> GetFilterTitleFromValue
            => (value) => Task.FromResult(value);

        private string GetStatusTypeFromFile(ExchangeDocument exchangeDocument)
        {
            if (It.IsEmpty(exchangeDocument.EventHistory))
            {
                return New;
            }

            var fileDownloaded = exchangeDocument.EventHistory
                .Any(history => history.EventType == ExchangeDocumentEventType.DownloadedByReceiver);

            return fileDownloaded ? Downloaded : New;
        }
    }
}