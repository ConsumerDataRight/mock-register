namespace CDR.Register.Discovery.API.Business.Models
{
    public class DataRecipientBrandModel
    {
        public string DataRecipientBrandId { get; set; }
        public string BrandName { get; set; }
        public string LogoUri { get; set; }
        public SoftwareProductModel[] SoftwareProducts { get; set; }
        public string Status { get; set; }
    }
}