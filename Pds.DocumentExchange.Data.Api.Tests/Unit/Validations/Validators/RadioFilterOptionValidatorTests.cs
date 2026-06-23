using FluentValidation.TestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pds.DocumentExchange.Data.Api.Enums;
using Pds.DocumentExchange.Data.Api.Models.Filters;
using Pds.DocumentExchange.Data.Api.Validations.ModelValidators;
using Pds.DocumentExchange.Data.Services.Enums;

namespace Pds.DocumentExchange.Data.Api.Tests.Unit.Validations.Validators
{
    [TestClass]
    public class RadioFilterOptionValidatorTests
    {
        private readonly RadioFilterOptionValidator _validator = new RadioFilterOptionValidator();

        [TestMethod, TestCategory("Unit")]
        public void Validate_WhenFilterOptionTypeIsInvalid_ShouldHaveValidationError()
        {
            // Arrange
            var invalidFilterOptionType = "invalid-filter-option-type";

            // Assert
            _validator.ShouldHaveValidationErrorFor(filterOption => filterOption.Type, invalidFilterOptionType);
        }

        [TestMethod, TestCategory("Unit")]
        public void Validate_WhenFilterKeyIsInvalid_ShouldHaveValidationError()
        {
            // Arrange
            var invalidKey = "invalid-filter-key";

            // Assert
            _validator.ShouldHaveValidationErrorFor(filterOption => filterOption.Key, invalidKey);
        }

        [TestMethod, TestCategory("Unit")]
        [DataRow(null)]
        [DataRow("")]
        [DataRow(" ")]
        public void Validate_WhenValueIsNullOrEmpty_ShouldNotHaveValidationError(string value)
        {
            // Arrange
            var filterOption = new RadioFilterOption
            {
                Key = FilterKey.Team.ToString(),
                Type = FilterOptionType.RadioFilterOption.ToString(),
                Value = value
            };

            // Act
            var result = _validator.TestValidate(filterOption);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [TestMethod, TestCategory("Unit")]
        public void Validate_WhenValueSpecified_ShouldNotHaveValidationError()
        {
            // Arrange
            var filterOption = new RadioFilterOption
            {
                Key = FilterKey.Team.ToString(),
                Type = FilterOptionType.RadioFilterOption.ToString(),
                Value = "value"
            };

            // Act
            var result = _validator.TestValidate(filterOption);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}