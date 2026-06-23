using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pds.DocumentExchange.Data.Services.DTOs;
using Pds.DocumentExchange.Data.Services.Enums;
using Pds.DocumentExchange.Data.Services.Implementations.Filters;
using System;
using System.Collections.Generic;

namespace Pds.DocumentExchange.Data.Services.Tests.Unit.Filters
{
    [TestClass, TestCategory("Unit")]
    public class UploadedDateRangeFilterTests
    {
        private readonly UploadedDateRangeFilter _dateRangeFilter = new UploadedDateRangeFilter();

        [TestMethod]
        public void IsElementInsideRange_WhenFileMetadataIsNull_Throws()
        {
            // Act
            Func<bool> func = () => _dateRangeFilter.IsElementInsideRange(null, new DateTime(2020, 1, 1), new DateTime(2050, 1, 1));

            //Assert
            func.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void IsElementInsideRange_WhenRangeIsInvalid_Throws()
        {
            // Arrange
            var exchangeDocument = CreateTestsExchangeDocuments(new DateTime(1990, 6, 20));

            // Act
            Func<bool> func = () => _dateRangeFilter.IsElementInsideRange(exchangeDocument, new DateTime(2020, 1, 1), new DateTime(2019, 1, 1));

            //Assert
            func.Should().Throw<ArgumentException>();
        }

        [TestMethod]
        [DynamicData(nameof(IsElementInsideRange_TestData))]
        public void IsElementInsideRange_ForParameters_ReturnsExpectedValue(DateTime fileDate, DateTime start, DateTime end, bool expectedInside)
        {
            // Arrange
            var exchangeDocument = CreateTestsExchangeDocuments(fileDate);

            // Act
            var result = _dateRangeFilter.IsElementInsideRange(exchangeDocument, start, end);

            //Assert
            result.Should().Be(expectedInside);
        }

        [TestMethod]
        public void FilterTitle_ReturnsExpectedTitle()
        {
            // Assert
            _dateRangeFilter.FilterTitle.Should().Be("Filter by date");
        }

        [TestMethod]
        public void FilterKey_ReturnsExpectedFilterKey()
        {
            // Assert
            _dateRangeFilter.FilterKey.Should().Be(FilterKey.UploadDate.ToString());
        }

        [TestMethod]
        public void FilterType_ReturnsDefaultFilterType()
        {
            // Assert
            _dateRangeFilter.FilterType.Should().Be(FilterType.DateRangeFilter);
        }

        private ExchangeDocument CreateTestsExchangeDocuments(DateTime uploadedDate)
            => new ExchangeDocument
            {
                DocumentReference = new DocumentReference
                {
                    FileName = "file-name.pdf"
                },
                EventHistory = new[]
                {
                    new ExchangeDocumentEvent
                    {
                        EventType = ExchangeDocumentEventType.SentByOrganisation,
                        EventDateTime = uploadedDate
                    }
                }
            };

        private static IEnumerable<object[]> IsElementInsideRange_TestData
        {
            get
            {
                yield return new object[]
                {
                    new DateTime(2020, 6, 1),
                    new DateTime(2020, 1, 1),
                    new DateTime(2020, 12, 1),
                    true
                };
                yield return new object[]
                {
                    new DateTime(2020, 1, 1),
                    new DateTime(2020, 1, 1),
                    new DateTime(2020, 2, 1),
                    true
                };
                yield return new object[]
                {
                    new DateTime(2020, 1, 1),
                    new DateTime(2020, 1, 1),
                    new DateTime(2020, 1, 2),
                    true
                };
                yield return new object[]
                {
                    new DateTime(2020, 1, 1),
                    new DateTime(2020, 1, 1),
                    new DateTime(2020, 1, 1),
                    true
                };
                yield return new object[]
                {
                    new DateTime(2020, 1, 1),
                    new DateTime(2020, 2, 1),
                    new DateTime(2020, 12, 1),
                    false
                };
                yield return new object[]
                {
                    new DateTime(2020, 1, 5),
                    new DateTime(2020, 1, 1),
                    new DateTime(2020, 1, 4),
                    false
                };
                yield return new object[]
                {
                    new DateTime(2020, 1, 5),
                    new DateTime(2020, 1, 6),
                    new DateTime(2020, 1, 10),
                    false
                };
                yield return new object[]
                {
                    new DateTime(2020, 1, 2),
                    new DateTime(2020, 1, 1),
                    new DateTime(2020, 1, 1),
                    false
                };

                yield return new object[]
                {
                    new DateTime(2020, 6, 1, 12, 34, 56),
                    new DateTime(2020, 1, 1),
                    new DateTime(2020, 12, 1),
                    true
                };
                yield return new object[]
                {
                    new DateTime(2020, 1, 1, 12, 34, 56),
                    new DateTime(2020, 1, 1),
                    new DateTime(2020, 2, 1),
                    true
                };
                yield return new object[]
                {
                    new DateTime(2020, 1, 1, 12, 34, 56),
                    new DateTime(2020, 1, 1),
                    new DateTime(2020, 1, 2),
                    true
                };
                yield return new object[]
                {
                    new DateTime(2020, 1, 2, 12, 34, 56),
                    new DateTime(2020, 1, 1),
                    new DateTime(2020, 1, 2),
                    true
                };
                yield return new object[]
                {
                    new DateTime(2020, 1, 1, 12, 34, 56),
                    new DateTime(2020, 1, 1),
                    new DateTime(2020, 1, 1),
                    true
                };
                yield return new object[]
                {
                    new DateTime(2020, 1, 1, 12, 34, 56),
                    new DateTime(2020, 2, 1),
                    new DateTime(2020, 12, 1),
                    false
                };
                yield return new object[]
                {
                    new DateTime(2020, 1, 5, 12, 34, 56),
                    new DateTime(2020, 1, 1),
                    new DateTime(2020, 1, 4),
                    false
                };
                yield return new object[]
                {
                    new DateTime(2020, 1, 5, 12, 34, 56),
                    new DateTime(2020, 1, 6),
                    new DateTime(2020, 1, 10),
                    false
                };
                yield return new object[]
                {
                    new DateTime(2020, 1, 2, 12, 34, 56),
                    new DateTime(2020, 1, 1),
                    new DateTime(2020, 1, 1),
                    false
                };
            }
        }
    }
}