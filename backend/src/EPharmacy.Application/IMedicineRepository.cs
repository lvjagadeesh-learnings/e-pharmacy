using EPharmacy.Domain;

namespace EPharmacy.Application;

/// <summary>
/// Port for reading medicines from the catalog. Implemented by Infrastructure.
/// </summary>
public interface IMedicineRepository
{
    Task<IReadOnlyList<Medicine>> ListAllAsync(CancellationToken cancellationToken);

    Task<Medicine?> FindByIdAsync(Guid id, CancellationToken cancellationToken);
}
