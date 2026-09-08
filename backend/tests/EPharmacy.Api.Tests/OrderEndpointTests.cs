using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using EPharmacy.Infrastructure;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace EPharmacy.Api.Tests;

public class OrderEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public OrderEndpointTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task POST_checkout_with_a_non_empty_cart_and_a_normal_card_succeeds_and_clears_the_cart()
    {
        var client = await CreateAuthenticatedClientAsync();
        var medicineId = await GetAnyMedicineIdAsync(client);
        await client.PostAsJsonAsync("/api/cart/items", new AddCartItemRequest(medicineId, 2));

        var response = await client.PostAsJsonAsync(
            "/api/checkout",
            new CheckoutRequest("1 Example St", "4111111111111111", "12/30", "123"));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("orderId").GetGuid().Should().NotBeEmpty();
        body.GetProperty("referenceNumber").GetString().Should().NotBeNullOrWhiteSpace();
        body.GetProperty("totalCents").GetInt32().Should().BeGreaterThan(0);

        var summary = await client.GetFromJsonAsync<JsonElement>("/api/cart/summary");
        summary.GetProperty("itemCount").GetInt32().Should().Be(0);
    }

    [Fact]
    public async Task POST_checkout_with_the_fixed_decline_test_card_returns_402_and_leaves_the_cart_populated()
    {
        var client = await CreateAuthenticatedClientAsync();
        var medicineId = await GetAnyMedicineIdAsync(client);
        await client.PostAsJsonAsync("/api/cart/items", new AddCartItemRequest(medicineId, 1));

        var response = await client.PostAsJsonAsync(
            "/api/checkout",
            new CheckoutRequest("1 Example St", FakePaymentGateway.AlwaysDeclinesCardNumber, "12/30", "123"));

        response.StatusCode.Should().Be((HttpStatusCode)402);

        var summary = await client.GetFromJsonAsync<JsonElement>("/api/cart/summary");
        summary.GetProperty("itemCount").GetInt32().Should().Be(1);
    }

    [Fact]
    public async Task POST_checkout_with_an_empty_cart_returns_400()
    {
        var client = await CreateAuthenticatedClientAsync();

        var response = await client.PostAsJsonAsync(
            "/api/checkout",
            new CheckoutRequest("1 Example St", "4111111111111111", "12/30", "123"));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task POST_checkout_requires_authentication()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/checkout",
            new CheckoutRequest("1 Example St", "4111111111111111", "12/30", "123"));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GET_orders_returns_the_placed_order_for_its_owner()
    {
        var client = await CreateAuthenticatedClientAsync();
        var medicineId = await GetAnyMedicineIdAsync(client);
        await client.PostAsJsonAsync("/api/cart/items", new AddCartItemRequest(medicineId, 2));
        var checkoutResponse = await client.PostAsJsonAsync(
            "/api/checkout",
            new CheckoutRequest("1 Example St", "4111111111111111", "12/30", "123"));
        var checkoutBody = await checkoutResponse.Content.ReadFromJsonAsync<JsonElement>();
        var orderId = checkoutBody.GetProperty("orderId").GetGuid();

        var response = await client.GetAsync($"/api/orders/{orderId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("orderId").GetGuid().Should().Be(orderId);
        body.GetProperty("items").GetArrayLength().Should().Be(1);
    }

    [Fact]
    public async Task GET_orders_returns_404_for_another_users_order()
    {
        var ownerClient = await CreateAuthenticatedClientAsync();
        var medicineId = await GetAnyMedicineIdAsync(ownerClient);
        await ownerClient.PostAsJsonAsync("/api/cart/items", new AddCartItemRequest(medicineId, 1));
        var checkoutResponse = await ownerClient.PostAsJsonAsync(
            "/api/checkout",
            new CheckoutRequest("1 Example St", "4111111111111111", "12/30", "123"));
        var checkoutBody = await checkoutResponse.Content.ReadFromJsonAsync<JsonElement>();
        var orderId = checkoutBody.GetProperty("orderId").GetGuid();

        var otherClient = await CreateAuthenticatedClientAsync();

        var response = await otherClient.GetAsync($"/api/orders/{orderId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private async Task<HttpClient> CreateAuthenticatedClientAsync()
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = true });
        var email = $"{Guid.NewGuid()}@example.com";
        await client.PostAsJsonAsync("/api/auth/register", new RegisterRequest(email, "s3cret-password!", "Ada Shopper"));
        return client;
    }

    private static async Task<Guid> GetAnyMedicineIdAsync(HttpClient client)
    {
        var medicines = await client.GetFromJsonAsync<JsonElement>("/api/medicines");
        return medicines[0].GetProperty("id").GetGuid();
    }

    private sealed record RegisterRequest(string Email, string Password, string DisplayName);

    private sealed record AddCartItemRequest(Guid MedicineId, int Quantity);

    private sealed record CheckoutRequest(string ShippingAddress, string CardNumber, string Expiry, string Cvc);
}
