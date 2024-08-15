# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [2.1.0] - 2024-08-16
### Changed
- Updated NuGet packages

## [2.0.0] - 2024-06-12
### Changed
- Replaced Postman collections with a Polyglot Notebook solution
- Migrated from .NET 6 to .NET 8
- Migrated docker compose from v1 to v2
- Added SSL Server Validation capability with switched off by default
- Added custom User-Agent header for request filtering on GetClientJwks

## [1.4.0] - 2024-03-13
### Changed
- Updated NuGet packages to avoid vulnerabilities

### Fixed
- Location for log file in appsettings file is fixed - [Issue 65](https://github.com/ConsumerDataRight/mock-register/issues/65)

## [1.3.4] - 2024-02-14
### Fixed
- lastUpdated field fixed to populate the correct value
- Remove the old Scope "cdr-register:bank:read" references
- Added property code_challenge_methods_supported in Mock Register

## [1.3.3] - 2024-02-01
### Fixed
- Audience validations for token endpoint have been fixed

## [1.3.2] - 2023-11-29
### Changed
- Refactored code and fixed code smells
- Removed Mock Data Holder Energy mTLS server certificate as Mock Data Holder Banking and Mock Data Holder Energy mTLS server certificates are now combined.

### Fixed
- Certificate DN for Register and Data Holder are updated - [Issue 60](https://github.com/ConsumerDataRight/mock-register/issues/60)

## [1.3.1] - 2023-10-30
### Changed
- Selflinks to use public host name / secure host name if configured.

## [1.3.0] - 2023-08-22
### Changed
- Client_id is now optional in the access token request.
- Removed client certificate thumbprint check from the access token request.
- Accreditation Number format has been updated in seed data.

## [1.2.1] - 2023-06-20

### Changed
- Regenerated all mTLS, SSA and TLS certificates to allow for another five years before they expire.
- Append base path to URIs in discovery document

### Fixed
- Links in readme

## [1.2.0] - 2023-06-07
### Added
- Added new API admin/metadata/data-recipients to add or update a Data Recipient into the Register.
- Added new API admin/metadata/data-holders to add or update a Data Holder into the Register.
- Added dynamic base path to allow changes to external facing URLs.

### Changed
- Retired end of life APIs.

## [1.1.1] - 2023-03-20
### Added
- The 'jti' claim in the client assertion sent to the token endpoint is now checked for re-use.  An error is raised if the jti value is re-used
- Register client certificate

### Changed
- Some error codes that were not aligned with the CDR Register are now aligned
- Database structure to move legal entity status to participation status

## [1.1.0] - 2022-10-05
### Added
- Logging middleware to create a centralised list of all API requests and responses

### Changed
- Get SSA x-v 1 and 2 now only return banking scopes. x-v 3 still returns banking and energy scopes
- Transactional database tables updated to temporal tables

### Fixed
- Issue occuring when using docker compose eco system. [Issue 52](https://github.com/ConsumerDataRight/mock-data-recipient/issues/52)

## [1.0.1] - 2022-08-30
### Changed
- Removed CDR.Register.IdentityServer project that relied on Identity Server 4 and replaced with CDR.Register.Infosec project.
- Updated package references.

### Fixed
- Fixed incorrect element in Get Data Holder Brands response. [Issue 1](https://github.com/ConsumerDataRight/sandbox/issues/1)
- Fixed issue with authorisation scopes for latest version of authenticated register APIs. Get Data Holder Brands and Get SSA.

## [1.0.0] - 2022-07-22
### Changed
- First version of the Mock Register deployed into the CDR Sandbox.

## [0.5.1] - 2022-06-09
### Added
- Split the Docker compose functionality. There is now a Docker Compose file to start the mock CDR ecosystem and a Docker Compose file to start the mock register on its own.

### Changed
- Updated container help instructions
- Updated Read Me
- Updated Certificate Management Read Me
- Build and Test action to archive test results. Add Test Report action.

## [0.5.0] - 2022-05-25
### Added
- Support for multiple industries added to the Mock Register. Banking, Energy, Telco and 'all' are now valid industries for GetDataRecipients, GetDataRecipientsStatus, GetSoftwareProductStatus, GetSoftwareStatementAssertion, GetDataHolderBrands, GetDataHoldersStatus. Banking industry only versions of these APIs are available for backwards compatibility.
- Energy and Telco as supported sectors.
- GetDataHoldersStatus API.
- Data Holder Brands entry to support and represent the upcoming [Mock Data Holder Energy](https://github.com/ConsumerDataRight/mock-data-holder-energy).
- Energy scopes for the [Mock Data Recipient](https://github.com/ConsumerDataRight/mock-data-recipient).

### Changed
- Upgraded the Mock Register codebase to .NET 6.
- Replaced SQLite database with MSSQL database.
- Changed the TLS certificates for the Mock Register to be signed by the Mock CDR CA.
- Extra steps detailed for using the solution in visual studio, docker containers and docker compose file.
- Regenerated all mTLS, SSA and TLS certificates to allow for another year before they expire.

## [0.4.0] - 2021-10-01
### Added 
- docker-compose file to orchestrate mock-register/mock-data-holder/mock-data-recipient containers.

### Changed
- Updated hosts in seed-data.json and appsettings.Production.json to reflect container names.
- Added docker-compose information to readme file

## [0.3.0] - 2021-09-06
### Added
 - `access_token` variable within the Postman collection, to make calling requests that need it easier.

### Security
 - Bumped `Microsoft.AspNetCore.Authentication.JwtBearer` from `5.0.5` to `5.0.9` due to [CVE-2021-34532](https://github.com/advisories/GHSA-q7cg-43mg-qp69)

## [0.2.0] - 2021-07-15
### Added
- Added an `appsettings.Pipeline.json` file to each project to set configuration that can be used in a DevOps build pipeline.
- Added `.github\workflows\dotnet.yml` file to build and test the source code on check in and PR to `develop` and `main`.
- Added `.github\workflows\docker.yml` file to build the docker container on check in and PR to `develop` and `main`.

### Changed
- Updated the `launchSettings.json` file for each project to include a `Production` and `Pipeline` profile.
- Removed the hardcoded connection string from the integration tests and replaced with configuration.

### Fixed
- Include `industry` field in Get Data Holder Brands response.
- Moved PR template to `.github/pull_request_template.md` so that it correctly autofills a new PR.
- Fixed when the SonarCloud action runs so that it doesnt error on PRs that aren't allowed access to secrets.

## [0.1.0] - 2021-06-04
### Added
- First release of the Mock Register.
