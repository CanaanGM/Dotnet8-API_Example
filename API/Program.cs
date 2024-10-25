
using Core;

using Infrastructure;
using Infrastructure.DatabaseContexts;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;

namespace API;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);


        builder.Services.AddControllers(options =>
        {
            //* General policy so that ALL controllers need auth
            var policy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();
            options.Filters.Add(new AuthorizeFilter(policy));
        });
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();


        // adding other projects to the IoC container
        builder.Services
            .RegisterInfrastructureServices()
            .RegisterCoreServices(builder.Configuration)
            ;

        var app = builder.Build();


        //migrations 
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<SqliteContext>();
        context.Database.Migrate();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.Use(async (context, next) =>
        {
            Console.WriteLine("Incoming Request Headers:");
            foreach (var header in context.Request.Headers)
            {
                Console.WriteLine($"{header.Key}: {header.Value}");
            }
            await next.Invoke();
        });


        app.UseAuthentication();
        app.UseAuthorization();


        app.MapControllers();

        app.Run();
    }
}
