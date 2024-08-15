using System;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;

namespace CDR.Register.API.Infrastructure.Filters
{
    /// <summary>
    /// Provides support for response caching using ETags.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
    public class ETagAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuted(ActionExecutedContext context)
        {
            var request = context.HttpContext.Request;
            var response = context.HttpContext.Response;

            if (request.Method == HttpMethod.Get.Method && response.StatusCode == (int)HttpStatusCode.OK)
            {
                var res = JsonConvert.SerializeObject(context.Result);

                // Generate etag string from the response body.
                var etag = GenerateETag(res);

                // Fetch etag from the incoming request header.
                if (request.Headers.TryGetValue(HeaderNames.IfNoneMatch, out var headerValues))
                {
                    var incomingEtag = headerValues.ToString().Trim('"');

                    // If both the etags are equal, so return a 304 Not Modified response.
                    if (incomingEtag.Equals(etag))
                    {
                        context.Result = new StatusCodeResult((int)HttpStatusCode.NotModified);
                    }
                }

                // Add ETag response header 
                response.Headers.Append(HeaderNames.ETag, $"\"{etag}\"");
            }

            base.OnActionExecuted(context);
        }

        private static string GenerateETag(string response)
        {
            if (String.IsNullOrEmpty(response))
                return String.Empty;

            using (var sha = SHA256.Create())
            {
                byte[] textData = System.Text.Encoding.UTF8.GetBytes(response);
                byte[] hash = sha.ComputeHash(textData);
                return BitConverter.ToString(hash).Replace("-", String.Empty);
            }
        }
    }
}
