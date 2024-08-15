using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Linq;

namespace CDR.Register.API.Infrastructure.Models
{
    public class CdrApiOptions
    {
        private static readonly List<CdrApiEndpointVersionOptions> _supportedApiVersions = new List<CdrApiEndpointVersionOptions>
            {
                //(?i) is to make the regex case insensitive
                //(%7B)* and (%7D)* represent the possibility of a { and } in the path, to cover the value '{industry}' for OAS generation purposes

                //Register
                new CdrApiEndpointVersionOptions(@"(?i)\/cdr-register\/v1\/(%7B)*[A-Za-z]*(%7D)*\/data-holders\/brands", true, 2),
                new CdrApiEndpointVersionOptions(@"(?i)\/cdr-register\/v1\/(%7B)*[A-Za-z]*(%7D)*\/data-holders\/status",false,1),
                new CdrApiEndpointVersionOptions(@"(?i)\/cdr-register\/v1\/(%7B)*[A-Za-z]*(%7D)*\/data-recipients",true,3),
                new CdrApiEndpointVersionOptions(@"(?i)\/cdr-register\/v1\/(%7B)*[A-Za-z]*(%7D)*\/data-recipients\/status",true,2),
                new CdrApiEndpointVersionOptions(@"(?i)\/cdr-register\/v1\/(%7B)*[A-Za-z]*(%7D)*\/data-recipients\/brands\/software-products\/status",true,2),
                new CdrApiEndpointVersionOptions(@"(?i)\/cdr-register\/v1\/(%7B)*[A-Za-z]*(%7D)*\/data-recipients\/brands\/[A-Za-z0-9\-]*\/software-products\/[A-Za-z0-9\-]*\/ssa",true,3),

                //Admin
                new CdrApiEndpointVersionOptions(@"(?i)\/admin\/metadata\/data-holders",true,1),
                new CdrApiEndpointVersionOptions(@"(?i)\/admin\/metadata\/data-recipients",true,1),
            };

        public List<CdrApiEndpointVersionOptions> EndpointVersionOptions { get; } = _supportedApiVersions;

        public string DefaultVersion { get; set; } = "1";

        public CdrApiEndpointVersionOptions? GetApiEndpointVersionOption(PathString path)
        {
            foreach (var supportedApi in EndpointVersionOptions.OrderByDescending(v => v.Path.Length))
            {
                var regEx = new System.Text.RegularExpressions.Regex(supportedApi.Path);
                if (regEx.IsMatch(path))
                {
                    return supportedApi;
                }
            }

            return null;
        }
    }
}
