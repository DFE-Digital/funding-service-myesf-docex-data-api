using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Pds.Core.Common.Organisation.Models;
using Pds.Core.Logging;
using Pds.DocumentExchange.Data.Services.Interfaces.FDS;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Api.Controllers
{
    /// <summary>
    /// The Organisation controller - Exposes API methods related to actions performed by the User.
    /// </summary>
    [ApiController]
    [Route("/api/[controller]")]
    public class OrganisationController : BaseApiController
    {
        private readonly ILoggerAdapter<OrganisationController> _logger;
        private readonly IOrganisationService _organisationService;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrganisationController"/> class.
        /// </summary>
        /// <param name="organisationService">The organisation service.</param>
        /// <param name="logger">The logger.</param>
        public OrganisationController(IOrganisationService organisationService, ILoggerAdapter<OrganisationController> logger)
        {
            _logger = logger;
            _organisationService = organisationService;
        }

        /// <summary>
        /// Searches for organisations whose name contains the given string.
        /// </summary>
        /// <param name="nameSearchTerm">The partial name to search for.</param>
        /// <param name="maxResults">The maximum number of results to retrieve.</param>
        /// <returns>The collection of matching organisations.</returns>
        [HttpGet("[action]/{nameSearchTerm}/{maxResults}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<IEnumerable<Organisation>>> Search(string nameSearchTerm, int maxResults)
        {
            try
            {
                IEnumerable<Organisation> organisations;

                _logger.LogInformation($"Started action: {nameof(Search)} for organisations: {nameSearchTerm} with {maxResults} max results");

                organisations = await _organisationService.GetOrganisationByName(nameSearchTerm, maxResults);

                _logger.LogInformation($"Finished action: {nameof(Search)}");

                return Ok(organisations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred in action: {nameof(Search)}");
                throw;
            }
        }

        /// <summary>
        /// Gets the organisation with the given identifier.
        /// </summary>
        /// <param name="ukprn">The ukprn to lookup.</param>
        /// <returns>The organisation with the given identifier.</returns>
        [HttpGet("{ukprn}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<Organisation>> GetOrganisation(string ukprn)
        {
            try
            {
                _logger.LogInformation($"Started action: {nameof(GetOrganisation)} for the ukprn {ukprn}");

                var organisation = await _organisationService.GetOrganisation(ukprn);

                _logger.LogInformation($"Finished action: {nameof(GetOrganisation)}");

                return Ok(organisation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred in action: {nameof(GetOrganisation)}");
                throw;
            }
        }
    }
}