﻿using System;
using CDR.Register.Domain.Models;
using CDR.Register.Repository.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CDR.Register.API.Infrastructure.Filters
{
    /// <summary>
    /// Checks the industry parameter is supported, if not then responds with BadRequest and appropriate ResponseErrorList.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
    public class CheckIndustryAttribute : ActionFilterAttribute
    {
        private readonly string _industryRestriction;

        public CheckIndustryAttribute()
        {
            this._industryRestriction = string.Empty;
        }

        public CheckIndustryAttribute(Industry industryRestriction)
        {
            this._industryRestriction = industryRestriction.ToString().ToUpper();
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (context.ActionArguments["industry"] is string industry && !this.IsValidIndustry(industry))
            {
                context.Result = new BadRequestObjectResult(new ResponseErrorList().AddInvalidIndustry());
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
            if (!string.IsNullOrEmpty(this._industryRestriction) && !string.Equals(this._industryRestriction, industryItem.ToString(), StringComparison.CurrentCultureIgnoreCase))
            {
                return false;
            }

            return true;
        }
    }
}
