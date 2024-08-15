using System;

namespace CDR.Register.API.Infrastructure.Models
{
    public class JsonWebKey
    {
        public string alg { get; set; } = string.Empty;
        public string e { get; set; } = string.Empty;
        public string[] key_ops { get; set; } = [];
        public string kid { get; set; } = string.Empty;
        public string kty { get; set; } = string.Empty;
        public string n { get; set; } = string.Empty;
        public string use { get; set; } = string.Empty;
        public string x5t { get; set; } = string.Empty;
        public string[] x5c { get; set; } = [];
    }
}
