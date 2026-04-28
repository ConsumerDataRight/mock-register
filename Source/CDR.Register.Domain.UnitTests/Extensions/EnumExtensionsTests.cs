using System.ComponentModel;
using CDR.Register.Domain.Extensions;
using Xunit;

namespace CDR.Register.Domain.UnitTests
{
    public class EnumExtensionsTests
    {
        private const string ExpectedDescription = "Value With Description";

        public enum TestValue
        {
            [Description(ExpectedDescription)]
            WithDescription = 100,
            SansDescription = 200,
        }

        [Theory]
        [InlineData(nameof(TestValue.WithDescription), true, TestValue.WithDescription)]
        [InlineData(nameof(TestValue.SansDescription), true, TestValue.SansDescription)]
        [InlineData(TestValue.WithDescription, true, TestValue.WithDescription)]
        [InlineData(TestValue.SansDescription, true, TestValue.SansDescription)]
        [InlineData(100, true, TestValue.WithDescription)]
        [InlineData(200, true, TestValue.SansDescription)]
        [InlineData("invalid", false, default)]
        [InlineData("", false, default)]
        [InlineData(0, false, default)]
        public void TryParseFromDescription_ReturnsExpectedValue(object obj, bool expectedResult, TestValue? expectedValue)
        {
            var input = obj.ToString();

            var result = input.TryParseFromDescription<TestValue>(out var value);

            Assert.Equal(expectedResult, result);

            if (result)
            {
                Assert.Equal(expectedValue, value);
            }
        }

        [Theory]
        [InlineData(nameof(TestValue.WithDescription), TestValue.WithDescription)]
        [InlineData(nameof(TestValue.SansDescription), TestValue.SansDescription)]
        [InlineData(TestValue.WithDescription, TestValue.WithDescription)]
        [InlineData(TestValue.SansDescription, TestValue.SansDescription)]
        [InlineData(100, TestValue.WithDescription)]
        [InlineData(200, TestValue.SansDescription)]
        public void ParseFromDescription_ReturnsExpectedValue(object obj, TestValue? expectedValue)
        {
            var input = obj.ToString();

            var result = input.ParseFromDescription<TestValue>();

            Assert.Equal(expectedValue, result);
        }

        [Theory]
        [InlineData(TestValue.WithDescription, ExpectedDescription)]
        [InlineData(TestValue.SansDescription, null)]
        public void GetDescription_ReturnsExpectedValue(TestValue value, string expected)
        {
            var result = value.GetDescription();

            Assert.Equal(expected, result);
        }
    }
}
