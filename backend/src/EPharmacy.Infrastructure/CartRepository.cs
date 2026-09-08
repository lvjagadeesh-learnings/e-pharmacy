using EPharmacy.Application;
using EPharmacy.Domain;
using Microsoft.EntityFrameworkCore;

namespace EPharmacy.Infrastructure;

public sealed class CartRepository : ICartRepository
{
    private readonly AppDbContext _dbContext;

    public CartRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<Cart> GetOrCreateForUserAsync(Guid userId, CancellationToken cancellationToken)
    {
        var cart = await _dbContext.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);

        if (cart is not null)
        {
            return cart;
        }

        cart = Cart.CreateEmpty(Guid.NewGuid(), userId);
        _dbContext.Carts.Add(cart);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return cart;
    }

    public async Task SaveAsync(Cart cart, CancellationToken cancellationToken)
    {
        if (_dbContext.Entry(cart).State == EntityState.Detached)
        {
            _dbContext.Carts.Update(cart);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
