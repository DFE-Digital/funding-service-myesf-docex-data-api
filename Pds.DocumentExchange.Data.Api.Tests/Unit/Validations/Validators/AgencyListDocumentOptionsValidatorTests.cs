using FluentValidation.TestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pds.DocumentExchange.Data.Api.Enums;
using Pds.DocumentExchange.Data.Api.Models;
using Pds.DocumentExchange.Data.Api.Models.Filters;
using Pds.DocumentExchange.Data.Api.Validations.ModelValidators;
using Pds.DocumentExchange.Data.Services.Enums;

namespace Pds.DocumentExchange.Data.Api.Tests.Unit.Validations.Validators
{
    [TestClass]
    public class AgencyListDocumentOptionsValidatorTests
    {
        private readonly AgencyListDocumentOptionsValidator _validator = new AgencyListDocumentOptionsValidator();

        [TestMethod, TestCategory("Unit")]
        [DataRow(0)]
        [DataRow(-1)]
        [DataRow(-200)]
        public void Validate_WhenPageNumberIsLessThanOne_ShouldHaveValidationError(int pageNumber)
        {
            // Assert
            _validator.ShouldHaveValidationErrorFor(options => options.PageNumber, pageNumber);
        }

        [TestMethod, TestCategory("Unit")]
        [DataRow(0)]
        [DataRow(-1)]
        [DataRow(-200)]
        public void Validate_WhenPageSizeIsLessThanOne_ShouldHaveValidationError(int pageSize)
        {
            // Assert
            _validator.ShouldHaveValidationErrorFor(options => options.PageSize, pageSize);
        }

        [TestMethod, TestCategory("Unit")]
        public void Validate_WhenContainsInvalidFilter_ShouldHaveValidationError()
        {
            // Arrange
            var invalidFilter = new ListFilterOption
            {
                Key = "invalid-filter-key",
                Type = "invalid-filter-option-type",
                Values = new[] { "value-1", "value-2" }
            };

            var options = new AgencyListDocumentOptions
            {
                FilterOptions = new[] { invalidFilter }
            };

            // Act
            var result = _validator.TestValidate(options);

            // Assert
            result.ShouldHaveValidationErrorFor($"{nameof(options.FilterOptions)}[0].{nameof(invalidFilter.Key)}");
        }

        [TestMethod, TestCategory("Unit")]
        public void Validate_WhenExchangeListDocumentOptionsIsValid_ShouldNotHaveValidationError()
        {
            // Arrange
            var filter = new ListFilterOption
            {
                Type = FilterOptionType.ListFilterOption.ToString(),
                Key = FilterKey.Status.ToString(),
                Values = new[] { "value-1", "value-2" }
            };

            var options = new AgencyListDocumentOptions
            {
                FilterOptions = new[] { filter },
                PageNumber = 1,
                PageSize = 25
            };

            // Act
            var result = _validator.TestValidate(options);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}