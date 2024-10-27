// Ignore Spelling: Middleware

using Microsoft.AspNetCore.Mvc;


using System.Text.Json;
using System.Text.Json.Serialization;

namespace API.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await _next(httpContext);
        }
        catch (Exception ex)
        {

            await HandleExceptionAsync(httpContext, ex);
        }

    }


    private async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        //_logger.LogError(ex, "An error occurred.");

        var problemDetails = new ProblemDetails
        {
            Title = "A problem occurred while processing your request, please try again later.",
            Detail = ex.Message,
            Status = StatusCodes.Status500InternalServerError,
            Type = "https://example.com/errors/internal-server-error", // Your custom error type URL
        };

        // Optionally, you can add custom headers to the response, such as 'WWW-Authenticate'
        //context.Response.Headers.Add("WWW-Authenticate", new StringValues("Bearer realm=\"example\""));
        problemDetails.Extensions.Add("Canaan's Property", "He is learning");
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/problem+json";

        await context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        }));

    }
}
