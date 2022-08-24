# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [1.0.1] - 2022-08-30
### Changed
- Removed CDR.Register.IdentityServer project that relied on Identity Server 4 and replaced with CDR.Register.Infosec project.
- Updated package references.

### Fixed
- Fixed incorrect element in Get Data Holder Brands response. [Issue 1](https://github.com/ConsumerDataRight/sandbox/issues/1)
- Fixed issue with authorisation scopes for latest version of authenticated register APIs. Get Data Holder Brands and Get SSA.

### Changed
- Removed CDR.Register.IdentityServer project that relied on Identity Server 4 and replaced with CDR.Register.Infosec project.

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
