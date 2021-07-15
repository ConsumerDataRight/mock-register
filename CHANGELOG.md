# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- Added an `appsettings.Pipeline.json` file to each project to set configuration that can be used in a DevOps build pipeline.
- Added `.github\workflows\dotnet.yml` file to build and test the source code on check in to `develop` and `main`.

### Changed
- Updated the `launchSettings.json` file for each project to include a `Production` and `Pipeline` profile.
- Removed the hardcoded connection string from the integration tests and replaced with configuration.

### Fixed
- Include `industry` field in Get Data Holder Brands response.


## [0.1.0] - 2021-06-04

### Added
- First release of the Mock Register.