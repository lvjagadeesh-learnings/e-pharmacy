using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using EPharmacy.Infrastructure;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EPharmacy.Api.Tests;

public class CatalogEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public CatalogEndpointTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GET_medicines_returns_200_with_a_non_empty_array_of_medicines()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/medicines");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload = await response.Content.ReadFromJsonAsync<JsonElement>();
        payload.ValueKind.Should().Be(JsonValueKind.Array);
        payload.GetArrayLength().Should().BeGreaterThan(0);

        var firstItem = payload[0];
        firstItem.TryGetProperty("id", out _).Should().BeTrue();
        firstItem.TryGetProperty("name", out _).Should().BeTrue();
        firstItem.TryGetProperty("description", out _).Should().BeTrue();
        firstItem.TryGetProperty("priceCents", out _).Should().BeTrue();
        firstItem.TryGetProperty("averageRating", out _).Should().BeTrue();
        firstItem.TryGetProperty("reviewCount", out _).Should().BeTrue();
    }

    [Fact]
    public async Task GET_medicines_does_not_require_authentication()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/medicines");

        response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task POST_review_without_a_qualifying_delivered_order_returns_403()
    {
        var client = await CreateAuthenticatedClientAsync();
        var medicineId = await GetAnyMedicineIdAsync(client);

        var response = await client.PostAsJsonAsync(
            $"/api/medicines/{medicineId}/reviews",
            new SubmitReviewRequest(5, "Great product!"));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task POST_review_requires_authentication()
    {
        var client = _factory.CreateClient();
        var medicineId = await GetAnyMedicineIdAsync(client);

        var response = await client.PostAsJsonAsync(
            $"/api/medicines/{medicineId}/reviews",
            new SubmitReviewRequest(5, "Great product!"));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task POST_review_after_a_delivered_order_succeeds_and_then_appears_on_the_medicine_and_updates_the_catalog_rating()
    {
        var client = await CreateAuthenticatedClientAsync();
        var medicineId = await GetAnyMedicineIdAsync(client);
        await PlaceDeliveredOrderAsync(client, medicineId);
        var comment = $"Worked great for my headache. ({Guid.NewGuid()})";

        var response = await client.PostAsJsonAsync(
            $"/api/medicines/{medicineId}/reviews",
            new SubmitReviewRequest(5, comment));

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var reviewsResponse = await client.GetFromJsonAsync<JsonElement>($"/api/medicines/{medicineId}/reviews");
        var addedReview = reviewsResponse.EnumerateArray().Single(r => r.GetProperty("comment").GetString() == comment);
        addedReview.GetProperty("rating").GetInt32().Should().Be(5);

        var medicines = await client.GetFromJsonAsync<JsonElement>("/api/medicines");
        var reviewedMedicine = medicines.EnumerateArray().Single(m => m.GetProperty("id").GetGuid() == medicineId);
        reviewedMedicine.GetProperty("reviewCount").GetInt32().Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task POST_review_a_second_time_for_the_same_medicine_returns_409()
    {
        var client = await CreateAuthenticatedClientAsync();
        var medicineId = await GetAnyMedicineIdAsync(client);
        await PlaceDeliveredOrderAsync(client, medicineId);
        await client.PostAsJsonAsync($"/api/medicines/{medicineId}/reviews", new SubmitReviewRequest(5, "Great!"));

        var response = await client.PostAsJsonAsync(
            $"/api/medicines/{medicineId}/reviews",
            new SubmitReviewRequest(4, "Still great."));

        response.StatusCode.Should().Be((HttpStatusCode)409);
    }

    private async Task PlaceDeliveredOrderAsync(HttpClient client, Guid medicineId)
    {
        await client.PostAsJsonAsync("/api/cart/items", new AddCartItemRequest(medicineId, 1));
        var checkoutResponse = await client.PostAsJsonAsync(
            "/api/checkout",
            new CheckoutRequest("1 Example St", "4111111111111111", "12/30", "123"));
        var checkoutBody = await checkoutResponse.Content.ReadFromJsonAsync<JsonElement>();
        var orderId = checkoutBody.GetProperty("orderId").GetGuid();

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"UPDATE \"Orders\" SET \"PlacedAtUtc\" = {DateTimeOffset.UtcNow.AddSeconds(-200)} WHERE \"Id\" = {orderId}");
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

    private sealed record SubmitReviewRequest(int Rating, string Comment);
}
