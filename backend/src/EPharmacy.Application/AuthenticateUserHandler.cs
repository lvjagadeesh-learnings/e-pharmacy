using EPharmacy.Domain;

namespace EPharmacy.Application;

public sealed record AuthenticateUserCommand(string Email, string Password);

public sealed record AuthenticateResult(bool Succeeded, User? User)
{
    public static AuthenticateResult Success(User user) => new(true, user);

    public static AuthenticateResult Failure() => new(false, null);
}

public sealed class AuthenticateUserHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;

    public AuthenticateUserHandler(IUserRepository userRepository, IPasswordHasher passwordHasher)
    {
        ArgumentNullException.ThrowIfNull(userRepository);
        ArgumentNullException.ThrowIfNull(passwordHasher);

        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<AuthenticateResult> HandleAsync(string email, string password, CancellationToken cancellationToken)
    {
        var user = await _userRepository.FindByEmailAsync(email, cancellationToken);
        if (user is null)
        {
            return AuthenticateResult.Failure();
        }

        var verified = _passwordHasher.Verify(user.PasswordHash, password);
        if (!verified)
        {
            return AuthenticateResult.Failure();
        }

        return AuthenticateResult.Success(user);
    }
}
