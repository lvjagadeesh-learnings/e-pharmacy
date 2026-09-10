namespace EPharmacy.Application;

public sealed class RemoveCartItemHandler
{
    private readonly ICartRepository _cartRepository;

    public RemoveCartItemHandler(ICartRepository cartRepository)
    {
        _cartRepository = cartRepository ?? throw new ArgumentNullException(nameof(cartRepository));
    }

    public async Task HandleAsync(Guid userId, Guid medicineId, CancellationToken cancellationToken)
    {
        var cart = await _cartRepository.GetOrCreateForUserAsync(userId, cancellationToken);
        cart.RemoveItem(medicineId);
        await _cartRepository.SaveAsync(cart, cancellationToken);
    }
}
