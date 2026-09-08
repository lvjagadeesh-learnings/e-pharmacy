using System.Security.Claims;
using EPharmacy.Application;
using EPharmacy.Domain;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace EPharmacy.Api.Endpoints;

public static class AuthEndpoints
{
    private const string DisplayNameClaimType = "displayName";

    public static WebApplication MapAuthEndpoints(this WebApplication app)
    {
        app.MapPost("/api/auth/register", async (
            RegisterUserRequest request,
            RegisterUserHandler handler,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            if (string.IsNullOrWhiteSpace(request.Email) || !request.Email.Contains('@') ||
                string.IsNullOrWhiteSpace(request.Password) ||
                string.IsNullOrWhiteSpace(request.DisplayName))
            {
                return Results.BadRequest(new { error = "Email, password, and display name are required, and email must be a valid address." });
            }

            var result = await handler.HandleAsync(request.Email, request.Password, request.DisplayName, cancellationToken);

            if (!result.Succeeded)
            {
                return Results.Conflict(new { error = result.Error });
            }

            var user = result.User!;
            await SignInAsync(httpContext, user);

            return Results.Created($"/api/users/{user.Id}", ToUserResponse(user));
        })
        .WithName("RegisterUser");

        app.MapPost("/api/auth/login", async (
            LoginRequest request,
            AuthenticateUserHandler handler,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                return Results.Unauthorized();
            }

            var result = await handler.HandleAsync(request.Email, request.Password, cancellationToken);

            if (!result.Succeeded)
            {
                return Results.Json(new { message = "Invalid email or password." }, statusCode: StatusCodes.Status401Unauthorized);
            }

            var user = result.User!;
            await SignInAsync(httpContext, user);

            return Results.Ok(ToUserResponse(user));
        })
        .WithName("LoginUser");

        app.MapGet("/api/auth/me", (ClaimsPrincipal user) =>
        {
            var id = user.FindFirstValue(ClaimTypes.NameIdentifier);
            var email = user.FindFirstValue(ClaimTypes.Email);
            var displayName = user.FindFirstValue(DisplayNameClaimType);

            return Results.Ok(new { id, email, displayName });
        })
        .RequireAuthorization()
        .WithName("GetCurrentUser");

        return app;
    }

    private static async Task SignInAsync(HttpContext httpContext, User user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(DisplayNameClaimType, user.DisplayName),
        };
        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await httpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));
    }

    private static object ToUserResponse(User user) => new
    {
        id = user.Id,
        email = user.Email,
        displayName = user.DisplayName,
    };
}

public sealed record RegisterUserRequest(string Email, string Password, string DisplayName);

public sealed record LoginRequest(string Email, string Password);

