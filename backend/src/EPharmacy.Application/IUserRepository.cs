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

/// <summary>
/// Thrown by <see cref="IUserRepository.AddAsync"/> when a concurrent registration already
/// claimed the same email between the caller's pre-check and the insert.
/// </summary>
public sealed class DuplicateEmailException : Exception;
