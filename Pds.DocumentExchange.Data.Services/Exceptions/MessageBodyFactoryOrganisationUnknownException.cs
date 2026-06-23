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
    public class MessageBodyFactoryOrganisationUnknownException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MessageBodyFactoryOrganisationUnknownException"/> class.
        /// </summary>
        public MessageBodyFactoryOrganisationUnknownException()
            : this("'organisation unknown'")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageBodyFactoryOrganisationUnknownException"/> class.
        /// </summary>
        /// <param name="organisationID">The organisation id.</param>
        public MessageBodyFactoryOrganisationUnknownException(string organisationID)
            : base($"No details available for: '{organisationID}'")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageBodyFactoryOrganisationUnknownException"/> class.
        /// </summary>
        /// <param name="organisationID">The organisation id.</param>
        /// <param name="innerException">Inner exception.</param>
        public MessageBodyFactoryOrganisationUnknownException(string organisationID, Exception innerException)
            : base($"No details available for: '{organisationID}'", innerException)
        {
        }
    }
}