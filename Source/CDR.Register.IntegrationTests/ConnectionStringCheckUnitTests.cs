#nullable enable

using System;
using FluentAssertions;
using FluentAssertions.Execution;
using Xunit;

namespace CDR.Register.IntegrationTests
{
    public class ConnectionStringCheckUnitTests
    {
        private const string PRODUCTION_SERVER_FOO = "foo" + ConnectionStringCheck.PRODUCTION_SERVER + "foo"; // blacklist is checking for substrings, so surround with "foo" to ensure we are testing this

        [Theory]
        [InlineData(PRODUCTION_SERVER_FOO)]
        [InlineData(PRODUCTION_SERVER_FOO, true)]
        public void WhenOnBlackList_ShouldThrowException(string connectionString, bool? uppercase = false)
        {
            if (uppercase == true)
            {
                connectionString = connectionString.ToUpper();
            }

            using (new AssertionScope())
            {
                // Act/Assert
                Action act = () => ConnectionStringCheck.Check(connectionString);
                using (new AssertionScope())
                {
                    act.Should().Throw<Exception>();
                }
            }
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("foo")]
        [InlineData("sql-cdrsandbox-dev.database.windows.net")]
        [InlineData("sql-cdrsandbox-test.database.windows.net")]
        [InlineData("localhost")]
        [InlineData("mssql")]
        public void WhenNotOnBlackList_ShouldNotThrowException(string? connectionString)
        {
            using (new AssertionScope())
            {
                // Act/Assert
                string? returnedConnectionString = null;
                Action act = () => returnedConnectionString = ConnectionStringCheck.Check(connectionString);
                using (new AssertionScope())
                {
                    act.Should().NotThrow<Exception>();
                    returnedConnectionString?.Should().Be(connectionString);
                }
            }
        }
    }
}
