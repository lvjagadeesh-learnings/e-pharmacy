using EPharmacy.Domain;

namespace EPharmacy.Application;

/// <summary>
/// Port for loading and persisting a user's cart. Implemented by Infrastructure.
/// </summary>
public interface ICartRepository
{
    Task<Cart> GetOrCreateForUserAsync(Guid userId, CancellationToken cancellationToken);

    Task SaveAsync(Cart cart, CancellationToken cancellationToken);
}
