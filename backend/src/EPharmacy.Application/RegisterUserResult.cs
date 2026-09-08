namespace EPharmacy.Application;

public sealed record RegisterUserResult(bool Succeeded, EPharmacy.Domain.User? User, string? Error)
{
    public static RegisterUserResult Success(EPharmacy.Domain.User user) => new(true, user, null);

    public static RegisterUserResult Failure(string error) => new(false, null, error);
}
