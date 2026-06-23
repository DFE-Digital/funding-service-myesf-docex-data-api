using System;

namespace Pds.DocumentExchange.Data.Services.Interfaces
{
    /// <summary>
    /// The interface for the Authorisation Response.
    /// </summary>
    public interface IAuthorisationResponse
    {
        /// <summary>
        /// Gets or sets unique number allocated to a provider on successful registration on the UKRLP.
        /// </summary>
        int? Ukprn { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the request is authorised.
        /// </summary>
        bool Authenticated { get; set; }

        /// <summary>
        /// Gets or sets a raised exception, if there was one.
        /// </summary>
        Exception Error { get; set; }
    }
}