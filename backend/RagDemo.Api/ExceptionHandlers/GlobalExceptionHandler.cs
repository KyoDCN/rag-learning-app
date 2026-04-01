using Microsoft.AspNetCore.Diagnostics;

namespace RagDemo.Api.Middlewares;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception ex, CancellationToken ct)
    {
        logger.LogError(ex, "Unhandled exception on {Method} {Path}", context.Request.Method, context.Request.Path);

        context.Response.StatusCode = ex switch
        {
            ArgumentNullException or ArgumentException or FormatException => StatusCodes.Status400BadRequest,
            KeyNotFoundException     => StatusCodes.Status404NotFound,
            UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
            OperationCanceledException  => StatusCodes.Status408RequestTimeout,
            HttpRequestException     => StatusCodes.Status502BadGateway,
            _                        => StatusCodes.Status500InternalServerError
        };

        await context.Response.WriteAsJsonAsync(new { error = ex.Message }, ct);

        return true;
    }
}