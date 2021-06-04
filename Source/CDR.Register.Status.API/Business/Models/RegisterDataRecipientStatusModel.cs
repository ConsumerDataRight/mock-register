namespace CDR.Register.Status.API.Business.Models
{
    public class RegisterDataRecipientStatusModel : BaseModel
    {
        public string DataRecipientId { get; set; }
        public string DataRecipientStatus { get; set; }
    }
}
