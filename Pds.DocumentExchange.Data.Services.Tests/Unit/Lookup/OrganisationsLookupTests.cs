using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pds.Core.Caching.Interfaces;
using Pds.Core.Caching.Models;
using Pds.Core.Common.Organisation.Enums;
using Pds.Core.Common.Organisation.Models;
using Pds.Core.Logging;
using Pds.DocumentExchange.Data.Services.DTOs.User;
using Pds.DocumentExchange.Data.Services.Implementations.Lookup;
using Pds.DocumentExchange.Data.Services.Interfaces.Caching;
using Pds.DocumentExchange.Data.Services.Interfaces.FDS;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Tests.Unit.Lookup
{
    [TestClass]
    [TestCategory("Unit")]
    public class OrganisationsLookupTests
    {
        private readonly Mock<ILoggerAdapter<OrganisationsLookup>> _mockLoggingService = new Mock<ILoggerAdapter<OrganisationsLookup>>(MockBehavior.Loose);
        private readonly Mock<IOrganisationService> _organisationService = new Mock<IOrganisationService>(MockBehavior.Strict);
        private readonly OrganisationsLookup _organisationsLookup;

        private readonly Mock<ICacheManager> _mockCacheManager = new Mock<ICacheManager>(MockBehavior.Strict);
        private readonly Mock<ICacheService> _mockCacheService = new Mock<ICacheService>(MockBehavior.Strict);
        private readonly Mock<ICacheOptionsProvider> _mockCacheOptionsProvider = new Mock<ICacheOptionsProvider>(MockBehavior.Strict);
        private readonly Mock<ICacheKeyBuilder> _mockCacheKeyBuilder = new Mock<ICacheKeyBuilder>(MockBehavior.Strict);

        public OrganisationsLookupTests()
        {
            _organisationsLookup = new OrganisationsLookup(
                _organisationService.Object,
                _mockLoggingService.Object,
                _mockCacheManager.Object);
        }


        #region OrganisationExists

        [TestMethod]
        public void OrganisationExists_ForNullParam_Throws()
        {
            // Arrange / Act
            Func<Task> act = async () => await _organisationsLookup.Exists(null);

            // Assert
            act.Should().ThrowAsync<ArgumentNullException>();
        }

        [TestMethod]
        public async Task OrganisationExists_WhenOrganisationExists_ReturnsTrue()
        {
            // Arrange
            var organisationId = new OrganisationIdentifier
            {
                Type = OrganisationIdentifierType.Ukprn,
                Value = "12345678"
            };

            var organisation = new Organisation()
            {
                Identifiers = new List<OrganisationIdentifier>()
                        {
                            new OrganisationIdentifier
                            {
                                Type = OrganisationIdentifierType.Ukprn,
                                Value = "12345678"
                            }
                        },
                Name = "test",
                OrganisationType = "test",
                OrganisationSubType = "test",
                Status = "Open",
                ParentOrganisation = null,
                ChildOrganisations = null
            };

            _organisationService
                .Setup(x => x.GetOrganisation(It.IsAny<string>()))
                .ReturnsAsync(organisation);

            // Act
            var result = await _organisationsLookup.Exists(organisationId);

            // Assert
            result.Should().BeTrue();

            Mock.VerifyAll(_mockLoggingService, _organisationService);
        }

        [TestMethod]
        public async Task OrganisationExists_WhenOrganisationDoesNotExist_ReturnsFalse()
        {
            // Arrange
            var organisationId = new OrganisationIdentifier
            {
                Type = OrganisationIdentifierType.Ukprn,
                Value = "12345678"
            };

            _organisationService
                .Setup(x => x.GetOrganisation(It.IsAny<string>()))
                .ReturnsAsync((Organisation)null);

            // Act
            var result = await _organisationsLookup.Exists(organisationId);

            // Assert
            result.Should().BeFalse();

            Mock.VerifyAll(_mockLoggingService, _organisationService);
        }

        #endregion


        #region Get

        [TestMethod]
        public void Get_ForNullParam_Throws()
        {
            // Arrange / Act
            Func<Task> act = async () => await _organisationsLookup.Get(null);

            // Assert
            act.Should().ThrowAsync<ArgumentNullException>();
        }

        [TestMethod]
        public async Task Get_WhenOrganisationExists_ReturnsOrganisation()
        {
            // Arrange
            var organisationId = new OrganisationIdentifier
            {
                Type = OrganisationIdentifierType.Ukprn,
                Value = "12345678"
            };

            var organisation = new Organisation()
            {
                Identifiers = new List<OrganisationIdentifier>()
                        {
                            new OrganisationIdentifier
                            {
                                Type = OrganisationIdentifierType.Ukprn,
                                Value = "12345678"
                            }
                        },
                Name = "test",
                OrganisationType = "test",
                OrganisationSubType = "test",
                Status = "Open",
                ParentOrganisation = null,
                ChildOrganisations = null
            };

            _organisationService
                .Setup(x => x.GetOrganisation(It.IsAny<string>()))
                .ReturnsAsync(organisation);

            // Act
            var result = await _organisationsLookup.Get(organisationId);

            // Assert
            result.Should().BeEquivalentTo(organisation);

            Mock.VerifyAll(_mockLoggingService, _organisationService);
        }

        [TestMethod]
        public async Task Get_WhenOrganisationDoesNotExist_ReturnsUnknownOrganisation()
        {
            // Arrange
            var organisationId = new OrganisationIdentifier
            {
                Type = OrganisationIdentifierType.Ukprn,
                Value = "12345678"
            };

            _organisationService
                .Setup(x => x.GetOrganisation(It.IsAny<string>()))
                .ReturnsAsync((Organisation)null);

            // Act
            var result = await _organisationsLookup.Get(organisationId);

            // Assert
            result.Should().BeOfType<UnknownOrganisation>();

            Mock.VerifyAll(_mockLoggingService, _organisationService);
        }

        [TestMethod]
        public void Get_ThrowsException()
        {
            // Arrange
            var organisationId = new OrganisationIdentifier
            {
                Type = OrganisationIdentifierType.Ukprn,
                Value = "12345678"
            };

            _organisationService
                .Setup(x => x.GetOrganisation(It.IsAny<string>()))
                .ThrowsAsync(new HttpRequestException("InternalServerError"));

            // Act
            Func<Task> act = async () => await _organisationsLookup.Get(organisationId);

            // Assert
            act.Should().ThrowAsync<HttpRequestException>();

            Mock.VerifyAll(_mockLoggingService, _organisationService);
        }

        #endregion


        #region GetSelfAndChildUkprns

        [TestMethod]
        public void GetSelfAndChildUkprns_ForNullParam_Throws()
        {
            // Arrange / Act
            Func<Task> act = async () => await _organisationsLookup.GetSelfAndChildUkprns(null);

            // Assert
            act.Should().ThrowAsync<ArgumentNullException>();
        }

        [TestMethod]
        public async Task GetSelfAndChildUkprns_WhenOrganisationExistsAndHasNoChildren_ReturnsOrganisationIdentifier()
        {
            // Arrange
            var organisationId = new OrganisationIdentifier
            {
                Type = OrganisationIdentifierType.Ukprn,
                Value = "12345678"
            };

            var organisation = new Organisation()
            {
                Identifiers = new List<OrganisationIdentifier>()
                        {
                            new OrganisationIdentifier
                            {
                                Type = OrganisationIdentifierType.Ukprn,
                                Value = "12345678"
                            }
                        },
                Name = "test",
                OrganisationType = "test",
                OrganisationSubType = "test",
                Status = "Open",
                ParentOrganisation = null,
                ChildOrganisations = null
            };

            _organisationService
                .Setup(x => x.GetOrganisation(It.IsAny<string>()))
                .ReturnsAsync(organisation);

            // Act
            var result = await _organisationsLookup.GetSelfAndChildUkprns(organisationId);

            // Assert
            result.Should().BeEquivalentTo(new List<OrganisationIdentifier> { organisationId });

            Mock.VerifyAll(_mockLoggingService, _organisationService);
        }

        [TestMethod]
        public async Task GetSelfAndChildUkprns_WhenOrganisationExistsAndHasChildren_ReturnsOrganisationAndChildrenIdentifier()
        {
            // Arrange
            var organisationId = new OrganisationIdentifier
            {
                Type = OrganisationIdentifierType.Ukprn,
                Value = "12345678"
            };

            var childOrganisationId = new OrganisationIdentifier
            {
                Type = OrganisationIdentifierType.Ukprn,
                Value = "99999999"
            };

            var childOrganisation = new Organisation
            {
                Identifiers = new[] { childOrganisationId },
                Name = "child-organisation-name",
                OrganisationType = "child-organisation-type",
                OrganisationSubType = "child-organisation-subtype",
                Status = "Open",
                ParentOrganisation = null,
                ChildOrganisations = null
            };

            var organisation = new Organisation
            {
                Identifiers = new[] { organisationId },
                Name = "organisation-name",
                OrganisationType = "organisation-type",
                OrganisationSubType = "organisation-subtype",
                Status = "Open",
                ParentOrganisation = null,
                ChildOrganisations = new[] { childOrganisation }
            };

            var response = new Organisation()
            {
                Identifiers = new List<OrganisationIdentifier>()
                        {
                            new OrganisationIdentifier
                            {
                                Type = OrganisationIdentifierType.Ukprn,
                                Value = "12345678"
                            }
                        },
                Name = "organisation-name",
                OrganisationType = "organisation-type",
                OrganisationSubType = "organisation-subtype",
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
                                Value = "99999999"
                            }
                        },
                        Name = "child-organisation-name",
                        Status = "Open",
                        OrganisationType = "child-organisation-type",
                        OrganisationSubType = "child-organisation-subtype"
                    }
                }
            };

            _organisationService
                .Setup(x => x.GetOrganisation(It.IsAny<string>()))
                .ReturnsAsync(response);

            // Act
            var result = await _organisationsLookup.GetSelfAndChildUkprns(organisationId);

            // Assert
            result.Should()
                .BeEquivalentTo(
                    new List<OrganisationIdentifier>
                    {
                        organisationId,
                        childOrganisationId
                    });

            Mock.VerifyAll(_mockLoggingService, _organisationService);
        }

        #endregion


        #region GetAllOrganisation

        [TestMethod]
        public async Task GetAllOrganisation_ReturnsOrganisations()
        {
            // Arrange
            var expectedResult = new Dictionary<string, Organisation>() { { "ukprn1", new Organisation() }, { "ukprn2", new Organisation() } };
            SetupMockCacheObjects();
            _mockCacheService.Setup(x => x.Get("mockCacheKey", It.IsAny<Func<Task<IDictionary<string, Organisation>>>>(), It.IsAny<CacheOptions>())).ReturnsAsync(expectedResult);

            // Act
            var response = await _organisationsLookup.GetAllOrganisations();

            // Assert
            response.Should().HaveCount(2);

            Mock.VerifyAll(_mockLoggingService, _organisationService);
        }

        [TestMethod]
        public async Task GetAllOrganisation_ReturnsNull()
        {
            // Arrange
            SetupMockCacheObjects();
            _mockCacheService.Setup(x => x.Get("mockCacheKey", It.IsAny<Func<Task<IDictionary<string, Organisation>>>>(), It.IsAny<CacheOptions>())).ReturnsAsync((IDictionary<string, Organisation>)null);

            // Act
            var response = await _organisationsLookup.GetAllOrganisations();

            // Assert
            response.Should().BeNull();

            Mock.VerifyAll(_mockLoggingService, _organisationService);
        }

        [TestMethod]
        public void GetAllOrganisation_ThrowsException()
        {
            // Arrange
            SetupMockCacheObjects();
            _mockCacheService.Setup(x => x.Get(It.IsAny<string>(), It.IsAny<Func<Task<IDictionary<string, Organisation>>>>(), It.IsAny<CacheOptions>())).ThrowsAsync(new HttpRequestException("InternalServerError"));

            // Act
            Func<Task> act = async () => await _organisationsLookup.GetAllOrganisations();

            // Assert
            act.Should().ThrowAsync<HttpRequestException>();

            Mock.VerifyAll(_mockLoggingService, _organisationService);
        }

        #endregion

        private void SetupMockCacheObjects()
        {
            _mockCacheManager.SetupGet(x => x.CacheService).Returns(_mockCacheService.Object);
            _mockCacheManager.SetupGet(x => x.CacheOptionsProvider).Returns(_mockCacheOptionsProvider.Object);
            _mockCacheManager.SetupGet(x => x.CacheKeyBuilder).Returns(_mockCacheKeyBuilder.Object);

            var mockCacheOptions = new CacheOptions
            {
                CacheNullData = false,
                MemoryCacheTTL = TimeSpan.FromSeconds(new Random().Next(1, 100))
            };

            _mockCacheKeyBuilder.Setup(x => x.BuildAllOrganisationsKey()).Returns("mockCacheKey");
            _mockCacheOptionsProvider.SetupGet(x => x.AllOrganisations).Returns(mockCacheOptions);
        }
    }
}