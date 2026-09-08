using FluentAssertions;

namespace EPharmacy.Domain.Tests;

public class UserTests
{
    [Fact]
    public void Create_sets_all_properties()
    {
        var id = Guid.NewGuid();
        var createdAtUtc = DateTimeOffset.UtcNow;

        var user = User.Create(id, "shopper@example.com", "hashed-password", "Ada Shopper", createdAtUtc);

        user.Id.Should().Be(id);
        user.Email.Should().Be("shopper@example.com");
        user.PasswordHash.Should().Be("hashed-password");
        user.DisplayName.Should().Be("Ada Shopper");
        user.CreatedAtUtc.Should().Be(createdAtUtc);
    }

    [Fact]
    public void Create_rejects_an_empty_id()
    {
        var act = () => User.Create(Guid.Empty, "shopper@example.com", "hashed-password", "Ada Shopper", DateTimeOffset.UtcNow);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_rejects_an_empty_email()
    {
        var act = () => User.Create(Guid.NewGuid(), "", "hashed-password", "Ada Shopper", DateTimeOffset.UtcNow);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_rejects_an_empty_password_hash()
    {
        var act = () => User.Create(Guid.NewGuid(), "shopper@example.com", "", "Ada Shopper", DateTimeOffset.UtcNow);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_rejects_an_empty_display_name()
    {
        var act = () => User.Create(Guid.NewGuid(), "shopper@example.com", "hashed-password", "", DateTimeOffset.UtcNow);

        act.Should().Throw<ArgumentException>();
    }
}
