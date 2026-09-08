using EPharmacy.Domain;
using FluentAssertions;
using Xunit;

namespace EPharmacy.Domain.Tests;

public class OrderStatusCalculatorTests
{
    private static readonly DateTimeOffset PlacedAtUtc = new(2024, 1, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Calculate_returns_Placed_at_time_of_placement()
    {
        OrderStatusCalculator.Calculate(PlacedAtUtc, PlacedAtUtc).Should().Be(OrderStatus.Placed);
    }

    [Fact]
    public void Calculate_returns_Processing_after_30_seconds()
    {
        OrderStatusCalculator.Calculate(PlacedAtUtc, PlacedAtUtc.AddSeconds(30)).Should().Be(OrderStatus.Processing);
    }

    [Fact]
    public void Calculate_returns_Shipped_after_90_seconds()
    {
        OrderStatusCalculator.Calculate(PlacedAtUtc, PlacedAtUtc.AddSeconds(90)).Should().Be(OrderStatus.Shipped);
    }

    [Fact]
    public void Calculate_returns_Delivered_after_180_seconds()
    {
        OrderStatusCalculator.Calculate(PlacedAtUtc, PlacedAtUtc.AddSeconds(180)).Should().Be(OrderStatus.Delivered);
    }

    [Fact]
    public void History_returns_only_statuses_reached_so_far_with_correct_timestamps()
    {
        var history = OrderStatusCalculator.History(PlacedAtUtc, PlacedAtUtc.AddSeconds(90));

        history.Should().HaveCount(3);
        history[0].Status.Should().Be(OrderStatus.Placed);
        history[0].ReachedAtUtc.Should().Be(PlacedAtUtc);
        history[1].Status.Should().Be(OrderStatus.Processing);
        history[1].ReachedAtUtc.Should().Be(PlacedAtUtc.AddSeconds(30));
        history[2].Status.Should().Be(OrderStatus.Shipped);
        history[2].ReachedAtUtc.Should().Be(PlacedAtUtc.AddSeconds(90));
    }

    [Fact]
    public void History_does_not_include_statuses_not_yet_reached()
    {
        var history = OrderStatusCalculator.History(PlacedAtUtc, PlacedAtUtc.AddSeconds(10));

        history.Should().HaveCount(1);
        history[0].Status.Should().Be(OrderStatus.Placed);
    }
}
