using EPharmacy.Domain;

namespace EPharmacy.Application;

public sealed record ReviewDto(Guid Id, string ReviewerDisplayName, int Rating, string Comment, DateTimeOffset CreatedAtUtc);

public sealed class ListReviewsForMedicineHandler
{
    private readonly IReviewRepository _reviewRepository;

    public ListReviewsForMedicineHandler(IReviewRepository reviewRepository)
    {
        _reviewRepository = reviewRepository ?? throw new ArgumentNullException(nameof(reviewRepository));
    }

    public async Task<IReadOnlyList<ReviewDto>> HandleAsync(Guid medicineId, CancellationToken cancellationToken)
    {
        var reviews = await _reviewRepository.ListForMedicineAsync(medicineId, cancellationToken);
        return reviews
            .Select(r => new ReviewDto(r.Id, r.ReviewerDisplayName, r.Rating, r.Comment, r.CreatedAtUtc))
            .ToList();
    }
}
