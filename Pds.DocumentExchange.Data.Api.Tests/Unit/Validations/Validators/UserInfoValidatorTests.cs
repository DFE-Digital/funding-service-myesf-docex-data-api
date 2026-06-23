using FluentValidation.TestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pds.DocumentExchange.Data.Api.Models;
using Pds.DocumentExchange.Data.Api.Validations.ModelValidators;

namespace Pds.DocumentExchange.Data.Api.Tests.Unit.Validations.Validators
{
    [TestClass]
    public class UserInfoValidatorTests
    {
        private readonly UserInfoValidator _validator = new UserInfoValidator();

        [TestMethod, TestCategory("Unit")]
        [DataRow(null)]
        [DataRow("")]
        [DataRow(" ")]
        public void Validate_WhenPrincipalIsNullOrEmpty_ShouldHaveValidationError(string principal)
        {
            // Assert
            _validator.ShouldHaveValidationErrorFor(userInfo => userInfo.Principal, principal);
        }

        [TestMethod, TestCategory("Unit")]
        public void Validate_WhenOrganisationInfoIsNull_ShouldHaveValidationError()
        {
            // Assert
            _validator.ShouldHaveValidationErrorFor(userInfo => userInfo.OrganisationInfo, null as OrganisationInfo);
        }

        [TestMethod, TestCategory("Unit")]
        public void Validate_WhenOrganisationInfoIsInvalid_ShouldHaveValidationError()
        {
            // Arrange
            var invalidOrganisationInfo = new OrganisationInfo
            {
                Name = "invalid-organisation",
                OrganisationIdentifier = null
            };

            var userInfo = new UserInfo
            {
                OrganisationInfo = invalidOrganisationInfo
            };

            // Act
            var result = _validator.TestValidate(userInfo);

            // Assert
            result.ShouldHaveValidationErrorFor($"{nameof(userInfo.OrganisationInfo)}.{nameof(invalidOrganisationInfo.OrganisationIdentifier)}");
        }

        [TestMethod, TestCategory("Unit")]
        public void Validate_WhenUserInfoIsValid_ShouldNotHaveValidationError()
        {
            // Arrange
            var userInfo = new UserInfo
            {
                Principal = "user-principal",
                FullName = "user-full-name",
                EmailAddress = "user-email@education.gov.uk",
                OrganisationInfo = new OrganisationInfo
                {
                    Name = "user-organisation-info-name",
                    OrganisationIdentifier = new OrganisationIdentifier
                    {
                        Type = Enums.OrganisationIdentifierType.Ukprn,
                        Value = "user-organisation-id"
                    }
                }
            };

            // Act
            var result = _validator.TestValidate(userInfo);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}