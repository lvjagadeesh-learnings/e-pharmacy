using EPharmacy.Domain;
using FluentAssertions;
using Moq;

namespace EPharmacy.Application.Tests;

public class SubmitReviewHandlerTests
{
    private readonly Mock<IMedicineRepository> _medicineRepository = new();
    private readonly Mock<IOrderRepository> _orderRepository = new();
    private readonly Mock<IReviewRepository> _reviewRepository = new();
    private readonly Mock<IUserRepository> _userRepository = new();

    private static Medicine CreateMedicine(Guid id) =>
        Medicine.Create(id, "Paracetamol 500mg", "Pain and fever relief tablets.", 599, null);

    private static User CreateUser(Guid id) =>
        User.Create(id, "ada@example.com", "hash", "Ada Shopper", DateTimeOffset.UtcNow);

    private static Order CreateDeliveredOrder(Guid userId, Guid medicineId) =>
        Order.Create(
            Guid.NewGuid(),
            userId,
            "1 Example St",
            new[] { new OrderItem(medicineId, "Paracetamol 500mg", 599, 1) },
            DateTimeOffset.UtcNow.AddSeconds(-200));

    private SubmitReviewHandler CreateHandler() =>
        new(_medicineRepository.Object, _orderRepository.Object, _reviewRepository.Object, _userRepository.Object);

    [Fact]
    public async Task HandleAsync_returns_MedicineNotFound_when_the_medicine_does_not_exist()
    {
        var userId = Guid.NewGuid();
        var medicineId = Guid.NewGuid();
        _medicineRepository.Setup(r => r.FindByIdAsync(medicineId, It.IsAny<CancellationToken>())).ReturnsAsync((Medicine?)null);
        var handler = CreateHandler();

        var result = await handler.HandleAsync(userId, medicineId, 5, "Great!", CancellationToken.None);

        result.Status.Should().Be(SubmitReviewStatus.MedicineNotFound);
    }

    [Fact]
    public async Task HandleAsync_returns_NotEligible_when_the_user_has_no_delivered_order_for_the_medicine()
    {
        var userId = Guid.NewGuid();
        var medicineId = Guid.NewGuid();
        _medicineRepository.Setup(r => r.FindByIdAsync(medicineId, It.IsAny<CancellationToken>())).ReturnsAsync(CreateMedicine(medicineId));
        _orderRepository.Setup(r => r.ListForUserAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(new List<Order>());
        var handler = CreateHandler();

        var result = await handler.HandleAsync(userId, medicineId, 5, "Great!", CancellationToken.None);

        result.Status.Should().Be(SubmitReviewStatus.NotEligible);
    }

    [Fact]
    public async Task HandleAsync_returns_AlreadyReviewed_when_the_user_has_already_reviewed_the_medicine()
    {
        var userId = Guid.NewGuid();
        var medicineId = Guid.NewGuid();
        _medicineRepository.Setup(r => r.FindByIdAsync(medicineId, It.IsAny<CancellationToken>())).ReturnsAsync(CreateMedicine(medicineId));
        _orderRepository.Setup(r => r.ListForUserAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Order> { CreateDeliveredOrder(userId, medicineId) });
        _reviewRepository.Setup(r => r.FindByUserAndMedicineAsync(userId, medicineId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Review.Create(Guid.NewGuid(), userId, medicineId, "Ada Shopper", 4, "Good.", DateTimeOffset.UtcNow));
        var handler = CreateHandler();

        var result = await handler.HandleAsync(userId, medicineId, 5, "Great!", CancellationToken.None);

        result.Status.Should().Be(SubmitReviewStatus.AlreadyReviewed);
    }

    [Fact]
    public async Task HandleAsync_persists_and_returns_success_when_eligible_and_not_already_reviewed()
    {
        var userId = Guid.NewGuid();
        var medicineId = Guid.NewGuid();
        _medicineRepository.Setup(r => r.FindByIdAsync(medicineId, It.IsAny<CancellationToken>())).ReturnsAsync(CreateMedicine(medicineId));
        _orderRepository.Setup(r => r.ListForUserAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Order> { CreateDeliveredOrder(userId, medicineId) });
        _reviewRepository.Setup(r => r.FindByUserAndMedicineAsync(userId, medicineId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Review?)null);
        _userRepository.Setup(r => r.FindByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(CreateUser(userId));
        var handler = CreateHandler();

        var result = await handler.HandleAsync(userId, medicineId, 5, "Great!", CancellationToken.None);

        result.Status.Should().Be(SubmitReviewStatus.Success);
        _reviewRepository.Verify(
            r => r.AddAsync(It.Is<Review>(review => review.UserId == userId && review.MedicineId == medicineId && review.Rating == 5), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
