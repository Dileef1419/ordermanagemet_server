using Microsoft.AspNetCore.Http;
using System.Diagnostics;

namespace SharedKernel.Middleware;

public class CorrelationIdMiddleware
{
    private const string HeaderName = "X-Correlation-Id";
    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Headers.TryGetValue(HeaderName, out var correlationId)
            || string.IsNullOrWhiteSpace(correlationId))
        {
            correlationId = Guid.NewGuid().ToString();
        }

        Activity.Current?.SetBaggage("correlation-id", correlationId!);
        context.Items["CorrelationId"] = correlationId.ToString();

        context.Response.OnStarting(() =>
        {
            context.Response.Headers.Append(HeaderName, correlationId);
            return Task.CompletedTask;
        });

        await _next(context);
    }
}
