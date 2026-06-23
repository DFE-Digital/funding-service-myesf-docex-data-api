using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Interfaces.Storage
{
    /// <summary>
    /// Represents a manager for directories.
    /// </summary>
    public interface IDirectoriesManager
    {
        /// <summary>
        /// Gets the directory by the key provided.
        /// </summary>
        /// <param name="directoryKey">The directory key.</param>
        /// <returns>The directory.</returns>
        Task<IDirectory> GetDirectory(string directoryKey);
    }
}