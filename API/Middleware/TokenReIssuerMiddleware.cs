// Ignore Spelling: Middleware

namespace API.Middleware;

public class TokenReIssuerMiddleware
{
    private readonly RequestDelegate _next;

    public TokenReIssuerMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await _next(httpContext);
        }
        catch (Exception)
        {

            throw;
        }
    }
}
