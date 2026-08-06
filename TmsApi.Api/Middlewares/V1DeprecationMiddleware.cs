namespace TmsApi.Api.Middleware;

public class V1DeprecationMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        //
        context.Response.OnStarting(() =>
        {
            // 
            if (context.Request.Path.StartsWithSegments("/api/v1"))
            {
                context.Response.Headers["Deprecation"] = "true";
                context.Response.Headers["Sunset"] = "Thu, 31 Dec 2026 00:00:00 GMT";
                context.Response.Headers["Link"] = $"<{context.Request.Scheme}://{context.Request.Host}/api/v2{context.Request.Path.Value?[7..]}>; rel=\"successor-version\"";
            }
            return Task.CompletedTask;
        });

        await next(context);
    }
}