@echo off
echo Start terminals for projects?
pause

wt --maximized ^
--title Gateway_MTLS -d ../CDR.Register.API.Gateway.mTLS dotnet run; ^
--title Gateway_TLS -d ../CDR.Register.API.Gateway.TLS dotnet run; ^
--title IdentityServer -d ../CDR.Register.IdentityServer dotnet run; ^
--title Discovery_API -d ../CDR.Register.Discovery.API dotnet run; ^
--title SSA_API -d ../CDR.Register.SSA.API dotnet run; ^
--title Status_API -d ../CDR.Register.Status.API dotnet run; ^
--title Admin_API -d ../CDR.Register.Admin.API dotnet run
 