using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CDR.Register.API.Infrastructure.Attributes
{
    // This attribute is to validate the media type and send customer error message as expected by the tests.
    // This attribute replaces [Consumes("application/x-www-form-urlencoded")] attribute but with custom error message.
    // This attribute also avoid the consumer to access the http request, validate and send custom error message.
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public class ValidateContentTypeFilterAttribute : ActionFilterAttribute
    {
        private readonly string _expectedContentType;

        public ValidateContentTypeFilterAttribute(string expectedContentType)
        {
            this._expectedContentType = expectedContentType;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var contentType = context.HttpContext.Request.ContentType;

            if (contentType == null)
            {
                context.Result = new ObjectResult(new
                {
                    error = "invalid_request",
                    error_description = $"Content-Type is not {this._expectedContentType}",
                })
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                };
            }
            else if (!contentType.StartsWith(this._expectedContentType, StringComparison.OrdinalIgnoreCase))
            {
                context.Result = new ObjectResult(new
                {
                    error = "invalid_request",
                    error_description = $"Content-Type is not {this._expectedContentType}",
                })
                {
                    StatusCode = StatusCodes.Status415UnsupportedMediaType,
                };
            }
        }
    }
}
