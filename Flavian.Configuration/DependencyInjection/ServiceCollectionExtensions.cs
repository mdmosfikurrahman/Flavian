using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Flavian.Application.Dto.Common;

namespace Flavian.Configuration.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static void RegisterApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        const string rootNamespace = "Flavian.";

        // Register Application Services
        ConventionRegistrar.RegisterByNamespace(
            services,
            interfaceNamespace: rootNamespace + "Application.Services.Interfaces",
            implementationNamespace: rootNamespace + "Application.Services.Implementations"
        );

        // Register Persistence Services
        ConventionRegistrar.RegisterByNamespace(
            services,
            interfaceNamespace: rootNamespace + "Persistence.Services.Interfaces",
            implementationNamespace: rootNamespace + "Persistence.Services.Implementations"
        );

        // Register Infrastructure Services
        ConventionRegistrar.RegisterByNamespace(
            services,
            interfaceNamespace: rootNamespace + "Infrastructure.Services.Interfaces",
            implementationNamespace: rootNamespace + "Infrastructure.Services.Implementations"
        );

        // Register Repositories
        ConventionRegistrar.RegisterByNamespace(
            services,
            interfaceNamespace: rootNamespace + "Persistence.Repositories.Interfaces",
            implementationNamespace: rootNamespace + "Persistence.Repositories.Implementations"
        );

        // Register Unit of Work
        ConventionRegistrar.RegisterByNamespace(
            services,
            interfaceNamespace: rootNamespace + "Persistence.UoW.Interface",
            implementationNamespace: rootNamespace + "Persistence.UoW.Implementation"
        );

        // Automatically register FluentValidation validators
        ConventionRegistrar.RegisterFluentValidators(services,
            validatorsRootNamespace: rootNamespace + "Application.Dto.Request"
        );

        ConventionRegistrar.RegisterFluentValidatorsFromNamespace(
            services,
            validatorsNamespaceRoot: rootNamespace + "Application.Validators"
        );

        // System-wide Services
        services.AddOptions()
            .Configure<JsonSerializerOptions>(options => { options.PropertyNameCaseInsensitive = true; });

        services.Configure<ServiceInfoOptions>(options =>
        {
            options.Services =
                configuration
                    .GetSection("Service-Info")
                    .Get<Dictionary<string, ServiceInfoItem>>()
                ?? new Dictionary<string, ServiceInfoItem>();
        });

        services.Configure<AppInfoOptions>(
            configuration.GetSection("AppInfo"));

        services.AddHttpClient();
    }
}
