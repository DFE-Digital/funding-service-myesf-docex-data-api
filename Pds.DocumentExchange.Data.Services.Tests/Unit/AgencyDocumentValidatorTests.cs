using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pds.Core.Common.Organisation.Enums;
using Pds.Core.Common.Organisation.Models;
using Pds.DocumentExchange.Data.Services.DTOs;
using Pds.DocumentExchange.Data.Services.Enums;
using Pds.DocumentExchange.Data.Services.Implementations;
using Pds.DocumentExchange.Data.Services.Interfaces.Lookup;
using Pds.DocumentExchange.Data.Services.Interfaces.Settings;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Tests.Unit
{
    [TestClass]
    public class AgencyDocumentValidatorTests
    {
        private readonly Mock<IProductsLookup> _productsLookup = new Mock<IProductsLookup>(MockBehavior.Strict);
        private readonly Mock<IConfigurationDataService> _configurationDataService = new Mock<IConfigurationDataService>(MockBehavior.Strict);

        private readonly AgencyDocumentValidator _agencyDocumentValidator;

        public AgencyDocumentValidatorTests()
        {
            _agencyDocumentValidator = new AgencyDocumentValidator(
                _productsLookup.Object,
                _configurationDataService.Object);
        }

        [TestMethod, TestCategory("Unit")]
        [DataRow(null)]
        [DataRow("")]
        [DataRow(" ")]
        public async Task ValidateDocument_WhenTeamIsEmpty_ThrowArgumentNullException(string team)
        {
            // Act
            Func<Task<AgencyDocumentErrorType>> func = () => _agencyDocumentValidator.ValidateDocument(team, "any-file-name.pdf", GetOrganisations());

            // Assert
            await func.Should().ThrowAsync<ArgumentNullException>();
        }

        [TestMethod, TestCategory("Unit")]
        [DataRow(null)]
        [DataRow("")]
        [DataRow(" ")]
        public async Task ValidateDocument_WhenFileNameIsEmpty_ReturnsDocumentNameInvalidFormat(string fileName)
        {
            // Act
            var result = await _agencyDocumentValidator.ValidateDocument("any-team", fileName, GetOrganisations());

            // Assert
            result.Should().Be(AgencyDocumentErrorType.DocumentNameInvalidFormat);
        }

        [TestMethod, TestCategory("Unit")]
        [DataRow("filename*.pdf")]
        [DataRow("file-name.pdf")]
        [DataRow("filename@#.pdf")]
        public async Task ValidateDocument_WhenFileNameContainsInvalidChars_ReturnsDocumentNameContainsInvalidCharacters(string fileName)
        {
            // Act
            var result = await _agencyDocumentValidator.ValidateDocument("any-team", fileName, GetOrganisations());

            // Assert
            result.Should().Be(AgencyDocumentErrorType.DocumentNameContainsInvalidCharacters);
        }

        [TestMethod, TestCategory("Unit")]
        [DataRow("12345678.pdf")]
        [DataRow("12345678_10001.pdf")]
        [DataRow("12345678__202021.pdf")]
        [DataRow("12345678_202021_.pdf")]
        [DataRow("12345678_202021_202021_extra.pdf")]
        public async Task ValidateDocument_WhenFileNameDoesntContainThreeParts_ReturnsDocumentNameInvalidFormat(string fileName)
        {
            // Act
            var result = await _agencyDocumentValidator.ValidateDocument("any-team", fileName, GetOrganisations());

            // Assert
            result.Should().Be(AgencyDocumentErrorType.DocumentNameInvalidFormat);
        }

        [TestMethod, TestCategory("Unit")]
        [DataRow("1234_10001_202021.pdf")]
        [DataRow("1234567_10001_202021.pdf")]
        public async Task ValidateDocument_WhenOrganisationIdHasInvalidFormat_ReturnsOrganisationIdentifierInvalidFormat(string fileName)
        {
            // Act
            var result = await _agencyDocumentValidator.ValidateDocument("any-team", fileName, GetOrganisations());

            // Assert
            result.Should().Be(AgencyDocumentErrorType.OrganisationIdentifierInvalidFormat);
        }

        [TestMethod, TestCategory("Unit")]
        public async Task ValidateDocument_WhenOrganisationIdDoesntExist_ReturnsOrganisationIdentifierNotRecognised()
        {
            // Arrange
            var organisationId = "12345678";
            var fileName = $"{organisationId}_10001_202021.pdf";

            // Act
            var result = await _agencyDocumentValidator.ValidateDocument("any-team", fileName, GetOrganisations());

            // Assert
            result.Should().Be(AgencyDocumentErrorType.OrganisationIdentifierNotRecognised);
        }

        [TestMethod, TestCategory("Unit")]
        [DataRow("12345678_product_202021.pdf")]
        [DataRow("12345678_12345ABC_202021.pdf")]
        public async Task ValidateDocument_WhenProductIdHasInvalidFormat_ReturnsProductIdentifierInvalidFormat(string fileName)
        {
            // Arrange
            _productsLookup
                .Setup(lookup => lookup.Exists(It.IsAny<string>()))
                .ReturnsAsync(false);

            // Act
            var result = await _agencyDocumentValidator.ValidateDocument("any-team", fileName, GetOrganisations("12345678"));

            // Assert
            result.Should().Be(AgencyDocumentErrorType.ProductIdentifierInvalidFormat);
        }

        [TestMethod, TestCategory("Unit")]
        public async Task ValidateDocument_WhenProductIdDoesntExist_ReturnsProductIdentifierInvalidFormat()
        {
            // Arrange
            var productId = "10001";
            var fileName = $"12345678_{productId}_202021.pdf";

            _productsLookup
                .Setup(productsLookup => productsLookup.Exists(productId))
                .ReturnsAsync(false);

            // Act
            var result = await _agencyDocumentValidator.ValidateDocument("any-team", fileName, GetOrganisations("12345678"));

            // Assert
            result.Should().Be(AgencyDocumentErrorType.ProductIdentifierInvalidFormat);
        }

        [TestMethod, TestCategory("Unit")]
        [DataRow("12345678_10003_20192X.docx")]
        [DataRow("12345678_10003_XVII.xlsx")]
        public async Task ValidateDocument_WhenAcademicYearFormatIsIncorrect_ReturnsAcademicYearInvalidFormat(string fileName)
        {
            // Arrange
            _productsLookup
                .Setup(productsLookup => productsLookup.Exists(It.IsAny<string>()))
                .ReturnsAsync(true);

            // Act
            var result = await _agencyDocumentValidator.ValidateDocument("any-team", fileName, GetOrganisations("12345678"));

            // Assert
            result.Should().Be(AgencyDocumentErrorType.AcademicYearInvalidFormat);
        }

        [TestMethod, TestCategory("Unit")]
        [DataRow("12345678_10003_201920")]
        [DataRow("12345678_10003_201516.")]
        public async Task ValidateDocument_WhenFileExtensionIsEmpty_ReturnsDocumentExtensionNotAccepted(string fileName)
        {
            // Arrange
            _productsLookup
                .Setup(productsLookup => productsLookup.Exists(It.IsAny<string>()))
                .ReturnsAsync(true);

            // Act
            var result = await _agencyDocumentValidator.ValidateDocument("any-team", fileName, GetOrganisations("12345678"));

            // Assert
            result.Should().Be(AgencyDocumentErrorType.DocumentExtensionNotAccepted);
        }

        [TestMethod, TestCategory("Unit")]
        [DataRow("12345678_10003_201920.exe")]
        [DataRow("12345678_10003_201516.zip")]
        public async Task ValidateDocument_WhenFileExtensionNotAccepted_ReturnsDocumentExtensionNotAccepted(string fileName)
        {
            // Arrange
            _productsLookup
                .Setup(productsLookup => productsLookup.Exists(It.IsAny<string>()))
                .ReturnsAsync(true);

            _configurationDataService
               .Setup(config => config.GetFileExtensions())
               .ReturnsAsync(allowedExtensions);

            // Act
            var result = await _agencyDocumentValidator.ValidateDocument("any-team", fileName, GetOrganisations("12345678"));

            // Assert
            result.Should().Be(AgencyDocumentErrorType.DocumentExtensionNotAccepted);
        }

        [TestMethod, TestCategory("Unit")]
        public async Task ValidateDocument_WhenTeamIsNotAllowedToPublish_ReturnsTeamNotAuthorisedToPublish()
        {
            // Arrange
            var team = "not-allowed-to-publish-team";
            var fileName = "12345678_10003_201920.docx";

            _productsLookup
                .Setup(productsLookup => productsLookup.Exists(It.IsAny<string>()))
                .ReturnsAsync(true);

            _configurationDataService
               .Setup(config => config.GetFileExtensions())
               .ReturnsAsync(allowedExtensions);

            _productsLookup
               .Setup(productsLookup => productsLookup.Get(It.IsAny<string>()))
               .ReturnsAsync((string productId) => new Product
               {
                   AgencyTeams = new[] { "the-team-allowed-to-publish-this-product" },
                   Identifier = int.Parse(productId),
                   Name = "the-product-name"
               });

            // Act
            var result = await _agencyDocumentValidator.ValidateDocument(team, fileName, GetOrganisations("12345678"));

            // Assert
            result.Should().Be(AgencyDocumentErrorType.TeamNotAuthorisedToPublish);
        }

        [TestMethod, TestCategory("Unit")]
        public async Task ValidateDocument_WhenNoErrorFound_ReturnsNoError()
        {
            // Arrange
            var team = "the-team-allowed-to-publish";
            var fileName = "12345678_10003_201920.docx";

            ////_organisationsLookup
            ////    .Setup(organisationLookup => organisationLookup.Exists(It.IsAny<OrganisationIdentifier>()))
            ////    .ReturnsAsync(true);

            _productsLookup
                .Setup(productsLookup => productsLookup.Exists(It.IsAny<string>()))
                .ReturnsAsync(true);

            _configurationDataService
               .Setup(config => config.GetFileExtensions())
               .ReturnsAsync(allowedExtensions);

            _productsLookup
               .Setup(productsLookup => productsLookup.Get(It.IsAny<string>()))
               .ReturnsAsync((string productId) => new Product
               {
                   AgencyTeams = new[] { team },
                   Identifier = int.Parse(productId),
                   Name = "the-product-name"
               });

            // Act
            var result = await _agencyDocumentValidator.ValidateDocument(team, fileName, GetOrganisations("12345678"));

            // Assert
            result.Should().Be(AgencyDocumentErrorType.NoError);
        }

        private readonly FileExtensionInfo[] allowedExtensions = new FileExtensionInfo[]
        {
            new FileExtensionInfo
            {
                Extension = "docx",
                CanInternalUserUpload = true
            },
            new FileExtensionInfo
            {
                Extension = "pdf",
                CanInternalUserUpload = true
            },
            new FileExtensionInfo
            {
                Extension = "zip",
                CanInternalUserUpload = false
            }
        };

        private IDictionary<string, Organisation> GetOrganisations(string ukprn = null)
        {
            var organisations = new Dictionary<string, Organisation>()
                    {
                        {
                          "87654321",
                          new Organisation
                          {
                              Name = $"test-organisation-87654321",
                              Identifiers = new[] { new OrganisationIdentifier() { Type = OrganisationIdentifierType.Ukprn, Value = "87654321" } }
                          }
                        },
                        {
                          "11223344",
                          new Organisation
                          {
                              Name = $"test-organisation-11223344",
                              Identifiers = new[] { new OrganisationIdentifier() { Type = OrganisationIdentifierType.Ukprn, Value = "11223344" } }
                          }
                        },
                        {
                          "99999999",
                          new Organisation
                          {
                              Name = $"test-organisation-99999999",
                              Identifiers = new[] { new OrganisationIdentifier() { Type = OrganisationIdentifierType.Ukprn, Value = "99999999" } }
                          }
                        }
                    };

            if (!string.IsNullOrEmpty(ukprn))
            {
                organisations.Add(
                    ukprn,
                    new Organisation
                    {
                        Name = $"test-organisation-{ukprn}",
                        Identifiers = new[] { new OrganisationIdentifier() { Type = OrganisationIdentifierType.Ukprn, Value = ukprn } }
                    });
            }

            return organisations;
        }
    }
}