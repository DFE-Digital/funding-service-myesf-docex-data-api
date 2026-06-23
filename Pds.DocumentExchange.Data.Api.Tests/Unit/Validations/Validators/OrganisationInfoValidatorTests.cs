using FluentValidation.TestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pds.DocumentExchange.Data.Api.Models;
using Pds.DocumentExchange.Data.Api.Validations.ModelValidators;

namespace Pds.DocumentExchange.Data.Api.Tests.Unit.Validations.Validators
{
    [TestClass]
    public class OrganisationInfoValidatorTests
    {
        private readonly OrganisationInfoValidator _validator = new OrganisationInfoValidator();

        [TestMethod, TestCategory("Unit")]
        public void Validate_WhenOrganisationIdentifierIsNull_ShouldHaveValidationError()
        {
            // Assert
            _validator.ShouldHaveValidationErrorFor(
                organisationInfo => organisationInfo.OrganisationIdentifier,
                null as OrganisationIdentifier);
        }

        [TestMethod, TestCategory("Unit")]
        public void Validate_WhenOrganisationIdentifierIsInvalid_ShouldHaveValidationError()
        {
            // Arrange
            var invalidOrganisationIdentifier = new OrganisationIdentifier
            {
                Type = Enums.OrganisationIdentifierType.Ukprn,
                Value = string.Empty
            };

            var organisationInfo = new OrganisationInfo
            {
                OrganisationIdentifier = invalidOrganisationIdentifier
            };

            // Act
            var result = _validator.TestValidate(organisationInfo);

            // Assert
            result.ShouldHaveValidationErrorFor($"{nameof(organisationInfo.OrganisationIdentifier)}.{nameof(invalidOrganisationIdentifier.Value)}");
        }

        [TestMethod, TestCategory("Unit")]
        public void Validate_WhenOrganisationInfoIsValid_ShouldNotHaveValidationError()
        {
            // Arrange
            var organisationInfo = new OrganisationInfo
            {
                OrganisationIdentifier = new OrganisationIdentifier
                {
                    Type = Enums.OrganisationIdentifierType.Ukprn,
                    Value = "user-organisation-id"
                }
            };

            // Act
            var result = _validator.TestValidate(organisationInfo);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}