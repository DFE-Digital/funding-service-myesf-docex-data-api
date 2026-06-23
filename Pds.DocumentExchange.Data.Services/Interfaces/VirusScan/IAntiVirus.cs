using Pds.DocumentExchange.Data.Services.DTOs;
using System.IO;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Interfaces
{
    /// <summary>
    /// An interface to provide the option to virus scan a file.
    /// </summary>
    public interface IAntivirus
    {
        /// <summary>
        /// Scan a virus using some antivirus software.
        /// </summary>
        /// <param name="fileName">The file name of the file we are scanning.</param>
        /// <param name="stream">The file stream.</param>
        /// <returns>A result of the file scan.</returns>
        Task<ScanResult> Scan(string fileName, Stream stream);
    }
}