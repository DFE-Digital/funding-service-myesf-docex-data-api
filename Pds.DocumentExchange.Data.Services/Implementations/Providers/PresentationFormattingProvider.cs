using Pds.DocumentExchange.Data.Services.Interfaces.Providers;
using Pds.Services.Common.Helpers;
using System.Text.RegularExpressions;

namespace Pds.DocumentExchange.Data.Services.Implementations.Providers
{
    /// <summary>
    /// The presentation formatting provider (implementation).
    /// </summary>
    public sealed class PresentationFormattingProvider :
        IProvidePresentationFormatting
    {
        private readonly Regex _spacingCharacters = new Regex(@"[\s\u2212\u2013\u2014\u2010_]+", RegexOptions.Compiled);
        private readonly Regex _disallowedCharacters = new Regex(@"[^\w-]+", RegexOptions.Compiled);
        private readonly Regex _multipleDashes = new Regex(@"-{2,}", RegexOptions.Compiled);

        /// <inheritdoc/>
        public string FormatProductNameWithFileExtension(string productName, string fileName)
        {
            if (It.IsEmpty(productName))
            {
                return string.Empty;
            }

            var flattenedName = ConvertToAllowedFilename(productName);

            if (It.Has(fileName))
            {
                var parts = fileName.Split('.');

                if (parts.Length > 1)
                {
                    flattenedName += $".{parts[parts.Length - 1]}";
                }
            }

            return flattenedName;
        }

        /// <summary>
        /// Strip out unallowed characters from a filename.
        /// </summary>
        /// <param name="filename">The filename to convert.</param>
        /// <returns>The converted filename.</returns>
        internal string ConvertToAllowedFilename(string filename)
        {
            if (It.IsEmpty(filename))
            {
                return string.Empty;
            }

            filename = _spacingCharacters.Replace(filename, "-");
            filename = _disallowedCharacters.Replace(filename, string.Empty);
            filename = _multipleDashes.Replace(filename, "-");

            return filename;
        }
    }
}