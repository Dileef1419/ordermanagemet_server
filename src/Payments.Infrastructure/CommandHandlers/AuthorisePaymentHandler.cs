using System.Text.Json;
using Payments.Application.Commands.AuthorisePayment;
using Payments.Application.DTOs;
using Payments.Domain.Aggregates;
using Payments.Infrastructure.Persistence;
using SharedKernel;

namespace Payments.Infrastructure.CommandHandlers;

public class AuthorisePaymentHandler : IAuthorisePaymentCommandHandler
{
    private readonly PaymentsDbContext _db;

    public AuthorisePaymentHandler(PaymentsDbContext db) => _db = db;

    public async Task<PaymentResponse> Handle(AuthorisePaymentCommand cmd, CancellationToken ct)
    {
        // Idempotency check
        var existing = await _db.ProcessedCommands.FindAsync(new object[] { cmd.IdempotencyKey }, ct);
        if (existing is not null)
            return JsonSerializer.Deserialize<PaymentResponse>(existing.ResultPayload!)!;

        var payment = Payment.Create(cmd.OrderId, cmd.Amount, cmd.Currency, cmd.IdempotencyKey);

        // Simulate gateway call — in production this calls Stripe/Adyen
        payment.Authorise($"gw-ref-{Guid.NewGuid():N}");

        await _db.Payments.AddAsync(payment, ct);

        foreach (var evt in payment.DomainEvents)
        {
            _db.Outbox.Add(new OutboxMessage
            {
                AggregateId = payment.Id,
                EventType = evt.GetType().Name,
                Payload = JsonSerializer.Serialize<object>(evt)
            });
        }

        var result = new PaymentResponse(payment.Id, payment.Status.ToString());
        _db.ProcessedCommands.Add(new ProcessedCommand
        {
            IdempotencyKey = cmd.IdempotencyKey,
            CommandType = nameof(AuthorisePaymentCommand),
            ResultPayload = JsonSerializer.Serialize(result),
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(7)
        });

        await _db.SaveChangesAsync(ct);
        payment.ClearDomainEvents();
        return result;
    }
}
