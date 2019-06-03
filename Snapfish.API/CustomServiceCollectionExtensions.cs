using System.IdentityModel.Tokens.Jwt;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using AspNet.Security.OpenIdConnect.Primitives;
using CorrelationId;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SintefSecure.Framework.SintefSecure.AspNetCore.OpenIdDict;
using SintefSecure.Framework.SintefSecure.Swagger;
using SintefSecure.Framework.SintefSecure.Swagger.OperationFilters;
using SintefSecure.Framework.SintefSecure.Swagger.SchemaFilters;
using SintefSecureBoilerplate.DAL.Identity;
using Snapfish.API.API.Constants;
using Snapfish.API.API.OperationFilters;
using Snapfish.API.API.Options;
using Swashbuckle.AspNetCore.Swagger;

namespace Snapfish.API.API
{
    /// <summary>
    /// <see cref="IServiceCollection"/> extension methods which extend ASP.NET Core services.
    /// </summary>
    public static class CustomServiceCollectionExtensions
    {
        public static IServiceCollection AddCorrelationIdFluent(this IServiceCollection services)
        {
            services.AddCorrelationId();
            return services;
        }

        /// <summary>
        /// Configures caching for the application. Registers the <see cref="IDistributedCache"/> and
        /// <see cref="IMemoryCache"/> types with the services collection or IoC container. The
        /// <see cref="IDistributedCache"/> is intended to be used in cloud hosted scenarios where there is a shared
        /// cache, which is shared between multiple instances of the application. Use the <see cref="IMemoryCache"/>
        /// otherwise.
        /// </summary>
        public static IServiceCollection AddCustomCaching(this IServiceCollection services) =>
            services
                // Adds IMemoryCache which is a simple in-memory cache.
                .AddMemoryCache()
                // Adds IDistributedCache which is a distributed cache shared between multiple servers. This adds a
                // default implementation of IDistributedCache which is not distributed. See below:
                .AddDistributedMemoryCache();
        // Uncomment the following line to use the Redis implementation of IDistributedCache. This will
        // override any previously registered IDistributedCache service.
        // Redis is a very fast cache provider and the recommended distributed cache provider.
        // .AddDistributedRedisCache(options => { ... });
        // Uncomment the following line to use the Microsoft SQL Server implementation of IDistributedCache.
        // Note that this would require setting up the session state database.
        // Redis is the preferred cache implementation but you can use SQL Server if you don't have an alternative.
        // .AddSqlServerCache(
        //     x =>
        //     {
        //         x.ConnectionString = "Server=.;Database=ASPNET5SessionState;Trusted_Connection=True;";
        //         x.SchemaName = "dbo";
        //         x.TableName = "Sessions";
        //     });

        /// <summary>
        /// Configures the settings by binding the contents of the appsettings.json file to the specified Plain Old CLR
        /// Objects (POCO) and adding <see cref="IOptions{TOptions}"/> objects to the services collection.
        /// </summary>
        public static IServiceCollection AddCustomOptions(
            this IServiceCollection services,
            IConfiguration configuration) =>
            services
                // Adds IOptions<ApplicationOptions> and ApplicationOptions to the services container.
                .Configure<ApplicationOptions>(configuration)
                .AddSingleton(x => x.GetRequiredService<IOptions<ApplicationOptions>>().Value)
// Adds IOptions<ForwardedHeadersOptions> to the services container.
                .Configure<ForwardedHeadersOptions>(configuration.GetSection(nameof(ApplicationOptions.ForwardedHeaders)))
// Adds IOptions<CompressionOptions> and CompressionOptions to the services container.
                .Configure<CompressionOptions>(configuration.GetSection(nameof(ApplicationOptions.Compression)))
                .AddSingleton(x => x.GetRequiredService<IOptions<CompressionOptions>>().Value)
                // Adds IOptions<CacheProfileOptions> and CacheProfileOptions to the services container.
                .Configure<CacheProfileOptions>(configuration.GetSection(nameof(ApplicationOptions.CacheProfiles)))
                .AddSingleton(x => x.GetRequiredService<IOptions<CacheProfileOptions>>().Value);

        /// <summary>
        /// Adds dynamic response compression to enable GZIP compression of responses. This is turned off for HTTPS
        /// requests by default to avoid the BREACH security vulnerability.
        /// </summary>
        public static IServiceCollection AddCustomResponseCompression(this IServiceCollection services) =>
            services
                .AddResponseCompression(
                    options =>
                    {
                        // Add additional MIME types (other than the built in defaults) to enable GZIP compression for.
                        var customMimeTypes = services
                                                  .BuildServiceProvider()
                                                  .GetRequiredService<CompressionOptions>()
                                                  .MimeTypes ?? Enumerable.Empty<string>();
                        options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(customMimeTypes);
                    })
                .Configure<GzipCompressionProviderOptions>(options => options.Level = CompressionLevel.Optimal);

        /// <summary>
        /// Adds the identity database context and ties the default token providers to the database table(s)
        /// </summary>
        public static IServiceCollection AddIdentityDataStores(this IServiceCollection services)
        {
            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<IdentityDatabaseContext>()
                .AddDefaultTokenProviders();
            return services;
        }


        public static IServiceCollection AddOpenIdDictServices(this IServiceCollection services,
            string tokenEndpoint = AuthorizationControllerRoute.Authorize, OpenIdDictAuthenticationScheme authenticationScheme = OpenIdDictAuthenticationScheme.Password,
            bool disableHttps = false, bool shouldUseJwt = false)
        {
            services.AddOpenIddict().AddCore(options =>
            {
                // Register the Entity Framework stores and models.
                options.UseEntityFrameworkCore()
                    .UseDbContext<IdentityDatabaseContext>();
            }).AddServer(options =>
            {
                // Register the ASP.NET Core MVC binder used by OpenIddict.
                // Note: if you don't call this method, you won't be able to
                // bind OpenIdConnectRequest or OpenIdConnectResponse parameters.
                options.UseMvc();
                // Enable the token endpoint.
                options.EnableTokenEndpoint("/" + tokenEndpoint);
                // Enable the authentication.
                if (authenticationScheme.HasFlag(OpenIdDictAuthenticationScheme.Password))
                {
                    options.AllowPasswordFlow();
                }

                if (authenticationScheme.HasFlag(OpenIdDictAuthenticationScheme.ResourceCredentialsScheme))
                {
                    options.AllowClientCredentialsFlow();
                }

                if (disableHttps)
                {
                    // During development, you can disable the HTTPS requirement.
                    options.DisableHttpsRequirement();
                }

                if (shouldUseJwt)
                {
                    options.UseJsonWebTokens();
                    options.AddEphemeralSigningKey();
                }

                // Register the OpenIddict validation handler.
                // Note: the OpenIddict validation handler is only compatible with the
                // default token format or with reference tokens and cannot be used with
                // JWT tokens. For JWT tokens, use the Microsoft JWT bearer handler.
            }).AddValidation();

            if (shouldUseJwt)
            {
                // Disable the automatic JWT -> WS-Federation claims mapping used by the JWT middleware:
                JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
                JwtSecurityTokenHandler.DefaultOutboundClaimTypeMap.Clear();

                services.AddAuthentication()
                    .AddJwtBearer(options =>
                    {
                        options.Authority = "http://localhost:58795/";
                        options.Audience = "resource_server";
                        options.RequireHttpsMetadata = false;
                        options.TokenValidationParameters = new TokenValidationParameters
                        {
                            NameClaimType = OpenIdConnectConstants.Claims.Subject,
                            RoleClaimType = OpenIdConnectConstants.Claims.Role
                        };
                    });

                // Alternatively, you can also use the introspection middleware.
                // Using it is recommended if your resource server is in a
                // different application/separated from the authorization server.
                //
                // services.AddAuthentication()
                //     .AddOAuthIntrospection(options =>
                //     {
                //         options.Authority = new Uri("http://localhost:58795/");
                //         options.Audiences.Add("resource_server");
                //         options.ClientId = "resource_server";
                //         options.ClientSecret = "875sqd4s5d748z78z7ds1ff8zz8814ff88ed8ea4z4zzd";
                //         options.RequireHttpsMetadata = false;
                //     });
            }

            return services;
        }

        /// <summary>
        /// Add custom routing settings which determines how URL's are generated.
        /// </summary>
        public static IServiceCollection AddCustomRouting(this IServiceCollection services) =>
            services.AddRouting(
                options =>
                {
                    // All generated URL's should be lower-case.
                    options.LowercaseUrls = true;
                });

        /// <summary>
        /// Adds the Strict-Transport-Security HTTP header to responses. This HTTP header is only relevant if you are
        /// using TLS. It ensures that content is loaded over HTTPS and refuses to connect in case of certificate
        /// errors and warnings.
        /// See https://developer.mozilla.org/en-US/docs/Web/Security/HTTP_strict_transport_security and
        /// http://www.troyhunt.com/2015/06/understanding-http-strict-transport.html
        /// Note: Including subdomains and a minimum maxage of 18 weeks is required for preloading.
        /// Note: You can refer to the following article to clear the HSTS cache in your browser:
        /// http://classically.me/blogs/how-clear-hsts-settings-major-browsers
        /// </summary>
        public static IServiceCollection AddCustomStrictTransportSecurity(this IServiceCollection services) =>
            services
                .AddHsts(
                    options =>
                    {
                        // Preload the HSTS HTTP header for better security. See https://hstspreload.org/
                        // options.IncludeSubDomains = true;
                        // options.MaxAge = TimeSpan.FromSeconds(31536000); // 1 Year
                        // options.Preload = true;
                    });

        public static IServiceCollection AddCustomApiVersioning(this IServiceCollection services) =>
            services.AddApiVersioning(
                options =>
                {
                    options.AssumeDefaultVersionWhenUnspecified = true;
                    options.ReportApiVersions = true;
                });

        /// <summary>
        /// Adds Swagger services and configures the Swagger services.
        /// </summary>
        public static IServiceCollection AddCustomSwagger(this IServiceCollection services) =>
            services.AddSwaggerGen(
                options =>
                {
                    var assembly = typeof(Startup).Assembly;
                    var assemblyProduct = assembly.GetCustomAttribute<AssemblyProductAttribute>().Product;
                    var assemblyDescription = assembly.GetCustomAttribute<AssemblyDescriptionAttribute>().Description;

                    options.DescribeAllEnumsAsStrings();
                    options.DescribeAllParametersInCamelCase();
                    options.DescribeStringEnumsInCamelCase();
                    options.EnableAnnotations();

                    // Add the XML comment file for this assembly, so it's contents can be displayed.
                    options.IncludeXmlCommentsIfExists(assembly);

                    options.OperationFilter<ApiVersionOperationFilter>();
                    options.OperationFilter<CorrelationIdOperationFilter>();
                    options.OperationFilter<ForbiddenResponseOperationFilter>();
                    options.OperationFilter<UnauthorizedResponseOperationFilter>();

                    // Show an example model for JsonPatchDocument<T>.
                    options.SchemaFilter<JsonPatchDocumentSchemaFilter>();
                    // Show an example model for ModelStateDictionary.
                    options.SchemaFilter<ModelStateDictionarySchemaFilter>();

                    var provider = services.BuildServiceProvider().GetRequiredService<IApiVersionDescriptionProvider>();
                    foreach (var apiVersionDescription in provider.ApiVersionDescriptions)
                    {
                        var info = new Info()
                        {
                            Title = assemblyProduct,
                            Description = apiVersionDescription.IsDeprecated ? $"{assemblyDescription} This API version has been deprecated." : assemblyDescription,
                            Version = apiVersionDescription.ApiVersion.ToString(),
                        };
                        options.SwaggerDoc(apiVersionDescription.GroupName, info);
                    }
                });
    }
}