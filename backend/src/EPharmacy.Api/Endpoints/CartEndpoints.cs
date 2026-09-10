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

        app.MapGet("/api/cart", async (
            GetCartHandler handler,
            ClaimsPrincipal user,
            CancellationToken cancellationToken) =>
        {
            var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var cart = await handler.HandleAsync(userId, cancellationToken);

            return Results.Ok(cart);
        })
        .RequireAuthorization()
        .WithName("GetCart");

        app.MapPatch("/api/cart/items/{medicineId:guid}", async (
            Guid medicineId,
            UpdateCartItemRequest request,
            UpdateCartItemHandler handler,
            ClaimsPrincipal user,
            CancellationToken cancellationToken) =>
        {
            var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            await handler.HandleAsync(userId, medicineId, request.Quantity, cancellationToken);

            return Results.NoContent();
        })
        .RequireAuthorization()
        .WithName("UpdateCartItem");

        app.MapDelete("/api/cart/items/{medicineId:guid}", async (
            Guid medicineId,
            RemoveCartItemHandler handler,
            ClaimsPrincipal user,
            CancellationToken cancellationToken) =>
        {
            var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            await handler.HandleAsync(userId, medicineId, cancellationToken);

            return Results.NoContent();
        })
        .RequireAuthorization()
        .WithName("RemoveCartItem");

        return app;
    }
}

public sealed record AddCartItemRequest(Guid MedicineId, int Quantity);

public sealed record UpdateCartItemRequest(int Quantity);
