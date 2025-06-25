namespace CDR.Register.Domain.ValueObjects
{
    public class BusinessRuleError
    {
        public BusinessRuleError(string errorCode, string errorTitle, string errorDetail)
        {
            this.Code = errorCode;
            this.Title = errorTitle;
            this.Detail = errorDetail;
        }

        public string Code { get; set; }

        public string Title { get; set; }

        public string Detail { get; set; }
    }
}
