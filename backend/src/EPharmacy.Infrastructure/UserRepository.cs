using EPharmacy.Application;
using EPharmacy.Domain;
using Microsoft.EntityFrameworkCore;

namespace EPharmacy.Infrastructure;

public sealed class UserRepository : IUserRepository
{
    private readonly AppDbContext _dbContext;

    public UserRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken) =>
        _dbContext.Users.SingleOrDefaultAsync(u => u.Email == email, cancellationToken);

    public async Task AddAsync(User user, CancellationToken cancellationToken)
    {
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
