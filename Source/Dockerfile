# check=skip=JSONArgsRecommended
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 7000
EXPOSE 7001
EXPOSE 7006

# Default ASPNETCORE_ENVIRONMENT to Release
ENV ASPNETCORE_ENVIRONMENT=Release 

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY . ./
RUN dotnet build --configuration Release

# Install ca certificate
RUN apt-get update && apt-get install -y sudo
RUN sudo cp ./CDR.Register.API.Gateway.mTLS/Certificates/ca.crt /usr/local/share/ca-certificates/ca.crt
RUN sudo update-ca-certificates

FROM build AS unit-tests
ENTRYPOINT ["dotnet", "test", "--configuration", "Release", "--filter", "Category=UnitTests", "--no-build", "--logger", "trx;verbosity=detailed", "--results-directory", "/testresults"]

FROM build AS integration-tests
WORKDIR /src/CDR.Register.IntegrationTests
ENTRYPOINT dotnet test --configuration Release --no-build --filter ${TEST_FILTER} --logger "trx;verbosity=detailed;LogFileName=results.trx;" --results-directory /testresults

FROM build AS publish

# Copy the build props 
COPY supervisord.conf /app/publish/supervisord.conf
COPY wait-until-admin-healthy-then-start.sh /app/publish/wait-until-admin-healthy-then-start.sh
RUN dotnet publish -c Release
RUN cp -r CDR.Register.API.Gateway.mTLS/bin/Release/net8.0/publish /app/publish/gateway-mtls
RUN cp -r CDR.Register.API.Gateway.TLS/bin/Release/net8.0/publish /app/publish/gateway-tls
RUN cp -r CDR.Register.SSA.API/bin/Release/net8.0/publish /app/publish/ssa
RUN cp -r CDR.Register.Status.API/bin/Release/net8.0/publish /app/publish/status
RUN cp -r CDR.Register.Admin.API/bin/Release/net8.0/publish /app/publish/admin
RUN cp -r CDR.Register.Discovery.API/bin/Release/net8.0/publish /app/publish/discovery
RUN cp -r CDR.Register.Infosec/bin/Release/net8.0/publish /app/publish/infosec

FROM base AS final
WORKDIR /app

COPY --from=publish /app/publish .

RUN apt-get update && apt-get install -y supervisor

# Install wget for use in health checks
RUN apt-get update && apt-get install -y wget && rm -rf /var/lib/apt/lists/*

RUN apt-get update && apt-get install -y sudo

# Trust CDR CA certificate
RUN sudo cp ./gateway-mtls/Certificates/ca.crt /usr/local/share/ca-certificates/ca.crt
RUN sudo update-ca-certificates

RUN addgroup --group appgroup --gid 2000 \
&& adduser \    
    --uid 1000 \
    --gid 2000 \
    "appuser" 

RUN chown -R appuser:appgroup /app \
    && chown -R appuser:appgroup /usr/bin \
    && chown -R appuser:appgroup /usr/local \
    && chown -R appuser:appgroup /tmp

    USER appuser:appgroup 

ENV ASPNETCORE_URLS=https://+:7000;https://+:7001

ENTRYPOINT ["/usr/bin/supervisord", "-c", "/app/supervisord.conf", "-u", "1000"]
