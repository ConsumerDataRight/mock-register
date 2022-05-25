namespace CDR.Register.Status.API.Business.Models
{
    public class RegisterDataHolderStatusModel : BaseModel
    {
        public string LegalEntityId { get; set; }
        public string Status { get; set; }
    }

}