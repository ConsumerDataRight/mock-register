namespace CDR.Register.Domain.Models
{
    public class MetaError
    {
        public MetaError(string urn)
        {
            this.Urn = urn;
        }

        public string Urn { get; set; }
    }
}
