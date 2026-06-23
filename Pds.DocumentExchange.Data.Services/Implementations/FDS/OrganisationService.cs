using AutoMapper;
using AutoMapper.Internal;
using Newtonsoft.Json;
using Pds.Core.Common.Organisation.Enums;
using Pds.Core.Common.Organisation.Models;
using Pds.Core.Logging;
using Pds.DocumentExchange.Data.Services.Constants;
using Pds.DocumentExchange.Data.Services.DTOs.Configuration;
using Pds.DocumentExchange.Data.Services.DTOs.FDS;
using Pds.DocumentExchange.Data.Services.Interfaces.FDS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Implementations.FDS
{
    /// <summary>
    /// Service to call fds api client to get provider details (using HTTP).
    /// </summary>
    public class OrganisationService : IOrganisationService
    {
        private readonly HttpClient _client;
        private readonly IMapper _mapper;
        private readonly ILoggerAdapter<OrganisationService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrganisationService"/> class.
        /// </summary>
        /// <param name="client">The HTTP Client.</param>
        /// <param name="fdsApiClientConfiguration">The fds api client configuration.</param>
        /// <param name="mapper">The mapper.</param>
        /// <param name="cacheManager">The cache manager.</param>
        /// <param name="logger">The logger.</param>
        public OrganisationService(
            HttpClient client,
            FdsApiClientConfiguration fdsApiClientConfiguration,
            IMapper mapper,
            ILoggerAdapter<OrganisationService> logger)
        {
            client.BaseAddress = new Uri(fdsApiClientConfiguration.Url);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", fdsApiClientConfiguration.ApimSubscriptionKey);
            _client = client;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// Gets the organisation info with the given ukprn.
        /// </summary>
        /// <param name="ukprn">The ukprn to lookup.</param>
        /// <returns>The organisation info.</returns>
        public async Task<Organisation> GetOrganisation(string ukprn)
        {
            var managementGroupOrgResponse = await SendApiRequest(FdsApiConstants.PaymentOrganisationQueryPath, FdsApiConstants.Ukprn, FdsApiConstants.FieldOperatorEquals, ukprn, isStatusFieldRequired: false);

            if (managementGroupOrgResponse.TotalCount > 0)
            {
                var managementGroupOrgChildResponse = await SendApiRequest(FdsApiConstants.LearningProviderQueryPath, FdsApiConstants.PaymentOrgUkprn, FdsApiConstants.FieldOperatorEquals, ukprn);

                if (managementGroupOrgChildResponse.TotalCount > 0)
                {
                    //Condition below added for the response which have same UKPRN in both provider and child organisation.
                    var provider = managementGroupOrgChildResponse.Data.FirstOrDefault(x => x.Child.Count() == 1 && x.Ukprn == x.Child.First().Ukprn);
                    if (provider != null)
                    {
                        var providersResult = await SendApiRequest(FdsApiConstants.LearningProviderQueryPath, FdsApiConstants.Ukprn, FdsApiConstants.FieldOperatorEquals, ukprn);

                        if (providersResult.TotalCount > 0)
                        {
                            return _mapper.Map<IEnumerable<Provider>, IEnumerable<Organisation>>(providersResult.Data).SingleOrDefault();
                        }
                    }

                    return _mapper.Map<IEnumerable<Provider>, IEnumerable<Organisation>>(managementGroupOrgChildResponse.Data).SingleOrDefault();
                }
                else
                {
                    //Condition below added for the organisation available in PaymentOrganisation Response
                    //but not available in Provider Response (SearchCriteria - PaymentOrganisation.UKPRN).
                    var providersResult = await SendApiRequest(FdsApiConstants.LearningProviderQueryPath, FdsApiConstants.Ukprn, FdsApiConstants.FieldOperatorEquals, ukprn);

                    if (providersResult.TotalCount > 0)
                    {
                        return _mapper.Map<IEnumerable<Provider>, IEnumerable<Organisation>>(providersResult.Data).SingleOrDefault();
                    }

                    return _mapper.Map<IEnumerable<Provider>, IEnumerable<Organisation>>(managementGroupOrgResponse.Data).SingleOrDefault();
                }
            }

            var providers = await SendApiRequest(FdsApiConstants.LearningProviderQueryPath, FdsApiConstants.Ukprn, FdsApiConstants.FieldOperatorEquals, ukprn);

            if (providers.TotalCount > 0)
            {
                return _mapper.Map<IEnumerable<Provider>, IEnumerable<Organisation>>(providers.Data).SingleOrDefault();
            }

            return null;
        }


        /// <summary>
        /// Gets the organisation info with the given name.
        /// </summary>
        /// <param name="name">The name to lookup.</param>
        /// <param name="maxResults">The max results.</param>
        /// <returns>The organisation info.</returns>
        public async Task<IEnumerable<Organisation>> GetOrganisationByName(string name, int maxResults)
        {
            var managementGroupOrgResponse = await SendApiRequest(FdsApiConstants.PaymentOrganisationQueryPath, FdsApiConstants.OrgNameField, FdsApiConstants.FieldOperatorContains, name, isStatusFieldRequired: false, pageSize: maxResults);

            var managementGroupOrganisations = managementGroupOrgResponse.Data?.Count() > 0 ? managementGroupOrgResponse.Data.Where(x => x.Ukprn != null || x.Ukprn > 0).ToList() : new List<Provider>();

            if (managementGroupOrganisations.Count < maxResults)
            {
                var openLearningProviderOrganisations = await SendApiRequest(FdsApiConstants.LearningProviderQueryPath, FdsApiConstants.OrgNameField, FdsApiConstants.FieldOperatorContains, name);

                if (openLearningProviderOrganisations.Data?.Count() > 0)
                {
                    var results = openLearningProviderOrganisations.Data.Concat(managementGroupOrganisations).DistinctBy(x => x.Ukprn).Take(maxResults);
                    return _mapper.Map<IEnumerable<Provider>, IEnumerable<Organisation>>(results);
                }
            }

            if (managementGroupOrganisations.Any())
            {
                return _mapper.Map<IEnumerable<Provider>, IEnumerable<Organisation>>(managementGroupOrganisations);
            }

            return null;
        }

        /// <summary>
        /// Get all the organisation info.
        /// </summary>
        /// <returns>List of organisations.</returns>
        public async Task<IDictionary<string, Organisation>> GetAllOrganisations()
        {
            IEnumerable<Provider> providers;
            var managementGroupOrgResponse = await SendApiRequest(FdsApiConstants.PaymentOrganisationQueryPath, isStatusFieldRequired: false, skipPaging: true);

            var managementGroupOrganisations = managementGroupOrgResponse.Data?.Where(x => x.Ukprn != null || x.Ukprn > 0).ToList();

            var openLearningProviderOrganisations = await SendApiRequest(FdsApiConstants.LearningProviderQueryPath, skipPaging: true);

            if (managementGroupOrganisations != null && openLearningProviderOrganisations.Data != null)
            {
                providers = openLearningProviderOrganisations.Data.Concat(managementGroupOrganisations).DistinctBy(x => x.Ukprn);
            }
            else if (managementGroupOrganisations != null)
            {
                providers = managementGroupOrganisations;
            }
            else
            {
                providers = openLearningProviderOrganisations.Data;
            }

            if (providers != null)
            {
                var organisations = _mapper.Map<IEnumerable<Provider>, IEnumerable<Organisation>>(providers);

                return organisations.ToDictionary(x => x.Identifiers.First(x => x.Type == OrganisationIdentifierType.Ukprn).Value);
            }

            return null;
        }

        private static List<SearchCriteria> BuildSearchCriteria(string searchFieldName, string searchValue, string searchOperator = FdsApiConstants.FieldOperatorEquals)
        {
            var searchCriteria = new SearchCriteria()
            {
                FieldName = searchFieldName,
                Value = searchValue,
                Operator = searchOperator
            };

            return new List<SearchCriteria> { searchCriteria };
        }

        private static FdsApiRequest BuildApiRequest(List<List<SearchCriteria>> searchCriteria, int pageNumber, int pageSize, bool skipPaging)
        {
            var request = new FdsApiRequest();
            if (searchCriteria.Any())
            {
                request.SearchCriteria = searchCriteria;
            }

            if (!skipPaging)
            {
                request.PageNumber = pageNumber;
                request.PageSize = pageSize;
            }

            request.SkipPaging = skipPaging;

            return request;
        }

        private async Task<FdsApiResponse> SendApiRequest(string url, string fieldName = null, string fieldOperatorComparer = null, string value = null, bool isStatusFieldRequired = true, bool skipPaging = false, int pageNumber = 0, int pageSize = 100)
        {
            var searchCriteriaList = new List<List<SearchCriteria>>();

            if (!string.IsNullOrEmpty(fieldName))
            {
                searchCriteriaList.Add(BuildSearchCriteria(fieldName, value, fieldOperatorComparer));
            }

            if (isStatusFieldRequired)
            {
                searchCriteriaList.Add(BuildSearchCriteria(FdsApiConstants.Status, FdsApiConstants.OpenOrgStatus, FdsApiConstants.FieldOperatorContains));
            }

            var request = BuildApiRequest(searchCriteriaList, pageNumber, pageSize, skipPaging);

            HttpContent content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");

            _logger.LogInformation($"Calling FDS api {_client.BaseAddress.AbsoluteUri}{url} with the request {JsonConvert.SerializeObject(request)}");

            var result = await _client.PostAsync(url, content);

            if (result.IsSuccessStatusCode)
            {
                var response = JsonConvert.DeserializeObject<FdsApiResponse>(await result.Content.ReadAsStringAsync());

                _logger.LogInformation($"Total records {response?.TotalCount} received");

                return response;
            }

            _logger.LogError($"FDS api returns {result.StatusCode} for {_client.BaseAddress.AbsoluteUri}{url} with request {JsonConvert.SerializeObject(request)}");

            throw new HttpRequestException($"FDS api returns {result.StatusCode}");
        }
    }
}