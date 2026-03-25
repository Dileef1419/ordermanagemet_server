using Microsoft.AspNetCore.RateLimiting;
using SharedKernel.Middleware;

var builder = WebApplication.CreateBuilder(args);

// ----- Swagger / OpenAPI -----
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "API Gateway", Version = "v1", Description = "YARP reverse proxy + aggregation endpoints" });
});

// ----- YARP Reverse Proxy -----
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// ----- Rate Limiting -----
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddFixedWindowLimiter("standard", config =>
    {
        config.PermitLimit = 300;
        config.Window = TimeSpan.FromMinutes(1);
        config.QueueLimit = 10;
    });
    options.AddFixedWindowLimiter("admin", config =>
    {
        config.PermitLimit = 1000;
        config.Window = TimeSpan.FromMinutes(1);
        config.QueueLimit = 50;
    });
});

builder.Services.AddHealthChecks();
builder.Services.AddHttpClient();

var app = builder.Build();

// ----- Swagger (Development only) -----
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "API Gateway v1");
        c.RoutePrefix = string.Empty;
    });
}

// ----- Middleware Pipeline -----
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseRateLimiter();

app.MapHealthChecks("/health");

// ===================== AGGREGATION ENDPOINTS =====================

// Full order view: fan-out to Orders + Payments + Fulfilment
app.MapGet("/api/v1/orders/{orderId:guid}/full", async (
    Guid orderId,
    IHttpClientFactory httpClientFactory,
    CancellationToken ct) =>
{
    var client = httpClientFactory.CreateClient();
    using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
    cts.CancelAfter(TimeSpan.FromSeconds(3));

    var orderTask = client.GetAsync($"http://localhost:5030/api/v1/orders/{orderId}", cts.Token);
    var paymentTask = client.GetAsync($"http://localhost:5040/api/v1/payments/by-order/{orderId}", cts.Token);
    var shipmentTask = client.GetAsync($"http://localhost:5050/api/v1/fulfilment/by-order/{orderId}", cts.Token);

    try
    {
        await Task.WhenAll(orderTask, paymentTask, shipmentTask);
    }
    catch (TaskCanceledException)
    {
        return Results.StatusCode(504); // Gateway Timeout
    }

    var orderJson = orderTask.Result.IsSuccessStatusCode
        ? await orderTask.Result.Content.ReadAsStringAsync(ct) : null;
    var paymentJson = paymentTask.Result.IsSuccessStatusCode
        ? await paymentTask.Result.Content.ReadAsStringAsync(ct) : null;
    var shipmentJson = shipmentTask.Result.IsSuccessStatusCode
        ? await shipmentTask.Result.Content.ReadAsStringAsync(ct) : null;

    if (orderJson is null)
        return Results.NotFound("Order not found.");

    return Results.Ok(new
    {
        order = System.Text.Json.JsonSerializer.Deserialize<object>(orderJson),
        payment = paymentJson is not null ? System.Text.Json.JsonSerializer.Deserialize<object>(paymentJson) : null,
        shipment = shipmentJson is not null ? System.Text.Json.JsonSerializer.Deserialize<object>(shipmentJson) : null
    });
})
.RequireRateLimiting("standard");

// ----- YARP Proxy -----
app.MapReverseProxy();

app.Run();
