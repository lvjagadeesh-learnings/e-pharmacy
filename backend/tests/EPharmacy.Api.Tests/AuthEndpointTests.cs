using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace EPharmacy.Api.Tests;

public class AuthEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public AuthEndpointTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task POST_register_returns_201_and_a_session_cookie_for_a_new_account()
    {
        var client = _factory.CreateClient();
        var request = new RegisterRequest($"{Guid.NewGuid()}@example.com", "s3cret-password!", "Ada Shopper");

        var response = await client.PostAsJsonAsync("/api/auth/register", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Should().Contain(h => h.Key == "Set-Cookie");
    }

    [Fact]
    public async Task POST_register_returns_409_when_the_email_is_already_registered()
    {
        var client = _factory.CreateClient();
        var request = new RegisterRequest($"{Guid.NewGuid()}@example.com", "s3cret-password!", "Ada Shopper");

        var first = await client.PostAsJsonAsync("/api/auth/register", request);
        first.StatusCode.Should().Be(HttpStatusCode.Created);

        var second = await client.PostAsJsonAsync("/api/auth/register", request);

        second.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task POST_register_returns_400_when_the_email_is_missing()
    {
        var client = _factory.CreateClient();
        var request = new RegisterRequest("", "s3cret-password!", "Ada Shopper");

        var response = await client.PostAsJsonAsync("/api/auth/register", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task POST_register_returns_400_when_the_email_shape_is_invalid()
    {
        var client = _factory.CreateClient();
        var request = new RegisterRequest("not-an-email", "s3cret-password!", "Ada Shopper");

        var response = await client.PostAsJsonAsync("/api/auth/register", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    private sealed record RegisterRequest(string Email, string Password, string DisplayName);
}
