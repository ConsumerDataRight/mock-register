using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CDR.Register.Repository.Entities
{
    public class LegalEntityStatus
    {
        [Key]
        public LegalEntityStatusEnum LegalEntityStatusId { get; set; }

        [MaxLength(100), Required]
        public string LegalEntityStatusCode { get; set; }
    }

    public enum LegalEntityStatusEnum : int
    {
        Active = 1,
        Removed = 2
    }
}
