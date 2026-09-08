using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace EPharmacy.Api.Tests;

public class CartEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public CartEndpointTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task POST_cart_items_returns_200_with_an_item_count_of_1_for_a_new_medicine()
    {
        var client = await CreateAuthenticatedClientAsync();
        var medicineId = await GetAnyMedicineIdAsync(client);

        var response = await client.PostAsJsonAsync("/api/cart/items", new AddCartItemRequest(medicineId, 1));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("itemCount").GetInt32().Should().Be(1);
    }

    [Fact]
    public async Task POST_cart_items_increments_the_count_instead_of_duplicating_a_line_when_the_same_medicine_is_added_twice()
    {
        var client = await CreateAuthenticatedClientAsync();
        var medicineId = await GetAnyMedicineIdAsync(client);

        await client.PostAsJsonAsync("/api/cart/items", new AddCartItemRequest(medicineId, 1));
        var response = await client.PostAsJsonAsync("/api/cart/items", new AddCartItemRequest(medicineId, 1));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("itemCount").GetInt32().Should().Be(2);
    }

    [Fact]
    public async Task POST_cart_items_returns_404_for_an_unknown_medicine_id()
    {
        var client = await CreateAuthenticatedClientAsync();

        var response = await client.PostAsJsonAsync("/api/cart/items", new AddCartItemRequest(Guid.NewGuid(), 1));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task POST_cart_items_requires_authentication()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/cart/items", new AddCartItemRequest(Guid.NewGuid(), 1));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GET_cart_summary_reflects_previously_added_items()
    {
        var client = await CreateAuthenticatedClientAsync();
        var medicineId = await GetAnyMedicineIdAsync(client);
        await client.PostAsJsonAsync("/api/cart/items", new AddCartItemRequest(medicineId, 3));

        var response = await client.GetAsync("/api/cart/summary");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("itemCount").GetInt32().Should().Be(3);
    }

    [Fact]
    public async Task GET_cart_summary_requires_authentication()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/cart/summary");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
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
}
