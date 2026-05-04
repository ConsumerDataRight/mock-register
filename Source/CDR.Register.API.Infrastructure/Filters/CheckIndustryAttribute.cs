using System;
using System.Linq;
using CDR.Register.Domain.Extensions;
using CDR.Register.Domain.Models;
using CDR.Register.Repository.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CDR.Register.API.Infrastructure.Filters
{
    /// <summary>
    /// Checks the <c>industry</c> parameter(s) are supported, if not then responds with <see cref="BadRequestObjectResult"/> and appropriate <see cref="ResponseErrorList"/> payload.
    /// </summary>
    /// <remarks>The industry parameter is case-insensitive.</remarks>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
    public class CheckIndustryAttribute : ActionFilterAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CheckIndustryAttribute"/> class, configured to allow any valid industry.
        /// </summary>
        public CheckIndustryAttribute()
        {
            this.IndustryRestrictions = [];
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CheckIndustryAttribute"/> class, configured to only allow industries specified in <paramref name="industryRestrictions"/>.
        /// </summary>
        /// <param name="industryRestrictions">The specific industries this endpoint allows.</param>
        public CheckIndustryAttribute(params Industry[] industryRestrictions)
        {
            this.IndustryRestrictions = industryRestrictions;
        }

        public Industry[] IndustryRestrictions { get; }

        /// <summary>
        /// Validate that the industry provided is allowed based on the industry restrictions configured for this instance.
        /// </summary>
        /// <remarks>Changes the action result to a <see cref="BadRequestObjectResult"/> with an appropriate error message if the industry is not allowed.</remarks>
        /// <param name="context">The executing action context.</param>
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (context.ActionArguments["industry"] is string industry && !this.IsValidIndustry(industry))
            {
                context.Result = new BadRequestObjectResult(new ResponseErrorList().AddInvalidIndustry());
            }

            base.OnActionExecuting(context);
        }

        /// <summary>
        /// Validate the industry provided is:
        /// <list type="bullet">
        ///   <item>not null</item>
        ///   <item>a valid industry</item>
        ///   <item>allowed based on restrictions</item>
        /// </list>
        /// </summary>
        /// <param name="industry">The industry requested.</param>
        /// <remarks>Industry value is case-insensitive.</remarks>
        /// <returns>A flag indicating if the industry is valid, and allowed.</returns>
        private bool IsValidIndustry(string industry)
        {
            // Industry needs to be provided.
            if (string.IsNullOrEmpty(industry))
            {
                return false;
            }

            // Convert the incoming industry value to an enum.
            if (!EnumExtensions.TryParseFromDescription(industry, out Industry industryItem))
            {
                return false;
            }

            // Check that the incoming industry matches the industry restriction, if any are specified.
            if (this.IndustryRestrictions.Any() && !this.IndustryRestrictions.Contains(industryItem))
            {
                return false;
            }

            return true;
        }
    }
}
