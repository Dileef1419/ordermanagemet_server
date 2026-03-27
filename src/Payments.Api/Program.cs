using Payments.Application;
using Payments.Infrastructure;
using SharedKernel.Filters;
using SharedKernel.Middleware;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ── Controllers + Global Filters ──
builder.Services.AddControllers(options =>
{
    options.Filters.Add<GlobalExceptionFilter>();
});

// ── Swagger / OpenAPI ──
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "Payments API",
        Version = "v1",
        Description = "Payment processing service — Authorise, Capture, Refund"
    });
});

// ── Application Layer ──
builder.Services.AddPaymentsApplication();

// ── Infrastructure Layer ──
builder.Services.AddPaymentsInfrastructure(builder.Configuration);

// ── Health Checks ──
builder.Services.AddHealthChecks();

var app = builder.Build();

// ── Swagger UI (Development Only) ──
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Payments API v1");
        c.RoutePrefix = string.Empty;
    });
}

// ── Middleware Pipeline ──
app.UseMiddleware<CorrelationIdMiddleware>();

// ── Endpoints ──
app.MapHealthChecks("/health");

// ── Database Startup ──
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<Payments.Infrastructure.Persistence.PaymentsDbContext>();
    context.Database.EnsureCreated();
    
    // ── Self-healing: Ensure CustomerId column exists (EnsureCreated doesn't update schema) ──
    try
    {
        Console.WriteLine("[Schema Check] Verifying pay.Payments schema...");
        context.Database.ExecuteSqlRaw(@"
            IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
                          WHERE TABLE_SCHEMA = 'pay' AND TABLE_NAME = 'Payments' 
                          AND COLUMN_NAME = 'CustomerId')
            BEGIN
                PRINT 'Adding CustomerId column to pay.Payments...';
                ALTER TABLE [pay].[Payments] ADD [CustomerId] UNIQUEIDENTIFIER NULL;
            END");
        Console.WriteLine("[Schema Check] Column check completed successfully.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[Schema Check] Could not add CustomerId to pay.Payments: {ex.Message}");
    }
}

app.MapControllers();

app.Run();