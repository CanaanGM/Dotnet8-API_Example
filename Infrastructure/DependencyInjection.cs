using Core.Identity;

using Infrastructure.DatabaseContexts;
using Infrastructure.Services;

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection RegisterInfrastructureServices(this IServiceCollection services)
    {
        services.AddDbContext<SqliteContext>();
        services.AddScoped<ITodoService, TodoService>();

        services.AddIdentityCore<ApplicationUser>(options =>
        {
            options.User.RequireUniqueEmail = true;
        })
          .AddRoles<ApplicationRole>()
          .AddEntityFrameworkStores<SqliteContext>()
          .AddUserStore<UserStore<ApplicationUser, ApplicationRole, SqliteContext, Guid>>()
          .AddRoleStore<RoleStore<ApplicationRole, SqliteContext, Guid>>()
          ;

        return services;
    }
}