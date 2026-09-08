using System.Security.Claims;
using EPharmacy.Application;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace EPharmacy.Api.Endpoints;

public static class AuthEndpoints
{
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
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Email, user.Email),
            };
            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await httpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

            return Results.Created($"/api/users/{user.Id}", new
            {
                id = user.Id,
                email = user.Email,
                displayName = user.DisplayName,
            });
        })
        .WithName("RegisterUser");

        return app;
    }
}

public sealed record RegisterUserRequest(string Email, string Password, string DisplayName);
