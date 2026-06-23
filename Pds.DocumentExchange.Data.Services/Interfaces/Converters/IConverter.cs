namespace Pds.DocumentExchange.Data.Services.Interfaces.Converters
{
    /// <summary>
    /// Interface providing a method to convert an object from one type to another type.
    /// </summary>
    /// <typeparam name="TInput">The input type.</typeparam>
    /// <typeparam name="TOutput">The output type.</typeparam>
    public interface IConverter<in TInput, out TOutput>
    {
        /// <summary>
        /// Converts the input object to an output object.
        /// </summary>
        /// <param name="input">The input object.</param>
        /// <returns>The output object.</returns>
        TOutput Convert(TInput input);
    }
}