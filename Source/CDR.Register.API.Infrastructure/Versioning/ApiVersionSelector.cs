using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using System;
using System.Collections.Generic;
using System.Linq;
using static CDR.Register.API.Infrastructure.Constants;

namespace CDR.Register.API.Infrastructure.Versioning
{
    public class ApiVersionSelector : IApiVersionSelector
    {
        private readonly Dictionary<string, int[]> _supportedApiVersions = new Dictionary<string, int[]> {
            { @"\/cdr-register\/v1\/[A-Za-z]*\/data-holders\/brands", new int[] { 1, 2 } },
            { @"\/cdr-register\/v1\/[A-Za-z]*\/data-holders\/status", new int[] { 1 } },
            { @"\/cdr-register\/v1\/[A-Za-z]*\/data-recipients", new int[] { 1, 2, 3 } },
            { @"\/cdr-register\/v1\/[A-Za-z]*\/data-recipients\/status", new int[] { 1, 2 } },
            { @"\/cdr-register\/v1\/[A-Za-z]*\/data-recipients\/brands\/software-products\/status", new int[] { 1, 2 } },
            { @"\/cdr-register\/v1\/[A-Za-z]*\/data-recipients\/brands\/[A-Za-z0-9\-]*\/software-products\/[A-Za-z0-9\-]*\/ssa", new int[] { 1, 2, 3 } },
        };

        private readonly ApiVersion _defaultVersion;

        public ApiVersionSelector(ApiVersioningOptions options)
        {
            _defaultVersion = options.DefaultApiVersion;
        }

        public ApiVersion SelectVersion(HttpRequest request, ApiVersionModel model)
        {
            // Try and get x-v value from request header
            if (!request.Headers.TryGetValue(Headers.X_V, out var x_v) || string.IsNullOrEmpty(x_v))
            {
                return _defaultVersion;
            }

            // x-v must be a positive integer.
            if (!int.TryParse(x_v, out int xvVersion) || xvVersion < 1)
            {
                // Raise an error.
                throw new InvalidVersionException(Headers.X_V);
            }

            // If requested version is 1, then just return.
            if (xvVersion == 1)
            {
                return _defaultVersion;
            }

            // Check if the requested version is supported by the API.
            var apiVersions = GetApiVersions(request.Path);

            // Matching api was not found.
            if (!apiVersions.Any())
            {
                return _defaultVersion;
            }

            // Version match.
            if (apiVersions.Contains(xvVersion))
            {
                return new ApiVersion(xvVersion, 0);
            }

            // No matching version, so check if a x-min-v header has been provided.
            if (!request.Headers.ContainsKey(Headers.X_MIN_V))
            {
                // x-min-v has not been provided, so throw an unsupported version error.
                throw new UnsupportedVersionException(apiVersions.Min(), apiVersions.Max());
            }

            // Check if the x-min-v is a positive integer.
            var x_min_v = request.Headers[Headers.X_MIN_V];

            // x-min-v must be a positive integer.
            if (!int.TryParse(x_min_v, out int xvMinVersion) || xvMinVersion < 1)
            {
                // Raise an invalid error.
                throw new InvalidVersionException(Headers.X_MIN_V);
            }

            // If x-min-v is greater than x-v then ignore it.
            if (xvMinVersion > xvVersion)
            {
                xvMinVersion = xvVersion;
            }

            // Find the largest supported version between x-min-v and x-v.
            var supportedVersions = apiVersions
                .Where(v => (v >= xvMinVersion && v <= xvVersion));

            // No supported versions were found.
            if (!supportedVersions.Any())
            {
                throw new UnsupportedVersionException(apiVersions.Min(), apiVersions.Max());
            }

            // Return the highest support version.
            return new ApiVersion(supportedVersions.OrderByDescending(v => v).Take(1).Single(), 0);
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
