using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Interfaces
{
    /// <summary>
    /// An interface to provide options to scan files.
    /// </summary>
    public interface IVirusScanProcessor
    {
        /// <summary>
        /// Runs the virus scan process for all the files in an organisation user's batch.
        /// </summary>
        /// <param name="batchIdentifier">The batch identifier.</param>
        /// <returns>An awaitable task.</returns>
        Task RunVirusScanOrganisation(string batchIdentifier);

        /// <summary>
        /// Runs the virus scan process for all the files in an agency's batch.
        /// </summary>
        /// <param name="batchIdentifier">The batch identifier.</param>
        /// <returns>An awaitable task.</returns>
        Task RunVirusScanAgency(string batchIdentifier);
    }
}