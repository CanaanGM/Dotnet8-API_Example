using Infrastructure.DatabaseContexts;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection RegisterInfrastructureServices(this IServiceCollection services)
    {
        services.AddDbContext<SqliteContext>();
        return services;
    }
}