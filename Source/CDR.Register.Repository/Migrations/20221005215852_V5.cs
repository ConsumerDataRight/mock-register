using CDR.Register.Repository.Entities;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CDR.Register.Repository.Migrations
{
    public partial class V5 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
			// Add the additional Participation to LE
			var participationTypeIds = new int[] { (int)ParticipationTypes.Dh, (int)ParticipationTypes.Dr };
			foreach (var participationTypeId in participationTypeIds)
			{
				migrationBuilder.Sql($"INSERT INTO [dbo].[Participation] (ParticipationId, LegalEntityId, ParticipationTypeId, IndustryId, StatusId)\r\nSELECT NEWID(), le.LegalEntityId, {participationTypeId}, 1, 1 FROM [dbo].[LegalEntity] le\r\nLEFT OUTER JOIN [dbo].[Participation] p on p.LegalEntityId = le.LegalEntityId AND p.ParticipationTypeId = {participationTypeId}\r\nWHERE p.ParticipationId IS NULL");
			}
		}

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
