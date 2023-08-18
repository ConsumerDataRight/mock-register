using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CDR.Register.Repository.Migrations
{
    public partial class V7 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {            
            // Autognerate ADR###### as the format for AccreditationNumbers eg ADR000099 for null 
            // Accreditation Number Fixing all the exisitng data. 
            var sqlQuery = @"DECLARE @Counter INT; 

                            SET @Counter = 0
                            
                            UPDATE LegalEntity 
                            SET AccreditationNumber = 'ADR' + RIGHT('000000' + CAST(@Counter AS VARCHAR(6)), 6), @Counter = @Counter + 1 
                            WHERE LegalEntityId  in 
	                            ( SELECT le.LegalEntityId
	                            FROM Participation P 
	                            INNER JOIN LegalEntity LE ON LE.LegalEntityId = P.LegalEntityId 
                                WHERE P.ParticipationTypeId = 2 );";

            migrationBuilder.Sql(sqlQuery);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
