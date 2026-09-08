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
            GetOrderHandler handler,
            ClaimsPrincipal user,
            CancellationToken cancellationToken) =>
        {
            var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var order = await handler.HandleAsync(userId, orderId, cancellationToken);

            if (order is null)
            {
                return Results.NotFound();
            }

            return Results.Ok(new
            {
                orderId = order.OrderId,
                referenceNumber = order.ReferenceNumber,
                shippingAddress = order.ShippingAddress,
                placedAtUtc = order.PlacedAtUtc,
                totalCents = order.TotalCents,
                status = order.Status.ToString(),
                statusHistory = order.StatusHistory.Select(e => new
                {
                    status = e.Status.ToString(),
                    reachedAtUtc = e.ReachedAtUtc,
                }),
                receivedAtUtc = order.ReceivedAtUtc,
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

        app.MapGet("/api/orders", async (
            ListOrdersHandler handler,
            ClaimsPrincipal user,
            CancellationToken cancellationToken) =>
        {
            var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var orders = await handler.HandleAsync(userId, cancellationToken);

            return Results.Ok(orders.Select(o => new
            {
                orderId = o.OrderId,
                referenceNumber = o.ReferenceNumber,
                placedAtUtc = o.PlacedAtUtc,
                totalCents = o.TotalCents,
                status = o.Status.ToString(),
            }));
        })
        .RequireAuthorization()
        .WithName("ListOrders");

        app.MapPost("/api/orders/{orderId:guid}/receive", async (
            Guid orderId,
            MarkOrderReceivedHandler handler,
            ClaimsPrincipal user,
            CancellationToken cancellationToken) =>
        {
            var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await handler.HandleAsync(userId, orderId, cancellationToken);

            return result.Status switch
            {
                MarkOrderReceivedStatus.NotFound => Results.NotFound(),
                MarkOrderReceivedStatus.NotYetDelivered => Results.Json(
                    new { error = "Order has not yet been delivered." },
                    statusCode: StatusCodes.Status409Conflict),
                MarkOrderReceivedStatus.AlreadyReceived => Results.Json(
                    new { error = "Order has already been marked as received." },
                    statusCode: StatusCodes.Status409Conflict),
                _ => Results.Ok(),
            };
        })
        .RequireAuthorization()
        .WithName("MarkOrderReceived");

        return app;
    }
}
