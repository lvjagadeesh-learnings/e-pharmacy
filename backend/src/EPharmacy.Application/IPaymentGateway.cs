namespace EPharmacy.Application;

public sealed record PaymentRequest(int AmountCents, string CardNumber, string Expiry, string Cvc);

public sealed record PaymentResult(bool Succeeded, string? FailureReason);

public interface IPaymentGateway
{
    Task<PaymentResult> ChargeAsync(PaymentRequest request, CancellationToken cancellationToken);
}
