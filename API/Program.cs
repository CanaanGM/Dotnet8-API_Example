
using API.Common.Errors;
using API.Middleware;

using Asp.Versioning;

using Core;

using Infrastructure;
using Infrastructure.DatabaseContexts;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Infrastructure;
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
        builder.Services.AddSwaggerGen(settings =>
        {
            settings.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "API_EXAMPLE.xml"));
        });


        // adding other projects to the IoC container
        builder.Services
            .RegisterInfrastructureServices()
            .RegisterCoreServices(builder.Configuration)
            ;

        builder.Services.AddSingleton<ProblemDetailsFactory, ApplicationProblemDetailsFactory>();

        // API version
        builder.Services.AddApiVersioning(settings =>
        {
            settings.ApiVersionReader = new UrlSegmentApiVersionReader();
            settings.DefaultApiVersion = new ApiVersion(1, 0);
            settings.AssumeDefaultVersionWhenUnspecified = true;
        })
            .AddApiExplorer(settings =>
            {
                settings.GroupNameFormat = "'v'VVV";
                settings.SubstituteApiVersionInUrl = true;
            })
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



        app.UseAuthentication();
        app.UseAuthorization();
        app.UseMiddleware<ExceptionMiddleware>();


        app.MapControllers();


        app.Run();
    }
}
