using Microsoft.AspNetCore.Diagnostics;

namespace RagDemo.Api.ExceptionHandlers;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> m_logger;
    private readonly IProblemDetailsService m_problemDetailsService;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IProblemDetailsService problemDetailsService)
    {
        m_logger = logger;
        m_problemDetailsService = problemDetailsService;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception ex, CancellationToken ct)
    {
        m_logger.LogError(ex, "Unhandled exception on {Method} {Path}", context.Request.Method, context.Request.Path);

        context.Response.StatusCode = ex switch
        {
            ArgumentNullException or ArgumentException or FormatException => StatusCodes.Status400BadRequest,
            KeyNotFoundException     => StatusCodes.Status404NotFound,
            UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
            OperationCanceledException  => StatusCodes.Status408RequestTimeout,
            HttpRequestException     => StatusCodes.Status502BadGateway,
            _                        => StatusCodes.Status500InternalServerError
        };

        await m_problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = context,
            Exception = ex,
            ProblemDetails =
            {
                Detail = ex.Message
            }
        });

        return true;
    }
}