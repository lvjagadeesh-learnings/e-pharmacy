namespace EPharmacy.Domain;

/// <summary>
/// A record of a single health check being performed, used to prove the full
/// stack (Api -> Application -> Infrastructure -> database) is wired
/// correctly. Not a real e-pharmacy domain concept.
/// </summary>
public sealed class HealthCheck
{
    public HealthCheck(Guid id, DateTimeOffset checkedAtUtc)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Id must not be empty.", nameof(id));
        }

        Id = id;
        CheckedAtUtc = checkedAtUtc;
    }

    public Guid Id { get; }

    public DateTimeOffset CheckedAtUtc { get; }

    public static HealthCheck CreateNow() => new(Guid.NewGuid(), DateTimeOffset.UtcNow);
}
