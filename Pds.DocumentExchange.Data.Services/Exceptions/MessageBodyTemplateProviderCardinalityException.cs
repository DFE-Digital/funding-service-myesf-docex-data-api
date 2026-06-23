using System;
using System.Diagnostics.CodeAnalysis;

namespace Pds.DocumentExchange.Data.Services.Exceptions
{
    /// <summary>
    /// Message body template provider cardinality exception.
    /// Constructors and decorators are here to satisfy the static analysis tool
    /// as a consequence, excluded from coverage as they can't be tested properly.
    /// </summary>
    [ExcludeFromCodeCoverage]
    [SuppressMessage("Major Code Smell", "S3925:\"ISerializable\" should be implemented correctly", Justification = "Legacy serialization support APIs are obsolete.")]
    public class MessageBodyTemplateProviderCardinalityException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MessageBodyTemplateProviderCardinalityException"/> class.
        /// </summary>
        public MessageBodyTemplateProviderCardinalityException()
            : this("'name unknown'")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageBodyTemplateProviderCardinalityException"/> class.
        /// </summary>
        /// <param name="propertyName">the property name.</param>
        public MessageBodyTemplateProviderCardinalityException(string propertyName)
            : base($"No items present on the property: {propertyName}")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageBodyTemplateProviderCardinalityException"/> class.
        /// </summary>
        /// <param name="message">message.</param>
        /// <param name="innerException">inner exception.</param>
        public MessageBodyTemplateProviderCardinalityException(string message, Exception innerException)
            : base($"No items present on the property: {message}", innerException)
        {
        }
    }
}