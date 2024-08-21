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

FROM build AS publish

# Copy the build props 
COPY ./Directory.Build.props /app/Directory.Build.props

COPY ./CDR.Register.API.Infrastructure/. /app/CDR.Register.API.Infrastructure
COPY ./CDR.Register.Repository/. /app/CDR.Register.Repository
COPY ./CDR.Register.Domain/. /app/CDR.Register.Domain
COPY ./CDR.Register.Admin.API/. /app/CDR.Register.Admin.API
COPY ./CDR.Register.Discovery.API/. /app/CDR.Register.Discovery.API
COPY ./CDR.Register.Status.API/. /app/CDR.Register.Status.API
COPY ./CDR.Register.SSA.API/. /app/CDR.Register.SSA.API
COPY ./CDR.Register.Infosec/. /app/CDR.Register.Infosec
COPY ./CDR.Register.API.Gateway.mTLS/. /app/CDR.Register.API.Gateway.mTLS
COPY ./CDR.Register.API.Gateway.TLS/. /app/CDR.Register.API.Gateway.TLS
COPY ./CDR.Register.API.Logger/. /app/CDR.Register.API.Logger

WORKDIR /app/CDR.Register.Admin.API
RUN dotnet publish -c Release -o /app/publish/admin
WORKDIR /app/CDR.Register.Discovery.API
RUN dotnet publish -c Release -o /app/publish/discovery
WORKDIR /app/CDR.Register.Status.API
RUN dotnet publish -c Release -o /app/publish/status
WORKDIR /app/CDR.Register.SSA.API
RUN dotnet publish -c Release -o /app/publish/ssa
WORKDIR /app/CDR.Register.Infosec
RUN dotnet publish -c Release -o /app/publish/infosec
WORKDIR /app/CDR.Register.API.Gateway.mTLS
RUN dotnet publish -c Release -o /app/publish/gateway-mtls
WORKDIR /app/CDR.Register.API.Gateway.TLS
RUN dotnet publish -c Release -o /app/publish/gateway-tls

COPY supervisord.conf /app/publish/supervisord.conf

COPY wait-until-admin-healthy-then-start.sh /app/publish/wait-until-admin-healthy-then-start.sh

FROM base AS final
WORKDIR /app

COPY --from=publish /app/publish/supervisord.conf .
COPY --from=publish /app/publish/wait-until-admin-healthy-then-start.sh .
COPY --from=publish /app/publish/discovery ./discovery
COPY --from=publish /app/publish/admin ./admin
COPY --from=publish /app/publish/status ./status
COPY --from=publish /app/publish/infosec ./infosec
COPY --from=publish /app/publish/gateway-mtls ./gateway-mtls
COPY --from=publish /app/publish/gateway-tls ./gateway-tls
COPY --from=publish /app/publish/ssa ./ssa

RUN apt-get update && apt-get install -y supervisor

RUN apt-get update && apt-get install -y sudo

# Install wget for use in health checks
RUN apt-get update && apt-get install -y wget && rm -rf /var/lib/apt/lists/*

RUN sudo cp ./gateway-mtls/Certificates/ca.crt /usr/local/share/ca-certificates/ca.crt

RUN sudo update-ca-certificates

RUN addgroup --group appgroup --gid 2000 \
&& adduser \    
    --uid 1000 \
    --gid 2000 \
    "appuser" 

RUN chown -R appuser:appgroup /app
RUN chown -R appuser:appgroup /usr/bin
RUN chown -R appuser:appgroup /usr/local
RUN chown -R appuser:appgroup /tmp
USER appuser:appgroup 

ENV ASPNETCORE_URLS=https://+:7000;https://+:7001

ENTRYPOINT ["/usr/bin/supervisord", "-c", "/app/supervisord.conf", "-u", "1000"]
