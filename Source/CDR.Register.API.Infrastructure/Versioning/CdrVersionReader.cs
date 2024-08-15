using CDR.Register.API.Infrastructure.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.Extensions.Primitives;

namespace CDR.Register.API.Infrastructure.Versioning
{
    /// <summary>
    /// The purpose of the reader is to check what the client has put in and return what we want from that. We return an error string if it is no good and we are able to handle changing min version.
    /// </summary>
    public class CdrVersionReader : IApiVersionReader
    {
        private readonly CdrApiOptions _options;

        public CdrVersionReader(CdrApiOptions options)
        {
            _options = options;
        }

        public void AddParameters(IApiVersionParameterDescriptionContext context)
        {
            context.AddParameter("x-v", ApiVersionParameterLocation.Header);
            context.AddParameter("x-min-v", ApiVersionParameterLocation.Header);
        }

        public string Read(HttpRequest request)
        {
            var endpointOption = _options.GetApiEndpointVersionOption(request.Path);

            if (endpointOption == null)
            {
                //handle any endpoint that hasn't been defined in options
                endpointOption = new CdrApiEndpointVersionOptions(string.Empty, false, int.Parse(_options.DefaultVersion));
            }
            else if (!endpointOption.IsVersioned)
            {
                return _options.DefaultVersion;
            }

            // If x-min-v is passed in, we expect it to be a Positive Integer, the x-v value is parsed out of the header and will be validated by the Package itself
            // Refer to Standards here - https://consumerdatastandardsaustralia.github.io/standards/#http-headers
            // When there is error in x-min-v, we return custom String x-min-v '<value>' this is then manipulated in ApiVersionErrorResponse for a user friendly error message.
            if (!request.Headers.TryGetValue("x-v", out var xvValue) || string.IsNullOrWhiteSpace(xvValue))
            {
                if (endpointOption.IsXVHeaderMandatory)
                {
                    return Domain.Constants.ErrorTitles.MissingVersion;
                }

                xvValue = endpointOption.CurrentMinVersion.ToString();
                request.Headers.Remove("x-v"); // Added this to handle both the missing header (just add) and the header with a blank value (update)
                request.Headers.Append("x-v", xvValue);
            }

            if (int.TryParse(xvValue, out int xvInt) && xvInt > 0)
            {
                request.Headers.TryGetValue("x-min-v", out var xvMinValue);

                xvValue = CalculateVersion(xvInt, xvMinValue, endpointOption);

                return xvValue;
            }

            return Domain.Constants.ErrorTitles.InvalidVersion;
        }

        private static string CalculateVersion(int xvInt, string xvMinString, CdrApiEndpointVersionOptions endpointOption)
        {
            if (!string.IsNullOrEmpty(xvMinString))
            {
                if (!int.TryParse(xvMinString, out int xvMinInt) || xvMinInt < 1)
                {
                    return Domain.Constants.ErrorTitles.InvalidVersion;
                }

                if (xvMinInt < xvInt)
                {
                    if (xvMinInt > endpointOption.CurrentMaxVersion || xvInt < endpointOption.CurrentMinVersion)
                    {
                        return Domain.Constants.ErrorTitles.UnsupportedVersion;
                    }

                    return xvInt > endpointOption.CurrentMaxVersion ? endpointOption.CurrentMaxVersion.ToString() : xvInt.ToString();
                }
            }

            if (xvInt < endpointOption.CurrentMinVersion || xvInt > endpointOption.CurrentMaxVersion)
            {
                return Domain.Constants.ErrorTitles.UnsupportedVersion;
            }

            return xvInt.ToString();
        }
    }
}
