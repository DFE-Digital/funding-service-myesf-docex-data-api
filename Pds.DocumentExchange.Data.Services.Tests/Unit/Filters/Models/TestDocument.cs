using System;

namespace Pds.DocumentExchange.Data.Services.Tests.Unit.Filters.Models
{
    public class TestDocument
    {
        public string Name { get; set; }

        public string DocumentType { get; set; }

        public string Organisation { get; set; }

        public DateTime UploadedDateTime { get; set; }

        public string Team { get; set; }

        public string Ukprn { get; set; }
    }
}