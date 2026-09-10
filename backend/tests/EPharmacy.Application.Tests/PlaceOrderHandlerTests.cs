using EPharmacy.Domain;
using FluentAssertions;
using Moq;

namespace EPharmacy.Application.Tests;

public class PlaceOrderHandlerTests
{
    private static readonly PaymentRequest AnyPaymentRequest = new(0, "4111111111111111", "12/30", "123");

    [Fact]
    public async Task HandleAsync_fails_fast_for_an_empty_cart_without_calling_the_payment_gateway()
    {
        var userId = Guid.NewGuid();
        var cart = Cart.CreateEmpty(Guid.NewGuid(), userId);

        var cartRepository = new Mock<ICartRepository>();
        cartRepository.Setup(r => r.GetOrCreateForUserAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(cart);
        var medicineRepository = new Mock<IMedicineRepository>();
        var paymentGateway = new Mock<IPaymentGateway>();
        var orderRepository = new Mock<IOrderRepository>();
        var userRepository = new Mock<IUserRepository>();
        var handler = new PlaceOrderHandler(cartRepository.Object, medicineRepository.Object, paymentGateway.Object, orderRepository.Object, userRepository.Object);

        var result = await handler.HandleAsync(userId, "1 Example St", "4111111111111111", "12/30", "123", CancellationToken.None);

        result.Status.Should().Be(PlaceOrderStatus.EmptyCart);
        paymentGateway.Verify(p => p.ChargeAsync(It.IsAny<PaymentRequest>(), It.IsAny<CancellationToken>()), Times.Never);
        orderRepository.Verify(r => r.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_fails_fast_for_a_stale_cart_whose_items_all_reference_deleted_medicines()
    {
        var userId = Guid.NewGuid();
        var medicineId = Guid.NewGuid();
        var cart = Cart.CreateEmpty(Guid.NewGuid(), userId);
        cart.AddItem(medicineId, 2);

        var cartRepository = new Mock<ICartRepository>();
        cartRepository.Setup(r => r.GetOrCreateForUserAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(cart);
        var medicineRepository = new Mock<IMedicineRepository>();
        medicineRepository.Setup(r => r.FindByIdAsync(medicineId, It.IsAny<CancellationToken>())).ReturnsAsync((Medicine?)null);
        var paymentGateway = new Mock<IPaymentGateway>();
        var orderRepository = new Mock<IOrderRepository>();
        var userRepository = new Mock<IUserRepository>();
        var handler = new PlaceOrderHandler(cartRepository.Object, medicineRepository.Object, paymentGateway.Object, orderRepository.Object, userRepository.Object);

        var result = await handler.HandleAsync(userId, "1 Example St", "4111111111111111", "12/30", "123", CancellationToken.None);

        result.Status.Should().Be(PlaceOrderStatus.EmptyCart);
        paymentGateway.Verify(p => p.ChargeAsync(It.IsAny<PaymentRequest>(), It.IsAny<CancellationToken>()), Times.Never);
        orderRepository.Verify(r => r.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_returns_a_failure_result_and_does_not_create_an_order_or_clear_the_cart_when_the_payment_is_declined()
    {
        var userId = Guid.NewGuid();
        var medicineId = Guid.NewGuid();
        var medicine = Medicine.Create(medicineId, "Paracetamol 500mg", "Pain and fever relief tablets.", 599, null);
        var cart = Cart.CreateEmpty(Guid.NewGuid(), userId);
        cart.AddItem(medicineId, 2);

        var cartRepository = new Mock<ICartRepository>();
        cartRepository.Setup(r => r.GetOrCreateForUserAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(cart);
        var medicineRepository = new Mock<IMedicineRepository>();
        medicineRepository.Setup(r => r.FindByIdAsync(medicineId, It.IsAny<CancellationToken>())).ReturnsAsync(medicine);
        var paymentGateway = new Mock<IPaymentGateway>();
        paymentGateway.Setup(p => p.ChargeAsync(It.IsAny<PaymentRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaymentResult(false, "Card declined."));
        var orderRepository = new Mock<IOrderRepository>();
        var userRepository = new Mock<IUserRepository>();
        var handler = new PlaceOrderHandler(cartRepository.Object, medicineRepository.Object, paymentGateway.Object, orderRepository.Object, userRepository.Object);

        var result = await handler.HandleAsync(userId, "1 Example St", "4000000000000002", "12/30", "123", CancellationToken.None);

        result.Status.Should().Be(PlaceOrderStatus.PaymentDeclined);
        orderRepository.Verify(r => r.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Never);
        cartRepository.Verify(r => r.SaveAsync(It.IsAny<Cart>(), It.IsAny<CancellationToken>()), Times.Never);
        cart.Items.Should().ContainSingle(item => item.MedicineId == medicineId && item.Quantity == 2);
    }

    [Fact]
    public async Task HandleAsync_creates_the_order_with_correct_snapshot_totals_and_clears_the_cart_when_the_payment_succeeds()
    {
        var userId = Guid.NewGuid();
        var medicineId = Guid.NewGuid();
        var medicine = Medicine.Create(medicineId, "Paracetamol 500mg", "Pain and fever relief tablets.", 599, null);
        var cart = Cart.CreateEmpty(Guid.NewGuid(), userId);
        cart.AddItem(medicineId, 2);

        var cartRepository = new Mock<ICartRepository>();
        cartRepository.Setup(r => r.GetOrCreateForUserAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(cart);
        var medicineRepository = new Mock<IMedicineRepository>();
        medicineRepository.Setup(r => r.FindByIdAsync(medicineId, It.IsAny<CancellationToken>())).ReturnsAsync(medicine);
        var paymentGateway = new Mock<IPaymentGateway>();
        paymentGateway.Setup(p => p.ChargeAsync(It.IsAny<PaymentRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaymentResult(true, null));
        Order? capturedOrder = null;
        var orderRepository = new Mock<IOrderRepository>();
        orderRepository.Setup(r => r.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .Callback<Order, CancellationToken>((order, _) => capturedOrder = order)
            .Returns(Task.CompletedTask);
        var userRepository = new Mock<IUserRepository>();
        var handler = new PlaceOrderHandler(cartRepository.Object, medicineRepository.Object, paymentGateway.Object, orderRepository.Object, userRepository.Object);

        var result = await handler.HandleAsync(userId, "1 Example St", "4111111111111111", "12/30", "123", CancellationToken.None);

        result.Status.Should().Be(PlaceOrderStatus.Success);
        result.TotalCents.Should().Be(1198);
        capturedOrder.Should().NotBeNull();
        capturedOrder!.TotalCents.Should().Be(1198);
        capturedOrder.Items.Should().ContainSingle(item => item.MedicineId == medicineId && item.Quantity == 2 && item.UnitPriceCents == 599);
        cart.Items.Should().BeEmpty();
        cartRepository.Verify(r => r.SaveAsync(cart, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_applies_a_10_percent_discount_for_members()
    {
        var userId = Guid.NewGuid();
        var medicineId = Guid.NewGuid();
        var medicine = Medicine.Create(medicineId, "Paracetamol 500mg", "Pain and fever relief tablets.", 599, null);
        var cart = Cart.CreateEmpty(Guid.NewGuid(), userId);
        cart.AddItem(medicineId, 2);
        var member = User.Create(userId, "member@example.com", "hashed-password", "Ada Member", DateTimeOffset.UtcNow);
        member.JoinMembership(DateTimeOffset.UtcNow);

        var cartRepository = new Mock<ICartRepository>();
        cartRepository.Setup(r => r.GetOrCreateForUserAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(cart);
        var medicineRepository = new Mock<IMedicineRepository>();
        medicineRepository.Setup(r => r.FindByIdAsync(medicineId, It.IsAny<CancellationToken>())).ReturnsAsync(medicine);
        PaymentRequest? capturedPaymentRequest = null;
        var paymentGateway = new Mock<IPaymentGateway>();
        paymentGateway.Setup(p => p.ChargeAsync(It.IsAny<PaymentRequest>(), It.IsAny<CancellationToken>()))
            .Callback<PaymentRequest, CancellationToken>((request, _) => capturedPaymentRequest = request)
            .ReturnsAsync(new PaymentResult(true, null));
        var orderRepository = new Mock<IOrderRepository>();
        var userRepository = new Mock<IUserRepository>();
        userRepository.Setup(r => r.FindByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(member);
        var handler = new PlaceOrderHandler(cartRepository.Object, medicineRepository.Object, paymentGateway.Object, orderRepository.Object, userRepository.Object);

        var result = await handler.HandleAsync(userId, "1 Example St", "4111111111111111", "12/30", "123", CancellationToken.None);

        // 2 x 599 = 1198 at full price; 10% off each unit (599 - 59 = 540) => 1080.
        result.Status.Should().Be(PlaceOrderStatus.Success);
        result.TotalCents.Should().Be(1080);
        capturedPaymentRequest!.AmountCents.Should().Be(1080);
    }
}
