namespace CDR.Register.Domain.Entities
{
    public class SoftwareStatementAssertion
    {
        public DataRecipientBrand DataRecipientBrand { get; set; }
        public SoftwareProduct SoftwareProduct { get; set; }
        public DataRecipientLegalEntity LegalEntity { get; set; }
    }
}
