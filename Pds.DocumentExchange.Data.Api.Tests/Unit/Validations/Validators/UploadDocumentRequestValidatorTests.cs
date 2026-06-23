using FluentValidation.TestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pds.DocumentExchange.Data.Api.Models;
using Pds.DocumentExchange.Data.Api.Validations.ModelValidators;

namespace Pds.DocumentExchange.Data.Api.Tests.Unit.Validations.Validators
{
    [TestClass]
    public class UploadDocumentRequestValidatorTests
    {
        private readonly UploadDocumentRequestValidator _validator = new UploadDocumentRequestValidator();

        [TestMethod, TestCategory("Unit")]
        [DataRow(null)]
        [DataRow("")]
        [DataRow(" ")]
        public void Validate_WhenFileNameIsNullOrEmpty_ShouldHaveValidationError(string fileName)
        {
            // Assert
            _validator.ShouldHaveValidationErrorFor(
                uploadDocumentRequestValidator => uploadDocumentRequestValidator.FileName,
                fileName);
        }

        [TestMethod, TestCategory("Unit")]
        public void Validate_WhenBytesIsNull_ShouldHaveValidationError()
        {
            // Assert
            _validator.ShouldHaveValidationErrorFor(
                uploadDocumentRequestValidator => uploadDocumentRequestValidator.Bytes,
                null as byte[]);
        }

        [TestMethod, TestCategory("Unit")]
        public void Validate_WhenBytesIsEmpty_ShouldHaveValidationError()
        {
            // Assert
            _validator.ShouldHaveValidationErrorFor(
                uploadDocumentRequestValidator => uploadDocumentRequestValidator.Bytes,
                new byte[] { });
        }

        [TestMethod, TestCategory("Unit")]
        public void Validate_WhenUserInfoIsNull_ShouldHaveValidationError()
        {
            // Assert
            _validator.ShouldHaveValidationErrorFor(
                uploadDocumentRequestValidator => uploadDocumentRequestValidator.UserInfo,
                null as UserInfo);
        }

        [TestMethod, TestCategory("Unit")]
        public void Validate_WhenUserInfoIsInvalid_ShouldHaveValidationError()
        {
            // Arrange
            var invalidUserInfo = new UserInfo
            {
                Principal = string.Empty,
                OrganisationInfo = null
            };

            var agencyPublishRequest = new UploadDocumentRequest
            {
                UserInfo = invalidUserInfo
            };

            // Act
            var result = _validator.TestValidate(agencyPublishRequest);

            // Assert
            result.ShouldHaveValidationErrorFor($"{nameof(agencyPublishRequest.UserInfo)}.{nameof(invalidUserInfo.Principal)}");
            result.ShouldHaveValidationErrorFor($"{nameof(agencyPublishRequest.UserInfo)}.{nameof(invalidUserInfo.OrganisationInfo)}");
        }

        [TestMethod, TestCategory("Unit")]
        public void Validate_WhenFromOrganisationIsNull_ShouldHaveValidationError()
        {
            // Assert
            _validator.ShouldHaveValidationErrorFor(
                uploadDocumentRequestValidator => uploadDocumentRequestValidator.FromOrganisation,
                null as OrganisationIdentifier);
        }

        [TestMethod, TestCategory("Unit")]
        public void Validate_WhenFromOrganisationIsInvalid_ShouldHaveValidationError()
        {
            // Arrange
            var invalidOrganisationIdentifier = new OrganisationIdentifier
            {
                Type = Enums.OrganisationIdentifierType.Ukprn,
                Value = string.Empty
            };

            var organisationInfo = new UploadDocumentRequest
            {
                FromOrganisation = invalidOrganisationIdentifier
            };

            // Act
            var result = _validator.TestValidate(organisationInfo);

            // Assert
            result.ShouldHaveValidationErrorFor($"{nameof(organisationInfo.FromOrganisation)}.{nameof(invalidOrganisationIdentifier.Value)}");
        }

        [TestMethod, TestCategory("Unit")]
        public void Validate_WhenUploadDocumentRequest_ShouldHaveValidationError()
        {
            // Arrange
            var userInfo = new UserInfo
            {
                Principal = "user-principal",
                FullName = "user-full-name",
                EmailAddress = "user-email@education.gov.uk",
                OrganisationInfo = new OrganisationInfo
                {
                    Name = "user-organisation",
                    OrganisationIdentifier = new OrganisationIdentifier
                    {
                        Type = Enums.OrganisationIdentifierType.Ukprn,
                        Value = "87654321"
                    }
                }
            };

            var organisationIdentifier = new OrganisationIdentifier
            {
                Type = Enums.OrganisationIdentifierType.Ukprn,
                Value = "12345678"
            };

            var organisationInfo = new UploadDocumentRequest
            {
                FileName = "upload-file.pdf",
                Bytes = new byte[] { 1, 2, 3, 4, 5 },
                UserInfo = userInfo,
                ProductIdentifier = 1001,
                FromOrganisation = organisationIdentifier
            };

            // Act
            var result = _validator.TestValidate(organisationInfo);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}