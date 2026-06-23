using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Interfaces
{
    /// <summary>
    /// The authorisation service.
    /// </summary>
    public interface IAuthorisationService
    {
        /// <summary>
        /// Validate a user via a token, and receive some limited details about that.
        /// </summary>
        /// <param name="token">A token to be validated.</param>
        /// <returns>An authentication response.</returns>
        Task<IAuthorisationResponse> Validate(string token);
    }
}