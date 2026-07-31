using FluentAssertions;
using Mapster;
using MapsterMapper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using Pds.Core.Common.Organisation.Enums;
using Pds.Core.Common.Organisation.Models;
using Pds.Core.Logging;
using Pds.DocumentExchange.Data.Services.DTOs.Configuration;
using Pds.DocumentExchange.Data.Services.DTOs.FDS;
using Pds.DocumentExchange.Data.Services.Implementations.FDS;
using Pds.DocumentExchange.Data.Services.Mapster;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Tests.Unit.Fds
{
    [TestClass]
    public class OrganisationServiceTests
    {
        private readonly HttpMessageHandler _httpMessageHandler = Mock.Of<HttpMessageHandler>();
        private readonly Mock<ILoggerAdapter<OrganisationService>> _logger = new Mock<ILoggerAdapter<OrganisationService>>();
        private readonly IMapper _mapper = new Mapper(new TypeAdapterConfig().Configure());

        [TestMethod]
        public async Task Get_All_Organisation()
        {
            //Arrange
            var config = GetFdsApiClientConfiguration();

            SetupMessageHandler(HttpStatusCode.OK, config.Url + "/api/PaymentOrganisation/query", SetFdsApiResponse());
            SetupMessageHandler(HttpStatusCode.OK, config.Url + "/api/Provider/query", SetFdsApiResponse(isMatResponse: false));

            var listOfOrganisation = GetOrganisations().Concat(GetOrganisations(isMatResponse: false)).ToDictionary(x => x.Identifiers.First(x => x.Type == OrganisationIdentifierType.Ukprn).Value);

            var organisationService = new OrganisationService(GetHttpClient(), config, _mapper, _logger.Object);

            //Act
            var result = await organisationService.GetAllOrganisations();

            //Assert
            result.Should().BeEquivalentTo(listOfOrganisation);
            VerifyMessageHandler(HttpMethod.Post, config.Url + "/api/PaymentOrganisation/query", 1);
            VerifyMessageHandler(HttpMethod.Post, config.Url + "/api/Provider/query", 1);
            Mock.Verify(_logger);
        }

        [TestMethod]
        public async Task Get_All_Organisation_From_PaymentOrganisation_Returns_Null()
        {
            //Arrange
            var response = new FdsApiResponse()
            {
                TotalCount = 0,
                PageNumber = 1,
                Data = null
            };

            var config = GetFdsApiClientConfiguration();

            SetupMessageHandler(HttpStatusCode.OK, config.Url + "/api/PaymentOrganisation/query", response);
            SetupMessageHandler(HttpStatusCode.OK, config.Url + "/api/Provider/query", SetFdsApiResponse(isMatResponse: false));

            var listOfOrganisation = GetOrganisations(isMatResponse: false).ToDictionary(x => x.Identifiers.First(x => x.Type == OrganisationIdentifierType.Ukprn).Value);

            var organisationService = new OrganisationService(GetHttpClient(), config, _mapper, _logger.Object);

            //Act
            var result = await organisationService.GetAllOrganisations();

            //Assert
            result.Should().BeEquivalentTo(listOfOrganisation);
            VerifyMessageHandler(HttpMethod.Post, config.Url + "/api/PaymentOrganisation/query", 1);
            VerifyMessageHandler(HttpMethod.Post, config.Url + "/api/Provider/query", 1);
            Mock.Verify(_logger);
        }

        [TestMethod]
        public async Task Get_All_Organisation_From_Provider_Returns_Null()
        {
            //Arrange
            var response = new FdsApiResponse()
            {
                TotalCount = 0,
                PageNumber = 1,
                Data = null
            };

            var config = GetFdsApiClientConfiguration();

            SetupMessageHandler(HttpStatusCode.OK, config.Url + "/api/PaymentOrganisation/query", SetFdsApiResponse());
            SetupMessageHandler(HttpStatusCode.OK, config.Url + "/api/Provider/query", response);

            var listOfOrganisation = GetOrganisations().ToDictionary(x => x.Identifiers.First(x => x.Type == OrganisationIdentifierType.Ukprn).Value);

            var organisationService = new OrganisationService(GetHttpClient(), config, _mapper, _logger.Object);

            //Act
            var result = await organisationService.GetAllOrganisations();

            //Assert
            result.Should().BeEquivalentTo(listOfOrganisation);
            VerifyMessageHandler(HttpMethod.Post, config.Url + "/api/PaymentOrganisation/query", 1);
            VerifyMessageHandler(HttpMethod.Post, config.Url + "/api/Provider/query", 1);
            Mock.Verify(_logger);
        }

        [TestMethod]
        public async Task Get_All_Organisation_From_PaymentOrganisation_And_Provider_Urls_Returns_Null()
        {
            //Arrange
            var response = new FdsApiResponse()
            {
                TotalCount = 0,
                PageNumber = 1,
                Data = null
            };

            var config = GetFdsApiClientConfiguration();

            SetupMessageHandler(HttpStatusCode.OK, config.Url + "/api/PaymentOrganisation/query", response);
            SetupMessageHandler(HttpStatusCode.OK, config.Url + "/api/Provider/query", response);

            var organisationService = new OrganisationService(GetHttpClient(), config, _mapper, _logger.Object);

            //Act
            var result = await organisationService.GetAllOrganisations();

            //Assert
            result.Should().BeEquivalentTo((Dictionary<string, Organisation>)null);
            VerifyMessageHandler(HttpMethod.Post, config.Url + "/api/PaymentOrganisation/query", 1);
            VerifyMessageHandler(HttpMethod.Post, config.Url + "/api/Provider/query", 1);
            Mock.Verify(_logger);
        }

        [TestMethod]
        public void Get_All_Organisation_Thorws_Exception_Calling_PaymentOrganisation_Url()
        {
            //Arrange
            var config = GetFdsApiClientConfiguration();

            SetupMessageHandler(HttpStatusCode.InternalServerError, config.Url + "/api/PaymentOrganisation/query", "InternalServerError");
            var organisationService = new OrganisationService(GetHttpClient(), config, _mapper, _logger.Object);

            //Act
            Func<Task> act = async () => await organisationService.GetAllOrganisations();

            //Assert
            act.Should().ThrowAsync<HttpRequestException>();
            VerifyMessageHandler(HttpMethod.Post, config.Url + "/api/PaymentOrganisation/query", 1);
            VerifyMessageHandler(HttpMethod.Post, config.Url + "/api/Provider/query", 0);
            Mock.Verify(_logger);
        }

        [TestMethod]
        public void Get_All_Organisation_Thorws_Exception_Calling_Provider_Url()
        {
            //Arrange
            var config = GetFdsApiClientConfiguration();

            SetupMessageHandler(HttpStatusCode.OK, config.Url + "/api/PaymentOrganisation/query", SetFdsApiResponse(true, true));
            SetupMessageHandler(HttpStatusCode.InternalServerError, config.Url + "/api/Provider/query", "InternalServerError");

            var organisationService = new OrganisationService(GetHttpClient(), config, _mapper, _logger.Object);

            //Act
            Func<Task> act = async () => await organisationService.GetAllOrganisations();

            //Assert
            act.Should().ThrowAsync<HttpRequestException>();
            VerifyMessageHandler(HttpMethod.Post, config.Url + "/api/PaymentOrganisation/query", 1);
            VerifyMessageHandler(HttpMethod.Post, config.Url + "/api/Provider/query", 1);
            Mock.Verify(_logger);
        }

        [TestMethod]
        public async Task Get_Organisation_By_Name_From_PaymentOrganisation_Url_With_MaxResults()
        {
            //Arrange
            var config = GetFdsApiClientConfiguration();

            SetupMessageHandler(HttpStatusCode.OK, config.Url + "/api/PaymentOrganisation/query", SetFdsApiResponse());

            var organisationService = new OrganisationService(GetHttpClient(), config, _mapper, _logger.Object);

            //Act
            var result = await organisationService.GetOrganisationByName("test", 2);

            //Assert
            result.Should().BeEquivalentTo(GetOrganisations());
            VerifyMessageHandler(HttpMethod.Post, config.Url + "/api/PaymentOrganisation/query", 1);
            VerifyMessageHandler(HttpMethod.Post, config.Url + "/api/Provider/query", 0);
            Mock.Verify(_logger);
        }

        [TestMethod]
        public async Task Get_Organisation_By_Name_With_MaxResults()
        {
            //Arrange
            var config = GetFdsApiClientConfiguration();

            SetupMessageHandler(HttpStatusCode.OK, config.Url + "/api/PaymentOrganisation/query", SetFdsApiResponse());
            SetupMessageHandler(HttpStatusCode.OK, config.Url + "/api/Provider/query", SetFdsApiResponse(false, false));

            var organisationService = new OrganisationService(GetHttpClient(), config, _mapper, _logger.Object);

            var listOfOrganisation = GetOrganisations().Concat(GetOrganisations(false, false));

            //Act
            var result = await organisationService.GetOrganisationByName("test", 3);

            //Assert
            result.Should().BeEquivalentTo(listOfOrganisation);
            VerifyMessageHandler(HttpMethod.Post, config.Url + "/api/PaymentOrganisation/query", 1);
            VerifyMessageHandler(HttpMethod.Post, config.Url + "/api/Provider/query", 1);
            Mock.Verify(_logger);
        }

        [TestMethod]
        public async Task Get_Organisation_By_Name_Returns_Unique_Results()
        {
            //Arrange
            var config = GetFdsApiClientConfiguration();

            var paymentOrganisations = new List<Provider>
            {
                new Provider()
                {
                    Name = "Nova Training",
                    Ukprn = 10027272,
                    Status = "Open",
                    Type = "16-19 provider",
                    SubType = "Independent learning provider",
                    ManagementGroup = null,
                    Child = null
                }
            };

            var paymentOrganisationResponse = new FdsApiResponse()
            {
                TotalCount = 1,
                PageNumber = 1,
                Data = paymentOrganisations
            };

            var providers = new List<Provider>
            {
                new Provider()
                {
                    Name = "Nova Training",
                    Ukprn = 10027272,
                    Status = "Open",
                    Type = "16-19 provider",
                    SubType = "Independent learning provider",
                    ManagementGroup = new Provider()
                    {
                        Name = "Nova Training",
                        Ukprn = 10027272,
                        Type = "provider"
                    },
                    Child = null
                },
                new Provider()
                {
                    Name = "School for Girls Training",
                    Ukprn = 10000001,
                    Status = "Open",
                    Type = "Local authority maintained schools",
                    SubType = "Voluntary aided school",
                    ManagementGroup = new Provider()
                    {
                        Name = "CAMDEN LONDON BOROUGH COUNCIL",
                        Ukprn = 10003988,
                        Type = "LA"
                    },
                    Child = null
                }
            };

            var providerResponse = new FdsApiResponse()
            {
                TotalCount = 2,
                PageNumber = 1,
                Data = providers
            };

            SetupMessageHandler(HttpStatusCode.OK, config.Url + "/api/PaymentOrganisation/query", paymentOrganisationResponse);
            SetupMessageHandler(HttpStatusCode.OK, config.Url + "/api/Provider/query", providerResponse);

            var organisationService = new OrganisationService(GetHttpClient(), config, _mapper, _logger.Object);

            var organisations = new List<Organisation>
            {
                new Organisation()
                {
                    Identifiers = new List<OrganisationIdentifier>()
                        {
                            new OrganisationIdentifier
                            {
                                Type = OrganisationIdentifierType.Ukprn,
                                Value = "10027272"
                            }
                        },
                    Name = "Nova Training",
                    OrganisationType = "16-19 provider",
                    OrganisationTypeDisplay = new DisplayValues() { Singular = "16-19 provider", Plural = "16-19 provider" },
                    OrganisationSubType = "Independent learning provider",
                    OrganisationSubTypeDisplay = new DisplayValues() { Singular = "Independent learning provider", Plural = "Independent learning provider" },
                    Status = "Open",
                    ParentOrganisation = new Organisation()
                    {
                        Identifiers = new List<OrganisationIdentifier>()
                                {
                                    new OrganisationIdentifier
                                    {
                                        Type = OrganisationIdentifierType.Ukprn,
                                        Value = "10027272"
                                    }
                                },
                        Name = "Nova Training",
                        OrganisationType = "ManagementGroup",
                        OrganisationTypeDisplay = new DisplayValues() { Singular = "Parent organisation", Plural = "Parent organisations" },
                        OrganisationSubType = "provider",
                        OrganisationSubTypeDisplay = new DisplayValues() { Singular = "provider", Plural = "provider" },
                    },
                    ChildOrganisations = new List<Organisation>()
                },
                new Organisation()
                {
                    Identifiers = new List<OrganisationIdentifier>()
                        {
                            new OrganisationIdentifier
                            {
                                Type = OrganisationIdentifierType.Ukprn,
                                Value = "10000001"
                            }
                        },
                    Name = "School for Girls Training",
                    OrganisationType = "Local authority maintained schools",
                    OrganisationTypeDisplay = new DisplayValues() { Singular = "Local authority maintained schools", Plural = "Local authority maintained schools" },
                    OrganisationSubType = "Voluntary aided school",
                    OrganisationSubTypeDisplay = new DisplayValues() { Singular = "Voluntary aided school", Plural = "Voluntary aided school" },
                    Status = "Open",
                    ParentOrganisation = new Organisation()
                    {
                        Identifiers = new List<OrganisationIdentifier>()
                                {
                                    new OrganisationIdentifier
                                    {
                                        Type = OrganisationIdentifierType.Ukprn,
                                        Value = "10003988"
                                    }
                                },
                        Name = "CAMDEN LONDON BOROUGH COUNCIL",
                        OrganisationType = "ManagementGroup",
                        OrganisationTypeDisplay = new DisplayValues() { Singular = "Parent organisation", Plural = "Parent organisations" },
                        OrganisationSubType = "LA",
                        OrganisationSubTypeDisplay = new DisplayValues() { Singular = "LA", Plural = "LA" },
                    },
                    ChildOrganisations = new List<Organisation>()
                }
            };

            //Act
            var result = await organisationService.GetOrganisationByName("Training", 10);

            //Assert
            result.Should().BeEquivalentTo(organisations);
            VerifyMessageHandler(HttpMethod.Post, config.Url + "/api/PaymentOrganisation/query", 1);
            VerifyMessageHandler(HttpMethod.Post, config.Url + "/api/Provider/query", 1);
            Mock.Verify(_logger);
        }

        [TestMethod]
        public async Task Get_Organisation_By_Name_Returns_Null_From_Both_PaymentOrganisation_And_Provider()
        {
            //Arrange
            var response = new FdsApiResponse()
            {
                TotalCount = 0,
                PageNumber = 1,
                Data = null
            };

            var config = GetFdsApiClientConfiguration();
            SetupMessageHandler(HttpStatusCode.OK, config.Url + "/api/PaymentOrganisation/query", response);
            SetupMessageHandler(HttpStatusCode.OK, config.Url + "/api/Provider/query", response);

            var organisationService = new OrganisationService(GetHttpClient(), config, _mapper, _logger.Object);

            //Act
            var result = await organisationService.GetOrganisationByName("test", 20);

            //Assert
            result.Should().Equal(null);
            VerifyMessageHandler(HttpMethod.Post, config.Url + "/api/PaymentOrganisation/query", 1);
            VerifyMessageHandler(HttpMethod.Post, config.Url + "/api/Provider/query", 1);
            Mock.Verify(_logger);
        }

        [TestMethod]
        public void Get_Organisation_By_Name_Thorws_Exception_From_Provider_Url()
        {
            //Arrange
            var config = GetFdsApiClientConfiguration();
            SetupMessageHandler(HttpStatusCode.OK, config.Url + "/api/PaymentOrganisation/query", SetFdsApiResponse());
            SetupMessageHandler(HttpStatusCode.InternalServerError, config.Url + "/api/Provider/query", "InternalServerError");
            var organisationService = new OrganisationService(GetHttpClient(), config, _mapper, _logger.Object);

            //Act
            Func<Task> act = async () => await organisationService.GetOrganisationByName("test", 20);

            //Assert
            act.Should().ThrowAsync<HttpRequestException>();
            VerifyMessageHandler(HttpMethod.Post, config.Url + "/api/PaymentOrganisation/query", 1);
            VerifyMessageHandler(HttpMethod.Post, config.Url + "/api/Provider/query", 1);
            Mock.Verify(_logger);
        }

        [TestMethod]
        public void Get_Organisation_By_Name_Thorws_Exception_From_PaymentOrganisation_Url()
        {
            //Arrange
            var config = GetFdsApiClientConfiguration();

            SetupMessageHandler(HttpStatusCode.InternalServerError, config.Url + "/api/PaymentOrganisation/query", "InternalServerError");
            var organisationService = new OrganisationService(GetHttpClient(), config, _mapper, _logger.Object);

            //Act
            Func<Task> act = async () => await organisationService.GetOrganisationByName("test", 20);

            //Assert
            act.Should().ThrowAsync<HttpRequestException>();
            VerifyMessageHandler(HttpMethod.Post, config.Url + "/api/PaymentOrganisation/query", 1);
            VerifyMessageHandler(HttpMethod.Post, config.Url + "/api/Provider/query", 0);
            Mock.Verify(_logger);
        }

        [TestMethod]
        public async Task Get_Organisation_From_PaymentOrganisation()
        {
            //Arrange
            var provider = new Provider()
            {
                Name = "test",
                Ukprn = 1,
                Status = "Open",
                Type = "test",
                SubType = "test",
                ManagementGroup = null,
                Child = new List<Provider>()
                {
                    new Provider()
                    {
                        Name = "test",
                        Ukprn = 2,
                        Status = "Open",
                        Type = "test",
                        SubType = "test"
                    },
                    new Provider()
                    {
                        Name = "test",
                        Ukprn = 3,
                        Status = "Open",
                        Type = "test",
                        SubType = "test"
                    }
                }
            };

            var providers = new List<Provider>
            {
                provider
            };

            var response = new FdsApiResponse()
            {
                TotalCount = 1,
                PageNumber = 1,
                Data = providers
            };

            var config = GetFdsApiClientConfiguration();
            SetupMessageHandler(HttpStatusCode.OK, config.Url + "/api/PaymentOrganisation/query", response);
            SetupMessageHandler(HttpStatusCode.OK, config.Url + "/api/Provider/query", response);

            var organisation = new Organisation()
            {
                Identifiers = new List<OrganisationIdentifier>()
                    {
                        new OrganisationIdentifier
                        {
                            Type = OrganisationIdentifierType.Ukprn,
                            Value = "1"
                        }
                    },
                Name = "test",
                OrganisationType = "ManagementGroup",
                OrganisationTypeDisplay = new DisplayValues() { Singular = "Parent organisation", Plural = "Parent organisations" },
                OrganisationSubType = "test",
                OrganisationSubTypeDisplay = new DisplayValues() { Singular = "test", Plural = "test" },
                Status = "Open",
                ParentOrganisation = null,
                ChildOrganisations = new List<Organisation>()
                    {
                        new Organisation()
                        {
                            Identifiers = new List<OrganisationIdentifier>()
                            {
                                new OrganisationIdentifier
                                {
                                    Type = OrganisationIdentifierType.Ukprn,
                                    Value = "2"
                                }
                            },
                            Name = "test",
                            Status = "Open",
                            OrganisationType = "test",
                            OrganisationTypeDisplay = new DisplayValues() { Singular = "test", Plural = "test" },
                            OrganisationSubType = "test",
                            OrganisationSubTypeDisplay = new DisplayValues() { Singular = "test", Plural = "test" },
                        },
                        new Organisation()
                        {
                            Identifiers = new List<OrganisationIdentifier>()
                            {
                                new OrganisationIdentifier
                                {
                                    Type = OrganisationIdentifierType.Ukprn,
                                    Value = "3"
                                }
                            },
                            Name = "test",
                            Status = "Open",
                            OrganisationType = "test",
                            OrganisationTypeDisplay = new DisplayValues() { Singular = "test", Plural = "test" },
                            OrganisationSubType = "test",
                            OrganisationSubTypeDisplay = new DisplayValues() { Singular = "test", Plural = "test" },
                        }
                    }
            };

            var organisationService = new OrganisationService(GetHttpClient(), config, _mapper, _logger.Object);

            //Act
            var result = await organisationService.GetOrganisation("1");

            //Assert
            result.Should().BeEquivalentTo(organisation);
            VerifyMessageHandler(HttpMethod.Post, config.Url + "/api/PaymentOrganisation/query", 1);
            VerifyMessageHandler(HttpMethod.Post, config.Url + "/api/Provider/query", 1);
            Mock.Verify(_logger);
        }

        [TestMethod]
        public async Task Get_Organisation_From_Provider()
        {
            //Arrange
            var provider = new Provider()
            {
                Name = "test",
                Ukprn = 2,
                Status = "Open",
                Type = "test",
                SubType = "test",
                ManagementGroup = new Provider()
                {
                    Name = "test",
                    Ukprn = 1,
                    Status = "Open",
                    Type = "test",
                    SubType = "test"
                },
                Child = null
            };

            var providers = new List<Provider>
            {
                provider
            };

            var providerResponse = new FdsApiResponse()
            {
                TotalCount = 1,
                PageNumber = 1,
                Data = providers
            };

            var matResponse = new FdsApiResponse()
            {
                TotalCount = 0,
                PageNumber = 1,
                Data = null
            };

            var config = GetFdsApiClientConfiguration();
            SetupMessageHandler(HttpStatusCode.OK, config.Url + "/api/PaymentOrganisation/query", matResponse);
            SetupMessageHandler(HttpStatusCode.OK, config.Url + "/api/Provider/query", providerResponse);

            var organisation = new Organisation()
            {
                Identifiers = new List<OrganisationIdentifier>()
                    {
                        new OrganisationIdentifier
                        {
                            Type = OrganisationIdentifierType.Ukprn,
                            Value = "2"
                        }
                    },
                Name = "test",
                OrganisationType = "test",
                OrganisationTypeDisplay = new DisplayValues() { Singular = "test", Plural = "test" },
                OrganisationSubType = "test",
                OrganisationSubTypeDisplay = new DisplayValues() { Singular = "test", Plural = "test" },
                Status = "Open",
                ParentOrganisation = new Organisation()
                {
                    Identifiers = new List<OrganisationIdentifier>()
                            {
                                new OrganisationIdentifier
                                {
                                    Type = OrganisationIdentifierType.Ukprn,
                                    Value = "1"
                                }
                            },
                    Name = "test",
                    Status = "Open",
                    OrganisationType = "ManagementGroup",
                    OrganisationTypeDisplay = new DisplayValues() { Singular = "Parent organisation", Plural = "Parent organisations" },
                    OrganisationSubType = "test",
                    OrganisationSubTypeDisplay = new DisplayValues() { Singular = "test", Plural = "test" },
                },
                ChildOrganisations = Enumerable.Empty<Organisation>()
            };

            var organisationService = new OrganisationService(GetHttpClient(), config, _mapper, _logger.Object);

            //Act
            var result = await organisationService.GetOrganisation("1");

            //Assert
            result.Should().BeEquivalentTo(organisation);
            VerifyMessageHandler(HttpMethod.Post, config.Url + "/api/PaymentOrganisation/query", 1);
            VerifyMessageHandler(HttpMethod.Post, config.Url + "/api/Provider/query", 1);
            Mock.Verify(_logger);
        }

        [TestMethod]
        public async Task Get_Organisation_From_Provider_Return_Data_As_Empty()
        {
            //Arrange
            var provider = new Provider()
            {
                Name = "test",
                Ukprn = 1,
                Status = "Open",
                Type = "test",
                SubType = "test",
                ManagementGroup = null,
                Child = null
            };

            var providers = new List<Provider>
            {
                provider
            };

            var matResponse = new FdsApiResponse()
            {
                TotalCount = 1,
                PageNumber = 1,
                Data = providers
            };

            var providerResponse = new FdsApiResponse()
            {
                TotalCount = 0,
                PageNumber = 1,
                Data = Array.Empty<Provider>()
            };

            var config = GetFdsApiClientConfiguration();
            SetupMessageHandler(HttpStatusCode.OK, config.Url + "/api/PaymentOrganisation/query", matResponse);
            SetupMessageHandler(HttpStatusCode.OK, config.Url + "/api/Provider/query", providerResponse);

            var organisation = new Organisation()
            {
                Identifiers = new List<OrganisationIdentifier>()
                    {
                        new OrganisationIdentifier
                        {
                            Type = OrganisationIdentifierType.Ukprn,
                            Value = "1"
                        }
                    },
                Name = "test",
                OrganisationType = "ManagementGroup",
                OrganisationTypeDisplay = new DisplayValues() { Singular = "Parent organisation", Plural = "Parent organisations" },
                OrganisationSubType = "test",
                OrganisationSubTypeDisplay = new DisplayValues() { Singular = "test", Plural = "test" },
                Status = "Open",
                ParentOrganisation = null,
                ChildOrganisations = Enumerable.Empty<Organisation>()
            };

            var organisationService = new OrganisationService(GetHttpClient(), config, _mapper, _logger.Object);

            //Act
            var result = await organisationService.GetOrganisation("1");

            //Assert
            result.Should().BeEquivalentTo(organisation);
            VerifyMessageHandler(HttpMethod.Post, config.Url + "/api/PaymentOrganisation/query", 1);
            VerifyMessageHandler(HttpMethod.Post, config.Url + "/api/Provider/query", 2);
            Mock.Verify(_logger);
        }

        [TestMethod]
        public async Task Get_Organisation_Returns_Null()
        {
            //Arrange
            var response = new FdsApiResponse()
            {
                TotalCount = 0,
                PageNumber = 1,
                Data = null
            };

            var config = GetFdsApiClientConfiguration();
            SetupMessageHandler(HttpStatusCode.OK, config.Url + "/api/PaymentOrganisation/query", response);
            SetupMessageHandler(HttpStatusCode.OK, config.Url + "/api/Provider/query", response);
            var organisationService = new OrganisationService(GetHttpClient(), config, _mapper, _logger.Object);

            //Act
            var result = await organisationService.GetOrganisation("1");

            //Assert
            result.Should().BeEquivalentTo((Organisation)null);
            VerifyMessageHandler(HttpMethod.Post, config.Url + "/api/PaymentOrganisation/query", 1);
            VerifyMessageHandler(HttpMethod.Post, config.Url + "/api/Provider/query", 1);
            Mock.Verify(_logger);
        }

        [TestMethod]
        public void Get_Organisation_Thorws_Exception_From_PaymentOrganisation_Url()
        {
            //Arrange
            var config = GetFdsApiClientConfiguration();

            SetupMessageHandler(HttpStatusCode.InternalServerError, config.Url + "/api/PaymentOrganisation/query", "InternalServerError");
            var organisationService = new OrganisationService(GetHttpClient(), config, _mapper, _logger.Object);

            //Act
            Func<Task> act = async () => await organisationService.GetOrganisation("1");

            //Assert
            act.Should().ThrowAsync<HttpRequestException>();
            VerifyMessageHandler(HttpMethod.Post, config.Url + "/api/PaymentOrganisation/query", 1);
            VerifyMessageHandler(HttpMethod.Post, config.Url + "/api/Provider/query", 0);
            Mock.Verify(_logger);
        }

        [TestMethod]
        public void Get_Organisation_Thorws_Exception_From_Provider_Url()
        {
            //Arrange
            var config = GetFdsApiClientConfiguration();
            SetupMessageHandler(HttpStatusCode.OK, config.Url + "/api/PaymentOrganisation/query", SetFdsApiResponse(true, false));
            SetupMessageHandler(HttpStatusCode.InternalServerError, config.Url + "/api/Provider/query", "InternalServerError");
            var organisationService = new OrganisationService(GetHttpClient(), config, _mapper, _logger.Object);

            //Act
            Func<Task> act = async () => await organisationService.GetOrganisation("1");

            //Assert
            act.Should().ThrowAsync<HttpRequestException>();
            VerifyMessageHandler(HttpMethod.Post, config.Url + "/api/PaymentOrganisation/query", 1);
            VerifyMessageHandler(HttpMethod.Post, config.Url + "/api/Provider/query", 1);
            Mock.Verify(_logger);
        }

        private static FdsApiResponse SetFdsApiResponse(bool isMatResponse = true, bool isListOfProviders = true)
        {
            int ukprn = isMatResponse ? 12345678 : 10000055;
            var provider = new Provider()
            {
                Name = "test",
                Ukprn = ukprn,
                Status = "Open",
                Type = "test",
                SubType = "test",
                ManagementGroup = null,
                Child = new List<Provider>()
                {
                    new Provider()
                    {
                        Name = "test",
                        Ukprn = ukprn + 1,
                        Status = "Open",
                        Type = "test",
                        SubType = "test"
                    },
                    new Provider()
                    {
                        Name = "test",
                        Ukprn = ukprn + 2,
                        Status = "Open",
                        Type = "test",
                        SubType = "test"
                    }
                }
            };

            var providerTwo = new Provider()
            {
                Name = "test",
                Ukprn = ukprn + 3,
                Status = "Open",
                Type = "test",
                SubType = "test",
                ManagementGroup = null,
                Child = new List<Provider>()
                    {
                        new Provider()
                        {
                            Name = "test",
                            Ukprn = ukprn + 4,
                            Status = "Open",
                            Type = "test",
                            SubType = "test"
                        },
                        new Provider()
                        {
                            Name = "test",
                            Ukprn = ukprn + 5,
                            Status = "Open",
                            Type = "test",
                            SubType = "test"
                        }
                    }
            };

            var providers = new List<Provider>
            {
                provider
            };

            if (isListOfProviders)
            {
                providers.Add(providerTwo);
            }

            return new FdsApiResponse()
            {
                TotalCount = providers.Count(),
                PageNumber = 1,
                Data = providers
            };
        }

        private static List<Organisation> GetOrganisations(bool isMatResponse = true, bool isListOfOrganisation = true)
        {
            int ukprn = isMatResponse ? 12345678 : 10000055;

            var organisation = new Organisation()
            {
                Identifiers = new List<OrganisationIdentifier>()
                    {
                        new OrganisationIdentifier
                        {
                            Type = OrganisationIdentifierType.Ukprn,
                            Value = ukprn.ToString()
                        }
                    },
                Name = "test",
                OrganisationType = "ManagementGroup",
                OrganisationTypeDisplay = new DisplayValues() { Singular = "Parent organisation", Plural = "Parent organisations" },
                OrganisationSubType = "test",
                OrganisationSubTypeDisplay = new DisplayValues() { Singular = "test", Plural = "test" },
                Status = "Open",
                ParentOrganisation = null,
                ChildOrganisations = new List<Organisation>()
                    {
                        new Organisation()
                        {
                            Identifiers = new List<OrganisationIdentifier>()
                            {
                                new OrganisationIdentifier
                                {
                                    Type = OrganisationIdentifierType.Ukprn,
                                    Value = (ukprn + 1).ToString()
                                }
                            },
                            Name = "test",
                            Status = "Open",
                            OrganisationType = "test",
                            OrganisationTypeDisplay = new DisplayValues() { Singular = "test", Plural = "test" },
                            OrganisationSubType = "test",
                            OrganisationSubTypeDisplay = new DisplayValues() { Singular = "test", Plural = "test" },
                        },
                        new Organisation()
                        {
                            Identifiers = new List<OrganisationIdentifier>()
                            {
                                new OrganisationIdentifier
                                {
                                    Type = OrganisationIdentifierType.Ukprn,
                                    Value = (ukprn + 2).ToString()
                                }
                            },
                            Name = "test",
                            Status = "Open",
                            OrganisationType = "test",
                            OrganisationTypeDisplay = new DisplayValues() { Singular = "test", Plural = "test" },
                            OrganisationSubType = "test",
                            OrganisationSubTypeDisplay = new DisplayValues() { Singular = "test", Plural = "test" },
                        }
                    }
            };

            var organisationTwo = new Organisation()
            {
                Identifiers = new List<OrganisationIdentifier>()
                    {
                        new OrganisationIdentifier
                        {
                            Type = OrganisationIdentifierType.Ukprn,
                            Value = (ukprn + 3).ToString()
                        }
                    },
                Name = "test",
                OrganisationType = "ManagementGroup",
                OrganisationTypeDisplay = new DisplayValues() { Singular = "Parent organisation", Plural = "Parent organisations" },
                OrganisationSubType = "test",
                OrganisationSubTypeDisplay = new DisplayValues() { Singular = "test", Plural = "test" },
                Status = "Open",
                ParentOrganisation = null,
                ChildOrganisations = new List<Organisation>()
                    {
                        new Organisation()
                        {
                            Identifiers = new List<OrganisationIdentifier>()
                            {
                                new OrganisationIdentifier
                                {
                                    Type = OrganisationIdentifierType.Ukprn,
                                    Value = (ukprn + 4).ToString()
                                }
                            },
                            Name = "test",
                            Status = "Open",
                            OrganisationType = "test",
                            OrganisationTypeDisplay = new DisplayValues() { Singular = "test", Plural = "test" },
                            OrganisationSubType = "test",
                            OrganisationSubTypeDisplay = new DisplayValues() { Singular = "test", Plural = "test" },
                        },
                        new Organisation()
                        {
                            Identifiers = new List<OrganisationIdentifier>()
                            {
                                new OrganisationIdentifier
                                {
                                    Type = OrganisationIdentifierType.Ukprn,
                                    Value = (ukprn + 5).ToString()
                                }
                            },
                            Name = "test",
                            Status = "Open",
                            OrganisationType = "test",
                            OrganisationTypeDisplay = new DisplayValues() { Singular = "test", Plural = "test" },
                            OrganisationSubType = "test",
                            OrganisationSubTypeDisplay = new DisplayValues() { Singular = "test", Plural = "test" },
                        }
                    }
            };

            var organisations = new List<Organisation>
            {
                organisation
            };

            if (isListOfOrganisation)
            {
                organisations.Add(organisationTwo);
            }

            return organisations;
        }

        private HttpClient GetHttpClient()
            => new HttpClient(_httpMessageHandler);

        private void SetupMessageHandler(HttpStatusCode statusCode, string url, object responseObject = null)
        {
            var expectedResponse = new HttpResponseMessage
            {
                StatusCode = statusCode
            };

            if (responseObject != null)
            {
                var responseContent = JsonConvert.SerializeObject(responseObject);
                expectedResponse.Content = new StringContent(
                responseContent,
                Encoding.UTF8,
                "application/json");
            }

            Mock.Get(_httpMessageHandler)
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(m => m.RequestUri.Equals(url)),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(expectedResponse);
        }

        private void VerifyMessageHandler(HttpMethod httpMethod, string expectedUri, int noOfTimesHit)
        {
            Mock.Get(_httpMessageHandler)
                .Protected()
                .Verify(
                    "SendAsync",
                    Times.Exactly(noOfTimesHit),
                    ItExpr.Is<HttpRequestMessage>(
                        req => req.Method.Equals(httpMethod)
                        && req.RequestUri.Equals(new Uri(expectedUri))),
                    ItExpr.IsAny<CancellationToken>());
        }

        private FdsApiClientConfiguration GetFdsApiClientConfiguration()
        {
            return new FdsApiClientConfiguration()
            {
                Url = "https://test.com",
                ApimSubscriptionKey = "12345678"
            };
        }
    }
}
