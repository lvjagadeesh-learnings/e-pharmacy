using EPharmacy.Domain;

namespace EPharmacy.Application;

public sealed record MedicineDto(Guid Id, string Name, string Description, int PriceCents, string? ImageUrl, double AverageRating, int ReviewCount);

/// <summary>
/// Orchestrates listing the medicine catalog, enriched with each medicine's
/// average rating and review count.
/// </summary>
public sealed class ListMedicinesHandler
{
    private readonly IMedicineRepository _medicineRepository;
    private readonly IReviewRepository _reviewRepository;

    public ListMedicinesHandler(IMedicineRepository medicineRepository, IReviewRepository reviewRepository)
    {
        _medicineRepository = medicineRepository ?? throw new ArgumentNullException(nameof(medicineRepository));
        _reviewRepository = reviewRepository ?? throw new ArgumentNullException(nameof(reviewRepository));
    }

    public async Task<IReadOnlyList<MedicineDto>> HandleAsync(CancellationToken cancellationToken)
    {
        var medicines = await _medicineRepository.ListAllAsync(cancellationToken);
        var ratingSummaries = await _reviewRepository.GetRatingSummariesAsync(
            medicines.Select(m => m.Id).ToList(),
            cancellationToken);

        return medicines
            .Select(medicine =>
            {
                ratingSummaries.TryGetValue(medicine.Id, out var summary);
                return new MedicineDto(
                    medicine.Id,
                    medicine.Name,
                    medicine.Description,
                    medicine.PriceCents,
                    medicine.ImageUrl,
                    summary?.AverageRating ?? 0,
                    summary?.ReviewCount ?? 0);
            })
            .ToList();
    }
}
