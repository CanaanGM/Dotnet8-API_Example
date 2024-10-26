using Core.Services;
using Core.Settings;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Core;

public static class DependencyInjection
{
    public static IServiceCollection RegisterCoreServices(
        this IServiceCollection services
        , ConfigurationManager configuration)
    {
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        services.AddSingleton<IJwtService, JwtService>();
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.JwtSettingsSection));
        services.Configure<RefreshTokenSettings>(configuration.GetSection(RefreshTokenSettings.RefreshTokenSettingsSection));


        // adds in the JWT authentication
        //
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

        }).AddJwtBearer(options =>
        {

            options.TokenValidationParameters = new TokenValidationParameters()
            {
                ValidateAudience = true,
                ValidAudience = configuration["JwtSettings:Audience"],
                ValidateIssuer = true,
                ValidIssuer = configuration["JwtSettings:Issuer"],
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(
                    System.Text.Encoding.UTF8.GetBytes(configuration["JwtSettings:Secret"])
                    ),
                ValidAlgorithms = [SecurityAlgorithms.HmacSha512]
            };

            //* for debugging, this comes in handy: 


            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    try
                    {

                        var authHeader = $"{context.Request.Headers["Authorization"]}";
                        var receivedTokenFromClient = authHeader.Split(' ')[1];
                        //var validTokenForTesting = "eyJhbGciOiJIUzUxMiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI3ZTg3NTQ0MS03NDUwLTRjODctYTcwYS03NTQwZjIwNjYyOTEiLCJqdGkiOiIzYjcyODA2My05NWUwLTRmOTUtYTIwNi1iZjA3ZDAzZTE4MTEiLCJpYXQiOjE3Mjk3NTY0MTksImVtYWlsIjoiY2FuYWFuQHRlc3QuY29tIiwibmFtZWlkIjpbImNhbmFhbkB0ZXN0LmNvbSIsImNhbmFhbkB0ZXN0LmNvbSJdLCJuYmYiOjE3Mjk3NTY0MTksImV4cCI6MTcyOTgzNzQxOSwiaXNzIjoiaXQgaXMgSSIsImF1ZCI6Ik15IEF1ZGllbmNlIn0.TY0qcQZoqwc90oMM-YWyzYtqoQVB2JWRCJ5tdKI8v71iLEZ5k6Dtp24UHIbA0tbUMZPwDHrZyQQjBRwMxp7a9A";

                        //context.Token = validTokenForTesting;
                        //Console.WriteLine(receivedTokenFromClient == validTokenForTesting);
                        Console.WriteLine($"Received Token: {receivedTokenFromClient}");
                        //Console.WriteLine($"Hardcoded Token: {validTokenForTesting}");
                    }
                    catch (Exception)
                    {

                        Console.WriteLine("there was no Token to interrogate");
                    }
                    return Task.CompletedTask;
                },
                OnAuthenticationFailed = context =>
                {
                    Console.WriteLine($"Authentication failed: {context.Exception.Message}");
                    return Task.CompletedTask;
                }
            };


        });
        Console.WriteLine("what ?!");
        return services;
    }
}
