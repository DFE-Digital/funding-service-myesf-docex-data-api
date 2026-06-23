using System;
using System.Diagnostics.CodeAnalysis;

namespace Pds.DocumentExchange.Data.Services.Exceptions
{
    /// <summary>
    /// Cosmos stored procedure management exception
    /// Constructors and decorators are here to satisfy the static analysis tool
    /// as a consequence, excluded from coverage as they can't be tested properly.
    /// </summary>
    [ExcludeFromCodeCoverage]
    [SuppressMessage("Major Code Smell", "S3925:\"ISerializable\" should be implemented correctly", Justification = "Legacy serialization support APIs are obsolete.")]
    public class CosmosStoredProcedureManagementException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CosmosStoredProcedureManagementException"/> class.
        /// </summary>
        public CosmosStoredProcedureManagementException()
            : base("One or more cosmos stored procedures are not up to date.")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CosmosStoredProcedureManagementException"/> class.
        /// </summary>
        /// <param name="message">message.</param>
        public CosmosStoredProcedureManagementException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CosmosStoredProcedureManagementException"/> class.
        /// </summary>
        /// <param name="message">message.</param>
        /// <param name="innerException">inner exception.</param>
        public CosmosStoredProcedureManagementException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}