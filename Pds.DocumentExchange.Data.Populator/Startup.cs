using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Pds.Core.ApiAuthentication;
using Pds.Core.BulkJobs;
using Pds.Core.BulkJobs.Redis;
using Pds.Core.Logging;
using Pds.DocumentExchange.Data.Populator.MvcConfiguration;
using System;
using System.Collections.Generic;

namespace Pds.DocumentExchange.Data.Populator
{
    public class Startup
    {
        private const string CurrentApiVersion = "v1.0.0";

        private static readonly List<string> _allowedEnvironments = new List<string>
        {
            "localdevelopment",
            "dev",
            "test",
            "at",
            "demo",
            "oat"
        };

        /// <summary>
        /// Gets the application configuration.
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// Gets the environment.
        /// </summary>
        public IWebHostEnvironment Environment { get; }

        /// <summary>
        /// The assembly name (field).
        /// </summary>
        private string _assemblyName;

        /// <summary>
        /// Gets the assembly name.
        /// </summary>
        public string AssemblyName =>
            _assemblyName ??= GetType().Assembly.GetName().Name;

        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        /// <param name="configuration">The application configuration.</param>
        /// <param name="environment">The hosting environment the application is running in.</param>
        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            Configuration = configuration;
            Environment = environment;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            if (!IsAllowedEnvironment())
            {
                return;
            }

            services
                .AddApiControllers()
                .AddAzureADAuthentication(Configuration)
                .AddBulkJobManager(
                    s =>
                        s.AddRedisBulkJobStorage(
                            options => options.RedisConnectionString = Configuration.GetValue<string>("Cache:Redis:ConnectionString")))
                .AddLoggerAdapter();

            if (Environment.IsDevelopment())
            {
                services
                    .DisableAuthentication(AssemblyName)
                    .AddSwaggerGen(options =>
                    {
                        options.SwaggerDoc(CurrentApiVersion, new OpenApiInfo { Title = AssemblyName, Version = CurrentApiVersion });
                    });
            }
            else
            {
                services
                    .AddSwaggerGen(options =>
                    {
                        options.SwaggerDoc(CurrentApiVersion, new OpenApiInfo { Title = AssemblyName, Version = CurrentApiVersion });
                        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                        {
                            Description = "JWT Authorization header using the Bearer scheme. Example: \"bearer {token}\"",
                            Name = "Authorization",
                            In = ParameterLocation.Header,
                            Type = SecuritySchemeType.ApiKey,
                            Scheme = "Bearer"
                        });
                        options.AddSecurityRequirement(new OpenApiSecurityRequirement
                        {
                            {
                                new OpenApiSecurityScheme
                                {
                                    Reference = new OpenApiReference
                                    {
                                        Type = ReferenceType.SecurityScheme,
                                        Id = "Bearer"
                                    }
                                },
                                Array.Empty<string>()
                            }
                        });
                    });
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (!IsAllowedEnvironment())
            {
                return;
            }

            app.UseDeveloperExceptionPage();

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint($"/swagger/{CurrentApiVersion}/swagger.json", AssemblyName);
            });

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        private bool IsAllowedEnvironment()
        {
            var environment = Configuration.GetValue<string>("PdsApplicationInsights:Environment").ToLowerInvariant();

            return _allowedEnvironments.Contains(environment);
        }
    }
}