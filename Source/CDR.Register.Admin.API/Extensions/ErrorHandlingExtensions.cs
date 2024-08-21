using CDR.Register.Admin.API.Business.Model;
using CDR.Register.Admin.API.Business.Validators;
using CDR.Register.API.Infrastructure.Models;
using CDR.Register.Domain.Models;
using CDR.Register.Domain.ValueObjects;
using System.Linq;

namespace CDR.Register.Admin.API.Extensions
{
    public static class ErrorHandlingExtensions
    {
        public static ResponseErrorList ToResponseErrorList(this BusinessRuleError businessRuleError)
        {
            var responseErrorList = new ResponseErrorList(businessRuleError.Code, businessRuleError.Title, businessRuleError.Detail);
            return responseErrorList;
        }

        public static ResponseErrorList GetValidationErrors(this LegalEntity legalEntity, LegalEntityValidator validator)
        {
            var result = validator.Validate(legalEntity);
            var responseErrorList = new ResponseErrorList();

            if (!result.IsValid)
            {
                responseErrorList.Errors.AddRange(result.Errors.Select(error =>
                    new Error(error.ErrorCode, error.ErrorMessage, error.CustomState?.ToString() ?? error.PropertyName)));
            }

            return responseErrorList;
        }
    }
}
