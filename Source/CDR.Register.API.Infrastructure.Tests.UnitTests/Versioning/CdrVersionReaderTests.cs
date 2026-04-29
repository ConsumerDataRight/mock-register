using System;
using System.Collections.Generic;
using CDR.Register.API.Infrastructure.Models;
using CDR.Register.API.Infrastructure.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using NSubstitute;
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
            var apiVersionReader = new CdrVersionReader(new CdrApiOptions());
            IReadOnlyCollection<string> expectedVersion = ["1"];
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
            var apiVersionReader = new CdrVersionReader(new CdrApiOptions());
            IReadOnlyCollection<string> expectedVersion = ["1"];

            var mockHttpHeaders = new HeaderDictionary(new Dictionary<string, StringValues>(StringComparer.OrdinalIgnoreCase))
            {
                { "x-v", new StringValues(string.Empty) },
            };

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
            var apiVersionReader = new CdrVersionReader(new CdrApiOptions());
            var mockHttpRequest = Substitute.For<HttpRequest>();
            mockHttpRequest.Path.Returns(new PathString("/cdr-register/v1/all/data-holders/brands"));

            // Act.
            var action = () => apiVersionReader.Read(mockHttpRequest);

            // Assert.
            var ex = Assert.Throws<MissingRequiredHeaderException>(action);
            Assert.Equal("x-v", ex.HeaderName);
        }

        [Fact]
        public void Read_EmptyXvHeader_ShouldReturnMissingVersion()
        {
            // Arrange.
            var apiVersionReader = new CdrVersionReader(new CdrApiOptions());

            var mockHttpHeaders = new HeaderDictionary(new Dictionary<string, StringValues>(StringComparer.OrdinalIgnoreCase))
            {
                { "x-v", new StringValues(string.Empty) },
            };

            var mockHttpRequest = Substitute.For<HttpRequest>();
            mockHttpRequest.Path.Returns(new PathString("/cdr-register/v1/all/data-holders/brands"));
            mockHttpRequest.Headers.Returns(mockHttpHeaders);

            // Act.
            var action = () => apiVersionReader.Read(mockHttpRequest);

            // Assert.
            var ex = Assert.Throws<MissingRequiredHeaderException>(action);
            Assert.Equal("x-v", ex.HeaderName);
        }

        [Fact]
        public void Read_NullXvHeader_ShouldReturnMissingVersion()
        {
            // Arrange.
            var apiVersionReader = new CdrVersionReader(new CdrApiOptions());

            var mockHttpHeaders = new HeaderDictionary(new Dictionary<string, StringValues>(StringComparer.OrdinalIgnoreCase))
            {
                { "x-v", default },
            };

            var mockHttpRequest = Substitute.For<HttpRequest>();
            mockHttpRequest.Path.Returns(new PathString("/cdr-register/v1/all/data-holders/brands"));
            mockHttpRequest.Headers.Returns(mockHttpHeaders);

            // Act.
            var action = () => apiVersionReader.Read(mockHttpRequest);

            // Assert.
            var ex = Assert.Throws<MissingRequiredHeaderException>(action);
            Assert.Equal("x-v", ex.HeaderName);
        }

        [Fact]
        public void Read_InvalidXvHeader_ShouldReturnInvalidVersion()
        {
            // Arrange.
            var apiVersionReader = new CdrVersionReader(new CdrApiOptions());

            var mockHttpHeaders = new HeaderDictionary(new Dictionary<string, StringValues>(StringComparer.OrdinalIgnoreCase))
            {
                { "x-v", new StringValues("foo") },
            };

            var mockHttpRequest = Substitute.For<HttpRequest>();
            mockHttpRequest.Path.Returns(new PathString("/cdr-register/v1/all/data-holders/brands"));
            mockHttpRequest.Headers.Returns(mockHttpHeaders);

            // Act.
            var action = () => apiVersionReader.Read(mockHttpRequest);

            // Assert.
            var ex = Assert.Throws<InvalidVersionException>(action);
            Assert.Equal("x-v", ex.HeaderName);
        }

        [Fact]
        public void Read_SetToZeroXvHeader_ShouldReturnInvalidVersion()
        {
            // Arrange.
            var apiVersionReader = new CdrVersionReader(new CdrApiOptions());

            var mockHttpHeaders = new HeaderDictionary(new Dictionary<string, StringValues>(StringComparer.OrdinalIgnoreCase))
            {
                { "x-v", new StringValues("0") },
            };

            var mockHttpRequest = Substitute.For<HttpRequest>();
            mockHttpRequest.Path.Returns(new PathString("/cdr-register/v1/all/data-holders/brands"));
            mockHttpRequest.Headers.Returns(mockHttpHeaders);

            // Act.
            var action = () => apiVersionReader.Read(mockHttpRequest);

            // Assert.
            var ex = Assert.Throws<InvalidVersionException>(action);
            Assert.Equal("x-v", ex.HeaderName);
        }

        [Fact]
        public void Read_LessThanZeroXvHeader_ShouldReturnInvalidVersion()
        {
            // Arrange.
            var apiVersionReader = new CdrVersionReader(new CdrApiOptions());

            var mockHttpHeaders = new HeaderDictionary(new Dictionary<string, StringValues>(StringComparer.OrdinalIgnoreCase))
            {
                { "x-v", new StringValues("-1") },
            };

            var mockHttpRequest = Substitute.For<HttpRequest>();
            mockHttpRequest.Path.Returns(new PathString("/cdr-register/v1/all/data-holders/brands"));
            mockHttpRequest.Headers.Returns(mockHttpHeaders);

            // Act.
            var action = () => apiVersionReader.Read(mockHttpRequest);

            // Assert.
            var ex = Assert.Throws<InvalidVersionException>(action);
            Assert.Equal("x-v", ex.HeaderName);
        }

        [Fact]
        public void Read_GreaterThanMaxXvHeader_ShouldReturnUnsupportedVersion()
        {
            // Arrange.
            var apiVersionReader = new CdrVersionReader(new CdrApiOptions());
            var mockHttpHeaders = new HeaderDictionary(new Dictionary<string, StringValues>(StringComparer.OrdinalIgnoreCase))
            {
                { "x-v", new StringValues("99") },
            };

            var mockHttpRequest = Substitute.For<HttpRequest>();
            mockHttpRequest.Path.Returns(new PathString("/cdr-register/v1/all/data-holders/brands"));
            mockHttpRequest.Headers.Returns(mockHttpHeaders);

            // Act.
            var action = () => apiVersionReader.Read(mockHttpRequest);

            // Assert.
            Assert.Throws<UnsupportedVersionException>(action);
        }

        [Fact]
        public void Read_ValidXvHeader_ShouldReturnValidVersion()
        {
            // Arrange.
            var apiVersionReader = new CdrVersionReader(new CdrApiOptions());
            IReadOnlyCollection<string> expectedVersion = ["2"];
            var mockHttpHeaders = new HeaderDictionary(new Dictionary<string, StringValues>(StringComparer.OrdinalIgnoreCase))
            {
                { "x-v", new StringValues("2") },
            };

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
            var apiVersionReader = new CdrVersionReader(new CdrApiOptions());
            IReadOnlyCollection<string> expectedVersion = ["2"];
            var mockHttpHeaders = new HeaderDictionary(new Dictionary<string, StringValues>(StringComparer.OrdinalIgnoreCase))
            {
                { "X-V", new StringValues("2") },
            };

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
            var apiVersionReader = new CdrVersionReader(new CdrApiOptions());
            IReadOnlyCollection<string> expectedVersion = ["1"];
            var mockHttpHeaders = new HeaderDictionary(new Dictionary<string, StringValues>(StringComparer.OrdinalIgnoreCase))
            {
                { "X-V", new StringValues("1") },
            };

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
            var apiVersionReader = new CdrVersionReader(new CdrApiOptions());
            IReadOnlyCollection<string> expectedVersion = ["4"];
            var mockHttpHeaders = new HeaderDictionary(new Dictionary<string, StringValues>(StringComparer.OrdinalIgnoreCase))
            {
                { "x-v", new StringValues("5") },
                { "x-min-v", new StringValues("4") },
            };

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
            var apiVersionReader = new CdrVersionReader(new CdrApiOptions());
            IReadOnlyCollection<string> expectedVersion = ["4"];

            var mockHttpHeaders = new HeaderDictionary(new Dictionary<string, StringValues>(StringComparer.OrdinalIgnoreCase))
            {
                { "x-v", new StringValues("5") },
                { "X-MIN-V", new StringValues("4") },
            };

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
            var apiVersionReader = new CdrVersionReader(new CdrApiOptions());
            IReadOnlyCollection<string> expectedVersion = ["3"];
            var mockHttpHeaders = new HeaderDictionary(new Dictionary<string, StringValues>(StringComparer.OrdinalIgnoreCase))
            {
                { "x-v", new StringValues("3") },
                { "x-min-v", new StringValues("4") },
            };

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
            var apiVersionReader = new CdrVersionReader(new CdrApiOptions());
            var mockHttpHeaders = new HeaderDictionary(new Dictionary<string, StringValues>(StringComparer.OrdinalIgnoreCase))
            {
                { "x-v", new StringValues("6") },
                { "x-min-v", new StringValues("5") },
            };

            var mockHttpRequest = Substitute.For<HttpRequest>();
            mockHttpRequest.Path.Returns(new PathString("/cdr-register/v1/all/data-recipients"));
            mockHttpRequest.Headers.Returns(mockHttpHeaders);

            // Act.
            var action = () => apiVersionReader.Read(mockHttpRequest);

            // Assert.
            Assert.Throws<UnsupportedVersionException>(action);
        }

        [Fact]
        public void Read_ValidXvInvalidXminV_ShouldReturnInvalidVersion()
        {
            // Arrange.
            var apiVersionReader = new CdrVersionReader(new CdrApiOptions());
            var mockHttpHeaders = new HeaderDictionary(new Dictionary<string, StringValues>(StringComparer.OrdinalIgnoreCase))
            {
                { "x-v", new StringValues("3") },
                { "x-min-v", new StringValues("foo") },
            };

            var mockHttpRequest = Substitute.For<HttpRequest>();
            mockHttpRequest.Path.Returns(new PathString("/cdr-register/v1/all/data-recipients"));
            mockHttpRequest.Headers.Returns(mockHttpHeaders);

            // Act.
            var action = () => apiVersionReader.Read(mockHttpRequest);

            // Assert.
            var ex = Assert.Throws<InvalidVersionException>(action);
            Assert.Equal("x-min-v", ex.HeaderName);
        }
    }
}
