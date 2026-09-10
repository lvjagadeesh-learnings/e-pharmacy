using EPharmacy.Domain;

namespace EPharmacy.Application;

public enum PlaceOrderStatus
{
    Success,
    EmptyCart,
    PaymentDeclined,
}

public sealed record PlaceOrderResult(PlaceOrderStatus Status, Guid? OrderId, int TotalCents, string? FailureReason);

public sealed class PlaceOrderHandler
{
    private readonly ICartRepository _cartRepository;
    private readonly IMedicineRepository _medicineRepository;
    private readonly IPaymentGateway _paymentGateway;
    private readonly IOrderRepository _orderRepository;
    private readonly IUserRepository _userRepository;

    public PlaceOrderHandler(
        ICartRepository cartRepository,
        IMedicineRepository medicineRepository,
        IPaymentGateway paymentGateway,
        IOrderRepository orderRepository,
        IUserRepository userRepository)
    {
        _cartRepository = cartRepository ?? throw new ArgumentNullException(nameof(cartRepository));
        _medicineRepository = medicineRepository ?? throw new ArgumentNullException(nameof(medicineRepository));
        _paymentGateway = paymentGateway ?? throw new ArgumentNullException(nameof(paymentGateway));
        _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    }

    public async Task<PlaceOrderResult> HandleAsync(
        Guid userId,
        string shippingAddress,
        string cardNumber,
        string expiry,
        string cvc,
        CancellationToken cancellationToken)
    {
        var cart = await _cartRepository.GetOrCreateForUserAsync(userId, cancellationToken);
        if (cart.Items.Count == 0)
        {
            return new PlaceOrderResult(PlaceOrderStatus.EmptyCart, null, 0, null);
        }

        var orderItems = new List<OrderItem>();
        foreach (var item in cart.Items)
        {
            var medicine = await _medicineRepository.FindByIdAsync(item.MedicineId, cancellationToken);
            if (medicine is null)
            {
                continue;
            }

            orderItems.Add(new OrderItem(medicine.Id, medicine.Name, medicine.PriceCents, item.Quantity));
        }

        // Every cart item resolved to a since-deleted medicine (a stale cart) — treat the same as
        // an empty cart instead of charging 0 and letting Order.Create reject an item-less order.
        if (orderItems.Count == 0)
        {
            return new PlaceOrderResult(PlaceOrderStatus.EmptyCart, null, 0, null);
        }

        // e-Pharmacy Plus members get 10% off every line item (see story 12), applied to the
        // snapshotted unit price so it flows through to both the charge and the stored order.
        var user = await _userRepository.FindByIdAsync(userId, cancellationToken);
        if (user?.IsMember == true)
        {
            orderItems = orderItems
                .Select(item => item with { UnitPriceCents = item.UnitPriceCents - item.UnitPriceCents / 10 })
                .ToList();
        }

        var totalCents = orderItems.Sum(item => item.UnitPriceCents * item.Quantity);

        var paymentResult = await _paymentGateway.ChargeAsync(
            new PaymentRequest(totalCents, cardNumber, expiry, cvc),
            cancellationToken);

        if (!paymentResult.Succeeded)
        {
            return new PlaceOrderResult(PlaceOrderStatus.PaymentDeclined, null, 0, paymentResult.FailureReason);
        }

        var order = Order.Create(Guid.NewGuid(), userId, shippingAddress, orderItems, DateTimeOffset.UtcNow);
        await _orderRepository.AddAsync(order, cancellationToken);

        cart.Clear();
        await _cartRepository.SaveAsync(cart, cancellationToken);

        return new PlaceOrderResult(PlaceOrderStatus.Success, order.Id, order.TotalCents, null);
    }
}
