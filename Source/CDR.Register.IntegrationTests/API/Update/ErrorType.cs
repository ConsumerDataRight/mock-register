namespace CDR.Register.IntegrationTests.API.Update
{
    public partial class ExpectedErrors
    {
        public enum ErrorType
        {
            MissingField,
            MissingHeader,
            InvalidField,
            InvalidVersion,
            UnsupportedVersion,
            Unauthorized,
        }
    }
}
