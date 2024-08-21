namespace CDR.Register.Domain.Entities
{
    public class DataHolderAuthentication
    {
        public string RegisterUType { get; set; }
        public string JwksEndpoint { get; set; }
    }

    public enum RegisterUType
    {
        Unknown = 0,
        SignedJwt = 1
    }
}