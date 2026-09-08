using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
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

    [Fact]
    public async Task POST_login_returns_200_and_a_session_cookie_for_correct_credentials()
    {
        var client = _factory.CreateClient();
        var email = $"{Guid.NewGuid()}@example.com";
        await client.PostAsJsonAsync("/api/auth/register", new RegisterRequest(email, "s3cret-password!", "Ada Shopper"));

        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, "s3cret-password!"));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Headers.Should().Contain(h => h.Key == "Set-Cookie");
    }

    [Fact]
    public async Task POST_login_returns_401_with_a_generic_message_for_the_wrong_password()
    {
        var client = _factory.CreateClient();
        var email = $"{Guid.NewGuid()}@example.com";
        await client.PostAsJsonAsync("/api/auth/register", new RegisterRequest(email, "s3cret-password!", "Ada Shopper"));

        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, "wrong-password"));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("message").GetString().Should().Be("Invalid email or password.");
    }

    [Fact]
    public async Task POST_login_returns_the_same_401_message_for_an_unknown_email()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest($"{Guid.NewGuid()}@example.com", "s3cret-password!"));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("message").GetString().Should().Be("Invalid email or password.");
    }

    [Fact]
    public async Task GET_me_returns_401_when_there_is_no_session_cookie()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/auth/me");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GET_me_returns_200_with_the_current_user_after_logging_in()
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = true });
        var email = $"{Guid.NewGuid()}@example.com";
        await client.PostAsJsonAsync("/api/auth/register", new RegisterRequest(email, "s3cret-password!", "Ada Shopper"));
        await client.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, "s3cret-password!"));

        var response = await client.GetAsync("/api/auth/me");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("email").GetString().Should().Be(email);
        body.GetProperty("displayName").GetString().Should().Be("Ada Shopper");
    }

    [Fact]
    public async Task POST_logout_clears_the_session_so_me_returns_401_afterwards()
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = true });
        var email = $"{Guid.NewGuid()}@example.com";
        await client.PostAsJsonAsync("/api/auth/register", new RegisterRequest(email, "s3cret-password!", "Ada Shopper"));
        await client.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, "s3cret-password!"));

        var logoutResponse = await client.PostAsync("/api/auth/logout", null);
        logoutResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var meResponse = await client.GetAsync("/api/auth/me");
        meResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private sealed record RegisterRequest(string Email, string Password, string DisplayName);

    private sealed record LoginRequest(string Email, string Password);
}
