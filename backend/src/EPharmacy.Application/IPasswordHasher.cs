namespace EPharmacy.Application;

/// <summary>
/// Port for hashing and verifying passwords. Implemented by Infrastructure.
/// </summary>
public interface IPasswordHasher
{
    string Hash(string password);

    bool Verify(string passwordHash, string password);
}
