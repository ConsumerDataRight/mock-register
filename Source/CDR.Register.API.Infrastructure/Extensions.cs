using AutoMapper.Configuration;
using CDR.Register.API.Infrastructure.Authorization;
using CDR.Register.API.Infrastructure.Models;
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
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
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
        public static string GetInfosecBaseUrl(this IConfiguration configuration, HttpContext context, bool isSecure = false)
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

            // @"^\/cts\/[a-zA-Z0-9\-]{1,36}\/register\/(.*)$";
            // A dynamic base path can be set by the Mock Register:BasePathExpression app setting.
            // This allows a regular expression to be set and matched rather than a static base path.
            var basePathExpression = configuration.GetValue<string>(Constants.ConfigurationKeys.BasePathExpression);
            if (!string.IsNullOrEmpty(basePathExpression))
            {
                app.Use((context, next) =>
                {
                    var matches = Regex.Matches(context.Request.Path, basePathExpression, RegexOptions.CultureInvariant | RegexOptions.IgnoreCase, matchTimeout: TimeSpan.FromMilliseconds(500));
                    if (matches.Any())
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
            var jwks = Task.Run(() => LoadJwks($"{metadataAddress}/jwks")).Result;
            // Default 2 mins*
            var clockSkew = configuration.GetValue<int>(Constants.ConfigurationKeys.ClockSkewSeconds, 120);

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer("Bearer", options =>
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
                    IssuerSigningKeys = options.Configuration.JsonWebKeySet.Keys
                };

                // Ignore server certificate issues when retrieving OIDC configuration and JWKS.
                options.BackchannelHttpHandler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true
                };

            });
            
            // Authorization
            services.AddMvcCore().AddAuthorization(options =>
            {
                options.AddPolicy(AuthorisationPolicy.DataHolderBrandsApi.ToString(), policy =>
                {
                    policy.Requirements.Add(new ScopeRequirement(CdsRegistrationScopes.BankRead));
                    policy.Requirements.Add(new MtlsRequirement());
                });

                options.AddPolicy(AuthorisationPolicy.GetSSA.ToString(), policy =>
                {
                    policy.Requirements.Add(new ScopeRequirement(CdsRegistrationScopes.BankRead));
                    policy.Requirements.Add(new MtlsRequirement());
                });
                options.AddPolicy(AuthorisationPolicy.DataHolderBrandsApiMultiIndustry.ToString(), policy =>
                {
                    policy.Requirements.Add(new ScopeRequirement(CdsRegistrationScopes.Read));
                    policy.Requirements.Add(new MtlsRequirement());
                });
                options.AddPolicy(AuthorisationPolicy.GetSSAMultiIndustry.ToString(), policy =>
                {
                    policy.Requirements.Add(new ScopeRequirement(CdsRegistrationScopes.Read));
                    policy.Requirements.Add(new MtlsRequirement());
                });
            });
            services.AddSingleton<IAuthorizationHandler, ScopeHandler>();
            services.AddSingleton<IAuthorizationHandler, DataRecipientSoftwareProductIdHandler>();
            services.AddSingleton<IAuthorizationHandler, MtlsHandler>();

            services.AddSwaggerGen(c =>
            {
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Please enter into field the word 'Bearer' following by space and JWT",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Scheme = "Bearer",
                    Type = SecuritySchemeType.ApiKey,
                    BearerFormat = "JWT"
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                        },
                        new List<string>()
                    }
                });
            });
        }

        static async Task<Microsoft.IdentityModel.Tokens.JsonWebKeySet> LoadJwks(string jwksUri)
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (a, b, c, d) => true
            };
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
            DateTime? updatedSince,
            int? currentPage,
            int totalPages,
            int? pageSize,
            string hostName = null)
        {
            var currentUrl = controller.Request.GetDisplayUrl();
            var links = new LinksPaginated
            {
                Self = new Uri(currentUrl)
            };

            if (string.IsNullOrEmpty(hostName))
            {
                if (controller.Request.Headers.TryGetValue("X-Forwarded-Host", out StringValues forwardedHosts))
                {
                    hostName = forwardedHosts.First();
                    links.Self = ReplaceUriHost(currentUrl, hostName);
                }
            }            

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

        public static Links GetSelf(this ControllerBase controller, string hostName = null)
        {
            var currentUrl = controller.Request.GetDisplayUrl();
            var links = new Links
            {
                Self = new Uri(currentUrl)
            };

            if (string.IsNullOrEmpty(hostName))
            {
                if (controller.Request.Headers.TryGetValue("X-Forwarded-Host", out StringValues forwardedHosts))
                {
                    hostName = forwardedHosts.First();
                    links.Self = ReplaceUriHost(currentUrl, hostName);
                }
            }

            return links;
        }

        public static Uri GetPageUri(
            this ControllerBase controller,
            string routeName,
            DateTime? updatedSince,
            int? currentPage,
            int? pageSize,
            string newHostName)
        {
            string url = null;

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
            var uriBuilder = new UriBuilder(url);

            // Replace the host with the forwarded host
            if (!string.IsNullOrEmpty(newHost))
            {
                var segments = newHost.Split(':');
                uriBuilder.Host = segments[0];

                if (segments.Length > 1)
                {
                    uriBuilder.Port = int.Parse(segments[1]);
                }
            }

            return uriBuilder.Uri;
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
            string value = configuration.GetValue<string>(key);
            if (string.IsNullOrEmpty(value))
            {
                return Array.Empty<string>();
            }

            return value.Split(delimiter);
        }

        public static string GetClientCertificateThumbprint(this HttpContext context, IConfiguration configuration)
        {
            var certThumbprintNameHttpHeaderName = configuration.GetValue<string>(Constants.ConfigurationKeys.CertThumbprintNameHttpHeaderName) ?? Constants.Headers.X_TLS_CLIENT_CERT_THUMBPRINT;

            if (context.Request.Headers.TryGetValue(certThumbprintNameHttpHeaderName, out StringValues headerThumbprints))
            {
                return headerThumbprints.First();
            }

            return "";
        }

        public static string GetClientCertificateCommonName(this HttpContext context, ILogger logger, IConfiguration configuration)
        {
            string headerCommonName;
            var certCommonNameHttpHeaderName = configuration.GetValue<string>(Constants.ConfigurationKeys.CertCommonNameHttpHeaderName) ?? Constants.Headers.X_TLS_CLIENT_CERT_COMMON_NAME;

            if (context.Request.Headers.TryGetValue(certCommonNameHttpHeaderName, out StringValues headerCommonNames))
            {
                headerCommonName = headerCommonNames.First();
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

            logger.LogInformation($"Received commonName of {headerCommonName} in header and parsed commonName as {commonName}");
            return commonName;
        }

        public static bool IsDistinguishedName(this string value)
        {
            try
            {
                X500DistinguishedName dn = new(value);
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

            if (commonName.IsDistinguishedName() )
            {
                commonName = commonName.GetCommonNameFromDistinguishedName();
            }

            return commonName.Trim('"');
        }

        public static string GetCommonNameFromDistinguishedName(this string distinguishedName)
        {
            try
            {
                X500DistinguishedName dn = new X500DistinguishedName(distinguishedName);
                var cnAttribute = dn.Decode(X500DistinguishedNameFlags.UseNewLines)
                    .Split('\n')
                    .FirstOrDefault(attr => attr.Trim().StartsWith("CN="));

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
    }
}
