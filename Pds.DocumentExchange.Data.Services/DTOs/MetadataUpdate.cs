namespace Pds.DocumentExchange.Data.Services.DTOs
{
    /// <summary>The representation of the subset of a batch used for an update.</summary>
    public class MetadataUpdate
    {
        /// <summary>Gets or sets the batch identifier.</summary>
        public string BatchIdentifier { get; set; }

        /// <summary>Gets or sets the metadata.</summary>
        public FileMetadata Metadata { get; set; }

        /// <summary>Gets or sets the fields to update.</summary>
        public string[] FieldsToUpdate { get; set; }

        /// <summary>Gets or sets the name of the existing file.</summary>
        public string ExistingFileName { get; set; }
    }
}