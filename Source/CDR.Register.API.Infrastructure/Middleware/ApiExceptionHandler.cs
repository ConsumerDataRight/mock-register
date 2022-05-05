using CDR.Register.API.Infrastructure.Models;
using CDR.Register.API.Infrastructure.Versioning;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Net;
using System.Threading.Tasks;

namespace CDR.Register.API.Infrastructure.Middleware
{
    public static class ApiExceptionHandler
    {
        public async static Task Handle(HttpContext context)
        {
            var exceptionDetails = context.Features.Get<IExceptionHandlerFeature>();
            var ex = exceptionDetails?.Error;

            if (ex != null)
            {
                var handledError = string.Empty;
                var statusCode = (int)HttpStatusCode.BadRequest;
                var jsonSerializerSettings = new JsonSerializerSettings()
                {
                    Formatting = Formatting.Indented,
                    NullValueHandling = NullValueHandling.Ignore,
                    ContractResolver = new DefaultContractResolver
                    {
                        NamingStrategy = new CamelCaseNamingStrategy()
                    }
                };

                if (ex is InvalidVersionException)
                {
                    statusCode = (int)HttpStatusCode.BadRequest;
                    handledError = JsonConvert.SerializeObject(new ResponseErrorList().InvalidXVInvalidVersion(), jsonSerializerSettings);
                }

                if (ex is UnsupportedVersionException exception)
                {
                    statusCode = (int)HttpStatusCode.NotAcceptable;
                    handledError = Newtonsoft.Json.JsonConvert.SerializeObject(new ResponseErrorList().InvalidXVUnsupportedVersion(exception.Message), jsonSerializerSettings);
                }

                if (!string.IsNullOrEmpty(handledError))
                {
                    context.Response.StatusCode = statusCode;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync(handledError).ConfigureAwait(false);
                }
            }
        }
    }
}
