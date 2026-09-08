using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;

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
    }

    [Fact]
    public async Task GET_medicines_does_not_require_authentication()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/medicines");

        response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
    }
}
