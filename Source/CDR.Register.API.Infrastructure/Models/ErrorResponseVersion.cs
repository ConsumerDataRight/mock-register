using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;

namespace CDR.Register.API.Infrastructure.Models
{
    public class ErrorResponseVersion : DefaultErrorResponseProvider
    {
        // note: in Web API the response type is HttpResponseMessage
        public override IActionResult CreateResponse(ErrorResponseContext context)
        {
            switch (context.ErrorCode)
            {
                case "UnsupportedApiVersion":
                    return new ObjectResult(new ResponseErrorList().InvalidXVUnsupportedVersion())
                    {
                        StatusCode = (int)HttpStatusCode.NotAcceptable
                    };
            }

            return base.CreateResponse(context);
        }
    }
}
