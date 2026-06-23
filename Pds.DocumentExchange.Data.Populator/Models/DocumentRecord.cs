using CsvHelper.Configuration.Attributes;
using System;

namespace Pds.DocumentExchange.Data.Populator.Models
{
    [Serializable]
    public class DocumentRecord
    {
        [Index(0)]
        public int Ukprn { get; set; }

        [Index(1)]
        public string ProviderId { get; set; }

        [Index(2)]
        public string AcademicYear { get; set; }

        [Index(3)]
        public string FileType { get; set; }

        [Index(4)]
        public string FileSize { get; set; }

        [Index(5)]
        public string ParentBatchIdentifier { get; set; }

        [Index(6)]
        public DateTime? BatchCreatedDate { get; set; }
    }
}
