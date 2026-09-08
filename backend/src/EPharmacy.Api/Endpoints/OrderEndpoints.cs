using System.Security.Claims;
using EPharmacy.Application;

namespace EPharmacy.Api.Endpoints;

public sealed record CheckoutRequest(string ShippingAddress, string CardNumber, string Expiry, string Cvc);

public static class OrderEndpoints
{
    public static WebApplication MapOrderEndpoints(this WebApplication app)
    {
        app.MapPost("/api/checkout", async (
            CheckoutRequest request,
            PlaceOrderHandler handler,
            ClaimsPrincipal user,
            CancellationToken cancellationToken) =>
        {
            if (string.IsNullOrWhiteSpace(request.ShippingAddress)
                || string.IsNullOrWhiteSpace(request.CardNumber)
                || string.IsNullOrWhiteSpace(request.Expiry)
                || string.IsNullOrWhiteSpace(request.Cvc))
            {
                return Results.BadRequest(new { error = "Shipping address and card details are required." });
            }

            var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await handler.HandleAsync(
                userId,
                request.ShippingAddress,
                request.CardNumber,
                request.Expiry,
                request.Cvc,
                cancellationToken);

            return result.Status switch
            {
                PlaceOrderStatus.EmptyCart => Results.BadRequest(new { error = "Your cart is empty." }),
                PlaceOrderStatus.PaymentDeclined => Results.Json(
                    new { error = result.FailureReason ?? "Payment was declined." },
                    statusCode: StatusCodes.Status402PaymentRequired),
                _ => Results.Created(
                    $"/api/orders/{result.OrderId}",
                    new
                    {
                        orderId = result.OrderId,
                        referenceNumber = result.OrderId!.Value.ToString("N")[..8].ToUpperInvariant(),
                        totalCents = result.TotalCents,
                    }),
            };
        })
        .RequireAuthorization()
        .WithName("Checkout");

        app.MapGet("/api/orders/{orderId:guid}", async (
            Guid orderId,
            IOrderRepository orderRepository,
            ClaimsPrincipal user,
            CancellationToken cancellationToken) =>
        {
            var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var order = await orderRepository.FindByIdAsync(orderId, cancellationToken);

            if (order is null || order.UserId != userId)
            {
                return Results.NotFound();
            }

            return Results.Ok(new
            {
                orderId = order.Id,
                referenceNumber = order.Id.ToString("N")[..8].ToUpperInvariant(),
                shippingAddress = order.ShippingAddress,
                placedAtUtc = order.PlacedAtUtc,
                totalCents = order.TotalCents,
                items = order.Items.Select(item => new
                {
                    medicineId = item.MedicineId,
                    name = item.Name,
                    unitPriceCents = item.UnitPriceCents,
                    quantity = item.Quantity,
                    lineTotalCents = item.UnitPriceCents * item.Quantity,
                }),
            });
        })
        .RequireAuthorization()
        .WithName("GetOrder");

        return app;
    }
}
