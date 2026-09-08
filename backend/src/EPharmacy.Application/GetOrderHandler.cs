using EPharmacy.Domain;

namespace EPharmacy.Application;

public sealed record OrderDetailDto(
    Guid OrderId,
    string ReferenceNumber,
    string ShippingAddress,
    DateTimeOffset PlacedAtUtc,
    int TotalCents,
    OrderStatus Status,
    IReadOnlyList<OrderStatusEvent> StatusHistory,
    DateTimeOffset? ReceivedAtUtc,
    IReadOnlyList<OrderItem> Items);

public sealed class GetOrderHandler
{
    private readonly IOrderRepository _orderRepository;

    public GetOrderHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
    }

    public async Task<OrderDetailDto?> HandleAsync(Guid userId, Guid orderId, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.FindByIdAsync(orderId, cancellationToken);
        if (order is null || order.UserId != userId)
        {
            return null;
        }

        var now = DateTimeOffset.UtcNow;
        return new OrderDetailDto(
            order.Id,
            order.Id.ToString("N")[..8].ToUpperInvariant(),
            order.ShippingAddress,
            order.PlacedAtUtc,
            order.TotalCents,
            OrderStatusCalculator.Calculate(order.PlacedAtUtc, now),
            OrderStatusCalculator.History(order.PlacedAtUtc, now),
            order.ReceivedAtUtc,
            order.Items.ToList());
    }
}
