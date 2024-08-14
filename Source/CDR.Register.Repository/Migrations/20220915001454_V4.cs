using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CDR.Register.Repository.Migrations
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("SonarAnalyzer", "S1192:Define a constant instead of using this literal 'LegalEntity' 6 times.", Justification = "Auto-generated migration file.")]
    public partial class V4 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LegalEntity_LegalEntityStatus_LegalEntityStatusId",
                table: "LegalEntity");

            migrationBuilder.DropTable(
                name: "LegalEntityStatus");

            migrationBuilder.DropIndex(
                name: "IX_LegalEntity_LegalEntityStatusId",
                table: "LegalEntity");

            migrationBuilder.DropColumn(
                name: "LegalEntityStatusId",
                table: "LegalEntity")
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalHistoryTableName", "LegalEntityHistory")
                .Annotation("SqlServer:TemporalHistoryTableSchema", null);

            migrationBuilder.AddColumn<int>(
                name: "ParticipationTypeId",
                table: "ParticipationStatus",
                type: "int",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "ParticipationStatus",
                keyColumn: "ParticipationStatusId",
                keyValue: 2,
                column: "ParticipationTypeId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ParticipationStatus",
                keyColumn: "ParticipationStatusId",
                keyValue: 3,
                column: "ParticipationTypeId",
                value: 2);

            migrationBuilder.UpdateData(
                table: "ParticipationStatus",
                keyColumn: "ParticipationStatusId",
                keyValue: 4,
                column: "ParticipationTypeId",
                value: 2);

            migrationBuilder.UpdateData(
                table: "ParticipationStatus",
                keyColumn: "ParticipationStatusId",
                keyValue: 5,
                column: "ParticipationTypeId",
                value: 2);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ParticipationTypeId",
                table: "ParticipationStatus");

            migrationBuilder.AddColumn<int>(
                name: "LegalEntityStatusId",
                table: "LegalEntity",
                type: "int",
                nullable: true);

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

            migrationBuilder.CreateIndex(
                name: "IX_LegalEntity_LegalEntityStatusId",
                table: "LegalEntity",
                column: "LegalEntityStatusId");

            migrationBuilder.AddForeignKey(
                name: "FK_LegalEntity_LegalEntityStatus_LegalEntityStatusId",
                table: "LegalEntity",
                column: "LegalEntityStatusId",
                principalTable: "LegalEntityStatus",
                principalColumn: "LegalEntityStatusId");
        }
    }
}
