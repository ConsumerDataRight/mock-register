using CDR.Register.API.Infrastructure.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using NSubstitute;
using System;
using System.Collections.Generic;
using Xunit;

namespace CDR.Register.API.Infrastructure.Tests.UnitTests.Versioning
{
    [Trait("Category", "UnitTests")]
    public partial class CdrVersionReaderTests
    {

        [Fact]
        public void Read_DataHolderStatus_NoXvHeader_ShouldReturn1()
        {
            // Arrange.
            var apiVersionReader = new CdrVersionReader(new Models.CdrApiOptions());
            var expectedVersion = "1";
            var mockHttpRequest = Substitute.For<HttpRequest>();
            mockHttpRequest.Path.Returns(new PathString("/cdr-register/v1/all/data-holders/status"));

            // Act.
            var actualVersion = apiVersionReader.Read(mockHttpRequest);

            // Assert.
            Assert.NotNull(actualVersion);
            Assert.Equal(expectedVersion, actualVersion);
        }

        [Fact]
        public void Read_DataHolderStatus_EmptyXvHeader_ShouldReturn1()
        {
            // Arrange.
            var apiVersionReader = new CdrVersionReader(new Models.CdrApiOptions());
            var expectedVersion = "1";

            var mockHttpHeaders = new HeaderDictionary(new Dictionary<string, StringValues>(StringComparer.OrdinalIgnoreCase));
            mockHttpHeaders.Add("x-v", new StringValues(""));

            var mockHttpRequest = Substitute.For<HttpRequest>();
            mockHttpRequest.Path.Returns(new PathString("/cdr-register/v1/all/data-holders/status"));
            mockHttpRequest.Headers.Returns(mockHttpHeaders);

            // Act.
            var actualVersion = apiVersionReader.Read(mockHttpRequest);

            // Assert.
            Assert.NotNull(actualVersion);
            Assert.Equal(expectedVersion, actualVersion);
        }

        [Fact]
        public void Read_NoXvHeader_ShouldReturnMissingVersion()
        {
            // Arrange.
            var apiVersionReader = new CdrVersionReader(new Models.CdrApiOptions());
            var mockHttpRequest = Substitute.For<HttpRequest>();
            mockHttpRequest.Path.Returns(new PathString("/cdr-register/v1/all/data-holders/brands"));
            var expectedVersion = "Missing Version";

            // Act.
            var actualVersion = apiVersionReader.Read(mockHttpRequest);

            // Assert.
            Assert.Equal(expectedVersion, actualVersion);
        }

        [Fact]
        public void Read_EmptyXvHeader_ShouldReturnMissingVersion()
        {
            // Arrange.
            var apiVersionReader = new CdrVersionReader(new Models.CdrApiOptions());

            var mockHttpHeaders = new HeaderDictionary(new Dictionary<string, StringValues>(StringComparer.OrdinalIgnoreCase));
            mockHttpHeaders.Add("x-v", new StringValues(""));

            var mockHttpRequest = Substitute.For<HttpRequest>();
            mockHttpRequest.Path.Returns(new PathString("/cdr-register/v1/all/data-holders/brands"));
            mockHttpRequest.Headers.Returns(mockHttpHeaders);
            var expectedVersion = "Missing Version";

            // Act.
            var actualVersion = apiVersionReader.Read(mockHttpRequest);

            // Assert.
            Assert.Equal(expectedVersion, actualVersion);
        }

        [Fact]
        public void Read_NullXvHeader_ShouldReturnMissingVersion()
        {
            // Arrange.
            var apiVersionReader = new CdrVersionReader(new Models.CdrApiOptions());

            var mockHttpHeaders = new HeaderDictionary(new Dictionary<string, StringValues>(StringComparer.OrdinalIgnoreCase));
            mockHttpHeaders.Add("x-v", new StringValues());

            var mockHttpRequest = Substitute.For<HttpRequest>();
            mockHttpRequest.Path.Returns(new PathString("/cdr-register/v1/all/data-holders/brands"));
            mockHttpRequest.Headers.Returns(mockHttpHeaders);
            var expectedVersion = "Missing Version";

            // Act.
            var actualVersion = apiVersionReader.Read(mockHttpRequest);

            // Assert.
            Assert.Equal(expectedVersion, actualVersion);
        }

        [Fact]
        public void Read_InvalidXvHeader_ShouldReturnInvalidVersion()
        {
            // Arrange.
            var apiVersionReader = new CdrVersionReader(new Models.CdrApiOptions());

            var mockHttpHeaders = new HeaderDictionary(new Dictionary<string, StringValues>(StringComparer.OrdinalIgnoreCase));
            mockHttpHeaders.Add("x-v", new StringValues("foo"));

            var mockHttpRequest = Substitute.For<HttpRequest>();
            mockHttpRequest.Path.Returns(new PathString("/cdr-register/v1/all/data-holders/brands"));
            mockHttpRequest.Headers.Returns(mockHttpHeaders);
            var expectedVersion = "Invalid Version";

            // Act.
            var actualVersion = apiVersionReader.Read(mockHttpRequest);

            // Assert.
            Assert.Equal(expectedVersion, actualVersion);
        }

        [Fact]
        public void Read_SetToZeroXvHeader_ShouldReturnInvalidVersion()
        {
            // Arrange.
            var apiVersionReader = new CdrVersionReader(new Models.CdrApiOptions());

            var mockHttpHeaders = new HeaderDictionary(new Dictionary<string, StringValues>(StringComparer.OrdinalIgnoreCase));
            mockHttpHeaders.Add("x-v", new StringValues("0"));

            var mockHttpRequest = Substitute.For<HttpRequest>();
            mockHttpRequest.Path.Returns(new PathString("/cdr-register/v1/all/data-holders/brands"));
            mockHttpRequest.Headers.Returns(mockHttpHeaders);
            var expectedVersion = "Invalid Version";

            // Act.
            var actualVersion = apiVersionReader.Read(mockHttpRequest);

            // Assert.
            Assert.Equal(expectedVersion, actualVersion);
        }

        [Fact]
        public void Read_LessThanZeroXvHeader_ShouldReturnInvalidVersion()
        {
            // Arrange.
            var apiVersionReader = new CdrVersionReader(new Models.CdrApiOptions());

            var mockHttpHeaders = new HeaderDictionary(new Dictionary<string, StringValues>(StringComparer.OrdinalIgnoreCase));
            mockHttpHeaders.Add("x-v", new StringValues("-1"));

            var mockHttpRequest = Substitute.For<HttpRequest>();
            mockHttpRequest.Path.Returns(new PathString("/cdr-register/v1/all/data-holders/brands"));
            mockHttpRequest.Headers.Returns(mockHttpHeaders);
            var expectedVersion = "Invalid Version";

            // Act.
            var actualVersion = apiVersionReader.Read(mockHttpRequest);

            // Assert.
            Assert.Equal(expectedVersion, actualVersion);
        }

        [Fact]
        public void Read_GreaterThanMaxXvHeader_ShouldReturnUnsupportedVersion()
        {
            // Arrange.
            var apiVersionReader = new CdrVersionReader(new Models.CdrApiOptions());
            var mockHttpHeaders = new HeaderDictionary(new Dictionary<string, StringValues>(StringComparer.OrdinalIgnoreCase));
            mockHttpHeaders.Add("x-v", new StringValues("99"));

            var mockHttpRequest = Substitute.For<HttpRequest>();
            mockHttpRequest.Path.Returns(new PathString("/cdr-register/v1/all/data-holders/brands"));
            mockHttpRequest.Headers.Returns(mockHttpHeaders);
            var expectedVersion = "Unsupported Version";

            // Act.
            var actualVersion = apiVersionReader.Read(mockHttpRequest);

            // Assert.
            Assert.Equal(expectedVersion, actualVersion);
        }

        [Fact]
        public void Read_ValidXvHeader_ShouldReturnValidVersion()
        {
            // Arrange.
            var apiVersionReader = new CdrVersionReader(new Models.CdrApiOptions());
            var expectedVersion = "2";
            var mockHttpHeaders = new HeaderDictionary(new Dictionary<string, StringValues>(StringComparer.OrdinalIgnoreCase));
            mockHttpHeaders.Add("x-v", new StringValues("2"));

            var mockHttpRequest = Substitute.For<HttpRequest>();
            mockHttpRequest.Path.Returns(new PathString("/cdr-register/v1/all/data-holders/brands"));
            mockHttpRequest.Headers.Returns(mockHttpHeaders);

            // Act.
            var actualVersion = apiVersionReader.Read(mockHttpRequest);

            // Assert.
            Assert.NotNull(actualVersion);
            Assert.Equal(expectedVersion, actualVersion);
        }

        [Fact]
        public void Read_UppercaseXvHeader_ShouldReturnValidVersion()
        {
            // Arrange.
            var apiVersionReader = new CdrVersionReader(new Models.CdrApiOptions());
            var expectedVersion = "2";
            var mockHttpHeaders = new HeaderDictionary(new Dictionary<string, StringValues>(StringComparer.OrdinalIgnoreCase));
            mockHttpHeaders.Add("X-V", new StringValues("2"));

            var mockHttpRequest = Substitute.For<HttpRequest>();
            mockHttpRequest.Path.Returns(new PathString("/cdr-register/v1/all/data-holders/brands"));
            mockHttpRequest.Headers.Returns(mockHttpHeaders);

            // Act.
            var actualVersion = apiVersionReader.Read(mockHttpRequest);

            // Assert.
            Assert.NotNull(actualVersion);
            Assert.Equal(expectedVersion, actualVersion);
        }

        [Fact]
        public void Read_InvalidPath_ShouldReturn1()
        {
            // Arrange.
            var apiVersionReader = new CdrVersionReader(new Models.CdrApiOptions());
            var expectedVersion = "1";
            var mockHttpHeaders = new HeaderDictionary(new Dictionary<string, StringValues>(StringComparer.OrdinalIgnoreCase));
            mockHttpHeaders.Add("X-V", new StringValues("1"));

            var mockHttpRequest = Substitute.For<HttpRequest>();
            mockHttpRequest.Path.Returns(new PathString("/invalid/path"));
            mockHttpRequest.Headers.Returns(mockHttpHeaders);

            // Act.
            var actualVersion = apiVersionReader.Read(mockHttpRequest);

            // Assert.
            Assert.NotNull(actualVersion);
            Assert.Equal(expectedVersion, actualVersion);
        }

        [Fact]
        public void Read_WithRange_ShouldReturnMaxVersion()
        {
            // Arrange.
            var apiVersionReader = new CdrVersionReader(new Models.CdrApiOptions());
            var expectedVersion = "3";
            var mockHttpHeaders = new HeaderDictionary(new Dictionary<string, StringValues>(StringComparer.OrdinalIgnoreCase));
            mockHttpHeaders.Add("x-v", new StringValues("4"));
            mockHttpHeaders.Add("x-min-v", new StringValues("3"));

            var mockHttpRequest = Substitute.For<HttpRequest>();
            mockHttpRequest.Path.Returns(new PathString("/cdr-register/v1/all/data-recipients"));
            mockHttpRequest.Headers.Returns(mockHttpHeaders);

            // Act.
            var actualVersion = apiVersionReader.Read(mockHttpRequest);

            // Assert.
            Assert.NotNull(actualVersion);
            Assert.Equal(expectedVersion, actualVersion);
        }

        [Fact]
        public void Read_WithUppercaseXminV_ShouldReturnMaxVersion()
        {
            // Arrange.
            var apiVersionReader = new CdrVersionReader(new Models.CdrApiOptions());
            var expectedVersion = "3";
            var mockHttpHeaders = new HeaderDictionary(new Dictionary<string, StringValues>(StringComparer.OrdinalIgnoreCase));
            mockHttpHeaders.Add("x-v", new StringValues("4"));
            mockHttpHeaders.Add("X-MIN-V", new StringValues("3"));

            var mockHttpRequest = Substitute.For<HttpRequest>();
            mockHttpRequest.Path.Returns(new PathString("/cdr-register/v1/all/data-recipients"));
            mockHttpRequest.Headers.Returns(mockHttpHeaders);

            // Act.
            var actualVersion = apiVersionReader.Read(mockHttpRequest);

            // Assert.
            Assert.NotNull(actualVersion);
            Assert.Equal(expectedVersion, actualVersion);
        }

        [Fact]
        public void Read_XminVGreaterThanXV_ShouldBeIgnored()
        {
            // Arrange.
            var apiVersionReader = new CdrVersionReader(new Models.CdrApiOptions());
            var expectedVersion = "3";
            var mockHttpHeaders = new HeaderDictionary(new Dictionary<string, StringValues>(StringComparer.OrdinalIgnoreCase));
            mockHttpHeaders.Add("x-v", new StringValues("3"));
            mockHttpHeaders.Add("x-min-v", new StringValues("4"));

            var mockHttpRequest = Substitute.For<HttpRequest>();
            mockHttpRequest.Path.Returns(new PathString("/cdr-register/v1/all/data-recipients"));
            mockHttpRequest.Headers.Returns(mockHttpHeaders);

            // Act.
            var actualVersion = apiVersionReader.Read(mockHttpRequest);

            // Assert.
            Assert.NotNull(actualVersion);
            Assert.Equal(expectedVersion, actualVersion);
        }

        [Fact]
        public void Read_InvalidRange_ShouldReturnUnsupportedVersion()
        {
            // Arrange.
            var apiVersionReader = new CdrVersionReader(new Models.CdrApiOptions());
            var mockHttpHeaders = new HeaderDictionary(new Dictionary<string, StringValues>(StringComparer.OrdinalIgnoreCase));
            mockHttpHeaders.Add("x-v", new StringValues("5"));
            mockHttpHeaders.Add("x-min-v", new StringValues("4"));

            var mockHttpRequest = Substitute.For<HttpRequest>();
            mockHttpRequest.Path.Returns(new PathString("/cdr-register/v1/all/data-recipients"));
            mockHttpRequest.Headers.Returns(mockHttpHeaders);
            var expectedVersion = "Unsupported Version";

            // Act.
            var actualVersion = apiVersionReader.Read(mockHttpRequest);

            // Assert.
            Assert.Equal(expectedVersion, actualVersion);
        }

        [Fact]
        public void Read_ValidXvInvalidXminV_ShouldReturnInvalidVersion()
        {
            // Arrange.
            var apiVersionReader = new CdrVersionReader(new Models.CdrApiOptions());
            var mockHttpHeaders = new HeaderDictionary(new Dictionary<string, StringValues>(StringComparer.OrdinalIgnoreCase));
            mockHttpHeaders.Add("x-v", new StringValues("3"));
            mockHttpHeaders.Add("x-min-v", new StringValues("foo"));

            var mockHttpRequest = Substitute.For<HttpRequest>();
            mockHttpRequest.Path.Returns(new PathString("/cdr-register/v1/all/data-recipients"));
            mockHttpRequest.Headers.Returns(mockHttpHeaders);
            var expectedVersion = "Invalid Version";

            // Act.
            var actualVersion = apiVersionReader.Read(mockHttpRequest);

            // Assert.
            Assert.Equal(expectedVersion, actualVersion);
        }
    }
}