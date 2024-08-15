using CDR.Register.Domain.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.Extensions.Logging;

namespace CDR.Register.API.Infrastructure.Versioning
{
    public class ApiVersionErrorResponse : DefaultErrorResponseProvider
    {
        private readonly ILogger<ApiVersionErrorResponse> _logger;

        public ApiVersionErrorResponse()
        {
            _logger = new LoggerFactory().CreateLogger<ApiVersionErrorResponse>();
        }

        public override IActionResult CreateResponse(ErrorResponseContext context)
        {
            var errorList = new ResponseErrorList();
            int statusCode;

            // Determine what the return status code and message will be, default to 500 - Internal Server Error
            if (context.Message.Contains(Domain.Constants.ErrorTitles.MissingVersion))
            {
                errorList.AddInvalidXVMissingRequiredHeader();
                statusCode = StatusCodes.Status400BadRequest;
            }
            else if (context.Message.Contains(Domain.Constants.ErrorTitles.InvalidVersion))
            {
                errorList.AddInvalidXVInvalidVersion();
                statusCode = StatusCodes.Status400BadRequest;
            }
            else if (context.Message.Contains(Domain.Constants.ErrorTitles.UnsupportedVersion))
            {
                errorList.AddInvalidXVUnsupportedVersion();
                statusCode = StatusCodes.Status406NotAcceptable;
            }
            else
            {
                errorList.AddUnexpectedError();
                statusCode = StatusCodes.Status500InternalServerError;
            }

            _logger.LogError("Error detail {Detail}", errorList.Errors[0].Detail);

            return new ObjectResult(errorList) { StatusCode = statusCode };
        }
    }
}
