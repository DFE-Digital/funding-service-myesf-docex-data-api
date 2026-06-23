using Pds.Services.Common.Helpers;
using System;
using System.Linq;

namespace Pds.DocumentExchange.Data.Services.Extensions
{
    /// <summary>
    /// The string extension methods.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Splits and trims a string into substrings based on the provided character separator.
        /// </summary>
        /// <param name="value">The string value.</param>
        /// <param name="separator">A character that delimits the substrings in this string.</param>
        /// <returns>An array whose elements contain the substrings from this instance that are delimited by separator.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the string value is null.</exception>
        public static string[] SplitAndTrim(this string value, char separator)
        {
            It.IsNull(value)
                .AsGuard<ArgumentNullException>(nameof(value));

            return value.Split(separator)
                .Select(item => item.Trim())
                .Where(item => !string.IsNullOrWhiteSpace(item))
                .ToArray();
        }

        /// <summary>
        /// Determines whether a string instance and another have the same value when the case is ignored.
        /// </summary>
        /// <param name="value">The string value.</param>
        /// <param name="other">The string to compare to the value.</param>
        /// <returns>True if both values are equal; otherwise false.</returns>
        public static bool IsEqualToIgnoreCase(this string value, string other)
            => value.Equals(other, StringComparison.OrdinalIgnoreCase);
    }
}