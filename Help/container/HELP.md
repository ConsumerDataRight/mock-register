# Use the pre-built image

The Mock Register image is available on [Docker Hub](https://hub.docker.com/r/consumerdataright/mock-register).

There are a number of ways that this image can be used.

## Pull the Mock Register image from Docker Hub

Run the following command to pull the latest image from Docker Hub:

```
docker pull consumerdataright/mock-register
```

You can also pull a specific image by supplying a tag, such as the name of the branch ("main" or "develop") or a release number:

```
# Pull the image from the main branch
docker pull consumerdataright/mock-register:main

# Pull the 0.5.0 version of the image
docker pull consumerdataright/mock-register:0.5.0
```

## Run the Mock Register

The Mock Register relies on SQL Server for data persistence so the container has a dependency on the SQL Server container image provided by Microsoft.

In order to run the Mock Register and related SQL Server instance, use the provided `docker-compose.yml` file.

```
# Navigate to the Source directory where the docker-compose.yml file is located.
cd .\Source

# Run the Mock Register and SQL Server containers.
docker-compose up -d

# When finished, stop the containers.
docker-compose down
```

**Note:** The `docker-compose.yml` file utilises the Microsoft SQL Server Image from Docker Hub. The Microsoft EULA for the Microsoft SQL Server Image must be accepted to continue.
Set the `ACCEPT_EULA` environment variable to `Y` within the `docker-compose.yml` if you accept the EULA.
See the [Microsoft SQL Server Image](https://hub.docker.com/_/microsoft-mssql-server) on Docker Hub for more information.

```
  mssql:
    container_name: sql1
    image: 'mcr.microsoft.com/mssql/server:2019-latest'
    ports:
      - '1433:1433'
    environment:
      - ACCEPT_EULA=Y
```

## Run a multi-container Mock CDR Ecosystem

Multiple containers can be run concurrently to simulate a CDR ecosystem.  The [Mock Register](https://github.com/ConsumerDataRight/mock-register), [Mock Data Holder](https://github.com/ConsumerDataRight/mock-data-holder), [Mock Data Holder Energy](https://github.com/ConsumerDataRight/mock-data-holder-energy) and [Mock Data Recipient](https://github.com/ConsumerDataRight/mock-data-recipient) containers can be run by using the `docker-compose.Ecosystem.yml` file:

e.g.:
```
# Navigate to the Source directory where the docker-compose.yml file is located.
cd .\Source

# Run the Mock Register and SQL Server containers.
docker-compose -f docker-compose.Ecosystem.yml up -d

# When finished, stop the containers.
docker-compose -f docker-compose.Ecosystem.yml down
```

**Note:** The `docker-compose.Ecosystem.yml` file utilises the Microsoft SQL Server Image from Docker Hub. The Microsoft EULA for the Microsoft SQL Server Image must be accepted to continue.
Set the `ACCEPT_EULA` environment variable to `Y` within the `docker-compose.Ecosystem.yml` if you accept the EULA.
See the [Microsoft SQL Server Image](https://hub.docker.com/_/microsoft-mssql-server) on Docker Hub for more information.

e.g.:
```
  mssql:
    container_name: sql1
    image: 'mcr.microsoft.com/mssql/server:2019-latest'
    ports:
      - '1433:1433'
    environment:
      - ACCEPT_EULA=Y
```
