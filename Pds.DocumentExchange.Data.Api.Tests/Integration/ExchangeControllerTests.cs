using FluentAssertions;
using FluentAssertions.Equivalency;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Testing.Platform.Extensions.Messages;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pds.Core.Caching.Models;
using Pds.Core.Common.Organisation.Models;
using Pds.DocumentExchange.Data.Api.Controllers;
using Pds.DocumentExchange.Data.Api.Enums;
using Pds.DocumentExchange.Data.Api.Models.Filters;
using Pds.DocumentExchange.Data.Populator.Models;
using Pds.DocumentExchange.Data.Populator.Scenarios;
using Pds.DocumentExchange.Data.Populator.Storage;
using Pds.DocumentExchange.Data.Services.DTOs;
using Pds.DocumentExchange.Data.Services.DTOs.Configuration;
using Pds.DocumentExchange.Data.Services.Implementations;
using Pds.DocumentExchange.Data.Services.Implementations.Filters;
using Pds.DocumentExchange.Data.Services.Implementations.Lookup;
using Pds.DocumentExchange.Data.Services.Implementations.Settings;
using Pds.DocumentExchange.Data.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using ExchangeDocument = Pds.DocumentExchange.Data.Services.DTOs.ExchangeDocument;

namespace Pds.DocumentExchange.Data.Api.Tests.Integration
{
    [TestClass, TestCategory("Integration")]
    public class ExchangeControllerTests : BaseIntegration, IDisposable
    {
        public ExchangeControllerTests()
        {
            SetupSystemProvider();
            SetUpConfig();
        }

        #region Summary

        [TestMethod]
        public async Task Summary_WhenUserInfoIsInvalid_ReturnsUnprocessableEntityObjectResult()
        {
            // Arrange
            var invalidUserInfo = new Models.UserInfo
            {
                Principal = string.Empty
            };

            var controller = GetExchangeController();

            // Act
            var result = await controller.Summary(invalidUserInfo);

            // Assert
            result.Result.Should().BeOfType<UnprocessableEntityObjectResult>();
        }

        [TestMethod]
        public async Task Summary_WhenUserInfoIsValid_ReturnsSummary()
        {
            // Arrange
            var userInfo = new Models.UserInfo
            {
                Principal = "principal",
                FullName = "full-name",
                EmailAddress = "email-address@education@gov.uk",
                OrganisationInfo = new Models.OrganisationInfo
                {
                    Name = "organisation-name",
                    OrganisationIdentifier = new Models.OrganisationIdentifier
                    {
                        Type = OrganisationIdentifierType.Ukprn,
                        Value = "12345678"
                    }
                }
            };

            var mappedSummary = new Summary
            {
                CountOfNewDocuments = 2,
                DocumentExchangeEnabled = true
            };

            var organisation = new Organisation
            {
                Name = "organisation-name",
                Identifiers = new List<OrganisationIdentifier>
                    {
                        new OrganisationIdentifier
                        {
                            Type = Pds.Core.Common.Organisation.Enums.OrganisationIdentifierType.Ukprn,
                            Value = "12345678"
                        }
                    }
            };

            SetUpAllCaches<List<ExchangeDocument>>();
            SetUpAllCaches<IEnumerable<Product>>();
            SetUpAllCaches<bool>();

            var organisations = new Dictionary<string, Organisation>
            {
                { "10000001", organisation }
            };

            var products = new List<Product>
            {
                new Product()
            };

            Mock.Get(MockMemoryCache)
                .Setup(x => x.Get<IDictionary<string, Organisation>>(It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(new CacheItem<IDictionary<string, Organisation>>(organisations));

            Mock.Get(OrganisationService).Setup(
               o => o.GetOrganisation(It.IsAny<string>()))
               .ReturnsAsync(organisation);

            var controller = GetExchangeController();

            var configuration = GetFilesInDocumentsUploadedConfiguration();
            var parameters = new DocumentsUploadedScenario.Parameters
            {
                Records = new[]
                {
                    new DocumentRecord
                    {
                        Ukprn = 12345678,
                        ProviderId = "10001",
                        AcademicYear = "202021",
                        FileType = "pdf",
                        FileSize = "100KB"
                    },
                    new DocumentRecord
                    {
                        Ukprn = 12345678,
                        ProviderId = "10002",
                        AcademicYear = "202021",
                        FileType = "pdf",
                        FileSize = "100KB"
                    }
                }
            };

            await DocumentsPublishedByAgencyPopulator.PopulateScenarioData(configuration, parameters);

            // Act
            var result = await controller.Summary(userInfo);

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>().Which.Value.Should().BeEquivalentTo(mappedSummary);

            Mock.VerifyAll(
                Mock.Get(MockMemoryCache),
                Mock.Get(MockDistributedCache),
                Mock.Get(OrganisationService));
        }

        #endregion


        #region Team Summary

        [TestMethod]
        public async Task TeamSummary_WhenTeamIsInvalid_ReturnsUnprocessableEntityObjectResult()
        {
            // Arrange
            var team = "invalid-team";

            SetUpAllCaches<IEnumerable<AgencyTeam>>();

            var controller = GetExchangeController();

            // Act
            var result = await controller.TeamSummary(team);

            // Assert
            result.Result.Should().BeOfType<UnprocessableEntityObjectResult>();

            Mock.VerifyAll(
                Mock.Get(MockMemoryCache),
                Mock.Get(MockDistributedCache));
        }

        [TestMethod]
        public async Task TeamSummary_WhenTeamIsValid_ReturnsSummary()
        {
            // Arrange
            var mappedSummary = new Models.Summary
            {
                CountOfNewDocuments = 2,
                DocumentExchangeEnabled = true
            };

            SetUpAllCaches<bool>();
            SetUpAllCaches<IEnumerable<AgencyTeam>>();
            SetUpAllCaches<IEnumerable<Product>>();

            var controller = GetExchangeController();

            var configuration = GetFilesInDocumentsUploadedConfiguration();
            var parameters = new DocumentsUploadedScenario.Parameters
            {
                Records = new[]
                {
                    new DocumentRecord
                    {
                        Ukprn = 12345678,
                        ProviderId = "10001",
                        AcademicYear = "202021",
                        FileType = "pdf",
                        FileSize = "100KB"
                    },
                    new DocumentRecord
                    {
                        Ukprn = 12345678,
                        ProviderId = "10002",
                        AcademicYear = "202021",
                        FileType = "pdf",
                        FileSize = "100KB"
                    }
                }
            };

            await DocumentsUploadedByExternalPopulator.PopulateScenarioData(configuration, parameters);

            // Act
            var result = await controller.TeamSummary(Team1);

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>().Which.Value.Should().BeEquivalentTo(mappedSummary);

            Mock.VerifyAll(
                Mock.Get(MockMemoryCache),
                Mock.Get(MockDistributedCache));
        }

        [TestMethod]
        public async Task TeamSummary_WhenMultipleValidTeams_ReturnsSummary()
        {
            // Arrange
            var teams = string.Concat(Team1, ",", Team2);

            var mappedSummary = new Models.Summary
            {
                CountOfNewDocuments = 4,
                DocumentExchangeEnabled = true
            };

            SetUpAllCaches<bool>();
            SetUpAllCaches<IEnumerable<AgencyTeam>>();
            SetUpAllCaches<IEnumerable<Product>>();

            var controller = GetExchangeController();

            var configuration = GetFilesInDocumentsUploadedConfiguration();
            var parameters = new DocumentsUploadedScenario.Parameters
            {
                Records = new[]
                {
                    new DocumentRecord
                    {
                        Ukprn = 12345678,
                        ProviderId = "10001",
                        AcademicYear = "202021",
                        FileType = "pdf",
                        FileSize = "100KB"
                    },
                    new DocumentRecord
                    {
                        Ukprn = 12345678,
                        ProviderId = "10002",
                        AcademicYear = "202021",
                        FileType = "pdf",
                        FileSize = "100KB"
                    },
                    new DocumentRecord
                    {
                        Ukprn = 12345678,
                        ProviderId = "10036",
                        AcademicYear = "202021",
                        FileType = "pdf",
                        FileSize = "100KB"
                    },
                    new DocumentRecord
                    {
                        Ukprn = 12345678,
                        ProviderId = "10037",
                        AcademicYear = "202021",
                        FileType = "pdf",
                        FileSize = "100KB"
                    }
                }
            };

            await DocumentsUploadedByExternalPopulator.PopulateScenarioData(configuration, parameters);

            // Act
            var result = await controller.TeamSummary(teams);

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>().Which.Value.Should().BeEquivalentTo(mappedSummary);

            Mock.VerifyAll(
                Mock.Get(MockMemoryCache),
                Mock.Get(MockDistributedCache));
        }

        #endregion


        #region CurrentProductVersionForOrganisation

        [TestMethod]
        public async Task CurrentProductVersionForOrganisation_WhenOrganisationIdentifierIsInvalid_ReturnsUnprocessableEntityObjectResult()
        {
            // Arrange
            var organisationIdentifier = new Models.OrganisationIdentifier
            {
                Type = OrganisationIdentifierType.Ukprn,
                Value = string.Empty
            };

            var productIdentifier = 10001;

            var controller = GetExchangeController();

            // Act
            var result = await controller.CurrentProductVersionForOrganisation(organisationIdentifier, productIdentifier);

            // Assert
            result.Result.Should().BeOfType<UnprocessableEntityObjectResult>();
        }

        [TestMethod]
        public async Task CurrentProductVersionForOrganisation_WhenProductIdentifierIsInvalid_ReturnsUnprocessableEntityObjectResult()
        {
            // Arrange
            var organisationIdentifier = new Models.OrganisationIdentifier
            {
                Type = OrganisationIdentifierType.Ukprn,
                Value = "12345678"
            };

            var productIdentifier = 0;

            SetUpAllCaches<IEnumerable<Product>>();

            var controller = GetExchangeController();

            // Act
            var result = await controller.CurrentProductVersionForOrganisation(organisationIdentifier, productIdentifier);

            // Assert
            result.Result.Should().BeOfType<UnprocessableEntityObjectResult>();

            ((ObjectResult)result.Result).Value.Should()
               .BeOfType<SerializableError>()
               .Which.Should()
               .BeEquivalentTo(
                   new Dictionary<string, object>
                   {
                        { "productIdentifier", new[] { "The provided product identifier does not exist." } }
                   });

            Mock.VerifyAll(
                Mock.Get(MockMemoryCache),
                Mock.Get(MockDistributedCache));
        }

        [TestMethod]
        public async Task CurrentProductVersionForOrganisation_WhenOrganisationAndProductIdentifierAreValid_ReturnsProductVersionNumber()
        {
            // Arrange
            var organisationIdentifier = new Models.OrganisationIdentifier
            {
                Type = OrganisationIdentifierType.Ukprn,
                Value = "12345678"
            };

            var productIdentifier = 10001;

            SetUpAllCaches<IEnumerable<Product>>();

            Mock.Get(SystemProvider.DateTime)
               .Setup(d => d.UtcNow())
               .Returns(new DateTime(2020, 12, 25));

            var controller = GetExchangeController();

            var configuration = GetFilesInDocumentsUploadedConfiguration();
            var parameters = new DocumentsUploadedScenario.Parameters
            {
                Records = new[]
                {
                    new DocumentRecord
                    {
                        Ukprn = 12345678,
                        ProviderId = "10001",
                        AcademicYear = "202021",
                        FileType = "pdf",
                        FileSize = "100KB"
                    },
                    new DocumentRecord
                    {
                        Ukprn = 12345678,
                        ProviderId = "10001",
                        AcademicYear = "202021",
                        FileType = "pdf",
                        FileSize = "100KB"
                    }
                }
            };

            await DocumentsUploadedByExternalPopulator.PopulateScenarioData(configuration, parameters);

            // Act
            var result = await controller.CurrentProductVersionForOrganisation(organisationIdentifier, productIdentifier);

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>().Which.Value.Should().Be(2);

            Mock.VerifyAll(
                Mock.Get(MockMemoryCache),
                Mock.Get(MockDistributedCache),
                Mock.Get(SystemProvider.DateTime));
        }

        #endregion


        #region DocumentsTeam

        [TestMethod]
        public async Task DocumentsTeam_WhenTeamIsInvalid_ReturnsUnprocessableEntity()
        {
            // Arrange
            var team = "invalid-team";
            var options = new Models.ExchangeListDocumentOptions();

            var controller = GetExchangeController();

            // Act
            var result = await controller.Documents(team, options);

            // Assert
            result.Result.Should().BeOfType<UnprocessableEntityObjectResult>();
        }

        [TestMethod]
        public async Task DocumentsTeam_WhenExchangeListDocumentOptionsIsInvalid_ReturnsUnprocessableEntity()
        {
            // Arrange
            var options = new Models.ExchangeListDocumentOptions();

            var controller = GetExchangeController();

            // Act
            var result = await controller.Documents(Team1, options);

            // Assert
            result.Result.Should().BeOfType<UnprocessableEntityObjectResult>();
        }

        #endregion


        #region Documents

        [TestMethod]
        public async Task Documents_WhenExchangeListDocumentOptionsIsInvalid_ReturnsUnprocessableEntity()
        {
            // Arrange
            var options = new Models.ExchangeListOrganisationDocumentOptions();

            var controller = GetExchangeController();

            // Act
            var result = await controller.Documents(options);

            // Assert
            result.Result.Should().BeOfType<UnprocessableEntityObjectResult>();
        }

        [TestMethod]
        public async Task Documents_WhenExchangeListDocumentOptionsIsValid_ReturnsExchangeDocumentsListResult()
        {
            // Arrange
            var inputOptions = new Models.ExchangeListOrganisationDocumentOptions
            {
                DocumentStatusOption = ExchangeDocumentDirection.SentByOrganisation,
                DocumentReferences = Enumerable.Empty<Models.DocumentReference>(),
                FilterOptions = Enumerable.Empty<IFilterOption>(),
                PageSize = 25,
                OrganisationIdentifier = new Models.OrganisationIdentifier
                {
                    Type = OrganisationIdentifierType.Ukprn,
                    Value = "11223344"
                }
            };

            var organisation = new Organisation
            {
                Name = "organisation-name",
                Identifiers = new List<OrganisationIdentifier>
                {
                        new OrganisationIdentifier
                        {
                            Type = Pds.Core.Common.Organisation.Enums.OrganisationIdentifierType.Ukprn,
                            Value = "12345678"
                        }
                },
                OrganisationSubType = "org-subtype"
            };

            SetUpAllCaches<List<ExchangeDocument>>();
            SetUpAllCaches<IEnumerable<Product>>();

            var organisations = new Dictionary<string, Organisation>
            {
                { "12345678", organisation }
            };

            var products = new List<Product>
            {
                new Product()
            };

            Mock.Get(MockMemoryCache)
                .Setup(x => x.Get<IDictionary<string, Organisation>>(It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(new CacheItem<IDictionary<string, Organisation>>(organisations));

            Mock.Get(OrganisationService).Setup(
               o => o.GetOrganisation(It.IsAny<string>()))
               .ReturnsAsync(organisation);

            var guid1 = Guid.NewGuid();
            var guid2 = Guid.NewGuid();

            var expectedResult = new Models.ListResult<Models.ExchangeDocument>
            {
                Items = new List<Models.ExchangeDocument>
                {
                    new Models.ExchangeDocument
                    {
                        AgencyTeam = string.Empty,
                        DocumentReference = new Models.DocumentReference
                        {
                            BatchIdentifier = guid1.ToString(),
                            FileName = "12345678_10002_202021.pdf",
                            ParentBatchIdentifier = guid1.ToString()
                        },
                        EventHistory = new List<Models.ExchangeDocumentEvent>
                        {
                            new Models.ExchangeDocumentEvent
                            {
                                EventDateTime = new DateTime(2021, 01, 21, 21, 21, 21),
                                EventType = ExchangeDocumentEventType.SentByOrganisation,
                                UserInfo = new Models.UserInfo
                                {
                                   EmailAddress = "test@test.com",
                                   FullName = "Test",
                                   IsViewAsProvider = false,
                                   OrganisationInfo = null,
                                   Principal = "Test1"
                                }
                            }
                        },
                        ExchangeDirection = ExchangeDocumentDirection.SentByOrganisation,
                        OrganisationInfo = new Models.OrganisationInfo
                        {
                            Name = "organisation-name",
                            OrganisationIdentifier = new Models.OrganisationIdentifier
                            {
                                Type = OrganisationIdentifierType.Ukprn,
                                Value = "12345678"
                            }
                        },
                        PreviousVersions = new List<Models.ExchangeDocument>(),
                        Product = new Models.Product
                        {
                            AgencyTeams = new List<string> { "DocumentExchangeAdministratorFundingCentre" },
                            CanOrganisationsUpload = false,
                            Identifier = 10002,
                            Name = "Data sharing protocol",
                            PluralName = "Data sharing protocols"
                        },
                        Version = 1,
                        Year = 202021
                    },
                    new Models.ExchangeDocument
                    {
                        AgencyTeam = string.Empty,
                        DocumentReference = new Models.DocumentReference
                        {
                            BatchIdentifier = guid2.ToString(),
                            FileName = "12345678_10001_202021.pdf",
                            ParentBatchIdentifier = guid2.ToString()
                        },
                        EventHistory = new List<Models.ExchangeDocumentEvent>
                        {
                            new Models.ExchangeDocumentEvent
                            {
                                EventDateTime = new DateTime(2021, 01, 21, 21, 21, 21),
                                EventType = ExchangeDocumentEventType.SentByOrganisation,
                                UserInfo = new Models.UserInfo
                                {
                                   EmailAddress = "test@test.com",
                                   FullName = "Test",
                                   IsViewAsProvider = false,
                                   OrganisationInfo = null,
                                   Principal = "Test1"
                                }
                            }
                        },
                        ExchangeDirection = ExchangeDocumentDirection.SentByOrganisation,
                        OrganisationInfo = new Models.OrganisationInfo
                        {
                            Name = "organisation-name",
                            OrganisationIdentifier = new Models.OrganisationIdentifier
                            {
                                Type = OrganisationIdentifierType.Ukprn,
                                Value = "12345678"
                            }
                        },
                        PreviousVersions = new List<Models.ExchangeDocument>(),
                        Product = new Models.Product
                        {
                            AgencyTeams = new List<string> { "DocumentExchangeAdministratorFundingCentre" },
                            CanOrganisationsUpload = false,
                            Identifier = 10001,
                            Name = "Allocation calculation toolkit",
                            PluralName = "Allocation calculation toolkits"
                        },
                        Version = 1,
                        Year = 202021
                    }
                },
                Filters = new List<IFilter>
                {
                    new ListFilter
                    {
                        Groups = new List<FilterGroup>(),
                        Key = "ProductIdList",
                        Title = "Filter by document type",
                        Type = "ListFilter",
                        Values = new List<FilterValue>()
                    },
                    new ListFilter
                    {
                        Groups = new List<FilterGroup>(),
                        Key = "AcademicYear",
                        Title = "Filter by academic year",
                        Type = "ListFilter",
                        Values = new List<FilterValue>()
                    }
                },
                TotalItems = 2,
                TotalPages = 1
            };

            var configuration = GetFilesInDocumentsUploadedConfiguration();
            var parameters = new DocumentsUploadedScenario.Parameters
            {
                Records = new[]
                {
                    new DocumentRecord
                    {
                        Ukprn = 12345678,
                        ProviderId = "10001",
                        AcademicYear = "202021",
                        FileType = "pdf",
                        FileSize = "100KB"
                    },
                    new DocumentRecord
                    {
                        Ukprn = 12345678,
                        ProviderId = "10002",
                        AcademicYear = "202021",
                        FileType = "pdf",
                        FileSize = "100KB"
                    }
                }
            };

            await DocumentsUploadedByExternalPopulator.PopulateScenarioData(configuration, parameters);

            Mock.Get(SystemProvider.DateTime)
               .Setup(d => d.UtcNow())
               .Returns(new DateTime(2020, 12, 25));

            var controller = GetExchangeController();

            // Act
            var result = await controller.Documents(inputOptions);

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(expectedResult, options =>
                    options.Excluding((IMemberInfo member) => member.Type == typeof(Models.DocumentReference))
                           .Excluding((IMemberInfo member) => member.Type == typeof(DateTime)));

            Mock.VerifyAll(
                Mock.Get(MockMemoryCache),
                Mock.Get(MockDistributedCache),
                Mock.Get(OrganisationService));
        }

        #endregion


        #region Download (Teams)

        [TestMethod]
        public async Task Download_Teams_ForInvalidDownloadRequest_ReturnsUnprocessableEntityObjectResult()
        {
            // Arrange
            var request = new Models.ExchangeDocumentDownloadRequest();

            var controller = GetExchangeController();

            // Act
            var result = await controller.Download("team1, team2", request);

            // Assert
            result.Result.Should().BeOfType<UnprocessableEntityObjectResult>();
        }

        [TestMethod]
        public async Task Download_Teams_ForInvalidTeams_ReturnsUnprocessableEntityObjectResult()
        {
            // Arrange
            var teams = "team1, team2";

            teams.Split(',');
            var request = new Models.ExchangeDocumentDownloadRequest();

            var controller = GetExchangeController();

            // Act
            var result = await controller.Download(teams, request);

            // Assert
            result.Result.Should().BeOfType<UnprocessableEntityObjectResult>();
        }

        [TestMethod]
        public async Task Download_Teams_ForValidParams_ReturnsOkResultContainingFileContent()
        {
            // Arrange
            var teams = $"{Team1}, {Team2}";

            var organisation = new Organisation
            {
                Name = "organisation-name",
                Identifiers = new List<OrganisationIdentifier>
                {
                        new OrganisationIdentifier
                        {
                            Type = Pds.Core.Common.Organisation.Enums.OrganisationIdentifierType.Ukprn,
                            Value = "10000055"
                        }
                },
                OrganisationSubType = "org-subtype"
            };

            SetUpAllCaches<IEnumerable<AgencyTeam>>();
            SetUpAllCaches<IEnumerable<ExchangeDocument>>();
            SetUpAllCaches<DisplayValues>();
            SetUpAllCaches<IEnumerable<Product>>();

            var organisations = new Dictionary<string, Organisation>
            {
                { "10000001", organisation }
            };

            var products = new List<Product>
            {
                new Product()
            };

            Mock.Get(MockMemoryCache)
                .Setup(x => x.Get<IDictionary<string, Organisation>>(It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(new CacheItem<IDictionary<string, Organisation>>(organisations));


            Mock.Get(OrganisationService).Setup(
              o => o.GetOrganisation(It.IsAny<string>()))
              .ReturnsAsync(organisation);

            var listOptions = new Models.ExchangeListDocumentOptions
            {
                DocumentStatusOption = ExchangeDocumentDirection.SentByOrganisation,
                DocumentReferences = new List<Models.DocumentReference>(),

                FilterOptions = new List<IFilterOption>()
                {
                    new TextBoxFilterOption()
                    {
                        Type = "TextBoxFilter",
                        Key = "Ukprn",
                        Value = "10000055"
                    }
                },
                PageSize = 25
            };

            var request = new Models.ExchangeDocumentDownloadRequest
            {
                UserInfo = new Models.UserInfo()
                {
                    Principal = "Test1",
                    FullName = "Test",
                    EmailAddress = "test@test.com",
                    OrganisationInfo = new Models.OrganisationInfo
                    {
                        OrganisationIdentifier = new Models.OrganisationIdentifier
                        {
                            Type = OrganisationIdentifierType.Ukprn,
                            Value = "10000055"
                        }
                    },
                    IsViewAsProvider = false
                },
                ListOptions = listOptions
            };

            var configuration = GetFilesInDocumentsUploadedConfiguration();
            var parameters = new DocumentsUploadedScenario.Parameters
            {
                Records = new[]
                {
                    new DocumentRecord
                    {
                        Ukprn = 10000055,
                        ProviderId = "10001",
                        AcademicYear = "202021",
                        FileType = "pdf",
                        FileSize = "100KB"
                    },
                    new DocumentRecord
                    {
                        Ukprn = 10000055,
                        ProviderId = "10002",
                        AcademicYear = "202021",
                        FileType = "pdf",
                        FileSize = "100KB"
                    }
                }
            };

            var expectedUnzippedFiles = new Dictionary<string, byte[]>()
            {
                { "10000055_10001_202021.pdf", new byte[102400] },
                { "10000055_10002_202021.pdf", new byte[102400] },
            };

            await DocumentsUploadedByExternalPopulator.PopulateScenarioData(configuration, parameters);

            Mock.Get(SystemProvider.DateTime)
               .Setup(d => d.UtcNow())
               .Returns(new DateTime(2020, 12, 25));

            var controller = GetExchangeController();

            // Act
            var result = await controller.Download(teams, request);

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>();

            var actualUnzippedFiles = UnzipFilesFromResult(result);

            actualUnzippedFiles.Should().HaveCount(2);
            actualUnzippedFiles.Should().BeEquivalentTo(expectedUnzippedFiles);

            Mock.VerifyAll(
                  Mock.Get(MockMemoryCache),
                  Mock.Get(SystemProvider.DateTime));
        }

        #endregion


        #region Delete

        [TestMethod]
        public async Task Delete_WhenExchangeDocumentDeleteRequestIsInvalid_ReturnsUnprocessableEntity()
        {
            // Arrange
            var deleteRequest = new Models.ExchangeDocumentDeleteRequest();

            var controller = GetExchangeController();

            // Act
            var result = await controller.Delete(deleteRequest);

            // Assert
            result.Result.Should().BeOfType<UnprocessableEntityObjectResult>();
        }

        [TestMethod]
        public async Task Delete_WhenExchangeDocumentDeleteRequestIsValid_DeletesDocumentsAndReturnsOk()
        {
            // Arrange
            Mock.Get(SystemProvider.DateTime)
              .Setup(d => d.UtcNow())
              .Returns(new DateTime(2021, 02, 25));

            var organisation = new Organisation
            {
                Name = "organisation-name",
                Identifiers = new List<OrganisationIdentifier>
                   {
                        new OrganisationIdentifier
                        {
                            Type = Pds.Core.Common.Organisation.Enums.OrganisationIdentifierType.Ukprn,
                            Value = "12345678"
                        }
                   }
            };

            SetUpAllCaches<IEnumerable<AgencyTeam>>();
            SetUpAllCaches<IEnumerable<Product>>();

            Mock.Get(OrganisationService).Setup(
              o => o.GetOrganisation(It.IsAny<string>()))
              .ReturnsAsync(organisation);

            var guid = "69b79ace-1c96-4b4a-978a-d8727707fa02";

            var documentReference = new Models.DocumentReferenceWithPreviousVersions
            {
                BatchIdentifier = guid,
                FileName = "12345678_10001_202021.pdf",
                ParentBatchIdentifier = guid
            };

            var deleteRequest = new Models.ExchangeDocumentDeleteRequest
            {
                UserInfo = new Models.UserInfo
                {
                    Principal = "principal",
                    FullName = "full-name",
                    EmailAddress = "email-address@education.gov.uk"
                },
                DocumentReferences = new[] { documentReference }
            };

            var expectedResult = new[]
            {
                new Models.ExchangeDocument
                {
                    AgencyTeam = "DocumentExchangeAdministratorFundingCentre",
                    EventHistory = new List<Models.ExchangeDocumentEvent>
                    {
                     new Models.ExchangeDocumentEvent
                     {
                        EventDateTime = new DateTime(2021, 01, 21, 21, 21, 21),
                        EventType = ExchangeDocumentEventType.SentByOrganisation,
                        UserInfo = new Models.UserInfo
                        {
                            EmailAddress = "test@test.com",
                            FullName = "Test",
                            IsViewAsProvider = false,
                            OrganisationInfo = null,
                            Principal = "Test1"
                        }
                     }
                    },
                    Version = 1,
                    PreviousVersions = new List<Models.ExchangeDocument> { },
                    DocumentReference = deleteRequest.DocumentReferences.First(),
                    Product = new Models.Product
                    {
                        AgencyTeams = new List<string> { "DocumentExchangeAdministratorFundingCentre" },
                        CanOrganisationsUpload = false,
                        Identifier = 10001,
                        Name = "Allocation calculation toolkit",
                        PluralName = "Allocation calculation toolkits"
                    },
                    OrganisationInfo = new Models.OrganisationInfo
                    {
                        Name = organisation.Name,
                        OrganisationIdentifier = new Models.OrganisationIdentifier
                        {
                            Type = OrganisationIdentifierType.Ukprn,
                            Value = organisation.Identifiers.First().Value
                        }
                    },
                    Year = 202021
                }
            };

            var configuration = GetFilesInDocumentsUploadedConfiguration();

            var records = new[]
            {
                    new DocumentRecord
                    {
                        Ukprn = 12345678,
                        ProviderId = "10001",
                        AcademicYear = "202021",
                        FileType = "pdf",
                        FileSize = "100KB",
                        ParentBatchIdentifier = guid
                    },
                    new DocumentRecord
                    {
                        Ukprn = 12345678,
                        ProviderId = "10002",
                        AcademicYear = "202021",
                        FileType = "pdf",
                        FileSize = "100KB",
                        ParentBatchIdentifier = "8ff54859-aa9c-4217-a11b-a43291921c6d"
                    }
            };

            var parameters = new DocumentsUploadedScenario.Parameters
            {
                Records = records
            };

            await DocumentsUploadedByExternalPopulator.PopulateScenarioData(configuration, parameters);

            var controller = GetExchangeController();

            // Act
            var result = await controller.Delete(deleteRequest);

            var azureCloudStorageAccount = new Services.Implementations.Storage.AzureCloudStorageAccount(configuration.BlobContainer.ConnectionString);

            var azureFileShareDirectory = new Services.Implementations.Storage.AzureBlobContainer(
               configuration.BlobContainer.ContainerName,
               azureCloudStorageAccount,
               new FileNameProvider(),
               CreateMockLoggerAdapter<Services.Implementations.Storage.AzureBlobContainer>());

            var files = await azureFileShareDirectory.GetFiles().ToListAsync();

            List<BatchMetadata> batchMetadata;

            using (var client = new DocumentsCosmosDb(configuration.CosmosDb))
            {
                batchMetadata = await client.GetEntityByParentId<BatchMetadata>(guid);
            }

            var cosmosDbFile = batchMetadata?.First().Files?.First();

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>();
            ((OkObjectResult)result.Result).Value.Should().BeEquivalentTo(expectedResult, options =>
                        options.Excluding((IMemberInfo member) => member.Type == typeof(DateTime)));

            files.Select(f => f.FileName).Should().NotContain("12345678_10001_202021.pdf");
            files.Select(f => f.FileName).Should().Contain("12345678_10002_202021.pdf");
            cosmosDbFile.Deleted.Should().BeTrue();
            cosmosDbFile.History.Select(h => h.Action).Should().Contain(Services.Enums.FileAction.UserDeleted);
        }

        [TestMethod]
        public async Task DeleteSingle_WhenExchangeDocumentDeleteRequestIsInvalid_ReturnsUnprocessableEntity()
        {
            // Arrange
            var deleteRequest = new Models.ExchangeDocumentDeleteRequest();

            var controller = GetExchangeController();

            // Act
            var result = await controller.DeleteSingle(deleteRequest);

            // Assert
            result.Result.Should().BeOfType<UnprocessableEntityObjectResult>();
        }

        [TestMethod]
        public async Task DeleteSingle_WhenExchangeDocumentDeleteRequestIsValid_DeletesDocumentAndReturnsOk()
        {
            // Arrange
            var organisation = new Organisation
            {
                Name = "organisation-name",
                Identifiers = new List<OrganisationIdentifier>
                   {
                        new OrganisationIdentifier
                        {
                            Type = Pds.Core.Common.Organisation.Enums.OrganisationIdentifierType.Ukprn,
                            Value = "12345678"
                        }
                   }
            };

            SetUpAllCaches<IEnumerable<AgencyTeam>>();
            SetUpAllCaches<IEnumerable<Product>>();

            Mock.Get(OrganisationService).Setup(
              o => o.GetOrganisation(It.IsAny<string>()))
              .ReturnsAsync(organisation);

            Mock.Get(SystemProvider.DateTime)
                  .Setup(d => d.UtcNow())
                  .Returns(new DateTime(2021, 02, 25));

            var guid = "69b79ace-1c96-4b4a-978a-d8727707fa02";

            var documentReference = new Models.DocumentReferenceWithPreviousVersions
            {
                BatchIdentifier = guid,
                FileName = "12345678_10001_202021.pdf",
                ParentBatchIdentifier = guid
            };

            var deleteRequest = new Models.ExchangeDocumentDeleteRequest
            {
                UserInfo = new Models.UserInfo
                {
                    Principal = "principal",
                    FullName = "full-name",
                    EmailAddress = "email-address@education.gov.uk"
                },
                DocumentReferences = new[] { documentReference }
            };

            var expectedResult = new Models.ExchangeDocument
            {
                AgencyTeam = "DocumentExchangeAdministratorFundingCentre",
                EventHistory = new List<Models.ExchangeDocumentEvent>
                {
                     new Models.ExchangeDocumentEvent
                     {
                        EventDateTime = new DateTime(2021, 01, 21, 21, 21, 21),
                        EventType = ExchangeDocumentEventType.PublishedByAgency,
                        UserInfo = new Models.UserInfo
                        {
                            EmailAddress = "test@test.com",
                            FullName = "Test",
                            IsViewAsProvider = false,
                            OrganisationInfo = null,
                            Principal = "Test1"
                        }
                     }
                },
                Version = 1,
                ExchangeDirection = ExchangeDocumentDirection.PublishedByAgency,
                PreviousVersions = new List<Models.ExchangeDocument> { },
                DocumentReference = deleteRequest.DocumentReferences.First(),
                Product = new Models.Product
                {
                    AgencyTeams = new List<string> { "DocumentExchangeAdministratorFundingCentre" },
                    CanOrganisationsUpload = false,
                    Identifier = 10001,
                    Name = "Allocation calculation toolkit",
                    PluralName = "Allocation calculation toolkits"
                },
                OrganisationInfo = new Models.OrganisationInfo
                {
                    Name = "organisation-name",
                    OrganisationIdentifier = new Models.OrganisationIdentifier
                    {
                        Type = OrganisationIdentifierType.Ukprn,
                        Value = "12345678"
                    }
                },
                Year = 202021
            };

            var configuration = GetFilesInDocumentsUploadedConfiguration();

            var records = new[]
            {
                    new DocumentRecord
                    {
                        Ukprn = 12345678,
                        ProviderId = "10001",
                        AcademicYear = "202021",
                        FileType = "pdf",
                        FileSize = "100KB",
                        ParentBatchIdentifier = guid
                    },
                    new DocumentRecord
                    {
                        Ukprn = 12345678,
                        ProviderId = "10002",
                        AcademicYear = "202021",
                        FileType = "pdf",
                        FileSize = "100KB",
                        ParentBatchIdentifier = "8ff54859-aa9c-4217-a11b-a43291921c6d"
                    }
            };

            var parameters = new DocumentsUploadedScenario.Parameters()
            {
                Records = records
            };

            await DocumentsPublishedByAgencyPopulator.PopulateScenarioData(configuration, parameters);

            var controller = GetExchangeController();

            // Act
            var result = await controller.DeleteSingle(deleteRequest);

            var azureCloudStorageAccount = new Services.Implementations.Storage.AzureCloudStorageAccount(configuration.BlobContainer.ConnectionString);

            var azureFileShareDirectory = new Services.Implementations.Storage.AzureBlobContainer(
               configuration.BlobContainer.ContainerName,
               azureCloudStorageAccount,
               new FileNameProvider(),
               CreateMockLoggerAdapter<Services.Implementations.Storage.AzureBlobContainer>());

            var files = await azureFileShareDirectory.GetFiles().ToListAsync();

            List<BatchMetadata> batchMetadata;

            using (var client = new DocumentsCosmosDb(configuration.CosmosDb))
            {
                batchMetadata = await client.GetEntityByParentId<BatchMetadata>(guid);
            }

            var cosmosDbFile = batchMetadata?.First().Files?.First();

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>();
            ((OkObjectResult)result.Result).Value.Should().BeEquivalentTo(expectedResult, options =>
                options.Excluding((IMemberInfo member) => member.Type == typeof(DateTime)));

            files.Select(f => f.FileName).Should().NotContain("12345678_10001_202021.pdf");
            files.Select(f => f.FileName).Should().Contain("12345678_10002_202021.pdf");
            cosmosDbFile.Deleted.Should().BeTrue();
            cosmosDbFile.History.Select(h => h.Action).Should().Contain(Services.Enums.FileAction.UserDeleted);
        }

        #endregion


        #region OrganisationFilesVirusScanSuccessful

        [TestMethod]
        public async Task OrganisationFilesVirusScanSuccessful_WhenDocumentReferenceIsInvalid_ReturnsUnprocessableEntity()
        {
            // Arrange
            var documentReference = new Models.DocumentReference();

            var controller = GetExchangeController();

            // Act
            var result = await controller.OrganisationFilesVirusScanSuccessful(documentReference);

            // Assert
            result.Should().BeOfType<UnprocessableEntityObjectResult>();
        }

        [TestMethod]
        public async Task OrganisationFilesVirusScanSuccessful_WhenDocumentReferenceIsValid_ProcessesFileAndReturnsOk()
        {
            // Arrange
            var documentReference = new Models.DocumentReference
            {
                FileName = "12345678_10002_202021.pdf",
                BatchIdentifier = "batch-id",
                ParentBatchIdentifier = "parent-batch-id"
            };

            SetUpAllCaches<IEnumerable<AgencyTeam>>();
            SetUpCache<string>(MockDistributedCache);

            Mock.Get(SystemProvider.DateTime)
               .Setup(d => d.UtcNow())
               .Returns(new DateTime(2020, 12, 25));

            var configuration = GetFilesInDocumentsUploadedConfiguration();
            var parameters = new DocumentsUploadedScenario.Parameters
            {
                Records = new[]
                {
                    new DocumentRecord
                    {
                        Ukprn = 12345678,
                        ProviderId = "10001",
                        AcademicYear = "202021",
                        FileType = "pdf",
                        FileSize = "100KB"
                    }
                }
            };

            await DocumentsUploadedByExternalPopulator.PopulateScenarioData(configuration, parameters);

            var controller = GetExchangeController();

            // Act
            var result = await controller.OrganisationFilesVirusScanSuccessful(documentReference);

            // Assert
            result.Should().BeOfType<OkResult>();

            Mock.VerifyAll(
               Mock.Get(MockMemoryCache),
               Mock.Get(MockDistributedCache),
               Mock.Get(SystemProvider.DateTime));
        }

        #endregion


        #region AgencyFilesVirusScanSuccessful

        [TestMethod]
        public async Task AgencyFilesVirusScanSuccessful_WhenDocumentReferenceIsInvalid_ReturnsUnprocessableEntity()
        {
            // Arrange
            var documentReference = new Models.DocumentReference();

            var controller = GetExchangeController();

            // Act
            var result = await controller.AgencyFilesVirusScanSuccessful(documentReference);

            // Assert
            result.Should().BeOfType<UnprocessableEntityObjectResult>();
        }

        [TestMethod]
        public async Task AgencyFilesVirusScanSuccessful_WhenDocumentReferenceIsValid_ProcessesFileAndReturnsOk()
        {
            // Arrange
            var documentReference = new Models.DocumentReference
            {
                FileName = "12345678_10002_202021.pdf",
                BatchIdentifier = "batch-id",
                ParentBatchIdentifier = "parent-batch-id"
            };

            SetUpAllCaches<IEnumerable<AgencyTeam>>();
            SetUpCache<string>(MockDistributedCache);

            Mock.Get(SystemProvider.DateTime)
               .Setup(d => d.UtcNow())
               .Returns(new DateTime(2020, 12, 25));

            var configuration = GetFilesInDocumentsUploadedConfiguration();
            var parameters = new DocumentsUploadedScenario.Parameters
            {
                Records = new[]
                {
                    new DocumentRecord
                    {
                        Ukprn = 12345678,
                        ProviderId = "10001",
                        AcademicYear = "202021",
                        FileType = "pdf",
                        FileSize = "100KB"
                    }
                }
            };

            await DocumentsUploadedByExternalPopulator.PopulateScenarioData(configuration, parameters);

            var controller = GetExchangeController();

            // Act
            var result = await controller.AgencyFilesVirusScanSuccessful(documentReference);

            // Assert
            result.Should().BeOfType<OkResult>();

            Mock.VerifyAll(
               Mock.Get(MockMemoryCache),
               Mock.Get(MockDistributedCache),
               Mock.Get(SystemProvider.DateTime));
        }

        #endregion


        public void Dispose()
        {
            Task.Run(() => DocumentsPublishedByAgencyPopulator.TearDownScenarioData(GetFilesInDocumentsUploadedConfiguration())).GetAwaiter().GetResult();
            Task.Run(() => DocumentsUploadedByExternalPopulator.TearDownScenarioData(GetFilesInDocumentsUploadedConfiguration())).GetAwaiter().GetResult();
        }

        private ExchangeController GetExchangeController()
        {
            var exchangeDocumentSelector = GetExchangeDocumentSelector();
            var virusScanResultProcessor = GetVirusScanResultProcessor();
            var documentDownloader = GetDocumentDownloader();
            var validationService = GetValidationService();
            var documentExchangeSummaries = GetDocumentExchangeSummaries();
            var productVersionService = GetProductVersionService();
            var documentDeletion = GetDocumentDeletion();

            return new ExchangeController(
                exchangeDocumentSelector,
                virusScanResultProcessor,
                documentDownloader,
                documentExchangeSummaries,
                productVersionService,
                Mapper,
                validationService,
                documentDeletion,
                CreateMockLoggerAdapter<ExchangeController>());
        }

        private IExchangeDocumentSelector GetExchangeDocumentSelector()
        {
            var agencyExchangeDocumentsService = GetAgencyExchangeDocumentsService();
            var organisationExchangeDocumentsService = GetOrganisationExchangeDocumentsService();
            var cosmosDbService = GetCosmosDbService();
            var cacheManager = GetCacheManager();
            var configurationDataService = new ConfigurationDataService(cosmosDbService, cacheManager, CreateMockLoggerAdapter<ConfigurationDataService>());
            var teamsLookup = new TeamsLookup(configurationDataService);
            var organisationsLookup = GetOrganisationsLookup();
            var productsLookup = new ProductsLookup(configurationDataService);
            var organisationSubtypesLookup = new OrganisationSubtypesLookup(cacheManager);
            var filtersFactory = new ExchangeDocumentFiltersFactory(productsLookup, configurationDataService, organisationSubtypesLookup);
            var filtersManager = GetFiltersManager(filtersFactory);
            var filterToListResultConverter = new FilterToListResultConverter<ExchangeDocument>(new PagingService());

            return new ExchangeDocumentSelector(
                agencyExchangeDocumentsService,
                organisationExchangeDocumentsService,
                teamsLookup,
                organisationsLookup,
                filtersManager,
                filterToListResultConverter,
                CreateMockLoggerAdapter<ExchangeDocumentSelector>());
        }

        private IAgencyExchangeDocumentsService GetAgencyExchangeDocumentsService()
        {
            var batchesService = GetBatchesService();
            var batchToExchangeDocumentConverter = GetBatchToExchangeDocumentConverter();
            var cacheManager = GetCacheManager();
            var configurationDataService = new ConfigurationDataService(
                GetCosmosDbService(),
                cacheManager,
                CreateMockLoggerAdapter<ConfigurationDataService>());

            return new AgencyExchangeDocumentsService(
                batchesService,
                batchToExchangeDocumentConverter,
                cacheManager,
                configurationDataService,
                CreateMockLoggerAdapter<AgencyExchangeDocumentsService>());
        }

        private IBatchesService GetBatchesService()
        {
            return new BatchesService(
                GetCosmosDbService(),
                CreateMockLoggerAdapter<BatchesService>());
        }

        private IBatchToExchangeDocumentConverter GetBatchToExchangeDocumentConverter()
        {
            var cosmosDbService = GetCosmosDbService();
            var cacheManager = GetCacheManager();
            var configurationDataService = new ConfigurationDataService(cosmosDbService, cacheManager, CreateMockLoggerAdapter<ConfigurationDataService>());
            var productsLookup = new ProductsLookup(configurationDataService);
            var organisationsLookup = GetOrganisationsLookup();
            var fileMetadataUserEncryptor = GetFileMetadataUserEncryptor();
            var logger = CreateMockLoggerAdapter<BatchToExchangeDocumentConverter>();
            return new BatchToExchangeDocumentConverter(productsLookup, organisationsLookup, fileMetadataUserEncryptor, Mapper, logger);
        }

        private IOrganisationExchangeDocumentsService GetOrganisationExchangeDocumentsService()
        {
            return new OrganisationExchangeDocumentsService(
                GetBatchesService(),
                GetBatchToExchangeDocumentConverter(),
                GetCacheManager(),
                GetOrganisationsLookup(),
                CreateMockLoggerAdapter<OrganisationExchangeDocumentsService>());
        }

        private IDocumentExchangeSummaries GetDocumentExchangeSummaries()
        {
            var agencyExchangeDocumentsService = GetAgencyExchangeDocumentsService();
            var organisationExchangeDocumentsService = GetOrganisationExchangeDocumentsService();
            var cosmosDbService = GetCosmosDbService();
            var cacheManager = GetCacheManager();
            var configurationDataService = new ConfigurationDataService(cosmosDbService, cacheManager, CreateMockLoggerAdapter<ConfigurationDataService>());
            var teamsLookup = new TeamsLookup(configurationDataService);

            return new DocumentExchangeSummaries(
                agencyExchangeDocumentsService,
                organisationExchangeDocumentsService,
                teamsLookup,
                configurationDataService);
        }

        private IProductVersionService GetProductVersionService()
        {
            return new ProductVersionService(
                GetCosmosDbService(),
                new AcademicYearCalculator(SystemProvider));
        }

        private IDocumentDeletion GetDocumentDeletion()
        {
            return new DocumentDeletion(
                 GetCosmosDbService(),
                 GetDirectoriesManager(),
                 SystemProvider,
                 GetFileMetadataUserEncryptor(),
                 GetBatchToExchangeDocumentConverter(),
                 Mapper,
                 new VirusScanResultProcessorConfiguration
                 {
                     FileCopyDestinationDirectoryKey = MockConfig["BlobContainers:ContainerName"]
                 },
                 CreateMockLoggerAdapter<DocumentDeletion>());
        }

        private Dictionary<string, byte[]> UnzipFilesFromResult(ActionResult<byte[]> result)
        {
            var objResult = (ObjectResult)result.Result;
            var zipBytes = (byte[])objResult.Value;

            var actualUnzippedFiles = new Dictionary<string, byte[]>();

            using (var memoryStream = new MemoryStream(zipBytes))
            using (var zipArchive = new ZipArchive(memoryStream))
            {
                foreach (ZipArchiveEntry entry in zipArchive.Entries)
                {
                    using var entryMemoryStream = new MemoryStream();
                    var entryStream = entry.Open();
                    entryStream.CopyTo(entryMemoryStream);

                    byte[] currentUnzippedFile = entryMemoryStream.ToArray();
                    actualUnzippedFiles.Add(entry.FullName, currentUnzippedFile);
                }
            }

            return actualUnzippedFiles;
        }
    }
}