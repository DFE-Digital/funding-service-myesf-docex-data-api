using FluentValidation.TestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pds.DocumentExchange.Data.Api.Enums;
using Pds.DocumentExchange.Data.Api.Models;
using Pds.DocumentExchange.Data.Api.Validations.ModelValidators;

namespace Pds.DocumentExchange.Data.Api.Tests.Unit.Validations.Validators
{
    [TestClass]
    public class ExchangeDocumentDeleteRequestValidatorTests
    {
        private readonly ExchangeDocumentDeleteRequestValidator _validator = new ExchangeDocumentDeleteRequestValidator();

        [TestMethod, TestCategory("Unit")]
        public void Validate_WhenUserInfoIsNull_ShouldHaveValidationError()
        {
            // Arrange
            var options = new ExchangeDocumentDeleteRequest
            {
                UserInfo = null
            };

            // Act
            var result = _validator.TestValidate(options);

            // Assert
            result.ShouldHaveValidationErrorFor($"{nameof(options.UserInfo)}");
        }

        [TestMethod, TestCategory("Unit")]
        public void Validate_WhenUserInfoIsInvalid_ShouldHaveValidationError()
        {
            // Arrange
            var invalidUserInfo = new UserInfo
            {
                Principal = string.Empty
            };

            var options = new ExchangeDocumentDeleteRequest
            {
                UserInfo = invalidUserInfo
            };

            // Act
            var result = _validator.TestValidate(options);

            // Assert
            result.ShouldHaveValidationErrorFor($"{nameof(options.UserInfo)}.{nameof(invalidUserInfo.Principal)}");
        }

        [TestMethod, TestCategory("Unit")]
        public void Validate_WhenContainsInvalidDocumentReference_ShouldHaveValidationError()
        {
            // Arrange
            var invalidDocumentReference = new DocumentReferenceWithPreviousVersions
            {
                FileName = "test-file-name.txt",
                BatchIdentifier = "batch-identifier",
                ParentBatchIdentifier = string.Empty
            };

            var options = new ExchangeDocumentDeleteRequest
            {
                DocumentReferences = new[] { invalidDocumentReference }
            };

            // Act
            var result = _validator.TestValidate(options);

            // Assert
            result.ShouldHaveValidationErrorFor($"{nameof(options.DocumentReferences)}[0].{nameof(invalidDocumentReference.ParentBatchIdentifier)}");
        }

        [TestMethod, TestCategory("Unit")]
        public void Validate_WhenExchangeDocumentDeleteRequestIsValid_ShouldNotHaveValidationError()
        {
            // Arrange
            var userInfo = new UserInfo
            {
                Principal = "user-principal",
                FullName = "full-name",
                EmailAddress = "user@education.gov.uk",
                OrganisationInfo = new OrganisationInfo
                {
                    Name = "organisation-name",
                    OrganisationIdentifier = new OrganisationIdentifier
                    {
                        Type = OrganisationIdentifierType.Ukprn,
                        Value = "12345678"
                    }
                }
            };

            var documentReference = new DocumentReferenceWithPreviousVersions
            {
                FileName = "test-file-name.txt",
                BatchIdentifier = "batch-identifier",
                ParentBatchIdentifier = "parent-batch-identifier"
            };

            var options = new ExchangeDocumentDeleteRequest
            {
                UserInfo = userInfo,
                DocumentReferences = new[] { documentReference }
            };

            // Act
            var result = _validator.TestValidate(options);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}