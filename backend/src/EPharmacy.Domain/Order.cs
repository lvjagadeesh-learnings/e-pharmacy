namespace EPharmacy.Domain;

public sealed record OrderItem(Guid MedicineId, string Name, int UnitPriceCents, int Quantity);

public sealed class Order
{
    private readonly List<OrderItem> _items;

    private Order(Guid id, Guid userId, string shippingAddress, DateTimeOffset placedAtUtc)
    {
        Id = id;
        UserId = userId;
        ShippingAddress = shippingAddress;
        _items = new List<OrderItem>();
        PlacedAtUtc = placedAtUtc;
    }

    public Guid Id { get; }

    public Guid UserId { get; }

    public string ShippingAddress { get; }

    public IReadOnlyCollection<OrderItem> Items => _items;

    public int TotalCents => _items.Sum(item => item.UnitPriceCents * item.Quantity);

    public DateTimeOffset PlacedAtUtc { get; }

    public DateTimeOffset? ReceivedAtUtc { get; private set; }

    public void MarkReceived(DateTimeOffset nowUtc)
    {
        if (ReceivedAtUtc is not null)
        {
            throw new InvalidOperationException("Order has already been marked as received.");
        }

        if (OrderStatusCalculator.Calculate(PlacedAtUtc, nowUtc) != OrderStatus.Delivered)
        {
            throw new InvalidOperationException("Order cannot be marked as received before it has been delivered.");
        }

        ReceivedAtUtc = nowUtc;
    }

    public static Order Create(Guid id, Guid userId, string shippingAddress, IReadOnlyCollection<OrderItem> items, DateTimeOffset placedAtUtc)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Id must not be empty.", nameof(id));
        }

        if (userId == Guid.Empty)
        {
            throw new ArgumentException("User id must not be empty.", nameof(userId));
        }

        if (string.IsNullOrWhiteSpace(shippingAddress))
        {
            throw new ArgumentException("Shipping address must not be empty.", nameof(shippingAddress));
        }

        if (items is null || items.Count == 0)
        {
            throw new ArgumentException("Order must contain at least one item.", nameof(items));
        }

        var order = new Order(id, userId, shippingAddress, placedAtUtc);
        order._items.AddRange(items);
        return order;
    }
}
