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
    public class ExchangeListOrganisationDocumentOptionsValidatorTests
    {
        private readonly ExchangeListOrganisationDocumentOptionsValidator _validator = new ExchangeListOrganisationDocumentOptionsValidator();

        [TestMethod, TestCategory("Unit")]
        public void Validate_WhenOrganisationIdentifierIsNull_ShouldHaveValidationError()
        {
            // Arrange
            var options = new ExchangeListOrganisationDocumentOptions
            {
                OrganisationIdentifier = null
            };

            // Act
            var result = _validator.TestValidate(options);

            // Assert
            result.ShouldHaveValidationErrorFor($"{nameof(options.OrganisationIdentifier)}");
        }

        [TestMethod, TestCategory("Unit")]
        public void Validate_WhenOrganisationIdentifierIsInvalid_ShouldHaveValidationError()
        {
            // Arrange
            var invalidOrganisationIdentifier = new OrganisationIdentifier
            {
                Type = Enums.OrganisationIdentifierType.CompanyRegistrationNumber,
                Value = string.Empty
            };

            var options = new ExchangeListOrganisationDocumentOptions
            {
                OrganisationIdentifier = invalidOrganisationIdentifier
            };

            // Act
            var result = _validator.TestValidate(options);

            // Assert
            result.ShouldHaveValidationErrorFor($"{nameof(options.OrganisationIdentifier)}.{nameof(invalidOrganisationIdentifier.Value)}");
        }

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
        public void Validate_WhenContainsInvalidDocumentReference_ShouldHaveValidationError()
        {
            // Arrange
            var invalidDocumentReference = new DocumentReference
            {
                FileName = "test-file-name.txt",
                BatchIdentifier = "batch-identifier",
                ParentBatchIdentifier = string.Empty
            };

            var options = new ExchangeListOrganisationDocumentOptions
            {
                DocumentReferences = new[] { invalidDocumentReference }
            };

            // Act
            var result = _validator.TestValidate(options);

            // Assert
            result.ShouldHaveValidationErrorFor($"{nameof(options.DocumentReferences)}[0].{nameof(invalidDocumentReference.ParentBatchIdentifier)}");
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

            var options = new ExchangeListOrganisationDocumentOptions
            {
                FilterOptions = new[] { invalidFilter }
            };

            // Act
            var result = _validator.TestValidate(options);

            // Assert
            result.ShouldHaveValidationErrorFor($"{nameof(options.FilterOptions)}[0].{nameof(invalidFilter.Key)}");
        }

        [TestMethod, TestCategory("Unit")]
        public void Validate_WhenExchangeListOrganisationDocumentOptionsIsValid_ShouldNotHaveValidationError()
        {
            // Arrange
            var organisationIdentifier = new OrganisationIdentifier
            {
                Type = Enums.OrganisationIdentifierType.Ukprn,
                Value = "12345678"
            };

            var documentReference = new DocumentReference
            {
                FileName = "test-file-name.txt",
                BatchIdentifier = "batch-identifier",
                ParentBatchIdentifier = "parent-batch-identifier"
            };

            var filter = new ListFilterOption
            {
                Type = FilterOptionType.ListFilterOption.ToString(),
                Key = FilterKey.Status.ToString(),
                Values = new[] { "value-1", "value-2" }
            };

            var options = new ExchangeListOrganisationDocumentOptions
            {
                OrganisationIdentifier = organisationIdentifier,
                DocumentReferences = new[] { documentReference },
                FilterOptions = new[] { filter },
                DocumentStatusOption = Enums.ExchangeDocumentDirection.PublishedByAgency,
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