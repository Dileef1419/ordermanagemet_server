using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Payments.Application.Commands.CapturePayment;
using Payments.Application.Commands.RefundPayment;
using Payments.Application.DTOs;
using Payments.Infrastructure.Persistence;
using SharedKernel;

namespace Payments.Infrastructure.CommandHandlers;

public class CapturePaymentHandler : ICapturePaymentCommandHandler
{
    private readonly PaymentsDbContext _db;
    public CapturePaymentHandler(PaymentsDbContext db) => _db = db;

    public async Task<PaymentResponse> Handle(CapturePaymentCommand cmd, CancellationToken ct)
    {
        var payment = await _db.Payments
            .Include(p => p.Attempts)
            .FirstOrDefaultAsync(p => p.Id == cmd.PaymentId, ct)
            ?? throw new InvalidOperationException($"Payment {cmd.PaymentId} not found.");

        var trackedAttemptIds = payment.Attempts.Select(a => a.Id).ToHashSet();

        payment.Capture();

        // Explicitly mark new attempts as Added so EF Core tracks them correctly
        foreach (var attempt in payment.Attempts)
        {
            if (!trackedAttemptIds.Contains(attempt.Id))
                _db.Entry(attempt).State = EntityState.Added;
        }

        foreach (var evt in payment.DomainEvents)
        {
            _db.Outbox.Add(new OutboxMessage
            {
                AggregateId = payment.Id,
                EventType = evt.GetType().Name,
                Payload = JsonSerializer.Serialize<object>(evt)
            });
        }

        await _db.SaveChangesAsync(ct);
        payment.ClearDomainEvents();
        return new PaymentResponse(payment.Id, payment.Status.ToString());
    }
}

public class RefundPaymentHandler : IRefundPaymentCommandHandler
{
    private readonly PaymentsDbContext _db;
    public RefundPaymentHandler(PaymentsDbContext db) => _db = db;

    public async Task<PaymentResponse> Handle(RefundPaymentCommand cmd, CancellationToken ct)
    {
        var payment = await _db.Payments
            .Include(p => p.Attempts)
            .FirstOrDefaultAsync(p => p.Id == cmd.PaymentId, ct)
            ?? throw new InvalidOperationException($"Payment {cmd.PaymentId} not found.");

        var trackedAttemptIds = payment.Attempts.Select(a => a.Id).ToHashSet();

        payment.Refund(cmd.Amount, cmd.Reason);

        // Explicitly mark new attempts as Added so EF Core tracks them correctly
        foreach (var attempt in payment.Attempts)
        {
            if (!trackedAttemptIds.Contains(attempt.Id))
                _db.Entry(attempt).State = EntityState.Added;
        }

        foreach (var evt in payment.DomainEvents)
        {
            _db.Outbox.Add(new OutboxMessage
            {
                AggregateId = payment.Id,
                EventType = evt.GetType().Name,
                Payload = JsonSerializer.Serialize<object>(evt)
            });
        }

        await _db.SaveChangesAsync(ct);
        payment.ClearDomainEvents();
        return new PaymentResponse(payment.Id, payment.Status.ToString());
    }
}
