using System;
using System.Collections.Generic;
using CDR.Register.Domain.Entities;
using Xunit;

namespace CDR.Register.Domain.UnitTests
{
    public class DataRecipientTests
    {
        /// <summary>
        /// When the data recipient has no brands, the last updated date should be null
        /// </summary>
        public static IEnumerable<object[]> GetEmptyDataRecipientBrands() => new List<object[]>
        {
            new object[] { Array.Empty<DataRecipientBrand>() },
            new object[] { null }
        };
        [Theory]
        [MemberData(nameof(GetEmptyDataRecipientBrands))]
        public void LastUpdated_NoBrands_ShouldReturnNull(DataRecipientBrand[] brands)
        {
            // Arrange
            var sut = new DataRecipient()
            {
                DataRecipientBrands = brands
            };

            // Act
            var lastUpdated = sut.LastUpdated;

            // Assert
            Assert.Null(lastUpdated);
        }

        /// <summary>
        /// When the data recipient has a list of brands, the last updated date should be the latest date from the brands list.
        /// </summary>
        [Fact]
        public void LastUpdated_HasBrands_ShouldReturnLastUpdatedDateFromLatestBrand()
        {
            // Arrange
            DateTime latestLastUpdated = DateTime.Now.AddDays(-1);
            var sut = new DataRecipient()
            {
                DataRecipientBrands = new DataRecipientBrand[]
                {
                    new DataRecipientBrand() { LastUpdated = latestLastUpdated.AddDays(-1) },
                    new DataRecipientBrand() { LastUpdated = latestLastUpdated },
                }
            };

            // Act
            var lastUpdated = sut.LastUpdated;

            // Assert
            Assert.Equal(latestLastUpdated.ToUniversalTime(), lastUpdated);
        }
    }
}
