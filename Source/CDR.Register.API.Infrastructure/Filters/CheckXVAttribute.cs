using System;
using System.Net;
using CDR.Register.API.Infrastructure.Models;
using CDR.Register.Repository.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CDR.Register.API.Infrastructure.Filters
{
    /// <summary>
    /// Checks the x-v header field is a supported version, if not then responds with BadRequest and appropriate ResponseErrorList
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
    public class CheckXVAttribute : ActionFilterAttribute
    {
        private readonly string _version;

        public CheckXVAttribute(string version)
        {
            // Parse in the response x-v version
            _version = version;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            // Was the x-v header sent in the request?
            bool headerSent = context.HttpContext.Request.Headers.ContainsKey("x-v");

            // Get x_v value from request header
            var x_v = context.HttpContext.Request.Headers["x-v"];
            var isNumber = int.TryParse(x_v, out int xvVersion);

            // Get the path
            string reqPath = context.HttpContext.Request.Path;

            // Am I a LEGACY request? (legacy requests start with /cdr-register/v1/banking/...
            bool isLegacy = (reqPath.StartsWith("/cdr-register/v1/banking/"));

            // x_v header does NOT EXIST
            if (!headerSent && !isLegacy)
            {
                // MULTI INDUSTRY REQUEST - return 400 - BadRequest - MissingRequiredHeader -> (IS OPTIONAL for Legacy request)
                context.Result = new ObjectResult(new ResponseErrorList().InvalidXVMissingRequiredHeader())
                {
                    StatusCode = (int)HttpStatusCode.BadRequest
                };
            }

            // x_v header EXISTS and the VALUE is EMPTY
            else if (string.IsNullOrEmpty(x_v))
            {
                // MULTI INDUSTRY REQUEST - is MANDATORY -> return 400 - BadRequest - InvalidVersion -> (IS OPTIONAL for Legacy request)
                if (!isLegacy)
                {
                    context.Result = new ObjectResult(new ResponseErrorList().InvalidXVInvalidVersion())
                    {
                        StatusCode = (int)HttpStatusCode.BadRequest
                    };
                }
            }

            // x_v header EXISTS and the VALUE is not a positive integer, eg invalid case is set to foo
            else if (!isNumber)
            {
                // LEGACY REQUEST - return 400 - BadRequest - InvalidVersion
                // MULTI INDUSTRY REQUEST - return 400 - BadRequest - InvalidVersion
                context.Result = new ObjectResult(new ResponseErrorList().InvalidXVInvalidVersion())
                {
                    StatusCode = (int)HttpStatusCode.BadRequest
                };                                
            }

            // x_v header EXISTS and the VALUE is NEGATIVE
            else if (xvVersion < 0)
            {                
                // LEGACY REQUEST - return 400 - BadRequest - InvalidVersion
                // MULTI INDUSTRY REQUEST - return 400 - BadRequest - InvalidVersion
                context.Result = new ObjectResult(new ResponseErrorList().InvalidXVInvalidVersion())
                {
                    StatusCode = (int)HttpStatusCode.BadRequest
                };                
            }

            // x_v header EXISTS and the VALUE doesn't match our version
            else if (x_v != _version)
            {
                // LEGACY REQUEST - return 406 - NotAcceptable - UnsupportedVersion
                // MULTI INDUSTRY REQUEST - return 406 - NotAcceptable - UnsupportedVersion
                context.Result = new ObjectResult(new ResponseErrorList().InvalidXVUnsupportedVersion())
                {
                    StatusCode = (int)HttpStatusCode.NotAcceptable
                };                
            }

            base.OnActionExecuting(context);
        }

        public override void OnActionExecuted(ActionExecutedContext context)
        {
            // Set version (x-v) we are responding with in the response header to the parsed in version
            context.HttpContext.Response.Headers["x-v"] = _version;

            base.OnActionExecuted(context);
        }
    }
}