using EPharmacy.Domain;

namespace EPharmacy.Application;

public sealed record MedicineRatingSummary(Guid MedicineId, double AverageRating, int ReviewCount);

/// <summary>
/// Port for reading and persisting medicine reviews. Implemented by Infrastructure.
/// </summary>
public interface IReviewRepository
{
    Task<Review?> FindByUserAndMedicineAsync(Guid userId, Guid medicineId, CancellationToken cancellationToken);

    Task AddAsync(Review review, CancellationToken cancellationToken);

    Task<IReadOnlyList<Review>> ListForMedicineAsync(Guid medicineId, CancellationToken cancellationToken);

    Task<IReadOnlyDictionary<Guid, MedicineRatingSummary>> GetRatingSummariesAsync(
        IReadOnlyCollection<Guid> medicineIds,
        CancellationToken cancellationToken);
}
