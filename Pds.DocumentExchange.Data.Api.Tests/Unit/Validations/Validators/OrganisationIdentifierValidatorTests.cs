using FluentValidation.TestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pds.DocumentExchange.Data.Api.Enums;
using Pds.DocumentExchange.Data.Api.Models;
using Pds.DocumentExchange.Data.Api.Validations.ModelValidators;

namespace Pds.DocumentExchange.Data.Api.Tests.Unit.Validations.Validators
{
    [TestClass]
    public class OrganisationIdentifierValidatorTests
    {
        private readonly OrganisationIdentifierValidator _validator = new OrganisationIdentifierValidator();

        [TestMethod, TestCategory("Unit")]
        [DataRow(null)]
        [DataRow("")]
        [DataRow(" ")]
        public void Validate_WhenValueIsNullOrEmpty_ShouldHaveValidationError(string value)
        {
            // Assert
            _validator.ShouldHaveValidationErrorFor(organisationId => organisationId.Value, value);
        }

        [TestMethod, TestCategory("Unit")]
        public void Validate_WhenValueIsNotEmpty_ShouldNotHaveValidationError()
        {
            // Arrange
            var organisationIdentifier = new OrganisationIdentifier
            {
                Type = OrganisationIdentifierType.Ukprn,
                Value = "12345678"
            };

            // Act
            var result = _validator.TestValidate(organisationIdentifier);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}