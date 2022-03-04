using CDR.Register.API.Infrastructure.Authorization;
using CDR.Register.API.Infrastructure.Models;
using CDR.Register.Repository.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
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
                    policy.Requirements.Add(new ScopeRequirement(CDSRegistrationScopes.BankRead, identityServerIssuer));
                    policy.Requirements.Add(new MTLSRequirement());
                });

                options.AddPolicy(AuthorisationPolicy.GetSSA.ToString(), policy =>
                {
                    policy.Requirements.Add(new ScopeRequirement(CDSRegistrationScopes.BankRead, identityServerIssuer));
                    policy.Requirements.Add(new MTLSRequirement());
                });
                options.AddPolicy(AuthorisationPolicy.DataHolderBrandsApiMultiIndustry.ToString(), policy =>
                {
                    policy.Requirements.Add(new ScopeRequirement(CDSRegistrationScopes.BankRead + " " + CDSRegistrationScopes.Read, identityServerIssuer));
                    policy.Requirements.Add(new MTLSRequirement());
                });
                options.AddPolicy(AuthorisationPolicy.GetSSAMultiIndustry.ToString(), policy =>
                {
                    policy.Requirements.Add(new ScopeRequirement(CDSRegistrationScopes.BankRead + " " + CDSRegistrationScopes.Read, identityServerIssuer));
                    policy.Requirements.Add(new MTLSRequirement());
                });
            });
            services.AddSingleton<IAuthorizationHandler, ScopeHandler>();
            services.AddSingleton<IAuthorizationHandler, DataRecipientSoftwareProductIdHandler>();
            services.AddSingleton<IAuthorizationHandler, MTLSHandler>();

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

        public static LinksPaginated GetPaginated(this ControllerBase controller, string routeName, DateTime? updatedSince, int? currentPage, int totalPages, int? pageSize)
        {
            var links = new LinksPaginated();

            string forwardedHost = null;
            if (controller.Request.Headers.TryGetValue("X-Forwarded-Host", out StringValues forwardedHosts))
            {
                forwardedHost = forwardedHosts.First();
            }

            links.Self = ReplaceUriHost(controller.Request.GetDisplayUrl(), forwardedHost);

            if (totalPages > 0)
            {
                links.First = controller.GetPageUri(routeName, updatedSince, 1, pageSize, forwardedHost);
                links.Last = controller.GetPageUri(routeName, updatedSince, totalPages, pageSize, forwardedHost);
                if (currentPage <= 1)
                {
                    links.Prev = null;
                }
                else
                {
                    links.Prev = controller.GetPageUri(routeName, updatedSince, currentPage - 1, pageSize, forwardedHost);
                }
                if (currentPage >= totalPages)
                {
                    links.Next = null;
                }
                else
                {
                    links.Next = controller.GetPageUri(routeName, updatedSince, currentPage + 1, pageSize, forwardedHost);
                }
            }

            return links;
        }

        public static Uri GetPageUri(this ControllerBase controller, string routeName, DateTime? updatedSince, int? currentPage, int? pageSize, string forwardedHost)
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
            else
            {
                url = url.Replace("updated_since", "updated-since");
                url = url.Replace("page_size", "page-size");

                return ReplaceUriHost(url, forwardedHost);
            }
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

        public static IndustryEnum ToIndustry(this string industry)
        {
            if (Enum.IsDefined(typeof(IndustryEnum), industry.ToUpper()))
                return (IndustryEnum)Enum.Parse(typeof(IndustryEnum), industry, true);
            else
                throw new NotSupportedException($"Invalid industry: {industry}");
        }
    }
}
