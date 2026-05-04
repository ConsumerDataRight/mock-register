using System;
using System.Net;
using System.Threading.Tasks;
using CDR.Register.API.Infrastructure.Versioning;
using CDR.Register.Domain.Models;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using static CDR.Register.API.Infrastructure.Constants;

namespace CDR.Register.API.Infrastructure.Middleware
{
    public static class ApiExceptionHandler
    {
        private static readonly JsonSerializerSettings SerializerSettings = new()
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore,
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy(),
            },
        };

        public static async Task Handle(HttpContext context)
        {
            var exceptionDetails = context.Features.Get<IExceptionHandlerFeature>();
            var ex = exceptionDetails?.Error;

            if (ex == null)
            {
                return;
            }

            (HttpStatusCode statusCode, ResponseErrorList errors) = ex switch
            {
                InvalidVersionException =>
                    (HttpStatusCode.BadRequest, new ResponseErrorList().AddInvalidXVInvalidVersion()),

                UnsupportedVersionException =>
                    (HttpStatusCode.NotAcceptable, new ResponseErrorList().AddInvalidXVUnsupportedVersion()),

                MissingRequiredHeaderException missingRequiredHeaderException when missingRequiredHeaderException?.HeaderName == Headers.X_V =>
                    (HttpStatusCode.BadRequest, new ResponseErrorList().AddInvalidXVMissingRequiredHeader()),

                _ =>
                    (HttpStatusCode.InternalServerError, new ResponseErrorList().AddUnexpectedError()),
            };

            if (errors.HasErrors())
            {
                var handledError = JsonConvert.SerializeObject(errors, SerializerSettings);
                context.Response.StatusCode = (int)statusCode;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(handledError).ConfigureAwait(false);
            }
        }
    }
}
