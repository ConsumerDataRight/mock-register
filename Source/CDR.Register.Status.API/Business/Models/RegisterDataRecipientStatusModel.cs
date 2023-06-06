namespace CDR.Register.Status.API.Business.Models
{
    public class RegisterDataRecipientStatusModel : BaseModel
    {
        public string LegalEntityId { get; set; }
        public string Status { get; set; }
    }
}