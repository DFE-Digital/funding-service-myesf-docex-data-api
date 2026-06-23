using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pds.Core.Common.Organisation.Enums;
using Pds.Core.Common.Organisation.Models;
using Pds.Core.Logging;
using Pds.DocumentExchange.Data.Api.Controllers;
using Pds.DocumentExchange.Data.Services.Interfaces.FDS;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Api.Tests.Unit
{
    [TestClass]
    public class OrganisationControllerTests
    {
        private readonly Mock<IOrganisationService> _organisationService = new Mock<IOrganisationService>(MockBehavior.Strict);
        private readonly Mock<ILoggerAdapter<OrganisationController>> _logger = new Mock<ILoggerAdapter<OrganisationController>>(MockBehavior.Loose);

        private readonly OrganisationController _organisationController;

        public OrganisationControllerTests()
        {
            _organisationController = new OrganisationController(_organisationService.Object, _logger.Object);
        }

        [TestMethod]
        public async Task Search_Organisation_By_Name_ReturnOK()
        {
            //Arrange
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
                            OrganisationSubType = "test"
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
                            OrganisationSubType = "test"
                        }
                    }
            };

            _organisationService
                .Setup(x => x.GetOrganisationByName(It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(new[] { organisation });

            var expected = new[] { organisation };

            // Act
            var result = await _organisationController.Search("school", 1);

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>()
                    .Which.Value.Should().BeEquivalentTo(expected);
            Mock.VerifyAll(_logger, _organisationService);
        }

        [TestMethod]
        public async Task Search_Organisation_By_Name_Return_null()
        {
            //Arrange
            _organisationService
                 .Setup(x => x.GetOrganisationByName(It.IsAny<string>(), It.IsAny<int>()))
                 .ReturnsAsync((IEnumerable<Organisation>)null);

            var expected = (IEnumerable<Organisation>)null;

            // Act
            var result = await _organisationController.Search("school", 1);

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>()
                    .Which.Value.Should().BeEquivalentTo(expected);
            Mock.VerifyAll(_logger, _organisationService);
        }

        [TestMethod]
        public async Task Get_Organisation_By_Ukprn_ReturnOK()
        {
            //Arrange
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
            var result = await _organisationController.GetOrganisation("test");

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>()
                    .Which.Value.Should().BeEquivalentTo(organisation);
            Mock.VerifyAll(_logger, _organisationService);
        }

        [TestMethod]
        public async Task Get_Organisation_By_Ukprn_Return_Null()
        {
            //Arrange
            _organisationService
                .Setup(x => x.GetOrganisation(It.IsAny<string>()))
                .ReturnsAsync((Organisation)null);

            // Act
            var result = await _organisationController.GetOrganisation("test");

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>()
                    .Which.Value.Should().BeEquivalentTo((Organisation)null);
            Mock.VerifyAll(_logger, _organisationService);
        }
    }
}
