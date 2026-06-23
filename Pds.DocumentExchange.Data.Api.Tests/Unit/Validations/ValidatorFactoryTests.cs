using FluentAssertions;
using FluentValidation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pds.DocumentExchange.Data.Api.Validations;
using System;

namespace Pds.DocumentExchange.Data.Api.Tests.Unit.Validations
{
    [TestClass]
    public class ValidatorFactoryTests
    {
        private readonly Mock<IServiceProvider> _serviceProvider = new Mock<IServiceProvider>(MockBehavior.Strict);
        private readonly ValidatorFactory _validatorFactory;

        public ValidatorFactoryTests()
        {
            _validatorFactory = new ValidatorFactory(_serviceProvider.Object);
        }

        [TestMethod, TestCategory("Unit")]
        public void GetValidator_WhenTypeDoesntExist_ReturnsNull()
        {
            // Arrange
            var type = new Mock<Type>().Object;

            _serviceProvider
                .Setup(serviceProvider => serviceProvider.GetService(It.IsAny<Type>()))
                .Returns(null);

            // Act
            var result = _validatorFactory.GetValidator(type);

            // Assert
            result.Should().BeNull();
        }

        [TestMethod, TestCategory("Unit")]
        public void GetValidator_WhenTypeExist_ReturnsValidator()
        {
            // Arrange
            var type = new Mock<Type>().Object;
            var validator = new Mock<IValidator>().Object;

            _serviceProvider
                .Setup(serviceProvider => serviceProvider.GetService(It.IsAny<Type>()))
                .Returns(validator);

            // Act
            var result = _validatorFactory.GetValidator(type);

            // Assert
            result.Should().Be(validator);
        }

        [TestMethod, TestCategory("Unit")]
        public void GetValidatorGeneric_WhenTypeDoesntExist_ReturnsNull()
        {
            // Arrange
            var type = new Mock<Type>().Object;

            _serviceProvider
                .Setup(serviceProvider => serviceProvider.GetService(It.IsAny<Type>()))
                .Returns(null);

            // Act
            var result = _validatorFactory.GetValidator<Type>();

            // Assert
            result.Should().BeNull();
        }

        [TestMethod, TestCategory("Unit")]
        public void GetValidatorGeneric_WhenTypeExist_ReturnsValidator()
        {
            // Arrange
            var type = new Mock<Type>().Object;
            var validator = new Mock<IValidator<Type>>().Object;

            _serviceProvider
                .Setup(serviceProvider => serviceProvider.GetService(It.IsAny<Type>()))
                .Returns(validator);

            // Act
            var result = _validatorFactory.GetValidator<Type>();

            // Assert
            result.Should().Be(validator);
        }
    }
}