using System.Security.Claims;
using EPharmacy.Application;

namespace EPharmacy.Api.Endpoints;

public static class CartEndpoints
{
    public static WebApplication MapCartEndpoints(this WebApplication app)
    {
        app.MapPost("/api/cart/items", async (
            AddCartItemRequest request,
            AddCartItemHandler handler,
            ClaimsPrincipal user,
            CancellationToken cancellationToken) =>
        {
            if (request.Quantity <= 0)
            {
                return Results.BadRequest(new { error = "Quantity must be positive." });
            }

            var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await handler.HandleAsync(userId, request.MedicineId, request.Quantity, cancellationToken);

            if (result.Status == AddCartItemStatus.MedicineNotFound)
            {
                return Results.NotFound(new { error = "Medicine not found." });
            }

            return Results.Ok(new { itemCount = result.ItemCount });
        })
        .RequireAuthorization()
        .WithName("AddCartItem");

        app.MapGet("/api/cart/summary", async (
            ICartRepository cartRepository,
            ClaimsPrincipal user,
            CancellationToken cancellationToken) =>
        {
            var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var cart = await cartRepository.GetOrCreateForUserAsync(userId, cancellationToken);

            return Results.Ok(new { itemCount = cart.TotalItemCount });
        })
        .RequireAuthorization()
        .WithName("GetCartSummary");

        return app;
    }
}

public sealed record AddCartItemRequest(Guid MedicineId, int Quantity);
