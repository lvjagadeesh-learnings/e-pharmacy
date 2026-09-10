using EPharmacy.Domain;
using FluentAssertions;
using Moq;

namespace EPharmacy.Application.Tests;

public class AddCartItemHandlerTests
{
    [Fact]
    public async Task HandleAsync_returns_not_found_and_does_not_touch_the_cart_when_the_medicine_does_not_exist()
    {
        var medicineRepository = new Mock<IMedicineRepository>();
        medicineRepository.Setup(r => r.FindByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Medicine?)null);
        var cartRepository = new Mock<ICartRepository>();
        var handler = new AddCartItemHandler(medicineRepository.Object, cartRepository.Object);

        var result = await handler.HandleAsync(Guid.NewGuid(), Guid.NewGuid(), 1, CancellationToken.None);

        result.Status.Should().Be(AddCartItemStatus.MedicineNotFound);
        cartRepository.Verify(r => r.GetOrCreateForUserAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        cartRepository.Verify(r => r.SaveAsync(It.IsAny<Cart>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_adds_the_medicine_to_the_cart_and_saves_it_when_the_medicine_exists()
    {
        var medicineId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var medicine = Medicine.Create(medicineId, "Paracetamol 500mg", "Pain and fever relief tablets.", 599, null);
        var cart = Cart.CreateEmpty(Guid.NewGuid(), userId);

        var medicineRepository = new Mock<IMedicineRepository>();
        medicineRepository.Setup(r => r.FindByIdAsync(medicineId, It.IsAny<CancellationToken>())).ReturnsAsync(medicine);
        var cartRepository = new Mock<ICartRepository>();
        cartRepository.Setup(r => r.GetOrCreateForUserAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(cart);
        var handler = new AddCartItemHandler(medicineRepository.Object, cartRepository.Object);

        var result = await handler.HandleAsync(userId, medicineId, 2, CancellationToken.None);

        result.Status.Should().Be(AddCartItemStatus.Success);
        result.ItemCount.Should().Be(2);
        cart.Items.Should().ContainSingle(item => item.MedicineId == medicineId && item.Quantity == 2);
        cartRepository.Verify(r => r.SaveAsync(cart, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_increments_the_quantity_when_the_medicine_is_already_in_the_cart()
    {
        var medicineId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var medicine = Medicine.Create(medicineId, "Paracetamol 500mg", "Pain and fever relief tablets.", 599, null);
        var cart = Cart.CreateEmpty(Guid.NewGuid(), userId);
        cart.AddItem(medicineId, 1);

        var medicineRepository = new Mock<IMedicineRepository>();
        medicineRepository.Setup(r => r.FindByIdAsync(medicineId, It.IsAny<CancellationToken>())).ReturnsAsync(medicine);
        var cartRepository = new Mock<ICartRepository>();
        cartRepository.Setup(r => r.GetOrCreateForUserAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(cart);
        var handler = new AddCartItemHandler(medicineRepository.Object, cartRepository.Object);

        var result = await handler.HandleAsync(userId, medicineId, 1, CancellationToken.None);

        result.ItemCount.Should().Be(2);
        cart.Items.Should().ContainSingle(item => item.MedicineId == medicineId && item.Quantity == 2);
    }

    [Fact]
    public void Constructor_rejects_a_null_medicine_repository()
    {
        var act = () => new AddCartItemHandler(null!, new Mock<ICartRepository>().Object);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_rejects_a_null_cart_repository()
    {
        var act = () => new AddCartItemHandler(new Mock<IMedicineRepository>().Object, null!);

        act.Should().Throw<ArgumentNullException>();
    }
}
