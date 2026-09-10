namespace EPharmacy.Domain;

public sealed record CartItem(Guid MedicineId, int Quantity);

public sealed class Cart
{
    private readonly List<CartItem> _items = new();

    private Cart(Guid id, Guid userId)
    {
        Id = id;
        UserId = userId;
    }

    public Guid Id { get; }

    public Guid UserId { get; }

    public IReadOnlyCollection<CartItem> Items => _items;

    public int TotalItemCount => _items.Sum(item => item.Quantity);

    public static Cart CreateEmpty(Guid id, Guid userId)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Id must not be empty.", nameof(id));
        }

        if (userId == Guid.Empty)
        {
            throw new ArgumentException("User id must not be empty.", nameof(userId));
        }

        return new Cart(id, userId);
    }

    public void AddItem(Guid medicineId, int quantity)
    {
        if (medicineId == Guid.Empty)
        {
            throw new ArgumentException("Medicine id must not be empty.", nameof(medicineId));
        }

        if (quantity <= 0)
        {
            throw new ArgumentException("Quantity must be positive.", nameof(quantity));
        }

        var existingIndex = _items.FindIndex(item => item.MedicineId == medicineId);
        if (existingIndex >= 0)
        {
            var existing = _items[existingIndex];
            _items[existingIndex] = existing with { Quantity = existing.Quantity + quantity };
        }
        else
        {
            _items.Add(new CartItem(medicineId, quantity));
        }
    }

    public void UpdateItemQuantity(Guid medicineId, int quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentException("Quantity must be positive.", nameof(quantity));
        }

        var existingIndex = _items.FindIndex(item => item.MedicineId == medicineId);
        if (existingIndex < 0)
        {
            throw new ArgumentException("Medicine is not in the cart.", nameof(medicineId));
        }

        _items[existingIndex] = _items[existingIndex] with { Quantity = quantity };
    }

    public void RemoveItem(Guid medicineId)
    {
        _items.RemoveAll(item => item.MedicineId == medicineId);
    }

    public void Clear()
    {
        _items.Clear();
    }
}
