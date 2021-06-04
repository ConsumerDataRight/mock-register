# Certificate Management

Certificates play an important part in the CDR ecosystem to establish trust between participants and protect communications.  **DigiCert** is the Certificate Authority (CA) for the CDR and the ACCC is responsible for provisioning DigiCert certificates to participants during the on-boarding process.

For more information, consult the [Certificate Management](https://cdr-register.github.io/register/#certificate-management) section of the Register Design.

The Mock Register will mimic the behaviour of the CDR ecosystem and therefore will use certificates in its interactions.  However, the use of DigiCert for this person is not feasible or scalable so an alternative approach must be adopted.

There are 3 areas where certificates are used within the Mock Register:
- mTLS
- TLS
- SSA signing and validation

## mTLS

**Mutual Transport Layer Security** is used extensively within the CDR ecosystem.  Data Recipients are provisioned client certificates and will present that certificate when interacting with a Data Holder for consumer data sharing and with the Register when discovering Data Holder Brands and request an SSA.  Data Holders are issued a server certificate for their side of the interaction.  All participants need to validate the certificates presented during the establishment of a mTLS session.

The mTLS certificates that can be used for Mock Solutions are listed below:

| Participant | File Name | Password |
|-------------|-----------|----------|
| Mock CA - Certificate Authority | mtls\ca.pfx | #M0ckCDRCA# |
| Mock Register - Server Certificate | mtls\register.pfx | #M0ckRegister# |
| Mock Data Holder - Server Certificate | mtls\server.pfx | #M0ckDataHolder# |
| Mock Data Recipient - Client Certificate | mtls\client.pfx | #M0ckDataRecipient# |

### Certificate Authority

A self-signed Root CA has been provisioned to handle certificate provisioning and to be used in the certificate validation processes.  The client certificate/s for a data recipient and the server certificate for a data holder will be generated from the self-signed Root CA.  The Register, DHs and DRs will trust valid certificates that have been generated from the self-signed Root CA.

The `openssl` commands to generate the Mock CDR Certificate Authority can be found in: `mtls\ca.cmd`.

### Mock Register - Server Certificate

The Mock Register is issued a server certificate by the Mock CDR CA for mTLS communication.

The `openssl` commands to generate the server certificate by the Mock CDR Certificate Authority can be found in: `mtls\register.cmd`.

### Mock Data Holder - Server Certificate

A Data Holder can use the server certificate issued by the Mock CDR CA for mTLS communication.

The `openssl` commands to generate the server certificate by the Mock CDR Certificate Authority can be found in: `mtls\server.cmd`.  One has already been generated and is available at: `mtls\server.pfx`.

### Mock Data Recipient - Client Certificate

A Data Recipient can use the client certificate issued by the Mock CDR CA for mTLS communication.

The `openssl` commands to generate the client certificate by the Mock CDR Certificate Authority can be found in: `mtls\client.cmd`.  One has already been generated and is available at: `mtls\client.pfx`.  This certificate has been configured for use within the metadata stored within the Mock Register repository.

## TLS

Endpoints that are not protected by mTLS are protected by TLS.  The server certificate used for TLS communication can be provisioned by the CDR CA, or alternatively participants can used a trusted third party CA.

For the mock solutions, self-signed TLS certificates are used.  These self-signed certificates will not use the Mock CDR CA, like the mTLS certificates do.

Some certificates have been generated already and are available in the `tls` folder:

| Participant | File Name | Password |
|-------------|-----------|----------|
| Mock Register | tls\mock-register.pfx | #M0ckRegister# |
| Mock Data Holder | tls\mock-data-holder.pfx | #M0ckDataHolder# |
| Mock Data Recipient | tls\mock-data-recipient.pfx | #M0ckDataRecipient# |

These certificates can be used by the community when developing CDR solutions, or you can generate your own self-signed certificates or alternatively use a third party generated certificate.

## SSA Signing and Validation

When a Data Recipient requests a Software Statement Assertion (SSA) from the Register, the contents of the SSA is signed by a private key managed by the Register.  The Data Holder that receives the SSA can verify its authenticity by utilising the public key data hosted at the Register's SSA JWKS endpoint (`/cdr-register/v1/jwks`).

The private/public key information used within the SSA and Dynamic Client Registration (DCR) process utilises an X509 certificate.  This certificate is a self-signed certificate that is generated using the `openssl` commands found in the `ssa\ssa.cmd` file.
