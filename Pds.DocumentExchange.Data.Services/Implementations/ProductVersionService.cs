using Pds.Core.Common.Organisation.Models;
using Pds.DocumentExchange.Data.Services.Constants;
using Pds.DocumentExchange.Data.Services.Interfaces;
using Pds.Services.Common.Helpers;
using System;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Implementations
{
    /// <summary>
    /// Service providing methods to get product versions.
    /// </summary>
    public class ProductVersionService : IProductVersionService
    {
        private readonly ICosmosDbService _cosmosDbService;
        private readonly IAcademicYearCalculator _academicYearCalculator;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProductVersionService"/> class.
        /// </summary>
        /// <param name="cosmosDbService">The cosmosDb service.</param>
        /// <param name="academicYearCalculator">The academic year calculator.</param>
        public ProductVersionService(
            ICosmosDbService cosmosDbService,
            IAcademicYearCalculator academicYearCalculator)
        {
            _cosmosDbService = cosmosDbService;
            _academicYearCalculator = academicYearCalculator;
        }

        /// <inheritdoc/>
        public async Task<int> GetCurrentProductVersion(OrganisationIdentifier organisationIdentifier, int productIdentifier)
        {
            It.IsNull(organisationIdentifier)
                .AsGuard<ArgumentNullException>(nameof(organisationIdentifier));

            var academicYear = _academicYearCalculator.GetCurrentAcademicYear();

            return await _cosmosDbService.GetCurrentVersionNumber(
                organisationIdentifier,
                EsfaOrganisationInfo.Identifier,
                academicYear,
                productIdentifier);
        }
    }
}