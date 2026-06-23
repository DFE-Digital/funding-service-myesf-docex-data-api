using FluentValidation.TestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pds.DocumentExchange.Data.Api.Models;
using Pds.DocumentExchange.Data.Api.Validations.ModelValidators;

namespace Pds.DocumentExchange.Data.Api.Tests.Unit.Validations.Validators
{
    [TestClass]
    public class DocumentReferenceValidatorTests
    {
        private readonly DocumentReferenceValidator _validator = new DocumentReferenceValidator();

        [TestMethod, TestCategory("Unit")]
        [DataRow(null)]
        [DataRow("")]
        [DataRow(" ")]
        public void Validate_WhenBatchIdentifierIsNullOrEmpty_ShouldHaveValidationError(string batchId)
        {
            // Assert
            _validator.ShouldHaveValidationErrorFor(documentReference => documentReference.BatchIdentifier, batchId);
        }

        [TestMethod, TestCategory("Unit")]
        [DataRow(null)]
        [DataRow("")]
        [DataRow(" ")]
        public void Validate_WhenParentBatchIdentifierIsNullOrEmpty_ShouldHaveValidationError(string parentBatchId)
        {
            // Assert
            _validator.ShouldHaveValidationErrorFor(documentReference => documentReference.ParentBatchIdentifier, parentBatchId);
        }

        [TestMethod, TestCategory("Unit")]
        [DataRow(null)]
        [DataRow("")]
        [DataRow(" ")]
        public void Validate_WhenFileNameIsNullOrEmpty_ShouldHaveValidationError(string fileName)
        {
            // Assert
            _validator.ShouldHaveValidationErrorFor(documentReference => documentReference.FileName, fileName);
        }

        [TestMethod, TestCategory("Unit")]
        public void Validate_WhenDocumentReferenceIsValid_ShouldNotHaveValidationError()
        {
            // Arrange
            var documentReference = new DocumentReference
            {
                BatchIdentifier = "batch-id",
                ParentBatchIdentifier = "parent-batch-id",
                FileName = "file-name.pdf"
            };

            // Act
            var result = _validator.TestValidate(documentReference);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}