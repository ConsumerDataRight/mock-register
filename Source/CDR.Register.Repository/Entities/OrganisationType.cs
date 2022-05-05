using System.ComponentModel.DataAnnotations;

namespace CDR.Register.Repository.Entities
{
    public class OrganisationType
    {
        [Key]
        public OrganisationTypes OrganisationTypeId { get; set; }

        [MaxLength(100), Required]
        public string OrganisationTypeCode { get; set; }
    }

    public enum OrganisationTypes
    {
        Unknown = 0,
        SoleTrader = 1,
        Company = 2,
        Partnership = 3,
        Trust = 4,
        GovernmentEntity = 5,
        Other = 6
    }
}