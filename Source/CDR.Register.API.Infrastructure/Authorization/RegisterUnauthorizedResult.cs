using CDR.Register.API.Infrastructure.Models;
using CDR.Register.Domain.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CDR.Register.API.Infrastructure.Authorization
{
    public class RegisterUnauthorizedResult : ObjectResult
    {
        public RegisterUnauthorizedResult(ResponseErrorList errorList) : base(errorList)
        {
            this.StatusCode = StatusCodes.Status401Unauthorized;
        }

        public RegisterUnauthorizedResult(Error error) : this(new ResponseErrorList(error))
        {
        }
    }
}
