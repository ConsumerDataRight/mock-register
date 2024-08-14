using Microsoft.EntityFrameworkCore.Migrations;

namespace CDR.Register.Repository.Migrations
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("SonarAnalyzer", "S1192:Define a constant instead of using this literal 'FK_Participation_IndustryType_IndustryId' 4 times.", Justification = "Auto-generated migration file.")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("SonarAnalyzer", "CA1861:Prefer 'static readonly' fields over constant array arguments if the called method is called repeatedly and is not mutating the passed array", Justification = "Auto-generated migration file.")]
    public partial class V2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Participation_IndustryType_IndustryId",
                table: "Participation");

            migrationBuilder.DropColumn(
                name: "IndustryCode",
                table: "LegalEntity");

            migrationBuilder.AlterColumn<int>(
                name: "IndustryId",
                table: "Participation",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "AccreditationLevelId",
                table: "LegalEntity",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AnzsicDivision",
                table: "LegalEntity",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LegalEntityStatusId",
                table: "LegalEntity",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AccreditationLevel",
                columns: table => new
                {
                    AccreditationLevelId = table.Column<int>(type: "int", nullable: false),
                    AccreditationLevelCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccreditationLevel", x => x.AccreditationLevelId);
                });

            migrationBuilder.CreateTable(
                name: "LegalEntityStatus",
                columns: table => new
                {
                    LegalEntityStatusId = table.Column<int>(type: "int", nullable: false),
                    LegalEntityStatusCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LegalEntityStatus", x => x.LegalEntityStatusId);
                });
            
            migrationBuilder.InsertData(
                table: "LegalEntityStatus",
                columns: new[] { "LegalEntityStatusId", "LegalEntityStatusCode" },
                values: new object[,]
                {
                    { 1, "Active" },
                    { 2, "Removed" }
                });

            migrationBuilder.InsertData(
                table: "AccreditationLevel",
                columns: new[] { "AccreditationLevelId", "AccreditationLevelCode" },
                values: new object[,]
                {
                    { 0, "Sponsored" },
                    { 1, "Unrestricted" }
                });

            //Updating exsiting data
            migrationBuilder.Sql(@"Update IndustryType Set IndustryTypeCode='Banking' where IndustryTypeId=1;
                                   Update IndustryType Set IndustryTypeCode='Energy' where IndustryTypeId=2;");


            migrationBuilder.CreateIndex(
                name: "IX_LegalEntity_AccreditationLevelId",
                table: "LegalEntity",
                column: "AccreditationLevelId");

            migrationBuilder.CreateIndex(
                name: "IX_LegalEntity_LegalEntityStatusId",
                table: "LegalEntity",
                column: "LegalEntityStatusId");

            migrationBuilder.AddForeignKey(
                name: "FK_LegalEntity_AccreditationLevel_AccreditationLevelId",
                table: "LegalEntity",
                column: "AccreditationLevelId",
                principalTable: "AccreditationLevel",
                principalColumn: "AccreditationLevelId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_LegalEntity_LegalEntityStatus_LegalEntityStatusId",
                table: "LegalEntity",
                column: "LegalEntityStatusId",
                principalTable: "LegalEntityStatus",
                principalColumn: "LegalEntityStatusId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Participation_IndustryType_IndustryId",
                table: "Participation",
                column: "IndustryId",
                principalTable: "IndustryType",
                principalColumn: "IndustryTypeId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LegalEntity_AccreditationLevel_AccreditationLevelId",
                table: "LegalEntity");

            migrationBuilder.DropForeignKey(
                name: "FK_LegalEntity_LegalEntityStatus_LegalEntityStatusId",
                table: "LegalEntity");

            migrationBuilder.DropForeignKey(
                name: "FK_Participation_IndustryType_IndustryId",
                table: "Participation");

            migrationBuilder.DropTable(
                name: "AccreditationLevel");

            migrationBuilder.DropTable(
                name: "LegalEntityStatus");

            migrationBuilder.DropIndex(
                name: "IX_LegalEntity_AccreditationLevelId",
                table: "LegalEntity");

            migrationBuilder.DropIndex(
                name: "IX_LegalEntity_LegalEntityStatusId",
                table: "LegalEntity");
            
            migrationBuilder.DropColumn(
                name: "AccreditationLevelId",
                table: "LegalEntity");

            migrationBuilder.DropColumn(
                name: "AnzsicDivision",
                table: "LegalEntity");

            migrationBuilder.DropColumn(
                name: "LegalEntityStatusId",
                table: "LegalEntity");

            migrationBuilder.AlterColumn<int>(
                name: "IndustryId",
                table: "Participation",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IndustryCode",
                table: "LegalEntity",
                type: "nvarchar(4)",
                maxLength: 4,
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Participation_IndustryType_IndustryId",
                table: "Participation",
                column: "IndustryId",
                principalTable: "IndustryType",
                principalColumn: "IndustryTypeId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
