using EPharmacy.Domain;
using FluentAssertions;
using Moq;

namespace EPharmacy.Application.Tests;

public class ListMedicinesHandlerTests
{
    private readonly Mock<IMedicineRepository> _medicineRepository = new();
    private readonly Mock<IReviewRepository> _reviewRepository = new();

    private ListMedicinesHandler CreateHandler() => new(_medicineRepository.Object, _reviewRepository.Object);

    [Fact]
    public async Task HandleAsync_merges_rating_summaries_into_each_medicine_dto()
    {
        var medicine1 = Medicine.Create(Guid.NewGuid(), "Paracetamol 500mg", "Pain and fever relief tablets.", 599, null);
        var medicine2 = Medicine.Create(Guid.NewGuid(), "Vitamin C 1000mg", "Immune support supplement.", 899, null);
        _medicineRepository.Setup(r => r.ListAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<Medicine> { medicine1, medicine2 });
        _reviewRepository
            .Setup(r => r.GetRatingSummariesAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, MedicineRatingSummary>
            {
                [medicine1.Id] = new MedicineRatingSummary(medicine1.Id, 4.5, 2),
            });
        var handler = CreateHandler();

        var result = await handler.HandleAsync(CancellationToken.None);

        var dto1 = result.Single(m => m.Id == medicine1.Id);
        dto1.AverageRating.Should().Be(4.5);
        dto1.ReviewCount.Should().Be(2);

        var dto2 = result.Single(m => m.Id == medicine2.Id);
        dto2.AverageRating.Should().Be(0);
        dto2.ReviewCount.Should().Be(0);
    }

    [Fact]
    public async Task HandleAsync_returns_an_empty_list_when_the_repository_has_no_medicines()
    {
        _medicineRepository.Setup(r => r.ListAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<Medicine>());
        _reviewRepository
            .Setup(r => r.GetRatingSummariesAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, MedicineRatingSummary>());
        var handler = CreateHandler();

        var result = await handler.HandleAsync(CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_rejects_a_null_medicine_repository()
    {
        var act = () => new ListMedicinesHandler(null!, _reviewRepository.Object);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_rejects_a_null_review_repository()
    {
        var act = () => new ListMedicinesHandler(_medicineRepository.Object, null!);

        act.Should().Throw<ArgumentNullException>();
    }
}
