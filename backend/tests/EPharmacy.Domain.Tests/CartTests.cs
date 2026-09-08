using EPharmacy.Domain;
using FluentAssertions;
using Xunit;

namespace EPharmacy.Domain.Tests;

public class CartTests
{
    [Fact]
    public void CreateEmpty_sets_id_and_user_id_with_no_items()
    {
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var cart = Cart.CreateEmpty(id, userId);

        cart.Id.Should().Be(id);
        cart.UserId.Should().Be(userId);
        cart.Items.Should().BeEmpty();
        cart.TotalItemCount.Should().Be(0);
    }

    [Fact]
    public void CreateEmpty_rejects_an_empty_id()
    {
        var act = () => Cart.CreateEmpty(Guid.Empty, Guid.NewGuid());

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void CreateEmpty_rejects_an_empty_user_id()
    {
        var act = () => Cart.CreateEmpty(Guid.NewGuid(), Guid.Empty);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void AddItem_on_an_empty_cart_adds_a_new_line_with_the_given_quantity()
    {
        var cart = Cart.CreateEmpty(Guid.NewGuid(), Guid.NewGuid());
        var medicineId = Guid.NewGuid();

        cart.AddItem(medicineId, 2);

        cart.Items.Should().ContainSingle(item => item.MedicineId == medicineId && item.Quantity == 2);
        cart.TotalItemCount.Should().Be(2);
    }

    [Fact]
    public void AddItem_with_an_already_present_medicine_increments_the_existing_line_instead_of_duplicating_it()
    {
        var cart = Cart.CreateEmpty(Guid.NewGuid(), Guid.NewGuid());
        var medicineId = Guid.NewGuid();

        cart.AddItem(medicineId, 1);
        cart.AddItem(medicineId, 2);

        cart.Items.Should().ContainSingle(item => item.MedicineId == medicineId && item.Quantity == 3);
        cart.TotalItemCount.Should().Be(3);
    }

    [Fact]
    public void AddItem_with_different_medicines_adds_separate_lines()
    {
        var cart = Cart.CreateEmpty(Guid.NewGuid(), Guid.NewGuid());
        var firstMedicineId = Guid.NewGuid();
        var secondMedicineId = Guid.NewGuid();

        cart.AddItem(firstMedicineId, 1);
        cart.AddItem(secondMedicineId, 1);

        cart.Items.Should().HaveCount(2);
        cart.TotalItemCount.Should().Be(2);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void AddItem_rejects_a_non_positive_quantity(int quantity)
    {
        var cart = Cart.CreateEmpty(Guid.NewGuid(), Guid.NewGuid());

        var act = () => cart.AddItem(Guid.NewGuid(), quantity);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void AddItem_rejects_an_empty_medicine_id()
    {
        var cart = Cart.CreateEmpty(Guid.NewGuid(), Guid.NewGuid());

        var act = () => cart.AddItem(Guid.Empty, 1);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void UpdateItemQuantity_replaces_an_existing_lines_quantity()
    {
        var cart = Cart.CreateEmpty(Guid.NewGuid(), Guid.NewGuid());
        var medicineId = Guid.NewGuid();
        cart.AddItem(medicineId, 1);

        cart.UpdateItemQuantity(medicineId, 5);

        cart.Items.Should().ContainSingle(item => item.MedicineId == medicineId && item.Quantity == 5);
        cart.TotalItemCount.Should().Be(5);
    }

    [Fact]
    public void UpdateItemQuantity_throws_for_an_unknown_medicine_id()
    {
        var cart = Cart.CreateEmpty(Guid.NewGuid(), Guid.NewGuid());

        var act = () => cart.UpdateItemQuantity(Guid.NewGuid(), 5);

        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void UpdateItemQuantity_rejects_a_non_positive_quantity(int quantity)
    {
        var cart = Cart.CreateEmpty(Guid.NewGuid(), Guid.NewGuid());
        var medicineId = Guid.NewGuid();
        cart.AddItem(medicineId, 1);

        var act = () => cart.UpdateItemQuantity(medicineId, quantity);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void RemoveItem_removes_an_existing_line()
    {
        var cart = Cart.CreateEmpty(Guid.NewGuid(), Guid.NewGuid());
        var medicineId = Guid.NewGuid();
        cart.AddItem(medicineId, 1);

        cart.RemoveItem(medicineId);

        cart.Items.Should().BeEmpty();
        cart.TotalItemCount.Should().Be(0);
    }

    [Fact]
    public void RemoveItem_is_a_no_op_for_an_unknown_medicine_id()
    {
        var cart = Cart.CreateEmpty(Guid.NewGuid(), Guid.NewGuid());
        var medicineId = Guid.NewGuid();
        cart.AddItem(medicineId, 1);

        var act = () => cart.RemoveItem(Guid.NewGuid());

        act.Should().NotThrow();
        cart.Items.Should().ContainSingle(item => item.MedicineId == medicineId);
    }
}
