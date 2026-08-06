using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace TmsApi.Api.ExceptionHandlers;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken ct)
    {
         logger.LogError(exception, "An error occurred: {Message}", exception.Message);
      var problem = new ProblemDetails
    {
        Instance = httpContext.Request.Path,
        Status = StatusCodes.Status500InternalServerError,
        Title = "An unexpected error occurred"
    };

        if (exception is ValidationException ve)
        {
            problem.Status = StatusCodes.Status400BadRequest;
            problem.Title = "Validation failed";
            problem.Extensions["errors"] = ve.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
        }

        httpContext.Response.StatusCode = problem.Status.Value;
        await httpContext.Response.WriteAsJsonAsync(problem, ct);
        return true;
    }
}