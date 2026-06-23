using Pds.DocumentExchange.Data.Services.DTOs;
using Pds.DocumentExchange.Data.Services.Interfaces;
using Pds.Services.Common.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Implementations
{
    /// <summary>
    /// Methods for generating and parsing file names.
    /// </summary>
    public class FileNameProvider : IFileNameProvider
    {
        /// <inheritdoc/>
        public async Task<string> GetNextAvailableFileName(
           string fileName,
           Func<string, Task<bool>> fileExists)
        {
            It.IsEmpty(fileName)
                .AsGuard<ArgumentNullException>(nameof(fileName));

            var availableFileName = fileName;
            var loopCounter = 2;

            while (await fileExists(availableFileName))
            {
                var originalFileName = Path.GetFileNameWithoutExtension(fileName);
                var fileExtension = Path.GetExtension(fileName);
                availableFileName = $"{originalFileName}-{loopCounter++}{fileExtension}";
            }

            return availableFileName;
        }

        /// <inheritdoc/>
        public string GenerateFileName(
            string organisationIdentifier,
            int productIdentifier,
            string academicYear,
            string originalFileName)
        {
            if (string.IsNullOrEmpty(organisationIdentifier))
            {
                throw new ArgumentException("The organisation identifier cannot be null or empty.");
            }

            if (string.IsNullOrEmpty(academicYear))
            {
                throw new ArgumentException("The academic year cannot be null or empty.");
            }

            if (string.IsNullOrEmpty(originalFileName))
            {
                throw new ArgumentException("File name cannot be null or empty.");
            }

            const int MaxFilenameLength = 260;

            if (originalFileName.Length > MaxFilenameLength)
            {
                throw new ArgumentException("Filename is too long.");
            }

            return $"{organisationIdentifier}_{productIdentifier}_{academicYear}_{Escape(originalFileName)}";
        }

        /// <inheritdoc/>
        public FileNameComponents GetComponents(string fullFileName, bool throwExceptionOnError = true)
        {
            if (string.IsNullOrEmpty(fullFileName))
            {
                if (!throwExceptionOnError)
                {
                    return new FileNameComponents();
                }

                throw new ArgumentException("Filename can't be empty or null");
            }

            var lastDotPos = fullFileName.LastIndexOf(".", StringComparison.Ordinal);
            var extension = lastDotPos >= 0 ? fullFileName.Substring(lastDotPos) : null;
            string uniqueAppendedVersion = null;

            if (extension != null && extension.Contains("-"))
            {
                uniqueAppendedVersion = extension.Substring(extension.IndexOf("-", StringComparison.Ordinal));
                extension = extension.Split('-')[0];
            }

            var parts = (lastDotPos >= 0 ? fullFileName.Substring(0, lastDotPos) : fullFileName).Split('_');

            if (parts.Length < 3 && throwExceptionOnError)
            {
                throw new FormatException("Not as many components to the filename as was expected");
            }

            int ukprn, year = -1;

            if (!int.TryParse(parts[0], out ukprn) && throwExceptionOnError)
            {
                throw new FormatException("UKPRN section isn't a number");
            }

            var yearSectionInvalid = parts.Length < 3 || string.IsNullOrEmpty(parts[2]) || !int.TryParse(parts[2].Split('-')[0], out year);

            if (yearSectionInvalid && throwExceptionOnError)
            {
                throw new FormatException("Year section isn't a number");
            }

            return new FileNameComponents
            {
                OrganisationIdentifier = ukprn,
                ProductIdentifier = parts.Length >= 2 ? parts[1] : null,
                AcademicYear = parts.Length >= 3 && !string.IsNullOrEmpty(parts[2]) ? year : null,
                OriginalFileName = parts.Length >= 4 ? Unescape(parts[3]) + extension : null,
                Extension = extension,
                UniqueAppendedVersion = uniqueAppendedVersion
            };
        }

        /// <inheritdoc/>
        public List<string> GetCsvListSetting(string stringToSplit)
        {
            return stringToSplit?.Split(',')?.ToList() ?? new List<string>();
        }

        /// <summary>
        /// Escape a filenames special characters.
        /// </summary>
        /// <param name="filename">The filename to escape.</param>
        /// <returns>The escaped value.</returns>
        private string Escape(string filename) => filename?.Replace("_", "¬");

        /// <summary>
        /// Unescapes a filename's special characters.
        /// </summary>
        /// <param name="filename">The filename to unescape.</param>
        /// <returns>The unescaped filename.</returns>
        private string Unescape(string filename) => filename?.Replace("¬", "_");
    }
}