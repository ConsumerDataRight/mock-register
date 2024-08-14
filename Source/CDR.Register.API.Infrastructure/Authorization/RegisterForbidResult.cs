using CDR.Register.API.Infrastructure.Models;
using CDR.Register.Domain.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CDR.Register.API.Infrastructure.Authorization
{
    public class RegisterForbidResult : ObjectResult
    {
        public RegisterForbidResult(ResponseErrorList errorList) : base(errorList)
        {
            this.StatusCode = StatusCodes.Status403Forbidden;
        }

        public RegisterForbidResult(Error error) : this(new ResponseErrorList(error))
        {
        }
    }
}
