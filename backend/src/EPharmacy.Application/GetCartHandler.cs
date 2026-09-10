using EPharmacy.Domain;

namespace EPharmacy.Application;

public sealed record CartLineDto(Guid MedicineId, string Name, int PriceCents, int Quantity, int LineTotalCents);

public sealed record CartDto(IReadOnlyList<CartLineDto> Lines, int SubtotalCents, int DiscountCents, int TotalCents);

public sealed class GetCartHandler
{
    private readonly ICartRepository _cartRepository;
    private readonly IMedicineRepository _medicineRepository;
    private readonly IUserRepository _userRepository;

    public GetCartHandler(ICartRepository cartRepository, IMedicineRepository medicineRepository, IUserRepository userRepository)
    {
        _cartRepository = cartRepository ?? throw new ArgumentNullException(nameof(cartRepository));
        _medicineRepository = medicineRepository ?? throw new ArgumentNullException(nameof(medicineRepository));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    }

    public async Task<CartDto> HandleAsync(Guid userId, CancellationToken cancellationToken)
    {
        var cart = await _cartRepository.GetOrCreateForUserAsync(userId, cancellationToken);

        var lines = new List<CartLineDto>();
        foreach (var item in cart.Items)
        {
            var medicine = await _medicineRepository.FindByIdAsync(item.MedicineId, cancellationToken);
            if (medicine is null)
            {
                continue;
            }

            var lineTotalCents = medicine.PriceCents * item.Quantity;
            lines.Add(new CartLineDto(medicine.Id, medicine.Name, medicine.PriceCents, item.Quantity, lineTotalCents));
        }

        var subtotalCents = lines.Sum(line => line.LineTotalCents);

        // Mirror PlaceOrderHandler's per-unit member discount so what's shown here matches what's charged.
        var user = await _userRepository.FindByIdAsync(userId, cancellationToken);
        var totalCents = user?.IsMember == true
            ? lines.Sum(line => (line.PriceCents - line.PriceCents / 10) * line.Quantity)
            : subtotalCents;

        return new CartDto(lines, subtotalCents, subtotalCents - totalCents, totalCents);
    }
}
