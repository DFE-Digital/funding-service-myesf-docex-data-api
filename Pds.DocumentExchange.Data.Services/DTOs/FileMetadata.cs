using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Pds.DocumentExchange.Data.Services.DTOs
{
    /// <summary>
    /// The file metadata, for example, its location, who uploaded it etc.
    /// </summary>
    public class FileMetadata
    {
        /// <summary>
        /// the agency id.
        /// </summary>
        private const int InternalAgencyID = -999;

        /// <summary>
        /// Gets or sets the filename of the data in the file storage mechanism its in.
        /// </summary>
        [JsonProperty(PropertyName = "Filename")]
        public string FileName { get; set; }

        /// <summary>
        /// Gets or sets what type of file the user said it was when they uploaded it (External) / information gleaned
        /// from the filename (Internal).
        /// </summary>
        [JsonProperty(PropertyName = "FileType")]
        public string ProductIdentifier { get; set; }

        /// <summary>
        /// Gets or sets any extra metadata that was given about the file.
        /// </summary>
        public Dictionary<string, string> Metadata { get; set; }

        /// <summary>
        /// Gets or sets the filename the file originally had before we changed it to store on our file systems.
        /// </summary>
        public string OriginalFileName { get; set; }

        /// <summary>
        /// Gets or sets the history of the file (such as when the document was last viewed via the portal (if ever)).
        /// </summary>
        public IEnumerable<FileMetadataHistory> History { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the file has been processed yet (i.e. virus scanned etc...)
        /// </summary>
        public bool Processed { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the virus scan verified the file as virus-free.
        /// </summary>
        [JsonProperty(PropertyName = "Okay")]
        public bool VirusScanSuccessful { get; set; }

        /// <summary>
        /// Gets or sets which provider sent the document (the ESFA is treated as a provider with a special UKRPN).
        /// </summary>
        [JsonProperty(PropertyName = "FromUKPRN")]
        public int FromUkprn { get; set; }

        /// <summary>
        /// Gets or sets which provider should receive the document (the ESFA is treated as a provider with a special UKRPN).
        /// </summary>
        [JsonProperty(PropertyName = "ToUKPRN")]
        public int ToUkprn { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the ESFA requested that this version of the document should be replaced.
        /// </summary>
        public bool ReplaceRequested { get; set; }

        /// <summary>
        /// Gets or sets the version of the file (worked out as a composite of From UKPRN, To UKPRN, File type and year).
        /// </summary>
        public int Version { get; set; }

        /// <summary>
        /// Gets or sets file expires at this time, and shouldn't be shown after.
        /// </summary>
        [JsonProperty(PropertyName = "ExpiresAt")]
        public DateTime? ExpiresAtDateTime { get; set; }

        /// <summary>
        /// Gets a value indicating whether is from agency.
        /// </summary>
        public bool IsFromAgency => FromUkprn == InternalAgencyID;

        /// <summary>
        /// Gets or sets a value indicating whether the file has been deleted.
        /// </summary>
        public bool Deleted { get; set; }
    }
}