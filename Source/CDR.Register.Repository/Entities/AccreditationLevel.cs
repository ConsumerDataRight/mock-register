using System.ComponentModel.DataAnnotations;

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

    public enum AccreditationLevelType
    {
        // Sponsored by Default
        Sponsored = 0,
        Unrestricted = 1
    }
}
