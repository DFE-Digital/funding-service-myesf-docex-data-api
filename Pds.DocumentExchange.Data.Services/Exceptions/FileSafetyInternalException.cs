using System;
using System.Diagnostics.CodeAnalysis;

namespace Pds.DocumentExchange.Data.Services.Exceptions
{
    /// <summary>
    /// File safety internal exception.
    /// Constructors and decorators are here to satisfy the static analysis tool
    /// as a consequence, excluded from coverage as they can't be tested properly.
    /// </summary>
    [ExcludeFromCodeCoverage]
    [SuppressMessage("Major Code Smell", "S3925:\"ISerializable\" should be implemented correctly", Justification = "Legacy serialization support APIs are obsolete.")]
    public class FileSafetyInternalException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FileSafetyInternalException"/> class.
        /// </summary>
        public FileSafetyInternalException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileSafetyInternalException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public FileSafetyInternalException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileSafetyInternalException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="innerException">Inner exception.</param>
        public FileSafetyInternalException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
