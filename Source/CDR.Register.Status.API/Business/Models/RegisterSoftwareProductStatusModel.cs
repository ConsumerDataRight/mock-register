namespace CDR.Register.Status.API.Business.Models
{
    public class RegisterSoftwareProductStatusModel : BaseModel
    {
        public string SoftwareProductId { get; set; }
        public string SoftwareProductStatus { get; set; }
    }
}
