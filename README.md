![Consumer Data Right Logo](https://raw.githubusercontent.com/ConsumerDataRight/mock-register/main/cdr-logo.png) 

[![Consumer Data Standards v1.17.0](https://img.shields.io/badge/Consumer%20Data%20Standards-v1.17.0-blue.svg)](https://consumerdatastandardsaustralia.github.io/standards/#introduction)
[![made-with-dotnet](https://img.shields.io/badge/Made%20with-.NET-1f425Ff.svg)](https://dotnet.microsoft.com/)
[![made-with-csharp](https://img.shields.io/badge/Made%20with-C%23-1f425Ff.svg)](https://docs.microsoft.com/en-us/dotnet/csharp/)
[![MIT License](https://img.shields.io/github/license/ConsumerDataRight/mock-register)](./LICENSE)
[![Pull Requests Welcome](https://img.shields.io/badge/PRs-welcome-brightgreen.svg)](./CONTRIBUTING.md)

# Consumer Data Right - Mock Register
This project includes source code, documentation and instructions for the Consumer Data Right (CDR) Mock Register.

The ACCC operates the CDR Register within the CDR ecosystem.  This repository contains a mock implementation of the Mock Register and is offered to help the community in the development and testing of their CDR solutions.

## Mock Register - Alignment
The Mock Register aligns to [v1.17.0](https://consumerdatastandardsaustralia.github.io/standards/#introduction) of the [Consumer Data Standards](https://consumerdatastandardsaustralia.github.io/standards/#introduction).

## Getting Started
There are a number of ways that the artefacts within this project can be used:
1. Build and deploy the source code
2. Use the pre-built image
3. Use the docker compose file to run a multi-container mock CDR Ecosystem

### Build and deploy the source code

To get started, clone the source code.
```
git clone https://github.com/ConsumerDataRight/mock-register.git
```

To get help on launching and debugging the solution, see the [help guide](./Help/debugging/HELP.md).

If you would like to contribute features or fixes back to the Mock Register repository, consult the [contributing guidelines](CONTRIBUTING.md).

### Use the pre-built image

A version of the Mock Register is built into a single Docker image that is made available via [docker hub](https://hub.docker.com/r/consumerdataright/mock-register).

#### Pull the latest image

```
docker pull consumerdataright/mock-register
```

To get help on running the Mock Register container, see the [help guide](./Help/container/HELP.md).

### Use the docker-compose.Ecosystem.yml file to run a multi-container Mock CDR Ecosystem

The [docker-compose.Ecosystem.yml file](Source/docker-compose.Ecosystem.yml) can be used to run multiple containers to create a Mock CDR Ecosystem.

To get help on launching the Mock CDR Ecosystem, see the [help guide](./Help/container/HELP.md). The [help guide](./Help/container/HELP.md) also contains instructions for swapping out one of the mock solutions running in the multi-container Mock CDR Ecosystem with a mock solution running in MS Visual Studio or with your own solution. 

## Try it out

Once the Mock Register is running, you can use the provided [Mock Register Postman API collection](Postman/README.md) to try it out.

## Certificate Management

Consult the [Certificate Management](CertificateManagement/README.md) documentation for more information about how certificates are used for the Mock Register.

## Loading your own data

When the Mock Register first starts it will load data from the included `seed-data.json` file that is included in the `CDR.Register.Admin.API` project.  This file includes dummy metadata for data holders and data recipients.  When calls are made to the Mock Register endpoints the dummy metadata is returned.

There are a couple of ways to load your own data into the Mock Register:
1. Provide your own `seed-data.json` file within the Mock Register
    - Mock Register container:
        - Within the `/app/admin/Data` folder of the container, make a copy of the `seed-data.Release.json` file, renaming to a name of your choice, e.g. `my-seed-data.json`.
        - Update your seed data file with your desired metadata.
        - Change the `/app/admin/appsettings.Release.json` file to load the new data file and overwrite the existing data:

        ```
        "SeedData": {
            "FilePath": "Data/my-seed-data.json",
            "OverwriteExistingData": true
        },
        ```

        - Restart the Mock Register container.

    - Mock Register source:
        - Within the `.\Source\CDR.Register.Admin.API\Data` folder, make a copy of the `seed-data.Development.json` file, renaming to a name of your choice, e.g. `my-seed-data.json`.
        - Update your seed data file with your desired metadata.
        - Change the `.\Source\appsettings.Development.json` file to load the new data file and overwrite the existing data:

        ```
        "SeedData": {
            "FilePath": "Data/my-seed-data.json",
            "OverwriteExistingData": true
        },
        ```

        - Build and run the Mock Register.

2. Use the Admin API endpoint to load data

The Mock Register includes an Admin API that allows metadata to be downloaded and uploaded from the repository.  

To retrieve the current metadata held within the repository make the following request to the Admin API:

```
GET https://localhost:7006/admin/metadata
```

The response will be metadata in JSON format that conforms to the same structure of the `seed-data.json` file.  This payload structure is also the same structure that is used to load new metadata via the Admin API.

To re-load the repository with metadata make the following request to the Admin API:

**Note: calling this API will delete all existing metadata and re-load with the provided metadata** 

```
POST https://localhost:7006/admin/metadata

{
    <JSON metadata - as per the GET /admin/metadata response or seed-data.json file>
}
```

**Note:** there is currently no authentication/authorisation applied to the Admin API endpoints as these are seen to be under the control of the container owner.  This can be added if there is community feedback to that effect or if a pull request is submitted.

## Mock Register - Architecture
The following diagram outlines the high level architecture of the Mock Register:

[<img src="mock-register-architecture.png" height='600' width='850' alt="Mock Register - Architecture"/>](mock-register-architecture.png)

## Mock Register - Components
The Mock Register contains the following components:

- TLS Gateway
  - Hosted at `https://localhost:7000`
  - Provides the base URL endpoint for TLS only Register communications
- mTLS Gateway
  - Hosted at `https://localhost:7001`
  - Provides the base URL endpoint for mTLS Register communications
  - Performs certificate validation
- Identity Provider
  - Hosted at `https://localhost:7002`
  - Register identity provider implementation utilising `.Net 6`
  - Accessed via the TLS and mTLS Gateways, depending on the target endpoint.
- Discovery API
  - Hosted at `https://localhost:7003`
  - Contains the `GetDataHolderBrands` and `GetDataRecipients` Register APIs.
  - Accessed via the TLS Gateway (`GetDataRecipients`) and mTLS gateway (`GetDataHolderBrands`).
- Status API
  - Hosted at `https://localhost:7004`
  - Contains the `GetDataRecipientsStatus`, `GetSoftwareProductsStatus` and `GetDataHoldersStatus` Register APIs.
  - Accessed via the TLS Gateway.
- SSA API
  - Hosted at `https://localhost:7005`
  - Contains the `SSA JWKS` and `GetSoftwareStatementAssertion` Register APIs.
  - Accessed via the TLS (`SSA JWKS`) and mTLS Gateway (`GetSoftwareStatementAssertion`).
- Admin API
  - Hosted at `https://localhost:7006`
  - Not part of the Register standard, but allows for the maintenance of metadata in the Mock Register repository.
  - A user interface may be added at some time in the future to provide user friendly access to the repository data.
  - Contains `loopback` dummy Data Recipient endpoints used for testing purposes.
- Repository
  - A SQL database containing Register metadata.

## Technology Stack

The following technologies have been used to build the Mock Register:
- The source code has been written in `C#` using the `.Net 6` framework.
- The Identity Provider is implemented using `.Net 6`.
- The TLS and mTLS Gateways have been implemented using `Ocelot`.
- The Repository utilises a `SQL` instance.

# Testing

A collection of API requests has been made available in [Postman](https://www.postman.com/) in order to test the Mock Register and view the expected interactions.  See the Mock Register [Postman](Postman/README.md) documentation for more information.

# Frequently Asked Questions
See the [Frequently Asked Questions](./Help/faq/README.md) page for answers to some common questions and links to help files.

# Contribute
We encourage contributions from the community.  See our [contributing guidelines](CONTRIBUTING.md).

# Code of Conduct
This project has adopted the **Contributor Covenant**.  For more information see the [code of conduct](CODE_OF_CONDUCT.md).

# License
[MIT License](./LICENSE)

# Notes
The Mock Register is provided as a development tool and is not an exact replica of the production CDR Register.  
It conforms to the Consumer Data Standards, however there may be slight differences due to infrastructure and technology stack variances with the production CDR Register.
