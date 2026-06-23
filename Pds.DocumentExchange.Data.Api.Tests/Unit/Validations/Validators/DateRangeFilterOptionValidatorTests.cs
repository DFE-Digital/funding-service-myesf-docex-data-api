using FluentValidation.TestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pds.DocumentExchange.Data.Api.Enums;
using Pds.DocumentExchange.Data.Api.Models.Filters;
using Pds.DocumentExchange.Data.Api.Validations.ModelValidators;
using Pds.DocumentExchange.Data.Services.Enums;
using System;

namespace Pds.DocumentExchange.Data.Api.Tests.Unit.Validations.Validators
{
    [TestClass]
    public class DateRangeFilterOptionValidatorTests
    {
        private readonly DateRangeFilterOptionValidator _validator = new DateRangeFilterOptionValidator();

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
        public void Validate_WhenFilterFromIsEmpty_ShouldHaveValidationError()
        {
            // Arrange
            var filterOption = new DateRangeFilterOption
            {
                Key = FilterKey.UploadDate.ToString(),
                Type = FilterOptionType.DateRangeFilterOption.ToString(),
                From = null,
                To = new DateTime(2020, 12, 31)
            };

            // Act
            var result = _validator.TestValidate(filterOption);

            // Assert
            result.ShouldHaveValidationErrorFor(filterOption => filterOption.From);
        }

        [TestMethod, TestCategory("Unit")]
        public void Validate_WhenFilterToIsEmpty_ShouldHaveValidationError()
        {
            // Arrange
            var filterOption = new DateRangeFilterOption
            {
                Key = FilterKey.UploadDate.ToString(),
                Type = FilterOptionType.DateRangeFilterOption.ToString(),
                From = new DateTime(2020, 1, 1),
                To = null
            };

            // Act
            var result = _validator.TestValidate(filterOption);

            // Assert
            result.ShouldHaveValidationErrorFor(filterOption => filterOption.To);
        }

        [TestMethod, TestCategory("Unit")]
        public void Validate_WhenFilterFromIfGreaterThanTo_ShouldHaveValidationError()
        {
            // Arrange
            var filterOption = new DateRangeFilterOption
            {
                Key = FilterKey.UploadDate.ToString(),
                Type = FilterOptionType.DateRangeFilterOption.ToString(),
                From = new DateTime(2020, 12, 31),
                To = new DateTime(2020, 1, 1),
            };

            // Act
            var result = _validator.TestValidate(filterOption);

            // Assert
            result.ShouldHaveValidationErrorFor(filterOption => filterOption.To);
        }

        [TestMethod, TestCategory("Unit")]
        public void Validate_WhenFromAndToDatesAreSpecified_ShouldNotHaveValidationError()
        {
            // Arrange
            var filterOption = new DateRangeFilterOption
            {
                Key = FilterKey.UploadDate.ToString(),
                Type = FilterOptionType.DateRangeFilterOption.ToString(),
                From = new DateTime(2020, 1, 1),
                To = new DateTime(2020, 12, 31)
            };

            // Act
            var result = _validator.TestValidate(filterOption);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [TestMethod, TestCategory("Unit")]
        public void Validate_WhenFromAndToDatesAreNull_ShouldNotHaveValidationError()
        {
            // Arrange
            var filterOption = new DateRangeFilterOption
            {
                Key = FilterKey.UploadDate.ToString(),
                Type = FilterOptionType.DateRangeFilterOption.ToString(),
                From = null,
                To = null
            };

            // Act
            var result = _validator.TestValidate(filterOption);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}