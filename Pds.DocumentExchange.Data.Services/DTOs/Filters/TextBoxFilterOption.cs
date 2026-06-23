namespace Pds.DocumentExchange.Data.Services.DTOs.Filters
{
    /// <summary>
    /// The text box filter option.
    /// </summary>
    public class TextBoxFilterOption : ISingleValueFilterOption
    {
        /// <inheritdoc/>
        public string Type { get; set; }

        /// <inheritdoc/>
        public string Key { get; set; }

        /// <inheritdoc/>
        public string Value { get; set; }
    }
}