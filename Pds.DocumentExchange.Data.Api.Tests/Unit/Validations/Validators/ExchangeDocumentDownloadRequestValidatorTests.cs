using FluentValidation.TestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pds.DocumentExchange.Data.Api.Enums;
using Pds.DocumentExchange.Data.Api.Models;
using Pds.DocumentExchange.Data.Api.Validations.ModelValidators;
using System.Linq;

namespace Pds.DocumentExchange.Data.Api.Tests.Unit.Validations.Validators
{
    [TestClass]
    [TestCategory("Unit")]
    public class ExchangeDocumentDownloadRequestValidatorTests
    {
        private readonly ExchangeDocumentDownloadRequestValidator _validator =
            new ExchangeDocumentDownloadRequestValidator();

        [TestMethod]
        public void Validate_WhenUserInfoIsNull_ShouldHaveValidationError()
        {
            // Arrange
            var options = new ExchangeDocumentDownloadRequest
            {
                UserInfo = null
            };

            // Act
            var result = _validator.TestValidate(options);

            // Assert
            result.ShouldHaveValidationErrorFor($"{nameof(options.UserInfo)}");
        }

        [TestMethod]
        public void Validate_WhenUserInfoIsInvalid_ShouldHaveValidationError()
        {
            // Arrange
            var invalidUserInfo = new UserInfo
            {
                Principal = string.Empty
            };

            var options = new ExchangeDocumentDownloadRequest
            {
                UserInfo = invalidUserInfo
            };

            // Act
            var result = _validator.TestValidate(options);

            // Assert
            result.ShouldHaveValidationErrorFor($"{nameof(options.UserInfo)}.{nameof(invalidUserInfo.Principal)}");
        }

        [TestMethod]
        public void Validate_WhenContainsInvalidDocumentReference_ShouldHaveValidationError()
        {
            // Arrange
            var invalidDocumentReference = new DocumentReference
            {
                FileName = "test-file-name.txt",
                BatchIdentifier = "batch-identifier",
                ParentBatchIdentifier = string.Empty
            };

            var options = new ExchangeDocumentDownloadRequest
            {
                ListOptions = new ExchangeListDocumentOptions
                {
                    DocumentReferences = new[] { invalidDocumentReference }
                }
            };

            // Act
            var result = _validator.TestValidate(options);

            // Assert
            result.ShouldHaveValidationErrorFor(
                $"{nameof(options.ListOptions)}.{nameof(options.ListOptions.DocumentReferences)}[0].{nameof(invalidDocumentReference.ParentBatchIdentifier)}");
        }

        [TestMethod]
        [DataRow(true, true)]
        [DataRow(false, true)]
        [DataRow(false, false)]
        public void Validate_WhenUserInfoIsValidAndNoDocumentReferences_ShouldNotHaveValidationError(bool listOptionsNull, bool documentReferencesNull)
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

            var options = new ExchangeDocumentDownloadRequest
            {
                UserInfo = userInfo,
                ListOptions = listOptionsNull
                    ? null
                    : new ExchangeListDocumentOptions
                    {
                        DocumentReferences = documentReferencesNull
                            ? null
                            : Enumerable.Empty<DocumentReference>()
                    }
            };

            // Act
            var result = _validator.TestValidate(options);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [TestMethod]
        public void Validate_WhenExchangeDocumentDownloadRequestIsValid_ShouldNotHaveValidationError()
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

            var documentReference = new DocumentReference
            {
                FileName = "test-file-name.txt",
                BatchIdentifier = "batch-identifier",
                ParentBatchIdentifier = "parent-batch-identifier"
            };

            var options = new ExchangeDocumentDownloadRequest
            {
                UserInfo = userInfo,
                ListOptions = new ExchangeListDocumentOptions
                {
                    DocumentReferences = new[] { documentReference }
                }
            };

            // Act
            var result = _validator.TestValidate(options);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}