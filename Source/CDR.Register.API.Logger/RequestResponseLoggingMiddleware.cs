using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Web;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IO;
using Microsoft.Net.Http.Headers;
using Serilog;
using Serilog.Events;

namespace CDR.Register.API.Logger
{
    public class RequestResponseLoggingMiddleware
    {
        private const string HttpSummaryMessageTemplate =
            "HTTP {RequestMethod} {RequestScheme:l}://{RequestHost:l}{RequestPathBase:l}{RequestPath:l} responded {StatusCode} in {ElapsedTime:0.0000} ms.";

        private const string HttpSummaryExceptionMessageTemplate =
            "HTTP {RequestMethod} {RequestScheme:l}://{RequestHost:l}{RequestPathBase:l}{RequestPath:l} encountered following error {error}";

        private readonly string? _currentProcessName;
        private readonly RecyclableMemoryStreamManager _recyclableMemoryStreamManager;
        private readonly RequestDelegate _next;
        private readonly ILogger _requestResponseLogger;
        private readonly IConfiguration _configuration;

        private string? _requestMethod;
        private string? _requestBody;
        private string? _requestHeaders;
        private string? _requestPath;
        private string? _requestQueryString;
        private string? _statusCode;
        private string? _elapsedTime;
        private string? _responseHeaders;
        private string? _responseBody;
        private string? _requestHost;
        private string? _requestIpAddress;
        private string? _requestScheme;
        private string? _exceptionMessage;
        private string? _requestPathBase;
        private string? _clientId;
        private string? _softwareId;
        private string? _fapiInteractionId;
        private string? _dataHolderBrandId;

        public RequestResponseLoggingMiddleware(RequestDelegate next, IRequestResponseLogger requestResponseLogger, IConfiguration configuration)
        {
            this._requestResponseLogger = requestResponseLogger.Log.ForContext<RequestResponseLoggingMiddleware>();
            this._next = next ?? throw new ArgumentNullException(nameof(next));
            this._recyclableMemoryStreamManager = new RecyclableMemoryStreamManager();
            this._currentProcessName = System.Reflection.Assembly.GetEntryAssembly()?.GetName().Name;
            this._configuration = configuration;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            this.InitMembers();
            await this.ExtractRequestProperties(context);
            await this.ExtractResponseProperties(context);
        }

        private static void SetIdFromJwt(string jwt, string identifierType, ref string idToSet)
        {
            var handler = new JwtSecurityTokenHandler();
            if (handler.CanReadToken(jwt))
            {
                var decodedJwt = handler.ReadJwtToken(jwt);
                var id = decodedJwt.Claims.FirstOrDefault(x => x.Type == identifierType)?.Value ?? string.Empty;

                idToSet = id;
            }
        }

        private void InitMembers()
        {
            this._requestMethod = this._requestBody = this._requestHeaders = this._requestPath = this._requestQueryString =
            this._statusCode = this._elapsedTime = this._responseHeaders = this._responseBody = this._requestHost = this._requestScheme =
            this._exceptionMessage = this._requestPathBase = this._clientId = this._softwareId = this._fapiInteractionId = this._requestIpAddress = this._dataHolderBrandId = string.Empty;
        }

        private void LogWithContext()
        {
            var logger = this._requestResponseLogger
                .ForContext("SourceContext", this.GetSourceContext())
                .ForContext("RequestMethod", this._requestMethod)
                .ForContext("RequestBody", this._requestBody)
                .ForContext("RequestHeaders", this._requestHeaders)
                .ForContext("RequestPath", this._requestPath)
                .ForContext("RequestQueryString", this._requestQueryString)
                .ForContext("StatusCode", this._statusCode)
                .ForContext("RequestHost", this._requestHost)
                .ForContext("RequestIpAddress", this._requestIpAddress)
                .ForContext("ResponseHeaders", this._responseHeaders)
                .ForContext("ResponseBody", this._responseBody)
                .ForContext("ClientId", this._clientId)
                .ForContext("SoftwareId", this._softwareId)
                .ForContext("FapiInteractionId", this._fapiInteractionId)
                .ForContext("DataHolderBrandId", this._dataHolderBrandId);

            if (!string.IsNullOrEmpty(this._exceptionMessage))
            {
                logger.Error(HttpSummaryExceptionMessageTemplate, this._requestMethod, this._requestScheme, this._requestHost, this._requestPathBase, this._requestPath, this._exceptionMessage);
            }
            else
            {
                logger.Write(LogEventLevel.Information, HttpSummaryMessageTemplate, this._requestMethod, this._requestScheme, this._requestHost, this._requestPathBase, this._requestPath, this._statusCode, this._elapsedTime);
            }
        }

        private async Task ExtractRequestProperties(HttpContext context)
        {
            try
            {
                context.Request.EnableBuffering();
                await using var requestStream = this._recyclableMemoryStreamManager.GetStream();
                await context.Request.Body.CopyToAsync(requestStream);

                this._requestBody = this.ReadStreamInChunks(requestStream);
                context.Request.Body.Position = 0;

                this._requestHost = this.GetHost(context.Request);
                this._requestIpAddress = this.GetIpAddress(context);
                this._requestMethod = context.Request.Method;
                this._requestScheme = context.Request.Scheme;
                this._requestPath = context.Request.Path;
                this._requestQueryString = context.Request.QueryString.ToString();
                this._requestPathBase = context.Request.PathBase.ToString();

                IEnumerable<string> keyValues = context.Request.Headers.Keys.Select(key => key + ": " + string.Join(",", context.Request.Headers[key].ToArray()));
                this._requestHeaders = string.Join(Environment.NewLine, keyValues);

                this.ExtractIdFromRequest(context.Request);
            }
            catch (Exception ex)
            {
                this._exceptionMessage = ex.Message;
            }
        }

        private void ExtractIdFromRequest(HttpRequest request)
        {
            try
            {
                // try fetching from the clientid in the body for connect/par
                if (!string.IsNullOrEmpty(this._requestBody) && this._requestBody.Contains("client_assertion=") && string.IsNullOrEmpty(this._clientId))
                {
                    var nameValueCollection = HttpUtility.ParseQueryString(this._requestBody);
                    if (nameValueCollection != null)
                    {
                        var assertion = nameValueCollection["client_assertion"];

                        if (assertion != null)
                        {
                            // in this case we set the iss to clientid
                            this._softwareId = string.Empty;
                            SetIdFromJwt(assertion, ClaimIdentifiers.Iss, ref this._softwareId);
                        }
                    }
                }

                // try fetching x-fapi-interaction-id. After fetching we don't return as we need other important ids.
                this._fapiInteractionId = string.Empty;
                if (request.Headers.TryGetValue("x-fapi-interaction-id", out var interactionid))
                {
                    this._fapiInteractionId = interactionid;
                }

                // try fetching from the JWT in the authorization header
                var authorization = request.Headers[HeaderNames.Authorization];
                if (AuthenticationHeaderValue.TryParse(authorization, out var headerValue) && string.IsNullOrEmpty(this._softwareId))
                {
                    var scheme = headerValue.Scheme;
                    var parameter = headerValue.Parameter;

                    if (scheme == JwtBearerDefaults.AuthenticationScheme && parameter != null)
                    {
                        this._softwareId = string.Empty;
                        SetIdFromJwt(parameter, ClaimIdentifiers.ClientId, ref this._softwareId);
                    }
                }
            }
            catch (Exception ex)
            {
                this._exceptionMessage = ex.Message;
            }
        }

        private string ReadStreamInChunks(Stream stream)
        {
            try
            {
                const int readChunkBufferLength = 4096;
                stream.Seek(0, SeekOrigin.Begin);
                using var textWriter = new StringWriter();
                using var reader = new StreamReader(stream);
                var readChunk = new char[readChunkBufferLength];
                int readChunkLength;
                do
                {
                    readChunkLength = reader.ReadBlock(readChunk, 0, readChunkBufferLength);
                    textWriter.Write(readChunk, 0, readChunkLength);
                }
                while (readChunkLength > 0);
                return textWriter.ToString();
            }
            catch (Exception ex)
            {
                this._exceptionMessage = ex.Message;
            }

            return string.Empty;
        }

        private async Task ExtractResponseProperties(HttpContext httpContext)
        {
            var originalBodyStream = httpContext.Response.Body;
            await using var responseBody = this._recyclableMemoryStreamManager.GetStream();
            httpContext.Response.Body = responseBody;

            var sw = Stopwatch.StartNew();

            try
            {
                await this._next(httpContext);
            }
            catch (Exception ex)
            {
                this._exceptionMessage = ex.Message;
                throw;
            }
            finally
            {
                sw.Stop();
                this._elapsedTime = sw.ElapsedMilliseconds.ToString();

                responseBody.Seek(0, SeekOrigin.Begin);
                this._responseBody = await new StreamReader(responseBody).ReadToEndAsync();
                responseBody.Seek(0, SeekOrigin.Begin);

                IEnumerable<string> keyValues = httpContext.Response.Headers.Keys.Select(key => key + ": " + string.Join(",", httpContext.Response.Headers[key].ToArray()));
                this._responseHeaders = string.Join(System.Environment.NewLine, keyValues);

                this._statusCode = httpContext.Response.StatusCode.ToString();

                this.LogWithContext();

                // This is for middleware hooked before us to see our changes.
                // Otherwise the original stream would be seen which cannot be read again.
                // The middleware that sends the response to the client is affected as well.
                await responseBody.CopyToAsync(originalBodyStream);
            }
        }

        private string GetHost(HttpRequest request)
        {
            // 1. check if the X-Forwarded-Host header has been provided -> use that
            // 2. If not, use the request.Host
            string hostHeaderKey = this._configuration.GetValue<string>("SerilogRequestResponseLogger:HostNameHeaderKey") ?? "X-Forwarded-Host";

            if (!request.Headers.TryGetValue(hostHeaderKey, out var keys))
            {
                return request.Host.ToString();
            }

            return keys[0] ?? string.Empty;
        }

        private string? GetIpAddress(HttpContext context)
        {
            string ipHeaderKey = this._configuration.GetValue<string>("SerilogRequestResponseLogger:IPAddressHeaderKey") ?? "X-Forwarded-For";

            if (!context.Request.Headers.TryGetValue(ipHeaderKey, out var keys))
            {
                return context.Connection.RemoteIpAddress?.ToString();
            }

            // The Client IP address may contain a comma separated list of ip addresses based on the network devices
            // the traffic traverses through.  We get the first (and potentially only) ip address from the list as the client IP.
            // We also remove any port numbers that may be included on the client IP.
            return keys[0]?
                .Split(',')[0] // Get the first IP address in the list, in case there are multiple.
                .Split(':')[0]; // Strip off the port number, in case it is attached to the IP address.
        }

        private string GetSourceContext()
        {
            switch (this._currentProcessName)
            {
                case "CDR.Register.Discovery.API":
                    return "SB-REG-DISC";
                case "CDR.Register.Infosec":
                    return "SB-REG-IS";
                case "CDR.Register.SSA.API":
                    return "SB-REG-SSA";
                case "CDR.Register.Status.API":
                    return "SB-REG-STS";
            }

            return string.Empty;
        }

        private static class ClaimIdentifiers
        {
            public const string ClientId = "client_id";
            public const string Iss = "iss";
        }
    }
}
