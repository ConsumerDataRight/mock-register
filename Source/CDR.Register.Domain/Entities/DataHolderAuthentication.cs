namespace CDR.Register.Domain.Entities
{
    public class DataHolderAuthentication
    {
        public string RegisterUType { get; set; }
        public string JwksEndpoint { get; set; }
    }

    public enum RegisterUTypeEnum
    {
        Unknown = 0,
        SignedJwt = 1
    }
}