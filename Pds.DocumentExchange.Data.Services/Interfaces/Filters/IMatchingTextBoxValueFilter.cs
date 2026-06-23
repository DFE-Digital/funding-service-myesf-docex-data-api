namespace Pds.DocumentExchange.Data.Services.Interfaces.Filters
{
    /// <summary>
    /// The matching text-box value filter.
    /// </summary>
    /// <typeparam name="TElement">The element type.</typeparam>
    public interface IMatchingTextBoxValueFilter<TElement> : IMatchingValueFilter<TElement>
    {
        /// <summary>
        /// Gets the text-box hint.
        /// </summary>
        string TextBoxHint { get; }

        /// <summary>
        /// Gets the text-box input validation Regex.
        /// </summary>
        public string Regex { get; }

        /// <summary>
        /// Gets the validation error message.
        /// </summary>
        public string ValidationErrorMessage { get; }
    }
}