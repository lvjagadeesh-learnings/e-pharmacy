using EPharmacy.Application;
using Microsoft.AspNetCore.Identity;

namespace EPharmacy.Infrastructure;

/// <summary>
/// Wraps ASP.NET Core Identity's <see cref="PasswordHasher{TUser}"/> so the Application
/// layer doesn't need a User instance to hash or verify a password.
/// </summary>
public sealed class PasswordHasher : IPasswordHasher
{
    private readonly Microsoft.AspNetCore.Identity.PasswordHasher<object> _hasher = new();

    public string Hash(string password) => _hasher.HashPassword(new object(), password);

    public bool Verify(string passwordHash, string password) =>
        _hasher.VerifyHashedPassword(new object(), passwordHash, password) != PasswordVerificationResult.Failed;
}
