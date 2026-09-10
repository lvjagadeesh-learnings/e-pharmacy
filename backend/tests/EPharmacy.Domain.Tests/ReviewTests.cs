using EPharmacy.Domain;
using FluentAssertions;
using Xunit;

namespace EPharmacy.Domain.Tests;

public class ReviewTests
{
    [Fact]
    public void Create_sets_all_properties_on_the_happy_path()
    {
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var medicineId = Guid.NewGuid();
        var createdAtUtc = DateTimeOffset.UtcNow;

        var review = Review.Create(id, userId, medicineId, "Ada Shopper", 5, "Worked great for my headache.", createdAtUtc);

        review.Id.Should().Be(id);
        review.UserId.Should().Be(userId);
        review.MedicineId.Should().Be(medicineId);
        review.ReviewerDisplayName.Should().Be("Ada Shopper");
        review.Rating.Should().Be(5);
        review.Comment.Should().Be("Worked great for my headache.");
        review.CreatedAtUtc.Should().Be(createdAtUtc);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    public void Create_throws_when_rating_is_outside_1_to_5(int rating)
    {
        var act = () => Review.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Ada Shopper", rating, "Good.", DateTimeOffset.UtcNow);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_throws_when_comment_is_empty()
    {
        var act = () => Review.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Ada Shopper", 4, "  ", DateTimeOffset.UtcNow);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_throws_when_reviewer_display_name_is_empty()
    {
        var act = () => Review.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "  ", 4, "Good.", DateTimeOffset.UtcNow);

        act.Should().Throw<ArgumentException>();
    }
}
