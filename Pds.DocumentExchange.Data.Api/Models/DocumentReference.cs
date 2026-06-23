namespace Pds.DocumentExchange.Data.Api.Models
{
    /// <summary>
    /// The reference information of a document.
    /// </summary>
    public class DocumentReference
    {
        /// <summary>
        /// Gets or sets the batch identifier.
        /// </summary>
        public string BatchIdentifier { get; set; }

        /// <summary>
        /// Gets or sets the parent batch identifier.
        /// </summary>
        public string ParentBatchIdentifier { get; set; }

        /// <summary>
        /// Gets or sets the file name.
        /// </summary>
        public string FileName { get; set; }
    }
}