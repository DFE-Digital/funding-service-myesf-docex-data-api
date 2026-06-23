using FluentValidation.TestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pds.DocumentExchange.Data.Api.Enums;
using Pds.DocumentExchange.Data.Api.Models.Filters;
using Pds.DocumentExchange.Data.Api.Validations.ModelValidators;
using Pds.DocumentExchange.Data.Services.Enums;
using System.Linq;

namespace Pds.DocumentExchange.Data.Api.Tests.Unit.Validations.Validators
{
    [TestClass]
    public class ListFilterOptionValidatorTests
    {
        private readonly ListFilterOptionValidator _validator = new ListFilterOptionValidator();

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
        [DataRow(FilterKey.Status)]
        [DataRow(FilterKey.ProductIdList)]
        [DataRow(FilterKey.DocumentNameError)]
        [DataRow(FilterKey.AcademicYear)]
        [DataRow(FilterKey.Organisation)]
        [DataRow(FilterKey.ProviderType)]
        public void Validate_WhenValuesListIsNull_ShouldNotHaveValidationError(FilterKey filterKey)
        {
            // Arrange
            var filterOption = new ListFilterOption
            {
                Key = filterKey.ToString(),
                Type = FilterOptionType.ListFilterOption.ToString(),
                Values = null
            };

            // Act
            var result = _validator.TestValidate(filterOption);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [TestMethod, TestCategory("Unit")]
        [DataRow(FilterKey.Status)]
        [DataRow(FilterKey.ProductIdList)]
        [DataRow(FilterKey.DocumentNameError)]
        [DataRow(FilterKey.AcademicYear)]
        [DataRow(FilterKey.Organisation)]
        [DataRow(FilterKey.ProviderType)]
        public void Validate_WhenValuesListIsEmpty_ShouldNotHaveValidationError(FilterKey filterKey)
        {
            // Arrange
            var filterOption = new ListFilterOption
            {
                Key = filterKey.ToString(),
                Type = FilterOptionType.ListFilterOption.ToString(),
                Values = Enumerable.Empty<string>()
            };

            // Act
            var result = _validator.TestValidate(filterOption);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [TestMethod, TestCategory("Unit")]
        [DataRow(FilterKey.Status)]
        [DataRow(FilterKey.ProductIdList)]
        [DataRow(FilterKey.DocumentNameError)]
        [DataRow(FilterKey.AcademicYear)]
        [DataRow(FilterKey.Organisation)]
        [DataRow(FilterKey.ProviderType)]
        public void Validate_WhenValuesListHasValues_ShouldNotHaveValidationError(FilterKey filterKey)
        {
            // Arrange
            var filterOption = new ListFilterOption
            {
                Key = filterKey.ToString(),
                Type = FilterOptionType.ListFilterOption.ToString(),
                Values = new[] { "value-1", "value-2" }
            };

            // Act
            var result = _validator.TestValidate(filterOption);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}