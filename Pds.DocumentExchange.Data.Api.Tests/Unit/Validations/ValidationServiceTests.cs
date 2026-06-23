using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pds.DocumentExchange.Data.Api.Validations;
using Pds.DocumentExchange.Data.Services.Interfaces.Lookup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Api.Tests.Unit.Validations
{
    [TestClass]
    public class ValidationServiceTests
    {
        private readonly Mock<IValidatorFactory> _validatorFactory = new Mock<IValidatorFactory>(MockBehavior.Strict);
        private readonly Mock<ITeamsLookup> _teamValidator = new Mock<ITeamsLookup>(MockBehavior.Strict);
        private readonly Mock<IProductsLookup> _productsLookup = new Mock<IProductsLookup>(MockBehavior.Strict);

        private readonly ValidationService _validationService;

        public ValidationServiceTests()
        {
            _validationService = new ValidationService(
                _validatorFactory.Object,
                _teamValidator.Object,
                _productsLookup.Object);
        }

        #region Validate

        [TestMethod, TestCategory("Unit")]
        public void Validate_WhenValidatorNotFound_ThrowArgumentException()
        {
            // Arrange
            var userInfo = new Models.UserInfo
            {
                Principal = "user-principal"
            };

            _validatorFactory
                 .Setup(lookup => lookup.GetValidator<Models.UserInfo>())
                 .Returns(null as IValidator<Models.UserInfo>);

            // Act
            Func<bool> func = () => _validationService.Validate(userInfo, x => { Assert.IsNotNull(x); });

            // Assert
            func.Should().Throw<ArgumentException>();
        }

        [TestMethod, TestCategory("Unit")]
        public void Validate_WhenValidatorFoundAndModelIsValid_ReturnsTrue()
        {
            // Arrange
            var userInfo = new Models.UserInfo
            {
                Principal = "user-principal"
            };

            var validator = new Mock<IValidator<Models.UserInfo>>(MockBehavior.Strict);
            var validatorResult = new ValidationResult();

            _validatorFactory
                 .Setup(lookup => lookup.GetValidator<Models.UserInfo>())
                 .Returns(validator.Object);

            validator
                .Setup(validator => validator.Validate(It.IsAny<ValidationContext<Models.UserInfo>>()))
                .Returns(validatorResult);

            // Act
            var result = _validationService.Validate(userInfo, x => { Assert.IsNotNull(x); });

            // Assert
            result.Should().BeTrue();
        }

        [TestMethod, TestCategory("Unit")]
        public void Validate_WhenValidatorFoundAndModelIsInValid_ReturnsFalseAndUpdatesModelState()
        {
            // Arrange
            var userInfo = new Models.UserInfo
            {
                Principal = string.Empty
            };

            var validator = new Mock<IValidator<Models.UserInfo>>(MockBehavior.Strict);

            var validationFailure = new ValidationFailure("principal", "The user principal cannot be empty.");
            var validatorResult = new ValidationResult(new[] { validationFailure });

            _validatorFactory
                 .Setup(lookup => lookup.GetValidator<Models.UserInfo>())
                 .Returns(validator.Object);

            validator
                .Setup(validator => validator.Validate(It.IsAny<ValidationContext<Models.UserInfo>>()))
                .Returns(validatorResult);

            // Act
            var result = _validationService.Validate(userInfo, x => { Assert.IsNotNull(x); });

            // Assert
            result.Should().BeFalse();
        }

        #endregion


        #region ValidateTeam

        [TestMethod, TestCategory("Unit")]
        public async Task ValidateTeam_WhenTeamExists_ReturnsTrue()
        {
            // Arrange
            var teamIdentifier = "existing-team-identifier";
            var modelStateDict = new ModelStateDictionary();

            _teamValidator
                .Setup(teamValidator => teamValidator.Exists(teamIdentifier))
                .ReturnsAsync(true);

            // Act
            var result = await _validationService.ValidateTeam(teamIdentifier, modelStateDict.AddModelError);

            // Assert
            result.Should().BeTrue();

            modelStateDict.IsValid.Should().BeTrue();
            modelStateDict.Values.Should().BeNullOrEmpty();
        }

        [TestMethod, TestCategory("Unit")]
        [DataRow(null)]
        [DataRow("")]
        [DataRow(" ")]
        public async Task ValidateTeam_WhenTeamIdentifierIsNullOrEmpty_ReturnsFalseAndUpdateModelState(string teamName)
        {
            // Arrange
            var modelStateDict = new ModelStateDictionary();

            // Act
            var result = await _validationService.ValidateTeam(teamName, modelStateDict.AddModelError);

            // Assert
            result.Should().BeFalse();

            modelStateDict.IsValid.Should().BeFalse();
            modelStateDict.Values.Should().NotBeNullOrEmpty();
        }

        [TestMethod, TestCategory("Unit")]
        public async Task ValidateTeam_WhenTeamDoesntExist_ReturnsFalseAndUpdateModelState()
        {
            // Arrange
            var teamIdentifier = "non-existing-team-identifier";
            var modelStateDict = new ModelStateDictionary();

            _teamValidator
                .Setup(teamValidator => teamValidator.Exists(teamIdentifier))
                .ReturnsAsync(false);

            // Act
            var result = await _validationService.ValidateTeam(teamIdentifier, modelStateDict.AddModelError);

            // Assert
            result.Should().BeFalse();

            modelStateDict.IsValid.Should().BeFalse();
            modelStateDict.Values.Should().NotBeNullOrEmpty();
        }

        #endregion


        #region ValidateTeams

        [TestMethod, TestCategory("Unit")]
        public async Task ValidateTeams_WhenAllTeamsExist_ReturnsTrue()
        {
            // Arrange
            var teamIdentifier1 = "existing-team-identifier-1";
            var teamIdentifier2 = "existing-team-identifier-2";
            var modelStateDict = new ModelStateDictionary();

            _teamValidator
                .Setup(teamValidator => teamValidator.Exists(teamIdentifier1))
                .ReturnsAsync(true);

            _teamValidator
                .Setup(teamValidator => teamValidator.Exists(teamIdentifier2))
                .ReturnsAsync(true);

            // Act
            var result = await _validationService.ValidateTeams(new[] { teamIdentifier1, teamIdentifier2 }, modelStateDict.AddModelError);

            // Assert
            result.Should().BeTrue();

            modelStateDict.IsValid.Should().BeTrue();
            modelStateDict.Values.Should().BeNullOrEmpty();
        }

        [TestMethod, TestCategory("Unit")]
        [DataRow(true)]
        [DataRow(false)]
        public async Task ValidateTeams_WhenNoTeamsPassed_ReturnsFalseAndUpdateModelState(bool nullCollection)
        {
            // Arrange
            var modelStateDict = new ModelStateDictionary();

            IEnumerable<string> teams = nullCollection ? null : Enumerable.Empty<string>();

            // Act
            var result = await _validationService.ValidateTeams(teams, modelStateDict.AddModelError);

            // Assert
            result.Should().BeFalse();

            modelStateDict.IsValid.Should().BeFalse();
            modelStateDict.Values.Should().NotBeNullOrEmpty();
        }

        [TestMethod, TestCategory("Unit")]
        [DataRow(null)]
        [DataRow("")]
        [DataRow(" ")]
        public async Task ValidateTeams_WhenOneTeamIdentifierIsNullOrEmpty_ReturnsFalseAndUpdateModelState(string invalidTeamIdentifier)
        {
            // Arrange
            var modelStateDict = new ModelStateDictionary();
            var validTeamIdentifier = "valid-team-identifier";

            _teamValidator
                .Setup(teamValidator => teamValidator.Exists(validTeamIdentifier))
                .ReturnsAsync(true);

            // Act
            var result = await _validationService.ValidateTeams(new[] { validTeamIdentifier, invalidTeamIdentifier }, modelStateDict.AddModelError);

            // Assert
            result.Should().BeFalse();

            modelStateDict.IsValid.Should().BeFalse();
            modelStateDict.Values.Should().NotBeNullOrEmpty();
        }

        [TestMethod, TestCategory("Unit")]
        public async Task ValidateTeams_WhenOneTeamDoesntExist_ReturnsFalseAndUpdateModelState()
        {
            // Arrange
            var invalidTeamIdentifier = "non-existing-team-identifier";
            var validTeamIdentifier = "valid-team-identifier";
            var modelStateDict = new ModelStateDictionary();

            _teamValidator
                .Setup(teamValidator => teamValidator.Exists(invalidTeamIdentifier))
                .ReturnsAsync(false);

            _teamValidator
                .Setup(teamValidator => teamValidator.Exists(validTeamIdentifier))
                .ReturnsAsync(true);

            // Act
            var result = await _validationService.ValidateTeams(new[] { invalidTeamIdentifier, validTeamIdentifier }, modelStateDict.AddModelError);

            // Assert
            result.Should().BeFalse();

            modelStateDict.IsValid.Should().BeFalse();
            modelStateDict.Values.Should().NotBeNullOrEmpty();
        }

        #endregion


        #region ValidateProductIdentifier

        [TestMethod, TestCategory("Unit")]
        public async Task ValidateProductIdentifier_WhenProductIdentifierExists_ReturnsTrue()
        {
            // Arrange
            var productIdentifier = 1001;
            var modelStateDict = new ModelStateDictionary();

            _productsLookup
                .Setup(productsLookup => productsLookup.Exists(productIdentifier))
                .ReturnsAsync(true);

            // Act
            var result = await _validationService.ValidateProductIdentifier(productIdentifier, modelStateDict.AddModelError);

            // Assert
            result.Should().BeTrue();

            modelStateDict.IsValid.Should().BeTrue();
            modelStateDict.Values.Should().BeNullOrEmpty();
        }

        [TestMethod, TestCategory("Unit")]
        public async Task ValidateProductIdentifier_WhenProductIdentifierDoesntExist_ReturnsFalseAndUpdateModelState()
        {
            // Arrange
            var productIdentifier = 1001;
            var modelStateDict = new ModelStateDictionary();

            _productsLookup
                .Setup(productsLookup => productsLookup.Exists(productIdentifier))
                .ReturnsAsync(false);

            // Act
            var result = await _validationService.ValidateProductIdentifier(productIdentifier, modelStateDict.AddModelError);

            // Assert
            result.Should().BeFalse();

            modelStateDict.IsValid.Should().BeFalse();
            modelStateDict.Values.Should().NotBeNullOrEmpty();
        }

        #endregion
    }
}