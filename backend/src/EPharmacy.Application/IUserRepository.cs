using EPharmacy.Domain;

namespace EPharmacy.Application;

/// <summary>
/// Port for looking up and persisting users. Implemented by Infrastructure.
/// </summary>
public interface IUserRepository
{
    Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken);

    Task<User?> FindByIdAsync(Guid id, CancellationToken cancellationToken);

    Task AddAsync(User user, CancellationToken cancellationToken);

    Task SaveAsync(User user, CancellationToken cancellationToken);
}
