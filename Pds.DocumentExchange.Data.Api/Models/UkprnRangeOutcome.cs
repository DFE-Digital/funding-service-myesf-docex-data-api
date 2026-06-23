using CsvHelper.Configuration.Attributes;
using System;

namespace Pds.DocumentExchange.Data.Api.Models
{
    /// <summary>
    /// Structure to hold a UKPRN range process outcomes.
    /// </summary>
    public class UkprnRangeOutcome
    {
        /// <summary>
        /// Gets or sets the function instance ID.
        /// </summary>
        [Ignore]
        public Guid InstanceId { get; set; }

        /// <summary>
        /// Gets or sets when the range finished processing.
        /// </summary>
        [Format("dd-MM-yyyy HH:mm:ss")]
        [Name("Date and time")]
        [Index(1)]
        public DateTime FinishedAt { get; set; }

        /// <summary>
        /// Gets or sets the outcome type (parent or child).
        /// </summary>
        [Index(1)]
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the 'from' UKPRN.
        /// </summary>
        [Index(2)]
        public int From { get; set; }

        /// <summary>
        /// Gets or sets the 'to' UKPRN.
        /// </summary>
        [Index(3)]
        public int To { get; set; }

        /// <summary>
        /// Gets or sets the number of successfully processed organisations.
        /// </summary>
        [Name("Counts")]
        [Index(4)]
        public int NumberOfSuccessfullyProcessedOrganisations { get; set; }

        /// <summary>
        /// Gets or sets the page number.
        /// </summary>
        [Index(5)]
        public int? PageNumber { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the UKPRN range has been successfully processed.
        /// </summary>
        [Name("Outcome")]
        [Index(6)]
        [BooleanTrueValues("Succeeded")]
        [BooleanFalseValues("Failed")]
        public bool IsSuccess { get; set; }

        /// <summary>
        /// Gets or sets the error message when the outcome didn't succeed.
        /// </summary>
        [Index(7)]
        public string Error { get; set; }
    }
}