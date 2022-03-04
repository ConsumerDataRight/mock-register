using System;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using CDR.Register.Repository.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CDR.Register.Repository.Infrastructure
{
    public static class Extensions
    {
        public static void SeedDatabase(this ModelBuilder modelBuilder)
        {
            // Add Seed Data for the reference types
            modelBuilder.Entity<IndustryType>().HasData(
                new IndustryType { IndustryTypeId = IndustryEnum.BANKING, IndustryTypeCode = IndustryEnum.BANKING.ToString().ToLower() },
                new IndustryType { IndustryTypeId = IndustryEnum.ENERGY, IndustryTypeCode = IndustryEnum.ENERGY.ToString().ToLower() });

            modelBuilder.Entity<OrganisationType>().HasData(
                new OrganisationType { OrganisationTypeId = OrganisationTypeEnum.SoleTrader, OrganisationTypeCode = "SOLE_TRADER" },
                new OrganisationType { OrganisationTypeId = OrganisationTypeEnum.Company, OrganisationTypeCode = "COMPANY" },
                new OrganisationType { OrganisationTypeId = OrganisationTypeEnum.Partnership, OrganisationTypeCode = "PARTNERSHIP" },
                new OrganisationType { OrganisationTypeId = OrganisationTypeEnum.Trust, OrganisationTypeCode = "TRUST" },
                new OrganisationType { OrganisationTypeId = OrganisationTypeEnum.GovernmentEntity, OrganisationTypeCode = "GOVERNMENT_ENTITY" },
                new OrganisationType { OrganisationTypeId = OrganisationTypeEnum.Other, OrganisationTypeCode = "OTHER" });

            modelBuilder.Entity<ParticipationType>().HasData(
                new ParticipationType { ParticipationTypeId = ParticipationTypeEnum.Dh, ParticipationTypeCode = "DH" },
                new ParticipationType { ParticipationTypeId = ParticipationTypeEnum.Dr, ParticipationTypeCode = "DR" });

            modelBuilder.Entity<ParticipationStatus>().HasData(
                new ParticipationStatus { ParticipationStatusId = ParticipationStatusEnum.Active, ParticipationStatusCode = "ACTIVE" },
                new ParticipationStatus { ParticipationStatusId = ParticipationStatusEnum.Removed, ParticipationStatusCode = "REMOVED" },
                new ParticipationStatus { ParticipationStatusId = ParticipationStatusEnum.Suspended, ParticipationStatusCode = "SUSPENDED" },
                new ParticipationStatus { ParticipationStatusId = ParticipationStatusEnum.Revoked, ParticipationStatusCode = "REVOKED" },
                new ParticipationStatus { ParticipationStatusId = ParticipationStatusEnum.Surrendered, ParticipationStatusCode = "SURRENDERED" },
                new ParticipationStatus { ParticipationStatusId = ParticipationStatusEnum.Inactive, ParticipationStatusCode = "INACTIVE" });

            modelBuilder.Entity<BrandStatus>().HasData(
                new BrandStatus { BrandStatusId = BrandStatusEnum.Active, BrandStatusCode = "ACTIVE" },
                new BrandStatus { BrandStatusId = BrandStatusEnum.Inactive, BrandStatusCode = "INACTIVE" },
                new BrandStatus { BrandStatusId = BrandStatusEnum.Removed, BrandStatusCode = "REMOVED" });

            modelBuilder.Entity<SoftwareProductStatus>().HasData(
                new SoftwareProductStatus { SoftwareProductStatusId = SoftwareProductStatusEnum.Active, SoftwareProductStatusCode = "ACTIVE" },
                new SoftwareProductStatus { SoftwareProductStatusId = SoftwareProductStatusEnum.Inactive, SoftwareProductStatusCode = "INACTIVE" },
                new SoftwareProductStatus { SoftwareProductStatusId = SoftwareProductStatusEnum.Removed, SoftwareProductStatusCode = "REMOVED" });

            modelBuilder.Entity<RegisterUType>().HasData(
                new RegisterUType { RegisterUTypeId = RegisterUTypeEnum.SignedJwt, RegisterUTypeCode = "SIGNED-JWT" });
        }

        /// <summary>
        /// This is the initial database seed. If there are records in the database, this will not re-seed the database
        /// </summary>
        public async static Task SeedDatabaseFromJsonFile(
            this RegisterDatabaseContext registerDatabaseContext,
            string jsonFileFullPath,
            ILogger logger,
            bool overwriteExistingData = false)
        {
            if (!File.Exists(jsonFileFullPath))
            {
                logger.LogDebug($"Seed data file '{jsonFileFullPath}' not found.");
                return;
            }

            var json = await File.ReadAllTextAsync(jsonFileFullPath);
            await registerDatabaseContext.SeedDatabaseFromJson(json, logger, overwriteExistingData);
        }

        /// <summary>
        /// This is the initial database seed. If there are records in the database, this will not re-seed the database
        /// </summary>
        public async static Task<bool> SeedDatabaseFromJson(
            this RegisterDatabaseContext registerDatabaseContext,
            string json,
            ILogger logger,
            bool overwriteExistingData = false)
        {
            bool hasExistingData = await registerDatabaseContext.Participations.AnyAsync();
            if (hasExistingData && !overwriteExistingData)
            {
                logger.LogInformation("Existing data found in the repository and not set to overwrite.  Repository will not be seeded.  Exiting.");
                return false;
            }

            logger.LogInformation(hasExistingData ?
                 "Existing data found, but set to overwrite.  Seeding data..." :
                 "No existing data found.  Seeding data...");

            return await registerDatabaseContext.ReSeedDatabaseFromJson(json, logger);
        }

        /// <summary>
        /// Retrieves all participant metadata from the database, serialises to JSON and return as a string.
        /// </summary>
        public async static Task<string> GetJsonFromDatabase(
            this RegisterDatabaseContext registerDatabaseContext,
            ILogger logger)
        {
            var allData = await registerDatabaseContext.LegalEntities.AsNoTracking().OrderBy(l => l.LegalEntityName)
                .Include(prop => prop.Participations)
                    .ThenInclude(prop => prop.Brands)
                    .ThenInclude(prop => prop.SoftwareProducts)
                    .ThenInclude(prop => prop.Certificates)

                .Include(prop => prop.Participations)
                    .ThenInclude(prop => prop.Brands)
                    .ThenInclude(prop => prop.AuthDetails)

                .Include(prop => prop.Participations)
                    .ThenInclude(prop => prop.Brands)
                    .ThenInclude(prop => prop.Endpoint)

                .ToListAsync();

            return JsonConvert.SerializeObject(allData, new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore,
            });
        }

        /// <summary>
        /// Re-Seed the database from the input JSON data. All existing data in the database will be removed prior to creating the new data set.
        /// </summary>
        public async static Task<bool> ReSeedDatabaseFromJson(this RegisterDatabaseContext registerDatabaseContext, string json, ILogger logger)
        {
            using (var transaction = registerDatabaseContext.Database.BeginTransaction())
            {
                try
                {
                    logger.LogInformation("Removing the existing data from the repository...");

                    // Remove all existing data in the system
                    var existingLegalEntities = await registerDatabaseContext.LegalEntities.AsNoTracking().ToListAsync();
                    registerDatabaseContext.RemoveRange(existingLegalEntities);
                    registerDatabaseContext.SaveChanges();

                    logger.LogInformation("Existing data removed from the repository.");


                    logger.LogInformation("Adding JSON data to repository...");

                    // Re-create all participants from the incoming JSON.
                    var allData = JsonConvert.DeserializeObject<JObject>(json);
                    if (allData != null && allData.ContainsKey("LegalEntities"))
                    {
                        var newLegalEntities = allData["LegalEntities"].ToObject<LegalEntity[]>();
                        registerDatabaseContext.LegalEntities.AddRange(newLegalEntities);
                        registerDatabaseContext.SaveChanges();

                        // Finally commit the transaction
                        transaction.Commit();

                        logger.LogInformation("JSON data added to the repository.");
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    // Log any errors.
                    logger.LogError($"Error while seeding the database. Error: {ex}");
                    throw;
                }
                return false;
            }
        }
    }
}