using EPharmacy.Domain;
using FluentAssertions;
using Moq;

namespace EPharmacy.Application.Tests;

public class ListMedicinesHandlerTests
{
    [Fact]
    public async Task HandleAsync_returns_whatever_the_repository_returns()
    {
        var medicines = new List<Medicine>
        {
            Medicine.Create(Guid.NewGuid(), "Paracetamol 500mg", "Pain and fever relief tablets.", 599, null),
            Medicine.Create(Guid.NewGuid(), "Vitamin C 1000mg", "Immune support supplement.", 899, null),
        };
        var repository = new Mock<IMedicineRepository>();
        repository.Setup(r => r.ListAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(medicines);
        var handler = new ListMedicinesHandler(repository.Object);

        var result = await handler.HandleAsync(CancellationToken.None);

        result.Should().BeEquivalentTo(medicines);
    }

    [Fact]
    public async Task HandleAsync_returns_an_empty_list_when_the_repository_has_no_medicines()
    {
        var repository = new Mock<IMedicineRepository>();
        repository.Setup(r => r.ListAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<Medicine>());
        var handler = new ListMedicinesHandler(repository.Object);

        var result = await handler.HandleAsync(CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_rejects_a_null_repository()
    {
        var act = () => new ListMedicinesHandler(null!);

        act.Should().Throw<ArgumentNullException>();
    }
}
