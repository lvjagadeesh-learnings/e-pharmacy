using EPharmacy.Domain;

namespace EPharmacy.Application;

public sealed record OrderSummaryDto(
    Guid OrderId,
    string ReferenceNumber,
    DateTimeOffset PlacedAtUtc,
    int TotalCents,
    OrderStatus Status);

public sealed class ListOrdersHandler
{
    private readonly IOrderRepository _orderRepository;

    public ListOrdersHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
    }

    public async Task<IReadOnlyList<OrderSummaryDto>> HandleAsync(Guid userId, CancellationToken cancellationToken)
    {
        var orders = await _orderRepository.ListForUserAsync(userId, cancellationToken);
        var now = DateTimeOffset.UtcNow;

        return orders
            .OrderByDescending(order => order.PlacedAtUtc)
            .Select(order => new OrderSummaryDto(
                order.Id,
                order.Id.ToString("N")[..8].ToUpperInvariant(),
                order.PlacedAtUtc,
                order.TotalCents,
                OrderStatusCalculator.Calculate(order.PlacedAtUtc, now)))
            .ToList();
    }
}
