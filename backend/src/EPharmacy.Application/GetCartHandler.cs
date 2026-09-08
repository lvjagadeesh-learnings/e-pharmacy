using EPharmacy.Domain;

namespace EPharmacy.Application;

public sealed record CartLineDto(Guid MedicineId, string Name, int PriceCents, int Quantity, int LineTotalCents);

public sealed record CartDto(IReadOnlyList<CartLineDto> Lines, int SubtotalCents);

public sealed class GetCartHandler
{
    private readonly ICartRepository _cartRepository;
    private readonly IMedicineRepository _medicineRepository;

    public GetCartHandler(ICartRepository cartRepository, IMedicineRepository medicineRepository)
    {
        _cartRepository = cartRepository ?? throw new ArgumentNullException(nameof(cartRepository));
        _medicineRepository = medicineRepository ?? throw new ArgumentNullException(nameof(medicineRepository));
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
        return new CartDto(lines, subtotalCents);
    }
}
