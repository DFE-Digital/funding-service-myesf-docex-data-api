using FluentValidation.TestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pds.DocumentExchange.Data.Api.Models;
using Pds.DocumentExchange.Data.Api.Validations.ModelValidators;

namespace Pds.DocumentExchange.Data.Api.Tests.Unit.Validations.Validators
{
    [TestClass]
    public class AgencyPublishRequestValidatorTests
    {
        private readonly AgencyPublishRequestValidator _validator = new AgencyPublishRequestValidator();

        [TestMethod, TestCategory("Unit")]
        public void Validate_WhenUserInfoIsNull_ShouldHaveValidationError()
        {
            // Assert
            _validator.ShouldHaveValidationErrorFor(
                agencyPublishRequest => agencyPublishRequest.UserInfo,
                null as UserInfo);
        }

        [TestMethod, TestCategory("Unit")]
        [DataRow(null)]
        [DataRow("")]
        [DataRow(" ")]
        public void Validate_WhenUserPrincipalIsNullOrEmpty_ShouldHaveValidationError(string principal)
        {
            // Arrange
            var invalidUserInfo = new UserInfo
            {
                Principal = principal,
                FullName = "user-full-name",
                EmailAddress = "user-email@education@gov.uk"
            };

            var agencyPublishRequest = new AgencyPublishRequest
            {
                UserInfo = invalidUserInfo
            };

            // Act
            var result = _validator.TestValidate(agencyPublishRequest);

            // Assert
            result.ShouldHaveValidationErrorFor($"{nameof(agencyPublishRequest.UserInfo)}.{nameof(invalidUserInfo.Principal)}");
        }

        [TestMethod, TestCategory("Unit")]
        public void Validate_AgencyPublishRequestIsValid_ShouldNotHaveValidationError()
        {
            // Arrange
            var userInfo = new UserInfo
            {
                Principal = "user-principal"
            };

            var agencyPublishRequest = new AgencyPublishRequest
            {
                UserInfo = userInfo
            };

            // Act
            var result = _validator.TestValidate(agencyPublishRequest);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}