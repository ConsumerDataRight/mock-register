using System;
using CDR.Register.API.Infrastructure.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CDR.Register.API.Infrastructure.Filters
{
    /// <summary>
    /// Checks the industry parameter is supported, if not then responds with BadRequest and appropriate ResponseErrorList
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
    public class CheckIndustryAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var industry = context.ActionArguments["industry"] as string;
            if (industry != null && industry != "banking")
            {
                context.Result = new BadRequestObjectResult(new ResponseErrorList().InvalidIndustry());
            }

            base.OnActionExecuting(context);
        }
    }
}
