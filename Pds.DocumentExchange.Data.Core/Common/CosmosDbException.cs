using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;

namespace Pds.DocumentExchange.Data.Core.Common
{
    /// <summary>
    /// A custom Exception, needed to expose a specific Cosmos db related error condition.
    /// Constructors and decorators are here to satisfy the static analysis tool
    /// as a consequence, excluded from coverage as they can't be tested properly.
    /// </summary>
    /// <seealso cref="Exception" />
    [ExcludeFromCodeCoverage]
    public class CosmosDbException : Exception
    {
        /// <summary>Gets or sets the status code.</summary>
        public HttpStatusCode StatusCode { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CosmosDbException"/> class.
        /// </summary>
        public CosmosDbException()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CosmosDbException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public CosmosDbException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CosmosDbException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="innerException">Inner exception.</param>
        public CosmosDbException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}