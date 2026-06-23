namespace Pds.DocumentExchange.Data.Services.Interfaces.Providers
{
    /// <summary>
    /// I provide presentation formatting (contract).
    /// </summary>
    public interface IProvidePresentationFormatting
    {
        /// <summary>
        /// Format the product name with the original files extension.
        /// </summary>
        /// <param name="productName">The product name.</param>
        /// <param name="fileName">The file name.</param>
        /// <returns>A string composed of the product name and the extension of the original file.</returns>
        string FormatProductNameWithFileExtension(string productName, string fileName);
    }
}