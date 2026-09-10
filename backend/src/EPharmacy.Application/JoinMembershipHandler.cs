namespace EPharmacy.Application;

public enum JoinMembershipStatus
{
    Success,
    AlreadyMember,
    PaymentDeclined,
    UserNotFound,
}

public sealed record JoinMembershipResult(JoinMembershipStatus Status, DateTimeOffset? JoinedAtUtc, string? FailureReason);

/// <summary>
/// Enrolls a user in the single "e-Pharmacy Plus" membership tier, via a simulated
/// one-time charge on the same fake payment gateway checkout uses.
/// </summary>
public sealed class JoinMembershipHandler
{
    public const int MembershipFeeCents = 499;

    private readonly IUserRepository _userRepository;
    private readonly IPaymentGateway _paymentGateway;

    public JoinMembershipHandler(IUserRepository userRepository, IPaymentGateway paymentGateway)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _paymentGateway = paymentGateway ?? throw new ArgumentNullException(nameof(paymentGateway));
    }

    public async Task<JoinMembershipResult> HandleAsync(
        Guid userId,
        string cardNumber,
        string expiry,
        string cvc,
        CancellationToken cancellationToken)
    {
        var user = await _userRepository.FindByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            return new JoinMembershipResult(JoinMembershipStatus.UserNotFound, null, null);
        }

        if (user.IsMember)
        {
            return new JoinMembershipResult(JoinMembershipStatus.AlreadyMember, user.MembershipJoinedAtUtc, null);
        }

        var paymentResult = await _paymentGateway.ChargeAsync(
            new PaymentRequest(MembershipFeeCents, cardNumber, expiry, cvc),
            cancellationToken);

        if (!paymentResult.Succeeded)
        {
            return new JoinMembershipResult(JoinMembershipStatus.PaymentDeclined, null, paymentResult.FailureReason);
        }

        var joinedAtUtc = DateTimeOffset.UtcNow;
        user.JoinMembership(joinedAtUtc);
        await _userRepository.SaveAsync(user, cancellationToken);

        return new JoinMembershipResult(JoinMembershipStatus.Success, joinedAtUtc, null);
    }
}
