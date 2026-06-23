using System;
using System.Diagnostics.CodeAnalysis;

namespace Pds.DocumentExchange.Data.Services.Exceptions
{
    /// <summary>
    /// Batch analysis no uploader exception.
    /// Constructors and decorators are here to satisfy the static analysis tool
    /// as a consequence, excluded from coverage as they can't be tested properly.
    /// </summary>
    [ExcludeFromCodeCoverage]
    [SuppressMessage("Major Code Smell", "S3925:\"ISerializable\" should be implemented correctly", Justification = "Legacy serialization support APIs are obsolete.")]
    public class BatchAnalysisNoUploaderException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BatchAnalysisNoUploaderException"/> class.
        /// </summary>
        public BatchAnalysisNoUploaderException()
            : this("'id unknown'")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BatchAnalysisNoUploaderException"/> class.
        /// </summary>
        /// <param name="batchID">The batch id.</param>
        public BatchAnalysisNoUploaderException(string batchID)
            : base($"No uploader details present on batch: {batchID}")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BatchAnalysisNoUploaderException"/> class.
        /// </summary>
        /// <param name="batchID">The batch id.</param>
        /// <param name="innerException">Inner exception.</param>
        public BatchAnalysisNoUploaderException(string batchID, Exception innerException)
            : base($"No uploader details present on batch: {batchID}", innerException)
        {
        }
    }
}