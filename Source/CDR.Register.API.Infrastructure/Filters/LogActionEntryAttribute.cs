﻿using System;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Serilog.Context;

namespace CDR.Register.API.Infrastructure.Filters
{
    [AttributeUsage(AttributeTargets.Method)]
    public class LogActionEntryAttribute : ActionFilterAttribute
    {
        private readonly ILogger _logger;

        public LogActionEntryAttribute(ILogger<LogActionEntryAttribute> logger)
        {
            this._logger = logger;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var controller = context.RouteData.Values["controller"]?.ToString();
            var action = context.RouteData.Values["action"]?.ToString();
            using (LogContext.PushProperty("MethodName", action))
            {
                this._logger.LogInformation("Request received to {Controller}.{Action}", controller, action);
            }

            base.OnActionExecuting(context);
        }
    }
}
