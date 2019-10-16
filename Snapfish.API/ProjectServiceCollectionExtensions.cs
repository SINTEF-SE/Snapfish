using Microsoft.Extensions.DependencyInjection;
using SintefSecure.Framework.SintefSecure.Mapping;
using Snapfish.API.Services;
using Snapfish.API.ViewModels;
using Snapfish.API.Commands;

namespace Snapfish.API
{
    /// <summary>
    /// <see cref="IServiceCollection"/> extension methods add project services.
    /// </summary>
    /// <remarks>
    /// AddSingleton - Only one instance is ever created and returned.
    /// AddScoped - A new instance is created and returned for each request/response cycle.
    /// AddTransient - A new instance is created and returned each time.
    /// </remarks>
    public static class ProjectServiceCollectionExtensions
    {
        public static IServiceCollection AddProjectCommands(this IServiceCollection services) =>
            services 
                .AddSingleton<IGetSnapMetadatasCommand, GetSnapMetadatasCommand>();

        public static IServiceCollection AddProjectMappers(this IServiceCollection services) =>
            services;

        public static IServiceCollection AddProjectRepositories(this IServiceCollection services) =>
            services;
        public static IServiceCollection AddProjectServices(this IServiceCollection services) =>
            services
                .AddSingleton<IClockService, ClockService>();
    }
}