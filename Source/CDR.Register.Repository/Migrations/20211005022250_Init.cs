using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace CDR.Register.Repository.Migrations
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("SonarAnalyzer", "S1192:String literals should not be duplicated", Justification = "Auto-generated migration file.")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("SonarAnalyzer", "CA1861:Prefer 'static readonly' fields over constant array arguments if the called method is called repeatedly and is not mutating the passed array", Justification = "Auto-generated migration file.")]
    public partial class Init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BrandStatus",
                columns: table => new
                {
                    BrandStatusId = table.Column<int>(type: "int", nullable: false),
                    BrandStatusCode = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BrandStatus", x => x.BrandStatusId);
                });

            migrationBuilder.CreateTable(
                name: "IndustryType",
                columns: table => new
                {
                    IndustryTypeId = table.Column<int>(type: "int", nullable: false),
                    IndustryTypeCode = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IndustryType", x => x.IndustryTypeId);
                });

            migrationBuilder.CreateTable(
                name: "OrganisationType",
                columns: table => new
                {
                    OrganisationTypeId = table.Column<int>(type: "int", nullable: false),
                    OrganisationTypeCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganisationType", x => x.OrganisationTypeId);
                });

            migrationBuilder.CreateTable(
                name: "ParticipationStatus",
                columns: table => new
                {
                    ParticipationStatusId = table.Column<int>(type: "int", nullable: false),
                    ParticipationStatusCode = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParticipationStatus", x => x.ParticipationStatusId);
                });

            migrationBuilder.CreateTable(
                name: "ParticipationType",
                columns: table => new
                {
                    ParticipationTypeId = table.Column<int>(type: "int", nullable: false),
                    ParticipationTypeCode = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParticipationType", x => x.ParticipationTypeId);
                });

            migrationBuilder.CreateTable(
                name: "RegisterUType",
                columns: table => new
                {
                    RegisterUTypeId = table.Column<int>(type: "int", nullable: false),
                    RegisterUTypeCode = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegisterUType", x => x.RegisterUTypeId);
                });

            migrationBuilder.CreateTable(
                name: "SoftwareProductStatus",
                columns: table => new
                {
                    SoftwareProductStatusId = table.Column<int>(type: "int", nullable: false),
                    SoftwareProductStatusCode = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SoftwareProductStatus", x => x.SoftwareProductStatusId);
                });

            migrationBuilder.CreateTable(
                name: "LegalEntity",
                columns: table => new
                {
                    LegalEntityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LegalEntityName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LogoUri = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    RegistrationNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    RegistrationDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RegisteredCountry = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Abn = table.Column<string>(type: "nvarchar(11)", maxLength: 11, nullable: true),
                    Acn = table.Column<string>(type: "nvarchar(9)", maxLength: 9, nullable: true),
                    Arbn = table.Column<string>(type: "nvarchar(9)", maxLength: 9, nullable: true),
                    IndustryCode = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: true),
                    OrganisationTypeId = table.Column<int>(type: "int", nullable: true),
                    AccreditationNumber = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LegalEntity", x => x.LegalEntityId);
                    table.ForeignKey(
                        name: "FK_LegalEntity_OrganisationType_OrganisationTypeId",
                        column: x => x.OrganisationTypeId,
                        principalTable: "OrganisationType",
                        principalColumn: "OrganisationTypeId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Participation",
                columns: table => new
                {
                    ParticipationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LegalEntityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ParticipationTypeId = table.Column<int>(type: "int", nullable: false),
                    IndustryId = table.Column<int>(type: "int", nullable: false),
                    StatusId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Participation", x => x.ParticipationId);
                    table.ForeignKey(
                        name: "FK_Participation_IndustryType_IndustryId",
                        column: x => x.IndustryId,
                        principalTable: "IndustryType",
                        principalColumn: "IndustryTypeId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Participation_LegalEntity_LegalEntityId",
                        column: x => x.LegalEntityId,
                        principalTable: "LegalEntity",
                        principalColumn: "LegalEntityId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Participation_ParticipationStatus_StatusId",
                        column: x => x.StatusId,
                        principalTable: "ParticipationStatus",
                        principalColumn: "ParticipationStatusId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Participation_ParticipationType_ParticipationTypeId",
                        column: x => x.ParticipationTypeId,
                        principalTable: "ParticipationType",
                        principalColumn: "ParticipationTypeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Brand",
                columns: table => new
                {
                    BrandId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BrandName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LogoUri = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    BrandStatusId = table.Column<int>(type: "int", nullable: false),
                    ParticipationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Brand", x => x.BrandId);
                    table.ForeignKey(
                        name: "FK_Brand_BrandStatus_BrandStatusId",
                        column: x => x.BrandStatusId,
                        principalTable: "BrandStatus",
                        principalColumn: "BrandStatusId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Brand_Participation_ParticipationId",
                        column: x => x.ParticipationId,
                        principalTable: "Participation",
                        principalColumn: "ParticipationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AuthDetail",
                columns: table => new
                {
                    BrandId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RegisterUTypeId = table.Column<int>(type: "int", nullable: false),
                    JwksEndpoint = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthDetail", x => new { x.BrandId, x.RegisterUTypeId });
                    table.ForeignKey(
                        name: "FK_AuthDetail_Brand_BrandId",
                        column: x => x.BrandId,
                        principalTable: "Brand",
                        principalColumn: "BrandId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AuthDetail_RegisterUType_RegisterUTypeId",
                        column: x => x.RegisterUTypeId,
                        principalTable: "RegisterUType",
                        principalColumn: "RegisterUTypeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Endpoint",
                columns: table => new
                {
                    BrandId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Version = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: false),
                    PublicBaseUri = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ResourceBaseUri = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    InfosecBaseUri = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ExtensionBaseUri = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    WebsiteUri = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Endpoint", x => x.BrandId);
                    table.ForeignKey(
                        name: "FK_Endpoint_Brand_BrandId",
                        column: x => x.BrandId,
                        principalTable: "Brand",
                        principalColumn: "BrandId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SoftwareProduct",
                columns: table => new
                {
                    SoftwareProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SoftwareProductName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SoftwareProductDescription = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    LogoUri = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    SectorIdentifierUri = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ClientUri = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    TosUri = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    PolicyUri = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    RecipientBaseUri = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    RevocationUri = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    RedirectUris = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    JwksUri = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Scope = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    StatusId = table.Column<int>(type: "int", nullable: false),
                    BrandId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SoftwareProduct", x => x.SoftwareProductId);
                    table.ForeignKey(
                        name: "FK_SoftwareProduct_Brand_BrandId",
                        column: x => x.BrandId,
                        principalTable: "Brand",
                        principalColumn: "BrandId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SoftwareProduct_SoftwareProductStatus_StatusId",
                        column: x => x.StatusId,
                        principalTable: "SoftwareProductStatus",
                        principalColumn: "SoftwareProductStatusId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SoftwareProductCertificate",
                columns: table => new
                {
                    SoftwareProductCertificateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SoftwareProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CommonName = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Thumbprint = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SoftwareProductCertificate", x => x.SoftwareProductCertificateId);
                    table.ForeignKey(
                        name: "FK_SoftwareProductCertificate_SoftwareProduct_SoftwareProductId",
                        column: x => x.SoftwareProductId,
                        principalTable: "SoftwareProduct",
                        principalColumn: "SoftwareProductId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "BrandStatus",
                columns: new[] { "BrandStatusId", "BrandStatusCode" },
                values: new object[,]
                {
                    { 1, "ACTIVE" },
                    { 2, "INACTIVE" },
                    { 3, "REMOVED" }
                });

            migrationBuilder.InsertData(
                table: "IndustryType",
                columns: new[] { "IndustryTypeId", "IndustryTypeCode" },
                values: new object[] { 1, "banking" });

            migrationBuilder.InsertData(
                table: "IndustryType",
                columns: new[] { "IndustryTypeId", "IndustryTypeCode" },
                values: new object[] { 2, "energy" });

            migrationBuilder.InsertData(
                table: "OrganisationType",
                columns: new[] { "OrganisationTypeId", "OrganisationTypeCode" },
                values: new object[,]
                {
                    { 1, "SOLE_TRADER" },
                    { 2, "COMPANY" },
                    { 3, "PARTNERSHIP" },
                    { 4, "TRUST" },
                    { 5, "GOVERNMENT_ENTITY" },
                    { 6, "OTHER" }
                });

            migrationBuilder.InsertData(
                table: "ParticipationStatus",
                columns: new[] { "ParticipationStatusId", "ParticipationStatusCode" },
                values: new object[,]
                {
                    { 6, "INACTIVE" },
                    { 5, "SURRENDERED" },
                    { 4, "REVOKED" },
                    { 1, "ACTIVE" },
                    { 2, "REMOVED" },
                    { 3, "SUSPENDED" }
                });

            migrationBuilder.InsertData(
                table: "ParticipationType",
                columns: new[] { "ParticipationTypeId", "ParticipationTypeCode" },
                values: new object[,]
                {
                    { 1, "DH" },
                    { 2, "DR" }
                });

            migrationBuilder.InsertData(
                table: "RegisterUType",
                columns: new[] { "RegisterUTypeId", "RegisterUTypeCode" },
                values: new object[] { 1, "SIGNED-JWT" });

            migrationBuilder.InsertData(
                table: "SoftwareProductStatus",
                columns: new[] { "SoftwareProductStatusId", "SoftwareProductStatusCode" },
                values: new object[,]
                {
                    { 2, "INACTIVE" },
                    { 1, "ACTIVE" },
                    { 3, "REMOVED" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuthDetail_RegisterUTypeId",
                table: "AuthDetail",
                column: "RegisterUTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Brand_BrandStatusId",
                table: "Brand",
                column: "BrandStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_Brand_ParticipationId",
                table: "Brand",
                column: "ParticipationId");

            migrationBuilder.CreateIndex(
                name: "IX_LegalEntity_OrganisationTypeId",
                table: "LegalEntity",
                column: "OrganisationTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Participation_IndustryId",
                table: "Participation",
                column: "IndustryId");

            migrationBuilder.CreateIndex(
                name: "IX_Participation_LegalEntityId",
                table: "Participation",
                column: "LegalEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_Participation_ParticipationTypeId",
                table: "Participation",
                column: "ParticipationTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Participation_StatusId",
                table: "Participation",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_SoftwareProduct_BrandId",
                table: "SoftwareProduct",
                column: "BrandId");

            migrationBuilder.CreateIndex(
                name: "IX_SoftwareProduct_StatusId",
                table: "SoftwareProduct",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_SoftwareProductCertificate_SoftwareProductId",
                table: "SoftwareProductCertificate",
                column: "SoftwareProductId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuthDetail");

            migrationBuilder.DropTable(
                name: "Endpoint");

            migrationBuilder.DropTable(
                name: "SoftwareProductCertificate");

            migrationBuilder.DropTable(
                name: "RegisterUType");

            migrationBuilder.DropTable(
                name: "SoftwareProduct");

            migrationBuilder.DropTable(
                name: "Brand");

            migrationBuilder.DropTable(
                name: "SoftwareProductStatus");

            migrationBuilder.DropTable(
                name: "BrandStatus");

            migrationBuilder.DropTable(
                name: "Participation");

            migrationBuilder.DropTable(
                name: "IndustryType");

            migrationBuilder.DropTable(
                name: "LegalEntity");

            migrationBuilder.DropTable(
                name: "ParticipationStatus");

            migrationBuilder.DropTable(
                name: "ParticipationType");

            migrationBuilder.DropTable(
                name: "OrganisationType");
        }
    }
}
