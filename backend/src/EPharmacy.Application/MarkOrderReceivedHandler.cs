using EPharmacy.Domain;

namespace EPharmacy.Application;

public enum MarkOrderReceivedStatus
{
    Success,
    NotFound,
    NotYetDelivered,
    AlreadyReceived,
}

public sealed record MarkOrderReceivedResult(MarkOrderReceivedStatus Status);

public sealed class MarkOrderReceivedHandler
{
    private readonly IOrderRepository _orderRepository;

    public MarkOrderReceivedHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
    }

    public async Task<MarkOrderReceivedResult> HandleAsync(Guid userId, Guid orderId, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.FindByIdAsync(orderId, cancellationToken);
        if (order is null || order.UserId != userId)
        {
            return new MarkOrderReceivedResult(MarkOrderReceivedStatus.NotFound);
        }

        if (order.ReceivedAtUtc is not null)
        {
            return new MarkOrderReceivedResult(MarkOrderReceivedStatus.AlreadyReceived);
        }

        var now = DateTimeOffset.UtcNow;
        if (OrderStatusCalculator.Calculate(order.PlacedAtUtc, now) != OrderStatus.Delivered)
        {
            return new MarkOrderReceivedResult(MarkOrderReceivedStatus.NotYetDelivered);
        }

        order.MarkReceived(now);
        await _orderRepository.SaveAsync(order, cancellationToken);

        return new MarkOrderReceivedResult(MarkOrderReceivedStatus.Success);
    }
}
