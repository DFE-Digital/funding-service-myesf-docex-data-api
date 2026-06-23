using System;
using System.Diagnostics.CodeAnalysis;

namespace Pds.DocumentExchange.Data.Services.Exceptions
{
    /// <summary>
    /// Batch analysis organisation cardinality mismatch exception.
    /// Constructors and decorators are here to satisfy the static analysis tool
    /// as a consequence, excluded from coverage as they can't be tested properly.
    /// </summary>
    [ExcludeFromCodeCoverage]
    [SuppressMessage("Major Code Smell", "S3925:\"ISerializable\" should be implemented correctly", Justification = "Legacy serialization support APIs are obsolete.")]
    public class BatchAnalysisOrgCardinalityMismatchException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BatchAnalysisOrgCardinalityMismatchException"/> class.
        /// </summary>
        public BatchAnalysisOrgCardinalityMismatchException()
            : this("'id unknown'")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BatchAnalysisOrgCardinalityMismatchException"/> class.
        /// </summary>
        /// <param name="batchID">The batch id.</param>
        public BatchAnalysisOrgCardinalityMismatchException(string batchID)
            : base($"Organisation cardinality mismatch on parent batch: {batchID}")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BatchAnalysisOrgCardinalityMismatchException"/> class.
        /// </summary>
        /// <param name="batchID">The batch id.</param>
        /// <param name="innerException">Inner exception.</param>
        public BatchAnalysisOrgCardinalityMismatchException(string batchID, Exception innerException)
            : base($"Organisation cardinality mismatch on parent batch: {batchID}", innerException)
        {
        }
    }
}