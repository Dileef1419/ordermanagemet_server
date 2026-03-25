using Orders.Application.DTOs;
using SharedKernel;

namespace Orders.Application.Commands.PlaceOrder;

/// <summary>
/// Handles PlaceOrderCommand — orchestrates domain logic, outbox, idempotency.
/// Implementation lives in Orders.Infrastructure (dependency inversion).
/// </summary>
public interface IPlaceOrderCommandHandler : ICommandHandler<PlaceOrderCommand, OrderResponse> { }
