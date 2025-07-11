﻿using CDR.Register.API.Infrastructure;
using CDR.Register.API.Infrastructure.Filters;
using CDR.Register.API.Infrastructure.Middleware;
using CDR.Register.API.Logger;
using CDR.Register.Domain.Repositories;
using CDR.Register.Infosec.Extensions;
using CDR.Register.Infosec.Interfaces;
using CDR.Register.Infosec.Services;
using CDR.Register.Repository;
using CDR.Register.Repository.Infrastructure;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Converters;
using Serilog;

namespace CDR.Register.Infosec
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            this.Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpContextAccessor();

            services.AddRegisterInfosec(this.Configuration);

            services.AddControllers()
                .ConfigureApiBehaviorOptions(options =>
                {
                    options.InvalidModelStateResponseFactory = ModelStateErrorMiddleware.ExecuteResult;
                })
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
                });

            if (this.Configuration.GetSection("SerilogRequestResponseLogger") != null)
            {
                Log.Logger.Information("Adding request response logging middleware");
                services.AddRequestResponseLogging();
            }

            // if the distributed cache connection string has been set then use it, otherwise fall back to in-memory caching.
            if (this.UseDistributedCache())
            {
                services.AddStackExchangeRedisCache(options =>
                {
                    options.Configuration = this.Configuration.GetConnectionString(Constants.ConnectionStringNames.Cache);
                    options.InstanceName = "register-cache-";
                });

                services.AddDataProtection()
                    .SetApplicationName("reg-infosec")
                    .PersistKeysToStackExchangeRedis(
                        StackExchange.Redis.ConnectionMultiplexer.Connect(this.Configuration.GetConnectionString(Constants.ConnectionStringNames.Cache) ?? string.Empty),
                        "register-cache-dp-keys");
            }
            else
            {
                // Use in memory cache.
                services.AddDistributedMemoryCache();
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseExceptionHandler(exceptionHandlerApp =>
            {
                exceptionHandlerApp.Run(async context => await ApiExceptionHandler.Handle(context));
            });

            app.UseBasePathOrExpression(this.Configuration);

            app.UseSerilogRequestLogging();
            app.UseMiddleware<RequestResponseLoggingMiddleware>();

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        private bool UseDistributedCache()
        {
            var cacheConnectionString = this.Configuration.GetConnectionString(Constants.ConnectionStringNames.Cache);
            return !string.IsNullOrEmpty(cacheConnectionString);
        }
    }
}
