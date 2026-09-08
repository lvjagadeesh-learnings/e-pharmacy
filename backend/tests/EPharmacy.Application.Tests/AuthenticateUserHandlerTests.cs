using EPharmacy.Domain;
using FluentAssertions;
using Moq;

namespace EPharmacy.Application.Tests;

public class AuthenticateUserHandlerTests
{
    [Fact]
    public async Task HandleAsync_succeeds_when_the_email_exists_and_the_password_matches()
    {
        var user = User.Create(Guid.NewGuid(), "shopper@example.com", "hashed-s3cret", "Ada Shopper", DateTimeOffset.UtcNow);

        var userRepository = new Mock<IUserRepository>();
        userRepository
            .Setup(r => r.FindByEmailAsync("shopper@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var passwordHasher = new Mock<IPasswordHasher>();
        passwordHasher.Setup(h => h.Verify("hashed-s3cret", "s3cret!")).Returns(true);

        var handler = new AuthenticateUserHandler(userRepository.Object, passwordHasher.Object);

        var result = await handler.HandleAsync("shopper@example.com", "s3cret!", CancellationToken.None);

        result.Succeeded.Should().BeTrue();
        result.User.Should().Be(user);
    }

    [Fact]
    public async Task HandleAsync_fails_without_verifying_when_the_email_is_unknown()
    {
        var userRepository = new Mock<IUserRepository>();
        userRepository
            .Setup(r => r.FindByEmailAsync("nobody@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var passwordHasher = new Mock<IPasswordHasher>();

        var handler = new AuthenticateUserHandler(userRepository.Object, passwordHasher.Object);

        var result = await handler.HandleAsync("nobody@example.com", "s3cret!", CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.User.Should().BeNull();
        passwordHasher.Verify(h => h.Verify(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_fails_when_the_password_does_not_match()
    {
        var user = User.Create(Guid.NewGuid(), "shopper@example.com", "hashed-s3cret", "Ada Shopper", DateTimeOffset.UtcNow);

        var userRepository = new Mock<IUserRepository>();
        userRepository
            .Setup(r => r.FindByEmailAsync("shopper@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var passwordHasher = new Mock<IPasswordHasher>();
        passwordHasher.Setup(h => h.Verify("hashed-s3cret", "wrong-password")).Returns(false);

        var handler = new AuthenticateUserHandler(userRepository.Object, passwordHasher.Object);

        var result = await handler.HandleAsync("shopper@example.com", "wrong-password", CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.User.Should().BeNull();
    }

    [Fact]
    public void Constructor_rejects_a_null_user_repository()
    {
        var act = () => new AuthenticateUserHandler(null!, new Mock<IPasswordHasher>().Object);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_rejects_a_null_password_hasher()
    {
        var act = () => new AuthenticateUserHandler(new Mock<IUserRepository>().Object, null!);

        act.Should().Throw<ArgumentNullException>();
    }
}
