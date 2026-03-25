using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace SharedKernel.Filters;

/// <summary>
/// Global exception filter — maps domain exceptions to proper HTTP status codes.
/// Applied to all controllers via MVC filter pipeline.
/// </summary>
public class GlobalExceptionFilter : IExceptionFilter
{
    private readonly ILogger<GlobalExceptionFilter> _logger;

    public GlobalExceptionFilter(ILogger<GlobalExceptionFilter> logger) => _logger = logger;

    public void OnException(ExceptionContext context)
    {
        _logger.LogError(context.Exception,
            "Unhandled exception: {Message}", context.Exception.Message);

        var (statusCode, title) = context.Exception switch
        {
            ArgumentException => (HttpStatusCode.BadRequest, "Invalid Argument"),
            InvalidOperationException => (HttpStatusCode.Conflict, "Invalid Operation"),
            KeyNotFoundException => (HttpStatusCode.NotFound, "Not Found"),
            _ when context.Exception.GetType().Name.Contains("NotFound")
                => (HttpStatusCode.NotFound, "Not Found"),
            _ when context.Exception.GetType().Name.Contains("InvalidState")
                => (HttpStatusCode.Conflict, "Invalid State Transition"),
            _ => (HttpStatusCode.InternalServerError, "Internal Server Error")
        };

        context.Result = new ObjectResult(new ProblemDetails
        {
            Status = (int)statusCode,
            Title = title,
            Detail = context.Exception.Message,
            Instance = context.HttpContext.Request.Path
        })
        {
            StatusCode = (int)statusCode
        };

        context.ExceptionHandled = true;
    }
}
