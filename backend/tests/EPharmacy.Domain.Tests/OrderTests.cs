using EPharmacy.Domain;
using FluentAssertions;
using Xunit;

namespace EPharmacy.Domain.Tests;

public class OrderTests
{
    private static readonly OrderItem SampleItem = new(Guid.NewGuid(), "Paracetamol 500mg", 599, 2);

    [Fact]
    public void Create_computes_total_cents_from_item_unit_prices_times_quantities()
    {
        var firstItem = new OrderItem(Guid.NewGuid(), "Paracetamol 500mg", 599, 2);
        var secondItem = new OrderItem(Guid.NewGuid(), "Ibuprofen 200mg", 799, 1);

        var order = Order.Create(Guid.NewGuid(), Guid.NewGuid(), "1 Example St", new[] { firstItem, secondItem }, DateTimeOffset.UtcNow);

        order.TotalCents.Should().Be((599 * 2) + (799 * 1));
        order.Items.Should().HaveCount(2);
    }

    [Fact]
    public void Create_rejects_an_empty_items_collection()
    {
        var act = () => Order.Create(Guid.NewGuid(), Guid.NewGuid(), "1 Example St", Array.Empty<OrderItem>(), DateTimeOffset.UtcNow);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_rejects_an_empty_shipping_address()
    {
        var act = () => Order.Create(Guid.NewGuid(), Guid.NewGuid(), "  ", new[] { SampleItem }, DateTimeOffset.UtcNow);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_rejects_an_empty_id()
    {
        var act = () => Order.Create(Guid.Empty, Guid.NewGuid(), "1 Example St", new[] { SampleItem }, DateTimeOffset.UtcNow);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_rejects_an_empty_user_id()
    {
        var act = () => Order.Create(Guid.NewGuid(), Guid.Empty, "1 Example St", new[] { SampleItem }, DateTimeOffset.UtcNow);

        act.Should().Throw<ArgumentException>();
    }
}
