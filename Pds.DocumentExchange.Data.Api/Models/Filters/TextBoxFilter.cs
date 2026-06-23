namespace Pds.DocumentExchange.Data.Api.Models.Filters
{
    /// <summary>
    /// Class representing a radio button filter that can be applied against some items.
    /// </summary>
    public class TextBoxFilter : IFilter
    {
        /// <inheritdoc/>
        public string Title { get; set; }

        /// <inheritdoc/>
        public string Key { get; set; }

        /// <inheritdoc/>
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the text-box hint.
        /// </summary>
        public string Hint { get; set; }

        /// <summary>
        /// Gets or sets the text-box input validation Regex.
        /// </summary>
        public string Regex { get; set; }

        /// <summary>
        /// Gets or sets the validation error message.
        /// </summary>
        public string ValidationErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets the text-box filter value to be applied for this filter.
        /// </summary>
        public string Value { get; set; }
    }
}