namespace CDR.Register.Domain.ValueObjects
{
    public class BusinessRuleError
    {
        public string Code { get; set; }
        public string Title { get; set; }
        public string Detail { get; set; }

        public BusinessRuleError(string errorCode, string errorTitle, string errorDetail)
        {
            Code = errorCode;
            Title = errorTitle;
            Detail = errorDetail;
        }
    }
}
