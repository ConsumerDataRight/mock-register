@echo off
REM echo Start terminals for projects?
REM pause

REM wt --maximized ^
REM --title Gateway_MTLS -d ../CDR.Register.API.Gateway.mTLS dotnet run; ^
REM --title Gateway_TLS -d ../CDR.Register.API.Gateway.TLS dotnet run; ^
REM --title Infosec -d ../CDR.Register.Infosec dotnet run; ^
REM --title Discovery_API -d ../CDR.Register.Discovery.API dotnet run; ^
REM --title SSA_API -d ../CDR.Register.SSA.API dotnet run; ^
REM --title Status_API -d ../CDR.Register.Status.API dotnet run; ^
REM --title Admin_API -d ../CDR.Register.Admin.API dotnet run
 
docker compose -f ../docker-compose.IntegrationTests.yml up -d --build mssql mock-register
echo Supporting infrastructure for tests will now stop.
pause