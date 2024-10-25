using Core.Identity;

using Infrastructure.DatabaseContexts;
using Infrastructure.Services;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection RegisterInfrastructureServices(this IServiceCollection services)
    {
        services.AddDbContext<SqliteContext>();
        services.AddScoped<ITodoService, TodoService>();

        services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
        {
            options.User.RequireUniqueEmail = true;
        })
            .AddEntityFrameworkStores<SqliteContext>()
            .AddDefaultTokenProviders()
            .AddUserStore<UserStore<ApplicationUser, ApplicationRole, SqliteContext, Guid>>()
            .AddRoleStore<RoleStore<ApplicationRole, SqliteContext, Guid>>();


        return services;
    }
}