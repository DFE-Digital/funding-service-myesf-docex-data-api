using System.Collections.Generic;

namespace Pds.DocumentExchange.Data.Services.Interfaces
{
    /// <summary>
    /// The zip compression service interface.
    /// </summary>
    public interface IZipService
    {
        /// <summary>
        /// Creates a new zip file using the specified files collection.
        /// </summary>
        /// <param name="files">The files collection. Each item is a tuple containing the file name and its content.</param>
        /// <returns>The zip file content.</returns>
        byte[] ZipFiles(IEnumerable<(string Name, byte[] Content)> files);
    }
}