using EPharmacy.Domain;
using FluentAssertions;
using Moq;

namespace EPharmacy.Application.Tests;

public class RegisterUserHandlerTests
{
    [Fact]
    public async Task HandleAsync_hashes_the_password_and_persists_a_new_user()
    {
        var userRepository = new Mock<IUserRepository>();
        userRepository
            .Setup(r => r.FindByEmailAsync("shopper@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var passwordHasher = new Mock<IPasswordHasher>();
        passwordHasher.Setup(h => h.Hash("s3cret!")).Returns("hashed-s3cret");

        var handler = new RegisterUserHandler(userRepository.Object, passwordHasher.Object);

        var result = await handler.HandleAsync("shopper@example.com", "s3cret!", "Ada Shopper", CancellationToken.None);

        result.Succeeded.Should().BeTrue();
        result.User.Should().NotBeNull();
        result.User!.Email.Should().Be("shopper@example.com");
        result.User.PasswordHash.Should().Be("hashed-s3cret");
        result.User.DisplayName.Should().Be("Ada Shopper");
        userRepository.Verify(r => r.AddAsync(result.User, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_fails_when_the_email_is_already_registered()
    {
        var existingUser = User.Create(Guid.NewGuid(), "shopper@example.com", "hash", "Existing Shopper", DateTimeOffset.UtcNow);

        var userRepository = new Mock<IUserRepository>();
        userRepository
            .Setup(r => r.FindByEmailAsync("shopper@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        var passwordHasher = new Mock<IPasswordHasher>();

        var handler = new RegisterUserHandler(userRepository.Object, passwordHasher.Object);

        var result = await handler.HandleAsync("shopper@example.com", "s3cret!", "Ada Shopper", CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.Error.Should().NotBeNullOrEmpty();
        userRepository.Verify(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public void Constructor_rejects_a_null_user_repository()
    {
        var act = () => new RegisterUserHandler(null!, new Mock<IPasswordHasher>().Object);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_rejects_a_null_password_hasher()
    {
        var act = () => new RegisterUserHandler(new Mock<IUserRepository>().Object, null!);

        act.Should().Throw<ArgumentNullException>();
    }
}
