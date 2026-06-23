using System;
using System.Diagnostics.CodeAnalysis;

namespace Pds.DocumentExchange.Data.Services.Exceptions
{
    /// <summary>
    /// Batch analysis batch empty exception.
    /// Constructors and decorators are here to satisfy the static analysis tool
    /// as a consequence, excluded from coverage as they can't be tested properly.
    /// </summary>
    [ExcludeFromCodeCoverage]
    [SuppressMessage("Major Code Smell", "S3925:\"ISerializable\" should be implemented correctly", Justification = "Legacy serialization support APIs are obsolete.")]
    public class BatchAnalysisBatchEmptyException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BatchAnalysisBatchEmptyException"/> class.
        /// </summary>
        public BatchAnalysisBatchEmptyException()
            : this("'id unknown'")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BatchAnalysisBatchEmptyException"/> class.
        /// </summary>
        /// <param name="batchID">The batch id.</param>
        public BatchAnalysisBatchEmptyException(string batchID)
            : base($"No files present in the batch: {batchID}")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BatchAnalysisBatchEmptyException"/> class.
        /// </summary>
        /// <param name="batchID">The batch id.</param>
        /// <param name="innerException">Inner exception.</param>
        public BatchAnalysisBatchEmptyException(string batchID, Exception innerException)
            : base($"No files present in the batch: {batchID}", innerException)
        {
        }
    }
}