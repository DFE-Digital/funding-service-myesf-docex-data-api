using Pds.Core.Utils.Helpers;
using Pds.DocumentExchange.Data.Services.DTOs;
using Pds.DocumentExchange.Data.Services.Enums;
using Pds.DocumentExchange.Data.Services.Interfaces.Filters;
using System;
using System.Linq;

namespace Pds.DocumentExchange.Data.Services.Implementations.Filters
{
    /// <summary>
    /// The uploaded date range filter.
    /// </summary>
    public class UploadedDateRangeFilter : IDateRangeFilter<ExchangeDocument>
    {
        /// <inheritdoc/>
        public string FilterTitle
            => "Filter by date";

        /// <inheritdoc/>
        public string FilterKey
            => Enums.FilterKey.UploadDate.ToString();

        /// <inheritdoc/>
        public FilterType FilterType => FilterType.DateRangeFilter;

        /// <inheritdoc/>
        public bool IsElementInsideRange(ExchangeDocument element, DateTime start, DateTime end)
        {
            It.IsNull(element)
                .AsGuard<ArgumentNullException>();

            (start.CompareTo(end) > 0)
                .AsGuard<ArgumentException>("The start should be smaller or equal than the end.");

            bool isInside = false;


            var uploadedExternalHistory = element.ExchangeDirection == ExchangeDocumentDirection.SentByOrganisation ?
                element?.EventHistory?.SingleOrDefault(history => history.EventType == ExchangeDocumentEventType.SentByOrganisation) :
                element?.EventHistory?.SingleOrDefault(history => history.EventType == ExchangeDocumentEventType.PublishedByAgency);

            if (uploadedExternalHistory != null)
            {
                var uploadedDate = uploadedExternalHistory.EventDateTime.Date;
                isInside = start.CompareTo(uploadedDate) <= 0 && end.CompareTo(uploadedDate) >= 0;
            }

            return isInside;
        }
    }
}