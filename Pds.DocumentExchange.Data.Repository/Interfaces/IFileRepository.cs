using System.Collections.Generic;

namespace Pds.DocumentExchange.Data.Repository.Interfaces
{
    /// <summary>The FileRepository interface - a wrapper for reading files.</summary>
    public interface IFileRepository
    {
        /// <summary>Reads the file contents.</summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns>The contents of the file.</returns>
        string ReadSingleFileContents(string fileName);

        /// <summary>Reads multiple files matching a pattern.</summary>
        /// <param name="fileNamePattern">The file name pattern.</param>
        /// <returns>A dictionary containing a key value pair of filename and its contents.</returns>
        Dictionary<string, string> ReadMultipleFilesMatchingAPattern(string fileNamePattern);
    }
}