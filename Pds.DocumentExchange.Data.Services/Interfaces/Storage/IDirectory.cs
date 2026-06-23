using Pds.DocumentExchange.Data.Services.DTOs;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Interfaces.Storage
{
    /// <summary>
    /// Represents a directory.
    /// </summary>
    public interface IDirectory
    {
        /// <summary>
        /// Gets the directory name.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Deletes a file from the directory.
        /// </summary>
        /// <param name="fileName">The file name.</param>
        /// <returns>An awaitable task.</returns>
        Task Delete(string fileName);

        /// <summary>
        /// Checks if a file exists in the directory.
        /// </summary>
        /// <param name="fileName">The filename.</param>
        /// <returns>True is the file exists / false otherwise.</returns>
        Task<bool> FileExists(string fileName);

        /// <summary>
        /// Gets all files in the directory.
        /// </summary>
        /// <returns>A list of FileReferenceInfo.</returns>
        IAsyncEnumerable<FileReferenceInfo> GetFiles();

        /// <summary>
        /// Moves a files to another directory.
        /// </summary>
        /// <param name="destination">The destination directory.</param>
        /// <param name="fileName">The file name.</param>
        /// <returns>The file name the file was saved with.</returns>
        Task<string> Move(IDirectory destination, string fileName);

        /// <summary>
        /// Reads a file.
        /// </summary>
        /// <param name="fileName">The file name.</param>
        /// <returns>The file stream.</returns>
        Task<Stream> Read(string fileName);

        /// <summary>
        /// Saves a file in the directory.
        /// </summary>
        /// <param name="stream">The file stream.</param>
        /// <param name="fileName">The file name.</param>
        /// <returns>The file name the file was saved with.</returns>
        Task<string> Save(Stream stream, string fileName);

        /// <summary>
        /// Copies a file to another directory.
        /// </summary>
        /// <param name="destination">The destination directory.</param>
        /// <param name="fileName">The file name.</param>
        /// <returns>The file name the file was saved with.</returns>
        Task<string> Copy(IDirectory destination, string fileName);
    }
}