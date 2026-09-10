using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using EPharmacy.Infrastructure;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace EPharmacy.Api.Tests;

public class MembershipEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public MembershipEndpointTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task POST_join_without_authentication_returns_401()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/membership/join",
            new JoinMembershipRequest("4111111111111111", "12/30", "123"));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task POST_join_with_a_normal_card_succeeds_and_marks_the_user_a_member()
    {
        var client = await CreateAuthenticatedClientAsync();

        var response = await client.PostAsJsonAsync(
            "/api/membership/join",
            new JoinMembershipRequest("4111111111111111", "12/30", "123"));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("isMember").GetBoolean().Should().BeTrue();
        body.GetProperty("membershipJoinedAtUtc").GetDateTimeOffset().Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromMinutes(1));

        var me = await client.GetFromJsonAsync<JsonElement>("/api/auth/me");
        me.GetProperty("isMember").GetBoolean().Should().BeTrue();
    }

    [Fact]
    public async Task POST_join_a_second_time_returns_409()
    {
        var client = await CreateAuthenticatedClientAsync();
        await client.PostAsJsonAsync("/api/membership/join", new JoinMembershipRequest("4111111111111111", "12/30", "123"));

        var response = await client.PostAsJsonAsync(
            "/api/membership/join",
            new JoinMembershipRequest("4111111111111111", "12/30", "123"));

        response.StatusCode.Should().Be((HttpStatusCode)409);
    }

    [Fact]
    public async Task POST_join_with_the_fixed_decline_test_card_returns_402_and_does_not_mark_the_user_a_member()
    {
        var client = await CreateAuthenticatedClientAsync();

        var response = await client.PostAsJsonAsync(
            "/api/membership/join",
            new JoinMembershipRequest(FakePaymentGateway.AlwaysDeclinesCardNumber, "12/30", "123"));

        response.StatusCode.Should().Be((HttpStatusCode)402);

        var me = await client.GetFromJsonAsync<JsonElement>("/api/auth/me");
        me.GetProperty("isMember").GetBoolean().Should().BeFalse();
    }

    private async Task<HttpClient> CreateAuthenticatedClientAsync()
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = true });
        var email = $"{Guid.NewGuid()}@example.com";
        await client.PostAsJsonAsync("/api/auth/register", new RegisterRequest(email, "s3cret-password!", "Ada Shopper"));
        return client;
    }

    private sealed record RegisterRequest(string Email, string Password, string DisplayName);

    private sealed record JoinMembershipRequest(string CardNumber, string Expiry, string Cvc);
}
