using EPharmacy.Domain;

namespace EPharmacy.Application;

public enum AddCartItemStatus
{
    Success,
    MedicineNotFound,
}

public sealed record AddCartItemResult(AddCartItemStatus Status, int ItemCount);

public sealed class AddCartItemHandler
{
    private readonly IMedicineRepository _medicineRepository;
    private readonly ICartRepository _cartRepository;

    public AddCartItemHandler(IMedicineRepository medicineRepository, ICartRepository cartRepository)
    {
        _medicineRepository = medicineRepository ?? throw new ArgumentNullException(nameof(medicineRepository));
        _cartRepository = cartRepository ?? throw new ArgumentNullException(nameof(cartRepository));
    }

    public async Task<AddCartItemResult> HandleAsync(Guid userId, Guid medicineId, int quantity, CancellationToken cancellationToken)
    {
        var medicine = await _medicineRepository.FindByIdAsync(medicineId, cancellationToken);
        if (medicine is null)
        {
            return new AddCartItemResult(AddCartItemStatus.MedicineNotFound, 0);
        }

        var cart = await _cartRepository.GetOrCreateForUserAsync(userId, cancellationToken);
        cart.AddItem(medicineId, quantity);
        await _cartRepository.SaveAsync(cart, cancellationToken);

        return new AddCartItemResult(AddCartItemStatus.Success, cart.TotalItemCount);
    }
}
