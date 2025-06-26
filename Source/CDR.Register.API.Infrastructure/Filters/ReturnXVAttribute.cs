using System;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CDR.Register.API.Infrastructure.Filters
{
    /// <summary>
    /// Checks the x-v header field is a supported version, if not then responds with BadRequest and appropriate ResponseErrorList.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
    public class ReturnXVAttribute : ActionFilterAttribute
    {
        private readonly string _version;

        public ReturnXVAttribute(string version)
        {
            this._version = version;
        }

        public override void OnActionExecuted(ActionExecutedContext context)
        {
            // Set version (x-v) we are responding with in the response header to the parsed in version
            context.HttpContext.Response.Headers[Constants.Headers.X_V] = this._version;

            base.OnActionExecuted(context);
        }
    }
}
