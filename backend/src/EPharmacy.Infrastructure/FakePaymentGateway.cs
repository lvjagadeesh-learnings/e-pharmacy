using EPharmacy.Application;

namespace EPharmacy.Infrastructure;

/// <summary>
/// Simulated payment processor. Follows the same "test card" convention real sandbox
/// payment gateways (e.g. Stripe/PayPal) use: every card number succeeds except one
/// fixed, documented "always declines" value. No card data is ever persisted.
/// </summary>
public sealed class FakePaymentGateway : IPaymentGateway
{
    public const string AlwaysDeclinesCardNumber = "4000000000000002";

    public Task<PaymentResult> ChargeAsync(PaymentRequest request, CancellationToken cancellationToken)
    {
        if (request.CardNumber == AlwaysDeclinesCardNumber)
        {
            return Task.FromResult(new PaymentResult(false, "The card was declined."));
        }

        return Task.FromResult(new PaymentResult(true, null));
    }
}
