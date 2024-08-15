using System;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using CDR.Register.Repository.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using DomainEntities = CDR.Register.Domain.Entities;
using System.Collections.Generic;
using CDR.Register.Domain.ValueObjects;

namespace CDR.Register.Repository.Infrastructure
{
    public static class Extensions
    {
        public static void SeedDatabase(this ModelBuilder modelBuilder)
        {
            // Add Seed Data for the reference types
            modelBuilder.Entity<IndustryType>().HasData(
                new IndustryType { IndustryTypeId = Industry.BANKING, IndustryTypeCode = Industry.BANKING.ToString().ToLower() },
                new IndustryType { IndustryTypeId = Industry.ENERGY, IndustryTypeCode = Industry.ENERGY.ToString().ToLower() });

            modelBuilder.Entity<OrganisationType>().HasData(
                new OrganisationType { OrganisationTypeId = OrganisationTypes.SoleTrader, OrganisationTypeCode = "SOLE_TRADER" },
                new OrganisationType { OrganisationTypeId = OrganisationTypes.Company, OrganisationTypeCode = "COMPANY" },
                new OrganisationType { OrganisationTypeId = OrganisationTypes.Partnership, OrganisationTypeCode = "PARTNERSHIP" },
                new OrganisationType { OrganisationTypeId = OrganisationTypes.Trust, OrganisationTypeCode = "TRUST" },
                new OrganisationType { OrganisationTypeId = OrganisationTypes.GovernmentEntity, OrganisationTypeCode = "GOVERNMENT_ENTITY" },
                new OrganisationType { OrganisationTypeId = OrganisationTypes.Other, OrganisationTypeCode = "OTHER" });

            modelBuilder.Entity<ParticipationType>().HasData(
                new ParticipationType { ParticipationTypeId = ParticipationTypes.Dh, ParticipationTypeCode = "DH" },
                new ParticipationType { ParticipationTypeId = ParticipationTypes.Dr, ParticipationTypeCode = "DR" });

            modelBuilder.Entity<ParticipationStatus>().HasData(
                new ParticipationStatus { ParticipationStatusId = ParticipationStatusType.Active, ParticipationStatusCode = "ACTIVE", ParticipationTypeId = null },
                new ParticipationStatus { ParticipationStatusId = ParticipationStatusType.Removed, ParticipationStatusCode = "REMOVED", ParticipationTypeId = ParticipationTypes.Dh },
                new ParticipationStatus { ParticipationStatusId = ParticipationStatusType.Suspended, ParticipationStatusCode = "SUSPENDED", ParticipationTypeId = ParticipationTypes.Dr },
                new ParticipationStatus { ParticipationStatusId = ParticipationStatusType.Revoked, ParticipationStatusCode = "REVOKED", ParticipationTypeId = ParticipationTypes.Dr },
                new ParticipationStatus { ParticipationStatusId = ParticipationStatusType.Surrendered, ParticipationStatusCode = "SURRENDERED", ParticipationTypeId = ParticipationTypes.Dr },
                new ParticipationStatus { ParticipationStatusId = ParticipationStatusType.Inactive, ParticipationStatusCode = "INACTIVE", ParticipationTypeId = null });

            modelBuilder.Entity<BrandStatus>().HasData(
                new BrandStatus { BrandStatusId = BrandStatusType.Active, BrandStatusCode = "ACTIVE" },
                new BrandStatus { BrandStatusId = BrandStatusType.Inactive, BrandStatusCode = "INACTIVE" },
                new BrandStatus { BrandStatusId = BrandStatusType.Removed, BrandStatusCode = "REMOVED" });

            modelBuilder.Entity<SoftwareProductStatus>().HasData(
                new SoftwareProductStatus { SoftwareProductStatusId = SoftwareProductStatusType.Active, SoftwareProductStatusCode = "ACTIVE" },
                new SoftwareProductStatus { SoftwareProductStatusId = SoftwareProductStatusType.Inactive, SoftwareProductStatusCode = "INACTIVE" },
                new SoftwareProductStatus { SoftwareProductStatusId = SoftwareProductStatusType.Removed, SoftwareProductStatusCode = "REMOVED" });

            modelBuilder.Entity<RegisterUType>().HasData(
                new RegisterUType { RegisterUTypeId = RegisterUTypes.SignedJwt, RegisterUTypeCode = "SIGNED-JWT" });
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
                logger.LogDebug("Seed data file '{JsonFileFullPath}' not found.", jsonFileFullPath);
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
            if (hasExistingData)
            {
                if (!overwriteExistingData)
                {
                    logger.LogInformation("Existing data found in the repository and not set to overwrite.  Repository will not be seeded.  Exiting.");
                    return false;
                }

                logger.LogInformation("Existing data found, but set to overwrite.  Seeding data...");
            }
            else
            {
                logger.LogInformation("No existing data found.  Seeding data...");
            }

            return await registerDatabaseContext.ReSeedDatabaseFromJson(json, logger);
        }

        /// <summary>
        /// Retrieves all participant metadata from the database, serialises to JSON and return as a string.
        /// </summary>
        public async static Task<string> GetJsonFromDatabase(
            this RegisterDatabaseContext registerDatabaseContext)
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
                    if (allData != null && allData.ContainsKey("legalEntities"))
                    {
                        var newLegalEntities = allData["legalEntities"].ToObject<LegalEntity[]>();
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
                    logger.LogError(ex, "Error while seeding the database.");
                    throw;
                }
                return false;
            }
        }

        public static async Task<BusinessRuleError> AddOrUpdateDataRecipientLegalEntity(this DomainEntities.DataRecipient dataRecipient,
                                                                                      LegalEntity legalEntity, RegisterDatabaseContext registerDatabaseContext,
                                                                                      IRepositoryMapper repositoryMapper, ILogger<RegisterAdminRepository> logger)
        {
            BusinessRuleError error = null;

            foreach (var dataRecipientBrand in dataRecipient.DataRecipientBrands)
            {
                var dbBrand = await registerDatabaseContext.Brands.AsNoTracking().SingleOrDefaultAsync(x => x.BrandId == dataRecipientBrand.BrandId);
                var drBrandToSave = repositoryMapper.Map(dataRecipientBrand);
                drBrandToSave.LastUpdated = DateTime.UtcNow;

                //create new brand as it is a new one
                if (null == dbBrand)
                {
                    logger.LogInformation("New Brand of id:{BrandId} name:{BrandName} getting added to the repository.", dataRecipientBrand.BrandId, dataRecipientBrand.BrandName);

                    await registerDatabaseContext.Brands.AddAsync(drBrandToSave);
                }

                //handle participation
                if ((error = await drBrandToSave.AddOrUpdateDataRecipientParticipation(legalEntity, dataRecipient, registerDatabaseContext, logger)) != null)
                {
                    logger.LogError("Update participation encountered error of {Error}", @error);
                    return error;
                }

                //update the brand as it is an existing one.
                if (dbBrand != null)
                {
                    logger.LogInformation("Updating Brand of id:{BrandId} name:{BrandName}", dataRecipientBrand.BrandId, dataRecipientBrand.BrandName);
                    registerDatabaseContext.Brands.Update(drBrandToSave);
                }

                //handle software products
                if ((error = await drBrandToSave.UpsertRecipientBrandSoftwareProducts(registerDatabaseContext, repositoryMapper, dataRecipientBrand.SoftwareProducts, logger)) != null)
                {
                    logger.LogError("Update SoftwareProduct encountered error of {Error}", @error);
                    return error;
                }
            }

            return error;
        }

        public static async Task<BusinessRuleError> UpsertRecipientBrandSoftwareProducts(this Brand brand, RegisterDatabaseContext registerDbContext,
            IRepositoryMapper repositoryMapper, ICollection<DomainEntities.SoftwareProduct> softwareProducts, ILogger<RegisterAdminRepository> logger)
        {

            brand.SoftwareProducts ??= new List<SoftwareProduct>();

            foreach (var s in softwareProducts)
            {
                var existingSoftwareProduct = await registerDbContext.SoftwareProducts.AsNoTracking().SingleOrDefaultAsync(sp => sp.SoftwareProductId == s.SoftwareProductId);

                var softwareProduct = repositoryMapper.Map(s);

                if (existingSoftwareProduct != null)
                {
                    //check if we getting assigned to new brand.
                    if (existingSoftwareProduct.BrandId != brand.BrandId)
                    {
                        return new BusinessRuleError("urn:au-cds:error:cds-all:Field/Invalid", "Invalid Field",
                            $"Value '{existingSoftwareProduct.SoftwareProductId}' in SoftwareProductId is already associated with a different brand.");
                    }

                    softwareProduct.BrandId = brand.BrandId;
                    registerDbContext.SoftwareProducts.Update(softwareProduct);
                }

                if (null == existingSoftwareProduct)
                {
                    softwareProduct.BrandId = brand.BrandId;
                    logger.LogInformation("Adding new SoftwareProduct of id:{SoftwareProductId} for name:{SoftwareProdcutName}", softwareProduct.SoftwareProductId, softwareProduct.SoftwareProductName);
                    await registerDbContext.SoftwareProducts.AddAsync(softwareProduct);
                }

                await softwareProduct.UpsertSoftwareProductCertificates(registerDbContext, repositoryMapper, s.Certificates, logger);
            }

            return null;
        }

        public static async Task UpsertSoftwareProductCertificates(this SoftwareProduct softwareProduct, RegisterDatabaseContext registerDbContext,
            IRepositoryMapper repositoryMapper, ICollection<DomainEntities.SoftwareProductCertificateInfosec> certificates, ILogger<RegisterAdminRepository> logger)
        {
            softwareProduct.Certificates ??= [];

            foreach (var c in certificates)
            {
                var existingCertificate = await registerDbContext.SoftwareProductCertificates.SingleOrDefaultAsync(cert => 
                                                    cert.Thumbprint == c.Thumbprint && 
                                                    cert.SoftwareProductId == softwareProduct.SoftwareProductId && 
                                                    cert.CommonName == c.CommonName);                

                if (existingCertificate != null)
                {
                    existingCertificate.Thumbprint = c.Thumbprint;
                    existingCertificate.CommonName = c.CommonName;
                    registerDbContext.SoftwareProductCertificates.Update(existingCertificate);
                }

                if (null == existingCertificate)
                {
                    var certificate = repositoryMapper.Map(c);
                    certificate.SoftwareProductId = softwareProduct.SoftwareProductId;
                    await registerDbContext.SoftwareProductCertificates.AddAsync(certificate);
                    await registerDbContext.SaveChangesAsync();
                    logger.LogInformation("Adding new SoftwareProductCertificate of id:{SoftwareProductCertificateId} for SoftwareProductId:{SoftwareProductId}", certificate.SoftwareProductCertificateId, certificate.SoftwareProductId);
                }                
            }
        }

        public static async Task<BusinessRuleError> AddOrUpdateDataRecipientParticipation(this Brand brand, LegalEntity legalEntity, 
            DomainEntities.DataRecipient dataRecipient, RegisterDatabaseContext registerDatabaseContext, ILogger<RegisterAdminRepository> logger)
        {
            var existingParticipant = await registerDatabaseContext.Participations.
                Include(x => x.LegalEntity).
                SingleOrDefaultAsync(p => (p.LegalEntityId == legalEntity.LegalEntityId || p.Brands.Any(b => b.BrandId == brand.BrandId) && p.ParticipationTypeId == ParticipationTypes.Dr));

            var participationStatus = (ParticipationStatusType)Enum.Parse(typeof(Entities.ParticipationStatusType), dataRecipient.LegalEntity.Status, true);

            if (existingParticipant != null)
            {
                //check if there is a change in the participation between brand and legal entity
                if (existingParticipant.LegalEntityId != legalEntity.LegalEntityId)
                {
                    return new BusinessRuleError("urn:au-cds:error:cds-all:Field/Invalid", "Invalid Field", $"dataRecipientBrandId '{brand.BrandId}' is already associated with a different legal entity.");
                }

                brand.ParticipationId = existingParticipant.ParticipationId;
                existingParticipant.StatusId = participationStatus;
            }

            //create the Participation if required.
            if (existingParticipant == null)
            {
                var participant = new Participation
                {
                    LegalEntityId = legalEntity.LegalEntityId,
                    ParticipationTypeId = ParticipationTypes.Dr,
                    StatusId = participationStatus
                };

                legalEntity.Participations ??= new List<Participation>();
                legalEntity.Participations.Add(participant);

                //assigning new participation to the brand.
                brand.ParticipationId = participant.ParticipationId;

                await registerDatabaseContext.Participations.AddAsync(participant);
                await registerDatabaseContext.SaveChangesAsync();
                logger.LogInformation("Adding new Participation of id:{ParticipationId} getting added to the repository.", participant.ParticipationId);
            }            

            return null;
        }
    }
}