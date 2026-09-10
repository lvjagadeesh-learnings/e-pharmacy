using EPharmacy.Domain;
using FluentAssertions;
using Moq;

namespace EPharmacy.Application.Tests;

public class GetCartHandlerTests
{
    [Fact]
    public async Task HandleAsync_returns_an_empty_lines_array_and_zero_subtotal_for_an_empty_cart()
    {
        var userId = Guid.NewGuid();
        var cart = Cart.CreateEmpty(Guid.NewGuid(), userId);

        var cartRepository = new Mock<ICartRepository>();
        cartRepository.Setup(r => r.GetOrCreateForUserAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(cart);
        var medicineRepository = new Mock<IMedicineRepository>();
        var userRepository = new Mock<IUserRepository>();
        var handler = new GetCartHandler(cartRepository.Object, medicineRepository.Object, userRepository.Object);

        var result = await handler.HandleAsync(userId, CancellationToken.None);

        result.Lines.Should().BeEmpty();
        result.SubtotalCents.Should().Be(0);
        result.DiscountCents.Should().Be(0);
        result.TotalCents.Should().Be(0);
    }

    [Fact]
    public async Task HandleAsync_builds_a_line_per_cart_item_joined_with_medicine_data_and_sums_the_subtotal()
    {
        var userId = Guid.NewGuid();
        var firstMedicineId = Guid.NewGuid();
        var secondMedicineId = Guid.NewGuid();
        var firstMedicine = Medicine.Create(firstMedicineId, "Paracetamol 500mg", "Pain and fever relief tablets.", 599, null);
        var secondMedicine = Medicine.Create(secondMedicineId, "Ibuprofen 200mg", "Anti-inflammatory tablets.", 799, null);
        var cart = Cart.CreateEmpty(Guid.NewGuid(), userId);
        cart.AddItem(firstMedicineId, 2);
        cart.AddItem(secondMedicineId, 1);

        var cartRepository = new Mock<ICartRepository>();
        cartRepository.Setup(r => r.GetOrCreateForUserAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(cart);
        var medicineRepository = new Mock<IMedicineRepository>();
        medicineRepository.Setup(r => r.FindByIdAsync(firstMedicineId, It.IsAny<CancellationToken>())).ReturnsAsync(firstMedicine);
        medicineRepository.Setup(r => r.FindByIdAsync(secondMedicineId, It.IsAny<CancellationToken>())).ReturnsAsync(secondMedicine);
        var userRepository = new Mock<IUserRepository>();
        var handler = new GetCartHandler(cartRepository.Object, medicineRepository.Object, userRepository.Object);

        var result = await handler.HandleAsync(userId, CancellationToken.None);

        result.Lines.Should().HaveCount(2);
        result.Lines.Should().ContainSingle(line =>
            line.MedicineId == firstMedicineId &&
            line.Name == "Paracetamol 500mg" &&
            line.PriceCents == 599 &&
            line.Quantity == 2 &&
            line.LineTotalCents == 1198);
        result.Lines.Should().ContainSingle(line =>
            line.MedicineId == secondMedicineId &&
            line.Name == "Ibuprofen 200mg" &&
            line.PriceCents == 799 &&
            line.Quantity == 1 &&
            line.LineTotalCents == 799);
        result.SubtotalCents.Should().Be(1997);
        result.DiscountCents.Should().Be(0);
        result.TotalCents.Should().Be(1997);
    }

    [Fact]
    public async Task HandleAsync_applies_the_same_per_unit_member_discount_as_checkout()
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
        var userRepository = new Mock<IUserRepository>();
        userRepository.Setup(r => r.FindByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(member);
        var handler = new GetCartHandler(cartRepository.Object, medicineRepository.Object, userRepository.Object);

        var result = await handler.HandleAsync(userId, CancellationToken.None);

        // 2 x 599 = 1198 at full price; 10% off each unit (599 - 59 = 540) => 1080, matching PlaceOrderHandler.
        result.SubtotalCents.Should().Be(1198);
        result.TotalCents.Should().Be(1080);
        result.DiscountCents.Should().Be(118);
    }
}
