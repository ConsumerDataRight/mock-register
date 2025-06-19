using System.ComponentModel.DataAnnotations;
using CDR.Register.Repository.Enums;

namespace CDR.Register.Repository.Entities
{
    public class OrganisationType
    {
        [Key]
        public OrganisationTypes OrganisationTypeId { get; set; }

        [MaxLength(100)]
        [Required]
        public string OrganisationTypeCode { get; set; }
    }
}
