using System.Security.Claims;
using EPharmacy.Application;

namespace EPharmacy.Api.Endpoints;

public sealed record JoinMembershipRequest(string CardNumber, string Expiry, string Cvc);

public static class MembershipEndpoints
{
    public static WebApplication MapMembershipEndpoints(this WebApplication app)
    {
        app.MapPost("/api/membership/join", async (
            JoinMembershipRequest request,
            JoinMembershipHandler handler,
            ClaimsPrincipal user,
            CancellationToken cancellationToken) =>
        {
            if (string.IsNullOrWhiteSpace(request.CardNumber)
                || string.IsNullOrWhiteSpace(request.Expiry)
                || string.IsNullOrWhiteSpace(request.Cvc))
            {
                return Results.BadRequest(new { error = "Card details are required." });
            }

            var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await handler.HandleAsync(userId, request.CardNumber, request.Expiry, request.Cvc, cancellationToken);

            return result.Status switch
            {
                JoinMembershipStatus.AlreadyMember => Results.Conflict(new { error = "You're already an e-Pharmacy Plus member." }),
                JoinMembershipStatus.PaymentDeclined => Results.Json(
                    new { error = result.FailureReason ?? "Payment was declined." },
                    statusCode: StatusCodes.Status402PaymentRequired),
                JoinMembershipStatus.UserNotFound => Results.Unauthorized(),
                _ => Results.Ok(new { isMember = true, membershipJoinedAtUtc = result.JoinedAtUtc }),
            };
        })
        .RequireAuthorization()
        .WithName("JoinMembership");

        return app;
    }
}
