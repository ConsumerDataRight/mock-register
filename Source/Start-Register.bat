@echo off
echo Run solutions from .Net CLI using localhost and localdb from appsettings.Development.json
pause

setx ASPNETCORE_ENVIRONMENT Development

dotnet build CDR.Register.API.Gateway.mTLS
dotnet build CDR.Register.API.Gateway.TLS
dotnet build CDR.Register.Infosec
dotnet build CDR.Register.Discovery.API
dotnet build CDR.Register.SSA.API
dotnet build CDR.Register.Status.API
dotnet build CDR.Register.Admin.API

wt --maximized ^
--title MR_MTLS -d CDR.Register.API.Gateway.mTLS dotnet run --no-launch-profile; ^
--title MR_TLS -d CDR.Register.API.Gateway.TLS dotnet run --no-launch-profile; ^
--title MR_Infosec -d CDR.Register.Infosec dotnet run --no-launch-profile; ^
--title MR_Disc_API -d CDR.Register.Discovery.API dotnet run --no-launch-profile; ^
--title MR_SSA_API -d CDR.Register.SSA.API dotnet run --no-launch-profile; ^
--title MR_Stat_API -d CDR.Register.Status.API dotnet run --no-launch-profile; ^
--title MR_Admn_API -d CDR.Register.Admin.API dotnet run --no-launch-profile