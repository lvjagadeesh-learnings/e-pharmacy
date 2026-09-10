namespace EPharmacy.Domain;

/// <summary>
/// A medicine available for purchase in the catalog, with its price stored in minor units (cents).
/// </summary>
public sealed class Medicine
{
    private Medicine(Guid id, string name, string description, int priceCents, string? imageUrl)
    {
        Id = id;
        Name = name;
        Description = description;
        PriceCents = priceCents;
        ImageUrl = imageUrl;
    }

    public Guid Id { get; }

    public string Name { get; }

    public string Description { get; }

    public int PriceCents { get; }

    public string? ImageUrl { get; }

    public static Medicine Create(Guid id, string name, string description, int priceCents, string? imageUrl)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Id must not be empty.", nameof(id));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name must not be empty.", nameof(name));
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            throw new ArgumentException("Description must not be empty.", nameof(description));
        }

        if (priceCents < 0)
        {
            throw new ArgumentException("Price must not be negative.", nameof(priceCents));
        }

        return new Medicine(id, name, description, priceCents, imageUrl);
    }
}
