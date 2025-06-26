using System.ComponentModel.DataAnnotations;
using CDR.Register.Repository.Enums;

namespace CDR.Register.Repository.Entities
{
    public class AccreditationLevel
    {
        [Key]
        public AccreditationLevelType AccreditationLevelId { get; set; }

        [MaxLength(100)]
        [Required]
        public string AccreditationLevelCode { get; set; }
    }
}
