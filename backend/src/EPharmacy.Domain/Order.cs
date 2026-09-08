namespace EPharmacy.Domain;

public sealed record OrderItem(Guid MedicineId, string Name, int UnitPriceCents, int Quantity);

public sealed class Order
{
    private readonly List<OrderItem> _items;

    private Order(Guid id, Guid userId, string shippingAddress, IReadOnlyCollection<OrderItem> items, DateTimeOffset placedAtUtc)
    {
        Id = id;
        UserId = userId;
        ShippingAddress = shippingAddress;
        _items = new List<OrderItem>(items);
        PlacedAtUtc = placedAtUtc;
    }

    public Guid Id { get; }

    public Guid UserId { get; }

    public string ShippingAddress { get; }

    public IReadOnlyCollection<OrderItem> Items => _items;

    public int TotalCents => _items.Sum(item => item.UnitPriceCents * item.Quantity);

    public DateTimeOffset PlacedAtUtc { get; }

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

        return new Order(id, userId, shippingAddress, items, placedAtUtc);
    }
}
