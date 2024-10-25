using Infrastructure.DatabaseContexts;
using Infrastructure.Services;

using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection RegisterInfrastructureServices(this IServiceCollection services)
    {
        services.AddDbContext<SqliteContext>();
        services.AddScoped<ITodoService, TodoService>();
        return services;
    }
}