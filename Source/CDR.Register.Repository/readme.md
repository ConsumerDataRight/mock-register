# EF Migrations
## Install prerequisite tooling
`dotnet tool restore`

## Adding migrations
> Migrations are set up to use `MSSQLLocalDB\cdr-register-migrations` database during design time in order to not conflict with any other configuration such as the database used for `Development.Local` or other environments.

Update the migrations database
 - `dotnet ef database update --project CDR.Register.Repository.csproj -c CDR.Register.Repository.Infrastructure.RegisterDatabaseContext`

Examples for generating migration script which will be added under the migrations folder
- `dotnet ef migrations add <Name> --project CDR.Register.Repository.csproj -c CDR.Register.Repository.Infrastructure.RegisterDatabaseContext`
