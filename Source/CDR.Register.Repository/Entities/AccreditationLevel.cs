using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CDR.Register.Repository.Entities
{
    public class AccreditationLevel
    {
        [Key]
        public AccreditationLevelType AccreditationLevelId { get; set; }

        [MaxLength(100), Required]
        public string AccreditationLevelCode { get; set; }
    }

    public enum AccreditationLevelType
    {
        //Sponsored by Default 
        Sponsored = 0,                  
        Unrestricted = 1
    }
}
