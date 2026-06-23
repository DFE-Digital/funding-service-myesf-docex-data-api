using System;
using System.Diagnostics.CodeAnalysis;

namespace Pds.DocumentExchange.Data.Services.Exceptions
{
    /// <summary>
    /// Message body factory cardinality exception.
    /// Constructors and decorators are here to satisfy the static analysis tool
    /// as a consequence, excluded from coverage as they can't be tested properly.
    /// </summary>
    [ExcludeFromCodeCoverage]
    [SuppressMessage("Major Code Smell", "S3925:\"ISerializable\" should be implemented correctly", Justification = "Legacy serialization support APIs are obsolete.")]
    public class MessageBodyFactoryCardinalityException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MessageBodyFactoryCardinalityException"/> class.
        /// </summary>
        public MessageBodyFactoryCardinalityException()
            : this("'id unknown'")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageBodyFactoryCardinalityException"/> class.
        /// </summary>
        /// <param name="batchID">The batch id.</param>
        public MessageBodyFactoryCardinalityException(string batchID)
            : base($"No files present in batch: {batchID}")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageBodyFactoryCardinalityException"/> class.
        /// </summary>
        /// <param name="batchID">The batch id.</param>
        /// <param name="innerException">Inner exception.</param>
        public MessageBodyFactoryCardinalityException(string batchID, Exception innerException)
            : base($"No files present in batch: {batchID}", innerException)
        {
        }
    }
}