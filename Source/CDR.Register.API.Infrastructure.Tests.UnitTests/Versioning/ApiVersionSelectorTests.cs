using CDR.Register.API.Infrastructure.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.Extensions.Primitives;
using NSubstitute;
using System;
using System.Collections.Generic;
using Xunit;

namespace CDR.Register.API.Infrastructure.Tests.UnitTests.Versioning
{
    public partial class ApiVersionSelectorTests
    {
        [Fact]
        public void SelectVersion_NoXvHeader_ShouldReturn1()
        {
            // Arrange.
            var options = new ApiVersioningOptions() { DefaultApiVersion = new ApiVersion(1, 0) };
            var versionSelector = new ApiVersionSelector(options);
            var expectedVersion = new ApiVersion(1, 0);
            var mockHttpRequest = Substitute.For<HttpRequest>();
            mockHttpRequest.Path.Returns(new PathString("/cdr-register/v1/all/data-holders/brands"));

            // Act.
            var actualVersion = versionSelector.SelectVersion(mockHttpRequest, null); // apiVersionModel);

            // Assert.
            Assert.NotNull(actualVersion);
            Assert.Equal(expectedVersion.MajorVersion, actualVersion.MajorVersion);
            Assert.Equal(expectedVersion.MinorVersion, actualVersion.MinorVersion);
        }

        [Fact]
        public void SelectVersion_EmptyXvHeader_ShouldReturn1()
        {
            // Arrange.
            var options = new ApiVersioningOptions() { DefaultApiVersion = new ApiVersion(1, 0) };
            var versionSelector = new ApiVersionSelector(options);
            var expectedVersion = new ApiVersion(1, 0);

            var mockHttpHeaders = new HeaderDictionary(new Dictionary<string, StringValues>(StringComparer.OrdinalIgnoreCase));
            mockHttpHeaders.Add("x-v", new StringValues(""));

            var mockHttpRequest = Substitute.For<HttpRequest>();
            mockHttpRequest.Path.Returns(new PathString("/cdr-register/v1/all/data-holders/brands"));
            mockHttpRequest.Headers.Returns(mockHttpHeaders);

            // Act.
            var actualVersion = versionSelector.SelectVersion(mockHttpRequest, null); // apiVersionModel);

            // Assert.
            Assert.NotNull(actualVersion);
            Assert.Equal(expectedVersion.MajorVersion, actualVersion.MajorVersion);
            Assert.Equal(expectedVersion.MinorVersion, actualVersion.MinorVersion);
        }

        [Fact]
        public void SelectVersion_InvalidXvHeader_ShouldThrowInvalidVersionException()
        {
            // Arrange.
            var options = new ApiVersioningOptions() { DefaultApiVersion = new ApiVersion(1, 0) };
            var versionSelector = new ApiVersionSelector(options);
            var expectedVersion = new ApiVersion(1, 0);

            var mockHttpHeaders = new HeaderDictionary(new Dictionary<string, StringValues>(StringComparer.OrdinalIgnoreCase));
            mockHttpHeaders.Add("x-v", new StringValues("foo"));

            var mockHttpRequest = Substitute.For<HttpRequest>();
            mockHttpRequest.Path.Returns(new PathString("/cdr-register/v1/all/data-holders/brands"));
            mockHttpRequest.Headers.Returns(mockHttpHeaders);

            // Act.
            Assert.Throws<InvalidVersionException>(() => versionSelector.SelectVersion(mockHttpRequest, null));

            // Assert.
        }

        [Fact]
        public void SelectVersion_SetToZeroXvHeader_ShouldThrowInvalidVersionException()
        {
            // Arrange.
            var options = new ApiVersioningOptions() { DefaultApiVersion = new ApiVersion(1, 0) };
            var versionSelector = new ApiVersionSelector(options);
            var expectedVersion = new ApiVersion(1, 0);

            var mockHttpHeaders = new HeaderDictionary(new Dictionary<string, StringValues>(StringComparer.OrdinalIgnoreCase));
            mockHttpHeaders.Add("x-v", new StringValues("0"));

            var mockHttpRequest = Substitute.For<HttpRequest>();
            mockHttpRequest.Path.Returns(new PathString("/cdr-register/v1/all/data-holders/brands"));
            mockHttpRequest.Headers.Returns(mockHttpHeaders);

            // Act.
            Assert.Throws<InvalidVersionException>(() => versionSelector.SelectVersion(mockHttpRequest, null));

            // Assert.
        }

        [Fact]
        public void SelectVersion_LessThanZeroXvHeader_ShouldThrowInvalidVersionException()
        {
            // Arrange.
            var options = new ApiVersioningOptions() { DefaultApiVersion = new ApiVersion(1, 0) };
            var versionSelector = new ApiVersionSelector(options);
            var expectedVersion = new ApiVersion(1, 0);

            var mockHttpHeaders = new HeaderDictionary(new Dictionary<string, StringValues>(StringComparer.OrdinalIgnoreCase));
            mockHttpHeaders.Add("x-v", new StringValues("-1"));

            var mockHttpRequest = Substitute.For<HttpRequest>();
            mockHttpRequest.Path.Returns(new PathString("/cdr-register/v1/all/data-holders/brands"));
            mockHttpRequest.Headers.Returns(mockHttpHeaders);

            // Act.
            Assert.Throws<InvalidVersionException>(() => versionSelector.SelectVersion(mockHttpRequest, null));

            // Assert.
        }

        [Fact]
        public void SelectVersion_GreaterThanMaxXvHeader_ShouldThrowUnsupportedVersionException()
        {
            // Arrange.
            var options = new ApiVersioningOptions() { DefaultApiVersion = new ApiVersion(1, 0) };
            var versionSelector = new ApiVersionSelector(options);
            var expectedVersion = new ApiVersion(1, 0);
            var mockHttpHeaders = new HeaderDictionary(new Dictionary<string, StringValues>(StringComparer.OrdinalIgnoreCase));
            mockHttpHeaders.Add("x-v", new StringValues("99"));

            var mockHttpRequest = Substitute.For<HttpRequest>();
            mockHttpRequest.Path.Returns(new PathString("/cdr-register/v1/all/data-holders/brands"));
            mockHttpRequest.Headers.Returns(mockHttpHeaders);

            // Act.
            Assert.Throws<UnsupportedVersionException>(() => versionSelector.SelectVersion(mockHttpRequest, null));

            // Assert.
        }

        [Fact]
        public void SelectVersion_ValidXvHeader_ShouldReturnValidVersion()
        {
            // Arrange.
            var options = new ApiVersioningOptions() { DefaultApiVersion = new ApiVersion(1, 0) };
            var versionSelector = new ApiVersionSelector(options);
            var expectedVersion = new ApiVersion(2, 0);
            var mockHttpHeaders = new HeaderDictionary(new Dictionary<string, StringValues>(StringComparer.OrdinalIgnoreCase));
            mockHttpHeaders.Add("x-v", new StringValues("2"));

            var mockHttpRequest = Substitute.For<HttpRequest>();
            mockHttpRequest.Path.Returns(new PathString("/cdr-register/v1/all/data-holders/brands"));
            mockHttpRequest.Headers.Returns(mockHttpHeaders);

            // Act.
            var actualVersion = versionSelector.SelectVersion(mockHttpRequest, null); // apiVersionModel);

            // Assert.
            Assert.NotNull(actualVersion);
            Assert.Equal(expectedVersion.MajorVersion, actualVersion.MajorVersion);
            Assert.Equal(expectedVersion.MinorVersion, actualVersion.MinorVersion);
        }

        [Fact]
        public void SelectVersion_UppercaseXvHeader_ShouldReturnValidVersion()
        {
            // Arrange.
            var options = new ApiVersioningOptions() { DefaultApiVersion = new ApiVersion(1, 0) };
            var versionSelector = new ApiVersionSelector(options);
            var expectedVersion = new ApiVersion(2, 0);
            var mockHttpHeaders = new HeaderDictionary(new Dictionary<string, StringValues>(StringComparer.OrdinalIgnoreCase));
            mockHttpHeaders.Add("X-V", new StringValues("2"));

            var mockHttpRequest = Substitute.For<HttpRequest>();
            mockHttpRequest.Path.Returns(new PathString("/cdr-register/v1/all/data-holders/brands"));
            mockHttpRequest.Headers.Returns(mockHttpHeaders);

            // Act.
            var actualVersion = versionSelector.SelectVersion(mockHttpRequest, null); // apiVersionModel);

            // Assert.
            Assert.NotNull(actualVersion);
            Assert.Equal(expectedVersion.MajorVersion, actualVersion.MajorVersion);
            Assert.Equal(expectedVersion.MinorVersion, actualVersion.MinorVersion);
        }

        [Fact]
        public void SelectVersion_InvalidPath_ShouldReturn1()
        {
            // Arrange.
            var options = new ApiVersioningOptions() { DefaultApiVersion = new ApiVersion(1, 0) };
            var versionSelector = new ApiVersionSelector(options);
            var expectedVersion = new ApiVersion(1, 0);
            var mockHttpHeaders = new HeaderDictionary(new Dictionary<string, StringValues>(StringComparer.OrdinalIgnoreCase));
            mockHttpHeaders.Add("X-V", new StringValues("2"));

            var mockHttpRequest = Substitute.For<HttpRequest>();
            mockHttpRequest.Path.Returns(new PathString("/invalid/path"));
            mockHttpRequest.Headers.Returns(mockHttpHeaders);

            // Act.
            var actualVersion = versionSelector.SelectVersion(mockHttpRequest, null); // apiVersionModel);

            // Assert.
            Assert.NotNull(actualVersion);
            Assert.Equal(expectedVersion.MajorVersion, actualVersion.MajorVersion);
            Assert.Equal(expectedVersion.MinorVersion, actualVersion.MinorVersion);
        }

        [Fact]
        public void SelectVersion_WithRange_ShouldReturnMaxVersion()
        {
            // Arrange.
            var options = new ApiVersioningOptions() { DefaultApiVersion = new ApiVersion(1, 0) };
            var versionSelector = new ApiVersionSelector(options);
            var expectedVersion = new ApiVersion(3, 0);
            var mockHttpHeaders = new HeaderDictionary(new Dictionary<string, StringValues>(StringComparer.OrdinalIgnoreCase));
            mockHttpHeaders.Add("x-v", new StringValues("4"));
            mockHttpHeaders.Add("x-min-v", new StringValues("3"));

            var mockHttpRequest = Substitute.For<HttpRequest>();
            mockHttpRequest.Path.Returns(new PathString("/cdr-register/v1/all/data-recipients"));
            mockHttpRequest.Headers.Returns(mockHttpHeaders);

            // Act.
            var actualVersion = versionSelector.SelectVersion(mockHttpRequest, null); // apiVersionModel);

            // Assert.
            Assert.NotNull(actualVersion);
            Assert.Equal(expectedVersion.MajorVersion, actualVersion.MajorVersion);
            Assert.Equal(expectedVersion.MinorVersion, actualVersion.MinorVersion);
        }

        [Fact]
        public void SelectVersion_WithUppercaseXminV_ShouldReturnMaxVersion()
        {
            // Arrange.
            var options = new ApiVersioningOptions() { DefaultApiVersion = new ApiVersion(1, 0) };
            var versionSelector = new ApiVersionSelector(options);
            var expectedVersion = new ApiVersion(3, 0);
            var mockHttpHeaders = new HeaderDictionary(new Dictionary<string, StringValues>(StringComparer.OrdinalIgnoreCase));
            mockHttpHeaders.Add("x-v", new StringValues("4"));
            mockHttpHeaders.Add("X-MIN-V", new StringValues("3"));

            var mockHttpRequest = Substitute.For<HttpRequest>();
            mockHttpRequest.Path.Returns(new PathString("/cdr-register/v1/all/data-recipients"));
            mockHttpRequest.Headers.Returns(mockHttpHeaders);

            // Act.
            var actualVersion = versionSelector.SelectVersion(mockHttpRequest, null); // apiVersionModel);

            // Assert.
            Assert.NotNull(actualVersion);
            Assert.Equal(expectedVersion.MajorVersion, actualVersion.MajorVersion);
            Assert.Equal(expectedVersion.MinorVersion, actualVersion.MinorVersion);
        }

        [Fact]
        public void SelectVersion_XminVGreaterThanXV_ShouldBeIgnored()
        {
            // Arrange.
            var options = new ApiVersioningOptions() { DefaultApiVersion = new ApiVersion(1, 0) };
            var versionSelector = new ApiVersionSelector(options);
            var expectedVersion = new ApiVersion(2, 0);
            var mockHttpHeaders = new HeaderDictionary(new Dictionary<string, StringValues>(StringComparer.OrdinalIgnoreCase));
            mockHttpHeaders.Add("x-v", new StringValues("2"));
            mockHttpHeaders.Add("x-min-v", new StringValues("3"));

            var mockHttpRequest = Substitute.For<HttpRequest>();
            mockHttpRequest.Path.Returns(new PathString("/cdr-register/v1/all/data-recipients"));
            mockHttpRequest.Headers.Returns(mockHttpHeaders);

            // Act.
            var actualVersion = versionSelector.SelectVersion(mockHttpRequest, null); // apiVersionModel);

            // Assert.
            Assert.NotNull(actualVersion);
            Assert.Equal(expectedVersion.MajorVersion, actualVersion.MajorVersion);
            Assert.Equal(expectedVersion.MinorVersion, actualVersion.MinorVersion);
        }

        [Fact]
        public void SelectVersion_InvalidRange_ShouldThrowUnsupportedVersionException()
        {
            // Arrange.
            var options = new ApiVersioningOptions() { DefaultApiVersion = new ApiVersion(1, 0) };
            var versionSelector = new ApiVersionSelector(options);
            var expectedVersion = new ApiVersion(2, 0);
            var mockHttpHeaders = new HeaderDictionary(new Dictionary<string, StringValues>(StringComparer.OrdinalIgnoreCase));
            mockHttpHeaders.Add("x-v", new StringValues("5"));
            mockHttpHeaders.Add("x-min-v", new StringValues("4"));

            var mockHttpRequest = Substitute.For<HttpRequest>();
            mockHttpRequest.Path.Returns(new PathString("/cdr-register/v1/all/data-recipients"));
            mockHttpRequest.Headers.Returns(mockHttpHeaders);

            // Act.
            Assert.Throws<UnsupportedVersionException>(() => versionSelector.SelectVersion(mockHttpRequest, null));

            // Assert.
        }
    }
}