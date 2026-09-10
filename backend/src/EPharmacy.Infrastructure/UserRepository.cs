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

    public Task<User?> FindByIdAsync(Guid id, CancellationToken cancellationToken) =>
        _dbContext.Users.SingleOrDefaultAsync(u => u.Id == id, cancellationToken);

    public async Task AddAsync(User user, CancellationToken cancellationToken)
    {
        _dbContext.Users.Add(user);

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            // The unique index on Email caught a registration that raced past the caller's
            // pre-check; surface it as a typed conflict instead of an unhandled 500.
            throw new DuplicateEmailException();
        }
    }

    public async Task SaveAsync(User user, CancellationToken cancellationToken)
    {
        if (_dbContext.Entry(user).State == EntityState.Detached)
        {
            _dbContext.Users.Update(user);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
