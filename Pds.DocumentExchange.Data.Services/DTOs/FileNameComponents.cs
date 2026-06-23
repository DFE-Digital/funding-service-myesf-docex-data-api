namespace Pds.DocumentExchange.Data.Services.DTOs
{
    /// <summary>
    /// The parts that make up our extended filename.
    /// </summary>
    public class FileNameComponents
    {
        /// <summary>
        /// Gets or sets the organisation identifier the file is coming from or going to.
        /// </summary>
        public int OrganisationIdentifier { get; set; }

        /// <summary>
        /// Gets or sets the product identifier from the ESFA list of file types (for example - 00009).
        /// </summary>
        public string ProductIdentifier { get; set; }

        /// <summary>
        /// Gets or sets the academic year the file relates to.
        /// </summary>
        public int? AcademicYear { get; set; }

        /// <summary>
        /// Gets or sets the original file name the file had (for example - accounts2017.xlsx).
        /// </summary>
        public string OriginalFileName { get; set; }

        /// <summary>
        /// Gets or sets the filename extension.
        /// </summary>
        public string Extension { get; set; }

        /// <summary>
        /// Gets or sets whether the filename must be unique in storage.
        /// This allows for that by having a distinct hyphen and number on the end of the file.
        /// </summary>
        public string UniqueAppendedVersion { get; set; }
    }
}