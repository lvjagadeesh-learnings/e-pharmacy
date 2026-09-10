using EPharmacy.Domain;
using FluentAssertions;
using Moq;

namespace EPharmacy.Application.Tests;

public class MarkOrderReceivedHandlerTests
{
    private static readonly OrderItem SampleItem = new(Guid.NewGuid(), "Paracetamol 500mg", 599, 2);

    [Fact]
    public async Task HandleAsync_fails_when_the_order_does_not_exist()
    {
        var orderRepository = new Mock<IOrderRepository>();
        orderRepository.Setup(r => r.FindByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Order?)null);
        var handler = new MarkOrderReceivedHandler(orderRepository.Object);

        var result = await handler.HandleAsync(Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);

        result.Status.Should().Be(MarkOrderReceivedStatus.NotFound);
    }

    [Fact]
    public async Task HandleAsync_fails_when_the_order_is_not_owned_by_the_caller()
    {
        var owner = Guid.NewGuid();
        var order = Order.Create(Guid.NewGuid(), owner, "1 Example St", new[] { SampleItem }, DateTimeOffset.UtcNow.AddSeconds(-200));
        var orderRepository = new Mock<IOrderRepository>();
        orderRepository.Setup(r => r.FindByIdAsync(order.Id, It.IsAny<CancellationToken>())).ReturnsAsync(order);
        var handler = new MarkOrderReceivedHandler(orderRepository.Object);

        var result = await handler.HandleAsync(Guid.NewGuid(), order.Id, CancellationToken.None);

        result.Status.Should().Be(MarkOrderReceivedStatus.NotFound);
    }

    [Fact]
    public async Task HandleAsync_fails_when_the_order_has_not_yet_been_delivered()
    {
        var userId = Guid.NewGuid();
        var order = Order.Create(Guid.NewGuid(), userId, "1 Example St", new[] { SampleItem }, DateTimeOffset.UtcNow);
        var orderRepository = new Mock<IOrderRepository>();
        orderRepository.Setup(r => r.FindByIdAsync(order.Id, It.IsAny<CancellationToken>())).ReturnsAsync(order);
        var handler = new MarkOrderReceivedHandler(orderRepository.Object);

        var result = await handler.HandleAsync(userId, order.Id, CancellationToken.None);

        result.Status.Should().Be(MarkOrderReceivedStatus.NotYetDelivered);
    }

    [Fact]
    public async Task HandleAsync_succeeds_once_delivered_and_persists_the_change()
    {
        var userId = Guid.NewGuid();
        var order = Order.Create(Guid.NewGuid(), userId, "1 Example St", new[] { SampleItem }, DateTimeOffset.UtcNow.AddSeconds(-200));
        var orderRepository = new Mock<IOrderRepository>();
        orderRepository.Setup(r => r.FindByIdAsync(order.Id, It.IsAny<CancellationToken>())).ReturnsAsync(order);
        var handler = new MarkOrderReceivedHandler(orderRepository.Object);

        var result = await handler.HandleAsync(userId, order.Id, CancellationToken.None);

        result.Status.Should().Be(MarkOrderReceivedStatus.Success);
        order.ReceivedAtUtc.Should().NotBeNull();
        orderRepository.Verify(r => r.SaveAsync(order, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_fails_when_already_received()
    {
        var userId = Guid.NewGuid();
        var order = Order.Create(Guid.NewGuid(), userId, "1 Example St", new[] { SampleItem }, DateTimeOffset.UtcNow.AddSeconds(-200));
        order.MarkReceived(DateTimeOffset.UtcNow);
        var orderRepository = new Mock<IOrderRepository>();
        orderRepository.Setup(r => r.FindByIdAsync(order.Id, It.IsAny<CancellationToken>())).ReturnsAsync(order);
        var handler = new MarkOrderReceivedHandler(orderRepository.Object);

        var result = await handler.HandleAsync(userId, order.Id, CancellationToken.None);

        result.Status.Should().Be(MarkOrderReceivedStatus.AlreadyReceived);
    }
}
