using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace SmsBranaClient;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSmsBrana(this IServiceCollection services, Action<SmsBranaClientOptions> configureOptions)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureOptions);

        services.Configure(configureOptions);

        RegisterServices(services);
        
        return services;
    }

    public static IServiceCollection AddSmsBrana(this IServiceCollection services, Microsoft.Extensions.Configuration.IConfiguration configuration, string sectionName = "SmsBrana")
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.Configure<SmsBranaClientOptions>(configuration.GetSection(sectionName));

        RegisterServices(services);
        
        return services;
    }
    
    private static void RegisterServices(IServiceCollection services)
    {
        services.TryAddSingleton(TimeProvider.System);
        services.AddHttpClient<ISmsBranaClient, SmsBranaClient>();
    }
}
