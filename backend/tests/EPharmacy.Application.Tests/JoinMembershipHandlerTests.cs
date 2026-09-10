using EPharmacy.Domain;
using FluentAssertions;
using Moq;

namespace EPharmacy.Application.Tests;

public class JoinMembershipHandlerTests
{
    private static User CreateUser() =>
        User.Create(Guid.NewGuid(), "shopper@example.com", "hashed-password", "Ada Shopper", DateTimeOffset.UtcNow);

    [Fact]
    public async Task HandleAsync_charges_the_fee_and_marks_the_user_a_member_on_success()
    {
        var user = CreateUser();
        var userRepository = new Mock<IUserRepository>();
        userRepository.Setup(r => r.FindByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        var paymentGateway = new Mock<IPaymentGateway>();
        paymentGateway.Setup(p => p.ChargeAsync(
                It.Is<PaymentRequest>(r => r.AmountCents == JoinMembershipHandler.MembershipFeeCents),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaymentResult(true, null));
        var handler = new JoinMembershipHandler(userRepository.Object, paymentGateway.Object);

        var result = await handler.HandleAsync(user.Id, "4111111111111111", "12/30", "123", CancellationToken.None);

        result.Status.Should().Be(JoinMembershipStatus.Success);
        result.JoinedAtUtc.Should().NotBeNull();
        user.IsMember.Should().BeTrue();
        userRepository.Verify(r => r.SaveAsync(user, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_short_circuits_without_charging_when_already_a_member()
    {
        var user = CreateUser();
        user.JoinMembership(DateTimeOffset.UtcNow);
        var userRepository = new Mock<IUserRepository>();
        userRepository.Setup(r => r.FindByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        var paymentGateway = new Mock<IPaymentGateway>();
        var handler = new JoinMembershipHandler(userRepository.Object, paymentGateway.Object);

        var result = await handler.HandleAsync(user.Id, "4111111111111111", "12/30", "123", CancellationToken.None);

        result.Status.Should().Be(JoinMembershipStatus.AlreadyMember);
        paymentGateway.Verify(p => p.ChargeAsync(It.IsAny<PaymentRequest>(), It.IsAny<CancellationToken>()), Times.Never);
        userRepository.Verify(r => r.SaveAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_does_not_enroll_the_user_when_payment_is_declined()
    {
        var user = CreateUser();
        var userRepository = new Mock<IUserRepository>();
        userRepository.Setup(r => r.FindByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        var paymentGateway = new Mock<IPaymentGateway>();
        paymentGateway.Setup(p => p.ChargeAsync(It.IsAny<PaymentRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaymentResult(false, "The card was declined."));
        var handler = new JoinMembershipHandler(userRepository.Object, paymentGateway.Object);

        var result = await handler.HandleAsync(user.Id, "4000000000000002", "12/30", "123", CancellationToken.None);

        result.Status.Should().Be(JoinMembershipStatus.PaymentDeclined);
        user.IsMember.Should().BeFalse();
        userRepository.Verify(r => r.SaveAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
