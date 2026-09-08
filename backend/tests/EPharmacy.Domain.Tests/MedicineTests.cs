using FluentAssertions;

namespace EPharmacy.Domain.Tests;

public class MedicineTests
{
    [Fact]
    public void Create_sets_all_properties()
    {
        var id = Guid.NewGuid();

        var medicine = Medicine.Create(id, "Paracetamol 500mg", "Pain and fever relief tablets.", 599, "https://example.com/paracetamol.png");

        medicine.Id.Should().Be(id);
        medicine.Name.Should().Be("Paracetamol 500mg");
        medicine.Description.Should().Be("Pain and fever relief tablets.");
        medicine.PriceCents.Should().Be(599);
        medicine.ImageUrl.Should().Be("https://example.com/paracetamol.png");
    }

    [Fact]
    public void Create_allows_a_null_image_url()
    {
        var medicine = Medicine.Create(Guid.NewGuid(), "Paracetamol 500mg", "Pain and fever relief tablets.", 599, null);

        medicine.ImageUrl.Should().BeNull();
    }

    [Fact]
    public void Create_rejects_an_empty_id()
    {
        var act = () => Medicine.Create(Guid.Empty, "Paracetamol 500mg", "Pain and fever relief tablets.", 599, null);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_rejects_an_empty_name()
    {
        var act = () => Medicine.Create(Guid.NewGuid(), "", "Pain and fever relief tablets.", 599, null);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_rejects_an_empty_description()
    {
        var act = () => Medicine.Create(Guid.NewGuid(), "Paracetamol 500mg", "", 599, null);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_rejects_a_negative_price()
    {
        var act = () => Medicine.Create(Guid.NewGuid(), "Paracetamol 500mg", "Pain and fever relief tablets.", -1, null);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_allows_a_zero_price()
    {
        var medicine = Medicine.Create(Guid.NewGuid(), "Paracetamol 500mg", "Pain and fever relief tablets.", 0, null);

        medicine.PriceCents.Should().Be(0);
    }
}
