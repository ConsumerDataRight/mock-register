﻿using CDR.Register.API.Infrastructure.Models;
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
        private readonly string _industryRestriction;

        public CheckIndustryAttribute()
        {
            _industryRestriction = "";
        }

        public CheckIndustryAttribute(Industry industryRestriction)
        {
            _industryRestriction = industryRestriction.ToString().ToUpper();
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var industry = context.ActionArguments["industry"] as string;

            if (!IsValidIndustry(industry))
            {
                context.Result = new BadRequestObjectResult(new ResponseErrorList().InvalidIndustry());
            }

            base.OnActionExecuting(context);
        }

        private bool IsValidIndustry(string industry)
        {
            // Industry needs to be provided.
            if (string.IsNullOrEmpty(industry))
            {
                return false;
            }

            // Convert the incoming industry value to an enum.
            if (!Enum.TryParse<Industry>(industry.ToUpper(), out Industry industryItem))
            {
                return false;
            }

            // Check that the incoming industry matches the industry restriction, if set.
            if (!string.IsNullOrEmpty(_industryRestriction) && _industryRestriction != industryItem.ToString().ToUpper())
            {
                return false;
            }

            return true;
        }
    }
}
