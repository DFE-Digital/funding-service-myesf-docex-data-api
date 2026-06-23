using Pds.DocumentExchange.Data.Services.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Interfaces
{
    /// <summary>
    /// Interface providing methods for generating, parsing and validating file names.
    /// </summary>
    public interface IFileNameProvider
    {
        /// <summary>
        /// Gets the next available file name based on the specified validation function.
        /// </summary>
        /// <param name="fileName">The file name.</param>
        /// <param name="fileExists">The file exists validation function.</param>
        /// <returns>The next available file name.</returns>
        Task<string> GetNextAvailableFileName(string fileName, Func<string, Task<bool>> fileExists);

        /// <summary>
        /// Generates a file name based on the specified information.
        /// </summary>
        /// <param name="organisationIdentifier">The organisation identifier the file is going to/coming from.</param>
        /// <param name="productIdentifier">The product identifier.</param>
        /// <param name="academicYear">The academic year.</param>
        /// <param name="originalFileName">The original file name.</param>
        /// <returns>A filename containing the specified information.</returns>
        string GenerateFileName(
            string organisationIdentifier,
            int productIdentifier,
            string academicYear,
            string originalFileName);

        /// <summary>
        /// Breaks a full filename into its components.
        /// </summary>
        /// <remarks>The file name is a string containing data delimited by underscore characters. See example.</remarks>
        /// <param name="fullFileName">The full file name.</param>
        /// <param name="throwExceptionOnError">Value indicating if an exception will be thrown on error.</param>
        /// <returns>The component parts of the full file name.</returns>
        /// <example>
        /// File name: 12345678_10002_201819.pdf
        /// FileNameComponents { OrganisationIdentifier: 12345678, FileType: 10002, Year: 201819, Extension: .pdf }.
        /// </example>
        FileNameComponents GetComponents(string fullFileName, bool throwExceptionOnError = true);

        /// <summary>
        /// Get a list from passed comma separated string.
        /// </summary>
        /// <param name="stringToSplit">The comma separated string that needs to be returned as list.</param>
        /// <returns>List of split strings.</returns>
        List<string> GetCsvListSetting(string stringToSplit);
    }
}