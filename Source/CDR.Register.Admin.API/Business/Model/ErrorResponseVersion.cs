using CDR.Register.API.Infrastructure.Models;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using CDR.Register.API.Infrastructure;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System;

namespace CDR.Register.Admin.API.Business.Model
{
    public class ErrorResponseVersion : DefaultErrorResponseProvider
    {
        private readonly Dictionary<string, int[]> _supportedApiVersions = new Dictionary<string, int[]> {
            { @"\/admin\/metadata\/data-holders", new int[] { 1 } },
            { @"\/admin\/metadata\/data-recipients", new int[] { 1 } },
        };

        public override IActionResult CreateResponse(ErrorResponseContext context)
        {
            // The version was not specified.
            if (context.ErrorCode == "ApiVersionUnspecified")
            {
                return new ObjectResult(new ResponseErrorList(
                    new Error(Constants.ErrorCodes.HeaderMissing, Constants.ErrorTitles.HeaderMissing, Constants.Headers.X_V)))
                {
                    StatusCode = (int)HttpStatusCode.BadRequest
                };
            }

            // Get x-v from request header
            var versionHeaderValue = context.Request.Headers[Constants.Headers.X_V];
            var invalid_XV_Version = true;
            var apiVersions = GetApiVersions(context.Request.Path);
            var invalidVersionError = new Error(Constants.ErrorCodes.VersionInvalid, Constants.ErrorTitles.VersionInvalid, $"Expected '{string.Join(',', apiVersions)}'");

            // If the x-v is set, check that it is a postive integer.
            if (int.TryParse(versionHeaderValue, out int version))
            {
                invalid_XV_Version = version < 1;
            }

            if (invalid_XV_Version)
            {
                return new ObjectResult(new ResponseErrorList(invalidVersionError))
                {
                    StatusCode = (int)HttpStatusCode.BadRequest
                };
            }

            if (context.ErrorCode == "InvalidApiVersion")
            {
                return new ObjectResult(new ResponseErrorList(invalidVersionError))
                {
                    StatusCode = (int)HttpStatusCode.BadRequest
                };
            }

            if (context.ErrorCode == "UnsupportedApiVersion")
            {
                return new ObjectResult(new ResponseErrorList(invalidVersionError))
                {
                    StatusCode = (int)HttpStatusCode.BadRequest
                };
            }

            return base.CreateResponse(context);
        }

        private IEnumerable<int> GetApiVersions(PathString path)
        {
            foreach (var supportedApi in _supportedApiVersions.OrderByDescending(v => v.Key.Length))
            {
                var regEx = new System.Text.RegularExpressions.Regex(supportedApi.Key);
                if (regEx.IsMatch(path))
                {
                    return supportedApi.Value;
                }
            }

            return Array.Empty<int>();
        }
    }
}
