using EPharmacy.Domain;
using FluentAssertions;
using Moq;

namespace EPharmacy.Application.Tests;

public class UpdateCartItemHandlerTests
{
    [Fact]
    public async Task HandleAsync_with_a_positive_quantity_updates_the_line_and_saves_the_cart()
    {
        var userId = Guid.NewGuid();
        var medicineId = Guid.NewGuid();
        var cart = Cart.CreateEmpty(Guid.NewGuid(), userId);
        cart.AddItem(medicineId, 1);

        var cartRepository = new Mock<ICartRepository>();
        cartRepository.Setup(r => r.GetOrCreateForUserAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(cart);
        var handler = new UpdateCartItemHandler(cartRepository.Object);

        await handler.HandleAsync(userId, medicineId, 5, CancellationToken.None);

        cart.Items.Should().ContainSingle(item => item.MedicineId == medicineId && item.Quantity == 5);
        cartRepository.Verify(r => r.SaveAsync(cart, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_with_a_non_positive_quantity_removes_the_line_instead_of_updating_it()
    {
        var userId = Guid.NewGuid();
        var medicineId = Guid.NewGuid();
        var cart = Cart.CreateEmpty(Guid.NewGuid(), userId);
        cart.AddItem(medicineId, 1);

        var cartRepository = new Mock<ICartRepository>();
        cartRepository.Setup(r => r.GetOrCreateForUserAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(cart);
        var handler = new UpdateCartItemHandler(cartRepository.Object);

        await handler.HandleAsync(userId, medicineId, 0, CancellationToken.None);

        cart.Items.Should().BeEmpty();
        cartRepository.Verify(r => r.SaveAsync(cart, It.IsAny<CancellationToken>()), Times.Once);
    }
}

public class RemoveCartItemHandlerTests
{
    [Fact]
    public async Task HandleAsync_removes_the_line_and_saves_the_cart()
    {
        var userId = Guid.NewGuid();
        var medicineId = Guid.NewGuid();
        var cart = Cart.CreateEmpty(Guid.NewGuid(), userId);
        cart.AddItem(medicineId, 1);

        var cartRepository = new Mock<ICartRepository>();
        cartRepository.Setup(r => r.GetOrCreateForUserAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(cart);
        var handler = new RemoveCartItemHandler(cartRepository.Object);

        await handler.HandleAsync(userId, medicineId, CancellationToken.None);

        cart.Items.Should().BeEmpty();
        cartRepository.Verify(r => r.SaveAsync(cart, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_is_safe_to_call_for_a_medicine_not_in_the_cart()
    {
        var userId = Guid.NewGuid();
        var cart = Cart.CreateEmpty(Guid.NewGuid(), userId);

        var cartRepository = new Mock<ICartRepository>();
        cartRepository.Setup(r => r.GetOrCreateForUserAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(cart);
        var handler = new RemoveCartItemHandler(cartRepository.Object);

        var act = async () => await handler.HandleAsync(userId, Guid.NewGuid(), CancellationToken.None);

        await act.Should().NotThrowAsync();
    }
}
