namespace Pds.DocumentExchange.Data.Services.DTOs.Filters
{
    /// <summary>
    /// Class representing a single value radio filter option.
    /// </summary>
    public class RadioFilterOption : ISingleValueFilterOption
    {
        /// <inheritdoc/>
        public string Type { get; set; }

        /// <inheritdoc/>
        public string Key { get; set; }

        /// <inheritdoc/>
        public string Value { get; set; }
    }
}