using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CDR.Register.Repository.Infrastructure
{
    /// <summary>
    /// Factory for <see cref="RegisterDatabaseContext"/> that is used for EF migrations.
    /// </summary>
    public class RegisterDatabaseContextDesignTimeFactory : IDesignTimeDbContextFactory<RegisterDatabaseContext>
    {
        /// <summary>
        /// Creates the <see cref="RegisterDatabaseContext"/> configured appropriately for adding or updating migrations at design time.
        /// </summary>
        /// <remarks>By default when running commands such as <c>dotnet-ef migrations add</c> the tooling will try and use the db
        /// connection string from appsettings.json which is not set and is intentionally left blank.<br/>
        /// This will override that and point to a local db without requiring config changes that may accidentally be committed.</remarks>
        /// <param name="args">The args.</param>
        /// <returns>The configured context.</returns>
        public RegisterDatabaseContext CreateDbContext(string[] args)
        {
            var options = new DbContextOptionsBuilder<RegisterDatabaseContext>()
               .UseSqlServer("Data Source=(LocalDb)\\MSSQLLocalDB;database=cdr-register-migrations;trusted_connection=yes;Max Pool Size=500;Timeout=200;")
               .Options;

            return new RegisterDatabaseContext(options);
        }
    }
}
