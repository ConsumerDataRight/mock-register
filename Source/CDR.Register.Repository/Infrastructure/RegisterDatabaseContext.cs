using CDR.Register.Repository.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace CDR.Register.Repository.Infrastructure
{

	public class RegisterDatabaseContext : DbContext
	{
		public RegisterDatabaseContext()
		{

		}

		public RegisterDatabaseContext(DbContextOptions<RegisterDatabaseContext> options) : base(options)
		{
		}

		public DbSet<LegalEntity> LegalEntities { get; set; }
		public DbSet<Participation> Participations { get; set; }
		public DbSet<Brand> Brands { get; set; }
		public DbSet<BrandStatus> BrandStatuses { get; set; }
		public DbSet<Endpoint> Endpoints { get; set; }
		public DbSet<AuthDetail> AuthDetails { get; set; }
		public DbSet<SoftwareProduct> SoftwareProducts { get; set; }
		public DbSet<IndustryType> IndustryTypes { get; set; }
		public DbSet<OrganisationType> OrganisationTypes { get; set; }
		public DbSet<ParticipationStatus> ParticicipationStatuses { get; set; }
		public DbSet<ParticipationType> ParticipationTypes { get; set; }
		public DbSet<SoftwareProductStatus> SoftwareProductStatuses { get; set; }
		public DbSet<RegisterUType> RegisterUTypes { get; set; }
		public DbSet<SoftwareProductCertificate> SoftwareProductCertificates { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			foreach (var clrType in modelBuilder.Model.GetEntityTypes().Select(e => e.ClrType))
			{
				// Use the entity name instead of the Context.DbSet<T> name
				// refs https://docs.microsoft.com/en-us/ef/core/modeling/entity-types?tabs=fluent-api#table-name
				modelBuilder.Entity(clrType).ToTable(clrType.Name);
			}

			// Add composite primary keys
			modelBuilder.Entity<AuthDetail>()
				.ToTable("AuthDetail", t => t.IsTemporal())
				.HasKey(c => new { c.BrandId, c.RegisterUTypeId });

			// Configure 1-to-1 relationship.
			modelBuilder.Entity<Brand>()
				.ToTable("Brand", t => t.IsTemporal())
				.HasOne(b => b.Endpoint)
				.WithOne(e => e.Brand);

			// Other, Temporal table configurations
			modelBuilder.Entity<Endpoint>().ToTable("Endpoint", t => t.IsTemporal());
			modelBuilder.Entity<LegalEntity>().ToTable("LegalEntity", t => t.IsTemporal());
			modelBuilder.Entity<Participation>().ToTable("Participation", t => t.IsTemporal());
			modelBuilder.Entity<SoftwareProduct>().ToTable("SoftwareProduct", t => t.IsTemporal());
			modelBuilder.Entity<SoftwareProductCertificate>().ToTable("SoftwareProductCertificate", t => t.IsTemporal());

			// Seed the database with reference data and initial data
			modelBuilder.SeedDatabase();
		}
	}
}
