﻿using System;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using SintefSecure.Framework.SintefSecure.AspNetCore;
using SintefSecureBoilerplate.DAL.Identity;
using Snapfish.API.Constants;
using Snapfish.API.Options;

namespace Snapfish.API
{
    public static partial class ApplicationBuilderExtensions
    {
        /// <summary>
        /// Adds developer friendly error pages for the application which contain extra debug and exception information.
        /// Note: It is unsafe to use this in production.
        /// </summary>
        public static IApplicationBuilder UseDeveloperErrorPages(this IApplicationBuilder application) =>
            application
                // When a database error occurs, displays a detailed error page with full diagnostic information. It is
                // unsafe to use this in production. Uncomment this if using a database.
                // .UseDatabaseErrorPage(DatabaseErrorPageOptions.ShowAll);
                // When an error occurs, displays a detailed error page with full diagnostic information.
                // See http://docs.asp.net/en/latest/fundamentals/diagnostics.html
                .UseDeveloperExceptionPage();

        /// <summary>
        /// Uses the static files middleware to serve static files. Also adds the Cache-Control and Pragma HTTP
        /// headers. The cache duration is controlled from configuration.
        /// See http://andrewlock.net/adding-cache-control-headers-to-static-files-in-asp-net-core/.
        /// </summary>
        public static IApplicationBuilder UseStaticFilesWithCacheControl(this IApplicationBuilder application)
        {
            var cacheProfile = application
                                   .ApplicationServices
                                   .GetRequiredService<CacheProfileOptions>()
                                   .Where(x => string.Equals(x.Key, CacheProfileName.StaticFiles,
                                       StringComparison.Ordinal))
                                   .Select(x => x.Value)
                                   .SingleOrDefault() ??
                               throw new InvalidOperationException(
                                   "CacheProfiles.StaticFiles section is missing in appsettings.json");
            return application
                .UseStaticFiles(
                    new StaticFileOptions()
                    {
                        OnPrepareResponse = context => { context.Context.ApplyCacheProfile(cacheProfile); },
                    });
        }

        /// <summary>
        /// Initializes the Identity database if it is not already initialized. If initialized, it binds the database to the application through dependency injection
        /// </summary>
        public static IApplicationBuilder InitializeIdentityDatabase(this IApplicationBuilder application, IHostingEnvironment hostingEnvironment,
            IdentityDatabaseContext identityContext)
        {
            if (hostingEnvironment.IsDevelopment())
            {
                IdentityDatabaseIntializer.InitializeDevelopmentEnvironment(identityContext);
            }
            else
            {
                IdentityDatabaseIntializer.InitializeProductionEnvironment(identityContext);
            }

            return application;
        }

        public static IApplicationBuilder UseCustomSwaggerUI(this IApplicationBuilder application) =>
            application.UseSwaggerUI(
                options =>
                {
                    // Set the Swagger UI browser document title.
                    options.DocumentTitle = typeof(Startup)
                        .Assembly
                        .GetCustomAttribute<AssemblyProductAttribute>()
                        .Product;
                    // Set the Swagger UI to render at '/'.
                    options.RoutePrefix = string.Empty;
                    // Show the request duration in Swagger UI.
                    options.DisplayRequestDuration();

                    var provider = application.ApplicationServices.GetService<IApiVersionDescriptionProvider>();
                    foreach (var apiVersionDescription in provider
                        .ApiVersionDescriptions
                        .OrderByDescending(x => x.ApiVersion))
                    {
                        options.SwaggerEndpoint(
                            $"/swagger/{apiVersionDescription.GroupName}/swagger.json",
                            $"Version {apiVersionDescription.ApiVersion}");
                    }
                });
    }
}