﻿using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using CDR.Register.API.Infrastructure.Exceptions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CDR.Register.API.Infrastructure
{
    /// <summary>
    /// Validates that a client certificate has been issued by the Mock CDR CA.
    /// </summary>
    public class CertificateValidator : ICertificateValidator
    {
        private readonly ILogger<CertificateValidator> _logger;
        private readonly IConfiguration _config;

        public CertificateValidator(ILogger<CertificateValidator> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;
        }

        public void ValidateClientCertificate(X509Certificate2 clientCert)
        {
            _logger.LogInformation($"Validating certificate within the {nameof(CertificateValidator)}");

            if (clientCert == null)
            {
                throw new ArgumentNullException(nameof(clientCert));
            }

            // Validate that the certificate has been issued by the Mock CDR CA.
            var rootCACertificate = new X509Certificate2(_config.GetValue<string>("RootCACertificatePath"));
            var ch = new X509Chain();
            ch.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
            ch.ChainPolicy.VerificationFlags = X509VerificationFlags.NoFlag;
            ch.ChainPolicy.CustomTrustStore.Clear();
            ch.ChainPolicy.CustomTrustStore.Add(rootCACertificate);
            ch.ChainPolicy.TrustMode = X509ChainTrustMode.CustomRootTrust;

            try
            {
                ch.Build(clientCert);
            }
            catch (Exception ex)
            {
                throw new ClientCertificateException("The certificate chain cannot be discovered from the provided client certificate.", ex);
            }

            if (ch.ChainStatus.Any())
            {
                throw new ClientCertificateException(ch.ChainStatus.First().StatusInformation);
            }
        }
    }
}
