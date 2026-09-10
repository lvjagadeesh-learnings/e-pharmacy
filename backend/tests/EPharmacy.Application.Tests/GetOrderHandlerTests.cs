using EPharmacy.Domain;
using FluentAssertions;
using Moq;

namespace EPharmacy.Application.Tests;

public class GetOrderHandlerTests
{
    private static readonly OrderItem SampleItem = new(Guid.NewGuid(), "Paracetamol 500mg", 599, 2);

    [Fact]
    public async Task HandleAsync_returns_null_when_the_order_does_not_exist()
    {
        var orderRepository = new Mock<IOrderRepository>();
        orderRepository.Setup(r => r.FindByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Order?)null);
        var handler = new GetOrderHandler(orderRepository.Object);

        var result = await handler.HandleAsync(Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task HandleAsync_returns_null_when_the_order_is_not_owned_by_the_caller()
    {
        var owner = Guid.NewGuid();
        var order = Order.Create(Guid.NewGuid(), owner, "1 Example St", new[] { SampleItem }, DateTimeOffset.UtcNow);
        var orderRepository = new Mock<IOrderRepository>();
        orderRepository.Setup(r => r.FindByIdAsync(order.Id, It.IsAny<CancellationToken>())).ReturnsAsync(order);
        var handler = new GetOrderHandler(orderRepository.Object);

        var result = await handler.HandleAsync(Guid.NewGuid(), order.Id, CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task HandleAsync_returns_computed_status_and_history_for_the_owners_order()
    {
        var userId = Guid.NewGuid();
        var placedAtUtc = DateTimeOffset.UtcNow.AddSeconds(-100);
        var order = Order.Create(Guid.NewGuid(), userId, "1 Example St", new[] { SampleItem }, placedAtUtc);
        var orderRepository = new Mock<IOrderRepository>();
        orderRepository.Setup(r => r.FindByIdAsync(order.Id, It.IsAny<CancellationToken>())).ReturnsAsync(order);
        var handler = new GetOrderHandler(orderRepository.Object);

        var result = await handler.HandleAsync(userId, order.Id, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Status.Should().Be(OrderStatus.Shipped);
        result.StatusHistory.Should().HaveCount(3);
        result.ReceivedAtUtc.Should().BeNull();
        result.TotalCents.Should().Be(order.TotalCents);
    }
}
