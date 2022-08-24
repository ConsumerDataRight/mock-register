using CDR.Register.API.Infrastructure.Authorization;
using CDR.Register.API.Infrastructure.Models;
using CDR.Register.Repository.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Authentication;

namespace CDR.Register.API.Infrastructure
{
    public static class Extensions
    {
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
            var identityServerUrl = configuration.GetValue<string>("IdentityServerUrl");
            var identityServerIssuer = configuration.GetValue<string>("IdentityServerIssuer");

            services.AddHttpContextAccessor();

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer("Bearer", options =>
            {
                options.Authority = identityServerUrl;
                options.RequireHttpsMetadata = true;
                options.Audience = "cdr-register";

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
                    policy.Requirements.Add(new ScopeRequirement(CdsRegistrationScopes.BankRead, identityServerIssuer));
                    policy.Requirements.Add(new MtlsRequirement());
                });

                options.AddPolicy(AuthorisationPolicy.GetSSA.ToString(), policy =>
                {
                    policy.Requirements.Add(new ScopeRequirement(CdsRegistrationScopes.BankRead, identityServerIssuer));
                    policy.Requirements.Add(new MtlsRequirement());
                });
                options.AddPolicy(AuthorisationPolicy.DataHolderBrandsApiMultiIndustry.ToString(), policy =>
                {
                    policy.Requirements.Add(new ScopeRequirement(CdsRegistrationScopes.Read, identityServerIssuer));
                    policy.Requirements.Add(new MtlsRequirement());
                });
                options.AddPolicy(AuthorisationPolicy.GetSSAMultiIndustry.ToString(), policy =>
                {
                    policy.Requirements.Add(new ScopeRequirement(CdsRegistrationScopes.Read, identityServerIssuer));
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
            else
            {
                var currentHostName = currentUrl.GetHostName();
                links.Self = new Uri(currentUrl.Replace(currentHostName, hostName));
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
            else
            {
                var currentHostName = currentUrl.GetHostName();
                links.Self = new Uri(currentUrl.Replace(currentHostName, hostName));
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

        public static string GetClientCertificateThumbprint(this HttpContext context)
        {
            if (context.Request.Headers.TryGetValue(Constants.Headers.X_TLS_CLIENT_CERT_THUMBPRINT, out StringValues headerThumbprints))
            {
                return headerThumbprints.First();
            }

            return "";
        }

        public static string GetClientCertificateCommonName(this HttpContext context)
        {
            if (context.Request.Headers.TryGetValue(Constants.Headers.X_TLS_CLIENT_CERT_COMMON_NAME, out StringValues headerCommonNames))
            {
                return headerCommonNames.First();
            }

            return "";
        }
    }
}
