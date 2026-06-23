using Pds.Core.Utils;
using Pds.DocumentExchange.Data.Services.Interfaces;

namespace Pds.DocumentExchange.Data.Services.Implementations
{
    /// <summary>
    /// Methods to calculate the academic year.
    /// </summary>
    public class AcademicYearCalculator : IAcademicYearCalculator
    {
        private readonly ISystemProvider _systemProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="AcademicYearCalculator"/> class.
        /// </summary>
        /// <param name="systemProvider">The system provider.</param>
        public AcademicYearCalculator(ISystemProvider systemProvider)
        {
            _systemProvider = systemProvider;
        }

        /// <inheritdoc/>
        public int GetCurrentAcademicYear()
        {
            var utcNow = _systemProvider.DateTime.UtcNow();

            var currentYear = utcNow.Month >= 9 ? utcNow.Year : utcNow.Year - 1;
            var nextYear = currentYear + 1;

            var nextYearString = nextYear.ToString();
            var nextYearLastTwoDigits = nextYearString.Substring(nextYearString.Length - 2, 2);

            return int.Parse($"{currentYear}{nextYearLastTwoDigits}");
        }
    }
}