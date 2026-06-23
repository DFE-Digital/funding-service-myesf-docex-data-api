using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pds.DocumentExchange.Data.Services.Extensions;
using System;

namespace Pds.DocumentExchange.Data.Services.Tests.Unit.Extensions
{
    [TestClass]
    public class StringExtensionsTests
    {
        #region SplitAndTrim

        [TestMethod, TestCategory("Unit")]
        [DataRow("one,two,three,four")]
        [DataRow("one ,two,  three,four ")]
        [DataRow("one, ,two,three,,four,      ")]
        public void SplitAndTrim_WhenInputIsValid_ReturnsStringArray(string input)
        {
            //Arrange
            var expectedResult = new[]
            {
                "one",
                "two",
                "three",
                "four"
            };

            // Act
            var result = input.SplitAndTrim(',');

            // Assert
            result.Should().BeEquivalentTo(expectedResult);
        }

        [TestMethod, TestCategory("Unit")]
        public void SplitAndTrim_WhenInputIsNull_Throws()
        {
            //Arrange
            string nullString = null;

            // Act
            Func<string[]> func = () => nullString.SplitAndTrim(',');

            // Assert
            func.Should().Throw<ArgumentNullException>();
        }

        [TestMethod, TestCategory("Unit")]
        [DataRow("")]
        [DataRow(" ")]
        [DataRow(",  ,     ")]
        public void SplitAndTrim_WhenInputIsEmptyOrWhiteSpace_ReturnsEmptyArray(string input)
        {
            // Act
            var result = input.SplitAndTrim(',');

            // Assert
            result.Should().BeEmpty();
        }

        #endregion


        #region IsEqualToIgnoreCase

        [TestMethod, TestCategory("Unit")]
        [DataRow("value", "value")]
        [DataRow("VALUE", "value")]
        [DataRow("value", "VALUE")]
        [DataRow("VaLuE", "vAlUe")]
        public void IsEqualToIgnoreCase_WhenValuesAreEqual_ReturnsTrue(string first, string second)
        {
            // Act
            var result = first.IsEqualToIgnoreCase(second);

            // Assert
            result.Should().BeTrue();
        }

        [TestMethod, TestCategory("Unit")]
        [DataRow("value", "another-value")]
        [DataRow("value", "value!")]
        [DataRow("value", "?value")]
        public void IsEqualToIgnoreCase_WhenValuesAreDifferent_ReturnsFalse(string first, string second)
        {
            // Act
            var result = first.IsEqualToIgnoreCase(second);

            // Assert
            result.Should().BeFalse();
        }

        #endregion
    }
}