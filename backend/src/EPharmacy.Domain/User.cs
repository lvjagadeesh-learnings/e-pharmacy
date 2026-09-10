namespace EPharmacy.Domain;

/// <summary>
/// A registered shopper account, created via sign-up and used to authenticate.
/// </summary>
public sealed class User
{
    private User(Guid id, string email, string passwordHash, string displayName, DateTimeOffset createdAtUtc)
    {
        Id = id;
        Email = email;
        PasswordHash = passwordHash;
        DisplayName = displayName;
        CreatedAtUtc = createdAtUtc;
    }

    public Guid Id { get; }

    public string Email { get; }

    public string PasswordHash { get; }

    public string DisplayName { get; }

    public DateTimeOffset CreatedAtUtc { get; }

    public bool IsMember { get; private set; }

    public DateTimeOffset? MembershipJoinedAtUtc { get; private set; }

    /// <summary>
    /// Enrolls the user in the "e-Pharmacy Plus" membership program.
    /// </summary>
    public void JoinMembership(DateTimeOffset joinedAtUtc)
    {
        if (IsMember)
        {
            throw new InvalidOperationException("User is already a member.");
        }

        IsMember = true;
        MembershipJoinedAtUtc = joinedAtUtc;
    }

    public static User Create(Guid id, string email, string passwordHash, string displayName, DateTimeOffset createdAtUtc)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Id must not be empty.", nameof(id));
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Email must not be empty.", nameof(email));
        }

        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            throw new ArgumentException("Password hash must not be empty.", nameof(passwordHash));
        }

        if (string.IsNullOrWhiteSpace(displayName))
        {
            throw new ArgumentException("Display name must not be empty.", nameof(displayName));
        }

        return new User(id, email, passwordHash, displayName, createdAtUtc);
    }
}
