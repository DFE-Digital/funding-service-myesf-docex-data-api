namespace Pds.DocumentExchange.Data.Repository.DTOs
{
    /// <summary>
    /// The stored procedure with continuation token result.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    public class StoredProcedureWithContinuationTokenResult<T>
    {
        /// <summary>
        /// Gets or sets the message of the result.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the continuation token of the result.
        /// </summary>
        public string LastContinuationToken { get; set; }

        /// <summary>
        /// Gets or sets the collection of documents of type T of the result.
        /// </summary>
        public T Documents { get; set; }
    }
}
