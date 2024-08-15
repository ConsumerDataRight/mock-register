using CDR.Register.API.Infrastructure.Authorization;
using CDR.Register.API.Infrastructure.Configuration;
using CDR.Register.API.Infrastructure.Models;
using CDR.Register.API.Infrastructure.SwaggerFilters;
using CDR.Register.Repository.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;

namespace CDR.Register.API.Infrastructure
{
    public static class Extensions
    {
        public static string GetInfosecBaseUrl(this IConfiguration configuration, HttpContext? context, bool isSecure = false)
        {
            var basePath = string.Empty;
            if (context.Request != null && context.Request.PathBase.HasValue)
            {
                basePath = context.Request.PathBase.ToString();
            }

            var hostName = isSecure
                ? configuration.GetValue<string>(Constants.ConfigurationKeys.SecureHostName)
                : configuration.GetValue<string>(Constants.ConfigurationKeys.PublicHostName);

            return $"{hostName}{basePath}/idp";
        }

        /// <summary>
        /// CTS conformance ids must be validated
        /// </summary>        
        /// <param name="context"></param>
        /// <returns></returns>        
        public static bool ValidateIssuer(this HttpContext context)
        {
            if (context.Request != null && context.Request.PathBase.HasValue)
            {
                // PathBase : /cts/{id}/register
                var issuer = context.User.Claims.FirstOrDefault(x => x.Type == "iss")?.Value;
                if (string.IsNullOrEmpty(issuer) && string.IsNullOrEmpty(context.Request.PathBase))
                {
                    return false;
                }

                // For a stronger match validating dynamic base path with an conformance ID instead of confromanceId only                
                return issuer?.Contains(context.Request.PathBase) ?? false;
            }

            return false;
        }

        public static void UseBasePathOrExpression(this IApplicationBuilder app, IConfiguration configuration)
        {
            var basePath = configuration.GetValue<string>(Constants.ConfigurationKeys.BasePath);
            if (!string.IsNullOrEmpty(basePath))
            {
                app.UsePathBase(basePath);
            }
            
            // A dynamic base path can be set by the Mock Register:BasePathExpression app setting.
            // This allows a regular expression to be set and matched rather than a static base path.
            var basePathExpression = configuration.GetValue<string>(Constants.ConfigurationKeys.BasePathExpression);
            if (!string.IsNullOrEmpty(basePathExpression))
            {
                app.Use((context, next) =>
                {
                    var matches = Regex.Matches(context.Request.Path, basePathExpression, RegexOptions.CultureInvariant | RegexOptions.IgnoreCase, matchTimeout: TimeSpan.FromMilliseconds(500));
                    if (matches.Count!=0)
                    {
                        var path = matches[0].Groups[0].Value;
                        var remainder = matches[0].Groups[1].Value;
                        context.Request.Path = $"/{remainder}";
                        context.Request.PathBase = path.Replace(remainder, "").TrimEnd('/');
                    }

                    return next(context);
                });
            }
        }

        public static IWebHostBuilder UseRegister(this IWebHostBuilder webBuilder, IConfiguration configuration)
        {
            webBuilder.UseKestrel((context, serverOptions) =>
            {
                serverOptions.Configure(context.Configuration.GetSection("Kestrel"))
                                .Endpoint("HTTPS", listenOptions =>
                                {
                                    listenOptions.HttpsOptions.SslProtocols = SslProtocols.Tls12;
                                });

                serverOptions.ConfigureHttpsDefaults(options =>
                {
                    options.SslProtocols = SslProtocols.Tls12;
                });
            })
            .UseIIS();

            return webBuilder;
        }

        public static void AddAuthenticationAuthorization(this IServiceCollection services, IConfiguration configuration)
        {
            var metadataAddress = configuration.GetValue<string>(Constants.ConfigurationKeys.OidcMetadataAddress);
            var jwks = Task.Run(() => LoadJwks($"{metadataAddress}/jwks", configuration)).Result;
            // Default 2 mins*
            var clockSkew = configuration.GetValue<int>(Constants.ConfigurationKeys.ClockSkewSeconds, 120);

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.Configuration = new OpenIdConnectConfiguration()
                {
                    JwksUri = $"{metadataAddress}/jwks",
                    JsonWebKeySet = jwks
                };

                options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters()
                {
                    ValidateAudience = true,
                    ValidAudience = "cdr-register",
                    ValidateIssuer = false,
                    RequireSignedTokens = true,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(clockSkew),
                    IssuerSigningKeys = options.Configuration.JsonWebKeySet?.Keys
                };

                // Ignore server certificate issues when retrieving OIDC configuration and JWKS.
                var handler = new HttpClientHandler();
                handler.SetServerCertificateValidation(configuration);
                options.BackchannelHttpHandler = handler;
            });

            // Authorization
            services.AddMvcCore().AddAuthorization(options =>
            {
                var allAuthPolicies = AuthorisationPolicies.GetAllPolicies();
                
                //Apply all listed policities from a single source of truth that is also used for self-documentation
                foreach(var pol in allAuthPolicies)
                {
                    options.AddPolicy(pol.Name, policy =>
                    {
                        policy.Requirements.Add(new ScopeRequirement(pol.ScopeRequirement));
                        if (pol.HasMtlsRequirement)
                        {
                            policy.Requirements.Add(new MtlsRequirement());
                        }
                    });
                }
            });
            services.AddSingleton<IAuthorizationHandler, ScopeHandler>();
            services.AddSingleton<IAuthorizationHandler, DataRecipientSoftwareProductIdHandler>();
            services.AddSingleton<IAuthorizationHandler, MtlsHandler>();

        }

        static async Task<Microsoft.IdentityModel.Tokens.JsonWebKeySet?> LoadJwks(string jwksUri, IConfiguration configuration)
        {
            var handler = new HttpClientHandler();
            handler.SetServerCertificateValidation(configuration);
            var httpClient = new HttpClient(handler);
            var httpResponse = await httpClient.GetAsync(jwksUri);

            var contentAsString = await httpResponse.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<Microsoft.IdentityModel.Tokens.JsonWebKeySet>(contentAsString);
        }

        public static string GetHostName(this string url)
        {
            return url.Replace("https://", "").Replace("http://", "").Split('/')[0];
        }

        public static LinksPaginated GetPaginated(
            this ControllerBase controller,
            string routeName,
            IConfiguration configuration,
            DateTime? updatedSince,
            int? currentPage,
            int totalPages,
            int? pageSize,
            string hostName = "",
            bool isSecure = false)
        {
            var currentUrl = controller.Request.GetDisplayUrl();
            var links = new LinksPaginated
            {
                Self = new Uri(currentUrl)
            };

            links.Self = ReplaceUriHost(currentUrl, controller.GetHostNameAsUri(configuration, hostName, isSecure));
            hostName = links.Self.ToString().GetHostName();

            if (totalPages > 0)
            {
                links.First = controller.GetPageUri(routeName, updatedSince, 1, pageSize, hostName);
                links.Last = controller.GetPageUri(routeName, updatedSince, totalPages, pageSize, hostName);
                if (currentPage <= 1)
                {
                    links.Prev = null;
                }
                else
                {
                    links.Prev = controller.GetPageUri(routeName, updatedSince, currentPage - 1, pageSize, hostName);
                }
                if (currentPage >= totalPages)
                {
                    links.Next = null;
                }
                else
                {
                    links.Next = controller.GetPageUri(routeName, updatedSince, currentPage + 1, pageSize, hostName);
                }
            }

            return links;
        }

        public static Links GetSelf(this ControllerBase controller, IConfiguration configuration, HttpContext context, string hostName = "")
        {
            var currentUrl = controller.Request.GetDisplayUrl();
            var links = new Links
            {
                Self = new Uri(currentUrl)
            };
            links.Self = ReplaceUriHost(currentUrl, controller.GetHostNameAsUri(configuration, hostName));
            return links;
        }

        public static string GetHostNameAsUri(this ControllerBase controller, IConfiguration configuration, string hostName, bool isSecure = false)
        {
            var hostNameToUse = isSecure
                ? configuration.GetValue<string>(Constants.ConfigurationKeys.SecureHostName)
                : configuration.GetValue<string>(Constants.ConfigurationKeys.PublicHostName);

            if (string.IsNullOrEmpty(hostNameToUse))
            {
                if (controller.Request.Headers.TryGetValue("X-Forwarded-Host", out StringValues forwardedHosts))
                {
                    hostNameToUse = forwardedHosts[0];                                        
                }
                else
                {
                    hostNameToUse = hostName;
                }

                if (hostNameToUse!= null && !hostNameToUse.StartsWith("https", StringComparison.OrdinalIgnoreCase))
                {
                    hostNameToUse = "https://" +  hostNameToUse;
                }
            }
            return hostNameToUse ?? "";
        }

        public static Uri? GetPageUri(
            this ControllerBase controller,
            string routeName,
            DateTime? updatedSince,
            int? currentPage,
            int? pageSize,
            string newHostName)
        {
            string? url = null;

            if (updatedSince.HasValue && currentPage.HasValue && pageSize.HasValue)
            {
                url = controller.Url.RouteUrl(
                    routeName,
                    new { updated_since = updatedSince.Value.ToUniversalTime().ToString("o"), page = currentPage, page_size = pageSize },
                    controller.Url.ActionContext.HttpContext.Request.Scheme);
            }
            else if (updatedSince.HasValue)
            {
                url = controller.Url.RouteUrl(
                    routeName,
                    new { updated_since = updatedSince.Value.ToUniversalTime().ToString("o") },
                    controller.Url.ActionContext.HttpContext.Request.Scheme);
            }
            else if (currentPage.HasValue && pageSize.HasValue)
            {
                url = controller.Url.RouteUrl(
                    routeName,
                    new { page = currentPage, page_size = pageSize },
                    controller.Url.ActionContext.HttpContext.Request.Scheme);
            }

            if (string.IsNullOrWhiteSpace(url))
            {
                return null;
            }

            url = url.Replace("updated_since", "updated-since");
            url = url.Replace("page_size", "page-size");

            var currentHost = url.GetHostName();
            return new Uri(url.Replace(currentHost, newHostName));
        }

        private static Uri ReplaceUriHost(string url, string newHost = null)
        {
            Uri originalUri = new(url);
            Uri replaceUri = new(newHost);

            // Update the Uri components
            UriBuilder modifiedUriBuilder = new(originalUri)
            {
                Host = replaceUri.Host,
                Port = replaceUri.IsDefaultPort ? -1 : replaceUri.Port,
            };

            return modifiedUriBuilder.Uri;
        }

        public static Industry ToIndustry(this string industry)
        {
            if (Enum.IsDefined(typeof(Industry), industry.ToUpper()))
                return (Industry)Enum.Parse(typeof(Industry), industry, true);
            else
                throw new NotSupportedException($"Invalid industry: {industry}");
        }

        public static IEnumerable<string> GetValueAsList(this IConfiguration configuration, string key, string delimiter)
        {
            string? value = configuration.GetValue<string>(key);
            if (string.IsNullOrEmpty(value))
            {
                return Array.Empty<string>();
            }

            return value.Split(delimiter);
        }

        public static string GetClientCertificateThumbprint(this HttpContext context, IConfiguration configuration)
        {
            var certThumbprintNameHttpHeaderName = configuration.GetValue<string>(Constants.ConfigurationKeys.CertThumbprintNameHttpHeaderName) ?? Constants.Headers.X_TLS_CLIENT_CERT_THUMBPRINT;

            if (context.Request.Headers.TryGetValue(certThumbprintNameHttpHeaderName, out StringValues headerThumbprints)  && headerThumbprints.Count > 0)
            {
                return headerThumbprints[0] ?? "";
            }

            return "";
        }

        public static string GetClientCertificateCommonName(this HttpContext context, ILogger logger, IConfiguration configuration)
        {
            string? headerCommonName;
            var certCommonNameHttpHeaderName = configuration.GetValue<string>(Constants.ConfigurationKeys.CertCommonNameHttpHeaderName) ?? Constants.Headers.X_TLS_CLIENT_CERT_COMMON_NAME;

            if (context.Request.Headers.TryGetValue(certCommonNameHttpHeaderName, out StringValues headerCommonNames) && headerCommonNames.Count > 0)
            {
                headerCommonName = headerCommonNames[0] ?? "";
            }
            else
            {
                return string.Empty;
            }

            var commonName = headerCommonName;

            if (commonName.IsDistinguishedName())
            {
                commonName = commonName.GetCommonNameFromDistinguishedName();
            }

            commonName = commonName.Trim('"');

            logger.LogInformation("Received commonName of {HeaderCommonName} in header and parsed commonName as {CommonName}", headerCommonName, commonName);
            return commonName;
        }

        public static bool IsDistinguishedName(this string value)
        {
            try
            {
                _ = new X500DistinguishedName(value);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static string GetCommonName(this string input)
        {
            var commonName = input;

            if (commonName.IsDistinguishedName())
            {
                commonName = commonName.GetCommonNameFromDistinguishedName();
            }

            return commonName.Trim('"');
        }

        public static string GetCommonNameFromDistinguishedName(this string distinguishedName)
        {
            try
            {
                X500DistinguishedName dn = new(distinguishedName);
                var cnAttribute = Array.Find(
                    dn.Decode(X500DistinguishedNameFlags.UseNewLines).Split('\n'), 
                    attr => attr.Trim().StartsWith("CN="));
                    

                if (cnAttribute != null)
                {
                    return cnAttribute.Trim().Substring(3);
                }

                return string.Empty; // Common Name not found
            }
            catch
            {
                return string.Empty;
            }

        }

        public static IServiceCollection AddCdrSwaggerGen(this IServiceCollection services, Action<CdrSwaggerOptions> configureRegisterSwaggerOptions, bool isVersioned = true)
        {
            var options = new CdrSwaggerOptions();
            configureRegisterSwaggerOptions(options);

            services.Configure(configureRegisterSwaggerOptions);

            if (isVersioned)
            {
                services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();

                //Required for our Swagger setup to work when endpoints have been versioned
                services.AddVersionedApiExplorer(opt =>
                {
                    opt.GroupNameFormat = options.VersionedApiGroupNameFormat;
                });

            }
            else
            {
                services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureUnversionedSwaggerOptions>();
            }

            services.AddSwaggerGen(c =>
            {
                // swagger comments from project xml documentation files
                var xmlFiles = Directory.GetFiles(AppContext.BaseDirectory, "*.xml", SearchOption.TopDirectoryOnly).ToList();
                xmlFiles.ForEach(fileName => c.IncludeXmlComments(fileName));
                c.EnableAnnotations(); // https://github.com/domaindrivendev/Swashbuckle.AspNetCore/blob/master/README.md#enrich-parameter-metadata

                c.DocumentFilter<CustomDocumentFilter>();
                c.ParameterFilter<CustomParameterFilter>();
                c.SchemaFilter<PropertyAlphabeticalOrderFilter>();
                c.OperationFilter<SetupApiVersionParamsOperationFilter>();
                c.OperationFilter<AuthorizationOperationFilter>();

                if (options.IncludeAuthentication)
                {
                    c.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme
                    {
                        Description = "JWT Authorization header using the Bearer scheme. Please enter into field the word 'Bearer' following by space and JWT.",
                        Name = "Authorization",
                        In = ParameterLocation.Header,
                        Scheme = JwtBearerDefaults.AuthenticationScheme,
                        Type = SecuritySchemeType.ApiKey,
                        BearerFormat = "JWT",
                    });

                    c.AddSecurityRequirement(new OpenApiSecurityRequirement
                    {
                        {
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = JwtBearerDefaults.AuthenticationScheme },
                            },
                            new List<string>()
                        },
                    });
                }
            });

            services.AddSwaggerGenNewtonsoftSupport();

            return services;
        }
    }
}
