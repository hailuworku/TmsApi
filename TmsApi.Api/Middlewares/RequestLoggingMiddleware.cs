using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
namespace TmsApi.Api.Middlewares;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();

        var correlationId = Guid.NewGuid().ToString("N")[..8];

        context.Response.Headers["X-Correlation-Id"] = correlationId;

        _logger.LogInformation("Request {Method} {Path} starting with Correlation ID: {CorrelationId}",
            context.Request.Method, context.Request.Path, correlationId);

        await _next(context);

        stopwatch.Stop();

        _logger.LogInformation("Request {Method} {Path} completed with Status: {StatusCode} in {Elapsed}ms (Correlation ID: {CorrelationId})",
            context.Request.Method, context.Request.Path, context.Response.StatusCode, stopwatch.ElapsedMilliseconds, correlationId);
    }
}