namespace EPharmacy.Domain;

public enum OrderStatus
{
    Placed,
    Processing,
    Shipped,
    Delivered,
}

public sealed record OrderStatusEvent(OrderStatus Status, DateTimeOffset ReachedAtUtc);

public static class OrderStatusCalculator
{
    private static readonly TimeSpan ProcessingAfter = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan ShippedAfter = TimeSpan.FromSeconds(90);
    private static readonly TimeSpan DeliveredAfter = TimeSpan.FromSeconds(180);

    public static OrderStatus Calculate(DateTimeOffset placedAtUtc, DateTimeOffset nowUtc)
    {
        var elapsed = nowUtc - placedAtUtc;

        if (elapsed >= DeliveredAfter) return OrderStatus.Delivered;
        if (elapsed >= ShippedAfter) return OrderStatus.Shipped;
        if (elapsed >= ProcessingAfter) return OrderStatus.Processing;
        return OrderStatus.Placed;
    }

    public static IReadOnlyList<OrderStatusEvent> History(DateTimeOffset placedAtUtc, DateTimeOffset nowUtc)
    {
        var history = new List<OrderStatusEvent> { new(OrderStatus.Placed, placedAtUtc) };
        var elapsed = nowUtc - placedAtUtc;

        if (elapsed >= ProcessingAfter) history.Add(new OrderStatusEvent(OrderStatus.Processing, placedAtUtc + ProcessingAfter));
        if (elapsed >= ShippedAfter) history.Add(new OrderStatusEvent(OrderStatus.Shipped, placedAtUtc + ShippedAfter));
        if (elapsed >= DeliveredAfter) history.Add(new OrderStatusEvent(OrderStatus.Delivered, placedAtUtc + DeliveredAfter));

        return history;
    }
}
