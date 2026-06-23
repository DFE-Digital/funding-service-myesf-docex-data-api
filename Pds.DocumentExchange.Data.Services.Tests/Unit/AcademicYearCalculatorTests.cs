using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pds.Core.Utils;
using Pds.DocumentExchange.Data.Services.Implementations;
using System;

namespace Pds.DocumentExchange.Data.Services.Tests.Unit
{
    [TestClass]
    public class AcademicYearCalculatorTests
    {
        private readonly Mock<ISystemProvider> _systemProvider = new Mock<ISystemProvider>(MockBehavior.Strict);
        private readonly AcademicYearCalculator _academicYearCalculator;

        public AcademicYearCalculatorTests()
        {
            _academicYearCalculator = new AcademicYearCalculator(_systemProvider.Object);
        }

        [TestMethod, TestCategory("Unit")]
        [DataRow("2020-01-01", 201920)]
        [DataRow("2020-09-01", 202021)]
        [DataRow("2021-08-01", 202021)]
        [DataRow("2021-12-01", 202122)]
        [DataRow("2000-05-01", 199900)]
        [DataRow("2050-10-01", 205051)]
        [DataRow("2100-10-01", 210001)]
        public void GetCurrentAcademicYear_ReturnsCorrectAcademicYear(string date, int academicYear)
        {
            // Arrange
            var currentDate = DateTime.Parse(date);

            _systemProvider
                .Setup(provider => provider.DateTime.UtcNow())
                .Returns(currentDate);

            // Act
            var result = _academicYearCalculator.GetCurrentAcademicYear();

            // Assert
            result.Should().Be(academicYear);
        }
    }
}