using CDR.Register.API.Infrastructure.Models;
using CDR.Register.Repository.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;

namespace CDR.Register.API.Infrastructure.Filters
{
    /// <summary>
    /// Checks the industry parameter is supported, if not then responds with BadRequest and appropriate ResponseErrorList
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
    public class CheckIndustryAttribute : ActionFilterAttribute
    {
        private readonly string _type;

        public CheckIndustryAttribute(IndustryEnum type = 0)
        {
            if (Enum.IsDefined(typeof(IndustryEnum), type.ToString()))
                _type = type.ToString().ToUpper();
            else
                _type = "";
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var industry = context.ActionArguments["industry"] as string;

            if (!string.IsNullOrEmpty(_type) && _type.Equals(IndustryEnum.BANKING.ToString()))
            {
                if (industry != null && industry.ToUpper() != _type)
                    context.Result = new BadRequestObjectResult(new ResponseErrorList().InvalidIndustry());
            }
            else
            {
                if (industry != null && !Enum.IsDefined(typeof(IndustryEnum), industry.ToUpper()))
                    context.Result = new BadRequestObjectResult(new ResponseErrorList().InvalidIndustry());
            }

            base.OnActionExecuting(context);
        }
    }
}
