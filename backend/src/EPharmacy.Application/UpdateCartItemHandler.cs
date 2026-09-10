namespace EPharmacy.Application;

public sealed class UpdateCartItemHandler
{
    private readonly ICartRepository _cartRepository;

    public UpdateCartItemHandler(ICartRepository cartRepository)
    {
        _cartRepository = cartRepository ?? throw new ArgumentNullException(nameof(cartRepository));
    }

    public async Task HandleAsync(Guid userId, Guid medicineId, int quantity, CancellationToken cancellationToken)
    {
        var cart = await _cartRepository.GetOrCreateForUserAsync(userId, cancellationToken);

        if (quantity <= 0)
        {
            cart.RemoveItem(medicineId);
        }
        else
        {
            cart.UpdateItemQuantity(medicineId, quantity);
        }

        await _cartRepository.SaveAsync(cart, cancellationToken);
    }
}
