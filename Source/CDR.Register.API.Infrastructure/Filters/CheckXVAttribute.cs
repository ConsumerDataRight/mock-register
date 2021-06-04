using System;
using System.Net;
using CDR.Register.API.Infrastructure.Models;
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
            _version = version;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            // Get x_v from request header
            var x_v = context.HttpContext.Request.Headers["x-v"];

            // x_v not set? ok since it's optional
            if (string.IsNullOrEmpty(x_v))
            {
            }
            // x_v is set, so if it doesnt match our version respond with BadRequest
            else if (x_v != _version)
            {
                context.Result = new ObjectResult(new ResponseErrorList().InvalidXV())
                {
                    StatusCode = (int)HttpStatusCode.NotAcceptable
                };
            }

            base.OnActionExecuting(context);
        }

        public override void OnActionExecuted(ActionExecutedContext context)
        {
            // Set version (x-v) we are responding with in the response header
            context.HttpContext.Response.Headers["x-v"] = _version;

            base.OnActionExecuted(context);
        }
    }
}
