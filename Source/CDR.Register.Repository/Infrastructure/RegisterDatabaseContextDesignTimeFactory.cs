using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CDR.Register.Repository.Infrastructure
{
    /// <summary>
    /// This is the DB context initialisation for the tooling such as migrations.
    /// When the tooling runs the migration, it looks first for a class that implements IDesignTimeDbContextFactory and if found, 
    /// it will use that for configuring the context. Runtime behavior is not affected by any configuration set in the factory class.
    /// </summary>
    public class RegisterDatabaseContextDesignTimeFactory : IDesignTimeDbContextFactory<RegisterDatabaseContext>
    {
        public RegisterDatabaseContextDesignTimeFactory()
        {
            // A parameter-less constructor is required by the EF Core CLI tools.
        }

        public RegisterDatabaseContext CreateDbContext(string[] args)
        {
            var options = new DbContextOptionsBuilder<RegisterDatabaseContext>()
               .UseSqlServer("foo") // connection string is only needed if using "dotnet ef database update ..." to actually run migrations from commandline
               .Options;

            return new RegisterDatabaseContext(options);
        }
    }
}