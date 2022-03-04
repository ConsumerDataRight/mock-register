namespace CDR.Register.Status.API.Business.Models
{
    public class RegisterSoftwareProductStatusModelV1 : BaseModel
    {
        public string SoftwareProductId { get; set; }
        public string SoftwareProductStatus { get; set; }
    }

    public class RegisterSoftwareProductStatusModel : BaseModel
    {
        public string SoftwareProductId { get; set; }
        public string Status { get; set; }
    }
}