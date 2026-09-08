using EPharmacy.Domain;

namespace EPharmacy.Application;

/// <summary>
/// Port for persisting and loading orders. Implemented by Infrastructure.
/// </summary>
public interface IOrderRepository
{
    Task AddAsync(Order order, CancellationToken cancellationToken);

    Task SaveAsync(Order order, CancellationToken cancellationToken);

    Task<Order?> FindByIdAsync(Guid orderId, CancellationToken cancellationToken);

    Task<IReadOnlyList<Order>> ListForUserAsync(Guid userId, CancellationToken cancellationToken);
}
