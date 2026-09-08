using EPharmacy.Domain;
using FluentAssertions;
using Moq;

namespace EPharmacy.Application.Tests;

public class RecordHealthCheckHandlerTests
{
    [Fact]
    public async Task HandleAsync_delegates_to_the_repository_and_returns_its_result()
    {
        var expected = HealthCheck.CreateNow();
        var repository = new Mock<IHealthCheckRepository>();
        repository
            .Setup(r => r.RecordAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var handler = new RecordHealthCheckHandler(repository.Object);

        var result = await handler.HandleAsync(CancellationToken.None);

        result.Should().Be(expected);
        repository.Verify(r => r.RecordAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void Constructor_rejects_a_null_repository()
    {
        var act = () => new RecordHealthCheckHandler(null!);

        act.Should().Throw<ArgumentNullException>();
    }
}
