using EPharmacy.Domain;

namespace EPharmacy.Application;

public enum SubmitReviewStatus
{
    Success,
    MedicineNotFound,
    NotEligible,
    AlreadyReviewed,
}

public sealed record SubmitReviewResult(SubmitReviewStatus Status, Review? Review);

public sealed class SubmitReviewHandler
{
    private readonly IMedicineRepository _medicineRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IReviewRepository _reviewRepository;
    private readonly IUserRepository _userRepository;

    public SubmitReviewHandler(
        IMedicineRepository medicineRepository,
        IOrderRepository orderRepository,
        IReviewRepository reviewRepository,
        IUserRepository userRepository)
    {
        _medicineRepository = medicineRepository ?? throw new ArgumentNullException(nameof(medicineRepository));
        _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
        _reviewRepository = reviewRepository ?? throw new ArgumentNullException(nameof(reviewRepository));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    }

    public async Task<SubmitReviewResult> HandleAsync(
        Guid userId,
        Guid medicineId,
        int rating,
        string comment,
        CancellationToken cancellationToken)
    {
        var medicine = await _medicineRepository.FindByIdAsync(medicineId, cancellationToken);
        if (medicine is null)
        {
            return new SubmitReviewResult(SubmitReviewStatus.MedicineNotFound, null);
        }

        var orders = await _orderRepository.ListForUserAsync(userId, cancellationToken);
        var now = DateTimeOffset.UtcNow;
        var isEligible = orders.Any(order =>
            order.Items.Any(item => item.MedicineId == medicineId) &&
            OrderStatusCalculator.Calculate(order.PlacedAtUtc, now) == OrderStatus.Delivered);

        if (!isEligible)
        {
            return new SubmitReviewResult(SubmitReviewStatus.NotEligible, null);
        }

        var existingReview = await _reviewRepository.FindByUserAndMedicineAsync(userId, medicineId, cancellationToken);
        if (existingReview is not null)
        {
            return new SubmitReviewResult(SubmitReviewStatus.AlreadyReviewed, null);
        }

        var user = await _userRepository.FindByIdAsync(userId, cancellationToken);
        var reviewerDisplayName = user?.DisplayName ?? "Anonymous";

        var review = Review.Create(Guid.NewGuid(), userId, medicineId, reviewerDisplayName, rating, comment, now);
        await _reviewRepository.AddAsync(review, cancellationToken);

        return new SubmitReviewResult(SubmitReviewStatus.Success, review);
    }
}
