using EPharmacy.Domain;

namespace EPharmacy.Application;

public sealed class RegisterUserHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;

    public RegisterUserHandler(IUserRepository userRepository, IPasswordHasher passwordHasher)
    {
        ArgumentNullException.ThrowIfNull(userRepository);
        ArgumentNullException.ThrowIfNull(passwordHasher);

        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<RegisterUserResult> HandleAsync(string email, string password, string displayName, CancellationToken cancellationToken)
    {
        var existing = await _userRepository.FindByEmailAsync(email, cancellationToken);
        if (existing is not null)
        {
            return RegisterUserResult.Failure("An account with this email already exists.");
        }

        var passwordHash = _passwordHasher.Hash(password);
        var user = User.Create(Guid.NewGuid(), email, passwordHash, displayName, DateTimeOffset.UtcNow);

        await _userRepository.AddAsync(user, cancellationToken);

        return RegisterUserResult.Success(user);
    }
}
