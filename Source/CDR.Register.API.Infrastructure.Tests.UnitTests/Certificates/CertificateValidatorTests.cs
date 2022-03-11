using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace CDR.Register.API.Infrastructure.Tests.UnitTests.Certificates
{
    public partial class CertificateValidatorTests
    {
        [Fact]
        public void IsValid_ValidCertificate_ShouldReturnTrue()
        {
            // Arrange.
            var expected = true;
            var logger = Substitute.For<ILogger<CertificateValidator>>();
            var rootCaPath = Path.Combine(Directory.GetCurrentDirectory(), "Certificates", "ca.pem");
            var inMemorySettings = new Dictionary<string, string> {
                {"RootCACertificatePath", rootCaPath}
            };

            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();
            var clientCertPath = Path.Combine(Directory.GetCurrentDirectory(), "Certificates", "client.pfx");
            var goodClientCert = new X509Certificate2(clientCertPath, "#M0ckDataRecipient#");
            var validator = new CertificateValidator(logger, configuration);

            // Act.
            var actual = validator.IsValid(goodClientCert);

            // Assert.
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void IsValid_NullCertificate_ShouldThrowException()
        {
            // Arrange.
            var logger = Substitute.For<ILogger<CertificateValidator>>();
            var rootCaPath = Path.Combine(Directory.GetCurrentDirectory(), "Certificates", "ca.pem");
            var inMemorySettings = new Dictionary<string, string> {
                {"RootCACertificatePath", rootCaPath}
            };

            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();
            var clientCertPath = Path.Combine(Directory.GetCurrentDirectory(), "Certificates", "client.pfx");
            var goodClientCert = new X509Certificate2(clientCertPath, "#M0ckDataRecipient#");
            var validator = new CertificateValidator(logger, configuration);

            // Act.
            Assert.Throws<ArgumentNullException>(() => validator.IsValid(null));

            // Assert.
        }

        [Fact]
        public void IsValid_SelfSignedCertificate_ShouldReturnFalse()
        {
            // Arrange.
            var expected = false;
            var logger = Substitute.For<ILogger<CertificateValidator>>();
            var rootCaPath = Path.Combine(Directory.GetCurrentDirectory(), "Certificates", "ca.pem");
            var inMemorySettings = new Dictionary<string, string> {
                {"RootCACertificatePath", rootCaPath}
            };

            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();
            var selfSignedCertPath = Path.Combine(Directory.GetCurrentDirectory(), "Certificates", "ssa.pfx");
            var selfSignedCert = new X509Certificate2(selfSignedCertPath, "#M0ckRegister#");
            var validator = new CertificateValidator(logger, configuration);

            // Act.
            var actual = validator.IsValid(selfSignedCert);

            // Assert.
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void IsValid_FakeMockCDRCACertificate_ShouldReturnFalse()
        {
            // Arrange.
            var expected = false;
            var logger = Substitute.For<ILogger<CertificateValidator>>();
            var rootCaPath = Path.Combine(Directory.GetCurrentDirectory(), "Certificates", "ca.pem");
            var inMemorySettings = new Dictionary<string, string> {
                {"RootCACertificatePath", rootCaPath}
            };

            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();
            var certPath = Path.Combine(Directory.GetCurrentDirectory(), "Certificates", "client-fake-cdr-root-ca.pfx");
            var cert = new X509Certificate2(certPath, "testonly");
            var validator = new CertificateValidator(logger, configuration);

            // Act.
            var actual = validator.IsValid(cert);

            // Assert.
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void IsValid_NonMockCDRCACertificate_ShouldReturnFalse()
        {
            // Arrange.
            var expected = false;
            var logger = Substitute.For<ILogger<CertificateValidator>>();
            var rootCaPath = Path.Combine(Directory.GetCurrentDirectory(), "Certificates", "ca.pem");
            var inMemorySettings = new Dictionary<string, string> {
                {"RootCACertificatePath", rootCaPath}
            };

            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();
            var certPath = Path.Combine(Directory.GetCurrentDirectory(), "Certificates", "client-non-cdr-root-ca.pfx");
            var cert = new X509Certificate2(certPath, "testonly");
            var validator = new CertificateValidator(logger, configuration);

            // Act.
            var actual = validator.IsValid(cert);

            // Assert.
            Assert.Equal(expected, actual);
        }

    }
}
