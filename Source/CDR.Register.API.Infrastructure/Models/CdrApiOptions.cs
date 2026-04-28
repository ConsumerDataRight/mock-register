using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace CDR.Register.API.Infrastructure.Models
{
    public class CdrApiOptions
    {
        private const string Id = @"[A-Za-z0-9\-]*"; // any letter, number, or hyphen
        private const string Industry = @"(%7B)*[A-Za-z-]*(%7D)*"; // (%7B)* and (%7D)* represent the possibility of a { and } in the path, to cover the value '{industry}' for OAS generation purposes
        private const string BasePathV1 = @$"\/cdr-register\/v1\/{Industry}";
        private const string AdminBasePath = @"\/admin";

        private static readonly CdrApiEndpointVersionOptions[] _supportedApiVersions =
            [
                new CdrApiEndpointVersionOptions(@$"{BasePathV1}\/data-holders\/brands", true, 2, 3, 3),
                new CdrApiEndpointVersionOptions(@$"{BasePathV1}\/data-holders\/status", false, 1, 2, 2),   // This is intentionally not enforcing x-v header to be mandatory inline with v1 of the standards, if v1 becomes obsolete isXvMandatory should be true.
                new CdrApiEndpointVersionOptions(@$"{BasePathV1}\/data-recipients", true, 3, 4, 4),
                new CdrApiEndpointVersionOptions(@$"{BasePathV1}\/data-recipients\/status", true, 2, 3, 3),
                new CdrApiEndpointVersionOptions(@$"{BasePathV1}\/data-recipients\/brands\/software-products\/status", true, 2, 3, 3),
                new CdrApiEndpointVersionOptions(@$"{BasePathV1}\/data-recipients\/brands\/{Id}\/software-products\/{Id}\/ssa", true, 3, 4, 4),
            ];

        private static readonly CdrApiEndpointVersionOptions[] _supportedAdminVersions =
            [
                new CdrApiEndpointVersionOptions(@$"{AdminBasePath}\/metadata\/data-holders", true, 1),
                new CdrApiEndpointVersionOptions(@$"{AdminBasePath}\/metadata\/data-recipients", true, 1),
            ];

        public List<CdrApiEndpointVersionOptions> EndpointVersionOptions { get; } = [.. _supportedApiVersions, .. _supportedAdminVersions];

        public string DefaultVersion { get; set; } = "1";

        public CdrApiEndpointVersionOptions? GetApiEndpointVersionOption(PathString path)
        {
            foreach (var supportedApi in this.EndpointVersionOptions.OrderByDescending(v => v.Path.Length))
            {
                var regEx = new System.Text.RegularExpressions.Regex(supportedApi.Path, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                if (regEx.IsMatch(path))
                {
                    return supportedApi;
                }
            }

            return null;
        }
    }
}
