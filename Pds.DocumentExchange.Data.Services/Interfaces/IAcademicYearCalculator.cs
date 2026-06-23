namespace Pds.DocumentExchange.Data.Services.Interfaces
{
    /// <summary>
    /// Interface providing methods to calculate the current academic year.
    /// </summary>
    public interface IAcademicYearCalculator
    {
        /// <summary>
        /// Gets the current academic year.
        /// </summary>
        /// <returns>An integer representing the current academic year.
        /// i.e. 201819, 201920, 202021.
        /// </returns>
        int GetCurrentAcademicYear();
    }
}