using System;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Interfaces
{
    /// <summary>
    /// An interface to provide the option of downloading reports.
    /// </summary>
    public interface IReportService
    {
        /// <summary>
        /// Returns MI Report document as a byte array.
        /// </summary>
        /// <param name="from">from date.</param>
        /// <param name="to">to date.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task<byte[]> GetMIReport(DateTime from, DateTime to);
    }
}
