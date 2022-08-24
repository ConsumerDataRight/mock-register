namespace CDR.Register.API.Infrastructure.Models
{
    public class JsonWebKey
    {
        public string alg { get; set; }
        public string e { get; set; }
        public string[] key_ops { get; set; }
        public string kid { get; set; }
        public string kty { get; set; }
        public string n { get; set; }
        public string use { get; set; }
        public string x5t { get; set; }
        public string[] x5c { get; set; }
    }
}
