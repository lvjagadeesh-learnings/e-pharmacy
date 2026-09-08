using FluentAssertions;

namespace EPharmacy.Domain.Tests;

public class HealthCheckTests
{
    [Fact]
    public void CreateNow_produces_a_check_with_a_non_empty_id_and_current_utc_timestamp()
    {
        var before = DateTimeOffset.UtcNow;

        var healthCheck = HealthCheck.CreateNow();

        var after = DateTimeOffset.UtcNow;

        healthCheck.Id.Should().NotBe(Guid.Empty);
        healthCheck.CheckedAtUtc.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
    }

    [Fact]
    public void Constructor_rejects_an_empty_id()
    {
        var act = () => new HealthCheck(Guid.Empty, DateTimeOffset.UtcNow);

        act.Should().Throw<ArgumentException>();
    }
}
