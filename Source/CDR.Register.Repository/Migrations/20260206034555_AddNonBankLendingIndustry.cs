using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CDR.Register.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddNonBankLendingIndustry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "IndustryType",
                columns: new[] { "IndustryTypeId", "IndustryTypeCode" },
                values: new object[] { 3, "non-bank-lending" });

            // Update existing industries to use lowercase values to match current code
            migrationBuilder.UpdateData("IndustryType", "IndustryTypeId", 1, "IndustryTypeCode", "banking");
            migrationBuilder.UpdateData("IndustryType", "IndustryTypeId", 2, "IndustryTypeCode", "energy");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "IndustryType",
                keyColumn: "IndustryTypeId",
                keyValue: 3);

            // Revert changes to existing industries
            migrationBuilder.UpdateData("IndustryType", "IndustryTypeId", 1, "IndustryTypeCode", "Banking");
            migrationBuilder.UpdateData("IndustryType", "IndustryTypeId", 2, "IndustryTypeCode", "Energy");
        }
    }
}
