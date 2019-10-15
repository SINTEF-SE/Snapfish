using System;
using AspNet.Security.OpenIdConnect.Primitives;
using CorrelationId;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SintefSecure.Framework.SintefSecure.AspNetCore;
using SintefSecure.Framework.SintefSecure.AspNetCore.OpenIdDict;
using SintefSecureBoilerplate.DAL.Identity;
using Snapfish.API.Database;
using Snapfish.API.Constants;

namespace Snapfish.API
{
    /// <summary>
    /// The main start-up class for the application.
    /// </summary>
    public class Startup : IStartup
    {
        private readonly IConfiguration configuration;
        private readonly IHostingEnvironment hostingEnvironment;

        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        /// <param name="configuration">The application configuration, where key value pair settings are stored. See
        /// http://docs.asp.net/en/latest/fundamentals/configuration.html</param>
        /// <param name="hostingEnvironment">The environment the application is running under. This can be Development,
        /// Staging or Production by default. See http://docs.asp.net/en/latest/fundamentals/environments.html</param>
        public Startup(IConfiguration configuration, IHostingEnvironment hostingEnvironment)
        {
            this.configuration = configuration;
            this.hostingEnvironment = hostingEnvironment;
        }

        /// <summary>
        /// Configures the services to add to the ASP.NET Core Injection of Control (IoC) container. This method gets
        /// called by the ASP.NET runtime. See
        /// http://blogs.msdn.com/b/webdev/archive/2014/06/17/dependency-injection-in-asp-net-vnext.aspx
        /// </summary>
        public IServiceProvider ConfigureServices(IServiceCollection services) =>
            services
                .AddCorrelationIdFluent()
                .AddCustomCaching()
                .AddCustomOptions(this.configuration)
                .AddCustomRouting()
                .AddResponseCaching()
                .AddCustomResponseCompression()
                .AddCustomStrictTransportSecurity()
                .AddCustomSwagger() 
                .AddDbContext<SnapContext>(options =>
                {
                    options.UseSqlServer(configuration.GetConnectionString("FiskinfoSnapfishConnection")); 
                })
                .AddDbContext<IdentityDatabaseContext>(options =>
                {
                    // Configure the context to use Microsoft SQL Server.
                    options.UseSqlServer(configuration.GetConnectionString("IdentityConnection"));
                    // Register the entity sets needed by OpenIddict.
                    // Note: use the generic overload if you need
                    // to replace the default OpenIddict entities.
                    options.UseOpenIddict();
                })
                .AddIdentityDataStores()
                //Configure Identity to use the same JWT claims as OpenIddict instead
                // of the legacy WS-Federation claims it uses by default (ClaimTypes),
                // which saves you from doing the mapping in your authorization controller.
                .Configure<IdentityOptions>(options =>
                {
                    options.ClaimsIdentity.UserNameClaimType = OpenIdConnectConstants.Claims.Name;
                    options.ClaimsIdentity.UserIdClaimType = OpenIdConnectConstants.Claims.Subject;
                    options.ClaimsIdentity.RoleClaimType = OpenIdConnectConstants.Claims.Role;
                })
                .AddOpenIdDictServices(authenticationScheme: OpenIdDictAuthenticationScheme.Password)
                .AddHttpContextAccessor()
                // Add useful interface for accessing the ActionContext outside a controller.
                .AddSingleton<IActionContextAccessor, ActionContextAccessor>()
                // Add useful interface for accessing the IUrlHelper outside a controller.
                .AddScoped(x => x
                    .GetRequiredService<IUrlHelperFactory>()
                    .GetUrlHelper(x.GetRequiredService<IActionContextAccessor>().ActionContext))
                .AddCustomApiVersioning()
                .AddMvcCore()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_1)
                .AddApiExplorer()
                .AddAuthorization()
                .AddDataAnnotations()
                .AddJsonFormatters()
                .AddCustomJsonOptions(this.hostingEnvironment)
                .AddCustomCors()
                .AddVersionedApiExplorer(x => x.GroupNameFormat =
                    "'v'VVV") // Version format: 'v'major[.minor][-status]
                .AddCustomMvcOptions(this.hostingEnvironment)
                .Services
                .AddProjectCommands()
                .AddProjectMappers()
                .AddProjectRepositories()
                .AddProjectServices()
                .BuildServiceProvider();

        /// <summary>
        /// Configures the application and HTTP request pipeline. Configure is called after ConfigureServices is
        /// called by the ASP.NET runtime.
        /// </summary>
        public void Configure(IApplicationBuilder application) =>
            application
                // Pass a GUID in a X-Correlation-ID HTTP header to set the HttpContext.TraceIdentifier.
                .UseCorrelationId()
                .UseAuthentication()
                .InitializeIdentityDatabase(hostingEnvironment,
                    application.ApplicationServices.GetRequiredService<IdentityDatabaseContext>())
                .UseForwardedHeaders()
                .UseResponseCaching()
                .UseResponseCompression()
                .UseCors(CorsPolicyName.AllowAny)
                .UseIf(
                    !this.hostingEnvironment.IsDevelopment(),
                    x => x.UseHsts())
                .UseIf(
                    this.hostingEnvironment.IsDevelopment(),
                    x => x.UseDeveloperErrorPages())
                .UseStaticFilesWithCacheControl()
                .UseMvc();
                //.UseSwagger()
                //.UseCustomSwaggerUI(); TODO: Peter fix swagger
    }
}