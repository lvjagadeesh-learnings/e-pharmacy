using System.Security.Claims;
using EPharmacy.Application;
using EPharmacy.Domain;

namespace EPharmacy.Api.Endpoints;

public sealed record SubmitReviewRequest(int Rating, string Comment);

public static class CatalogEndpoints
{
    public static WebApplication MapCatalogEndpoints(this WebApplication app)
    {
        app.MapGet("/api/medicines", async (ListMedicinesHandler handler, CancellationToken cancellationToken) =>
        {
            var medicines = await handler.HandleAsync(cancellationToken);
            return Results.Ok(medicines.Select(ToMedicineResponse));
        })
        .WithName("ListMedicines");

        app.MapGet("/api/medicines/{medicineId:guid}/reviews", async (
            Guid medicineId,
            ListReviewsForMedicineHandler handler,
            CancellationToken cancellationToken) =>
        {
            var reviews = await handler.HandleAsync(medicineId, cancellationToken);
            return Results.Ok(reviews.Select(ToReviewResponse));
        })
        .WithName("ListReviewsForMedicine");

        app.MapPost("/api/medicines/{medicineId:guid}/reviews", async (
            Guid medicineId,
            SubmitReviewRequest request,
            SubmitReviewHandler handler,
            ClaimsPrincipal user,
            CancellationToken cancellationToken) =>
        {
            if (request.Rating is < 1 or > 5 || string.IsNullOrWhiteSpace(request.Comment))
            {
                return Results.BadRequest(new { error = "A rating between 1 and 5 and a non-empty comment are required." });
            }

            var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await handler.HandleAsync(userId, medicineId, request.Rating, request.Comment, cancellationToken);

            return result.Status switch
            {
                SubmitReviewStatus.MedicineNotFound => Results.NotFound(new { error = "Medicine not found." }),
                SubmitReviewStatus.NotEligible => Results.Json(
                    new { error = "You can only review a medicine from a delivered order." },
                    statusCode: StatusCodes.Status403Forbidden),
                SubmitReviewStatus.AlreadyReviewed => Results.Json(
                    new { error = "You've already reviewed this item." },
                    statusCode: StatusCodes.Status409Conflict),
                _ => Results.Json(ToReviewResponse(result.Review!), statusCode: StatusCodes.Status201Created),
            };
        })
        .RequireAuthorization()
        .WithName("SubmitReview");

        return app;
    }

    private static object ToMedicineResponse(MedicineDto medicine) => new
    {
        id = medicine.Id,
        name = medicine.Name,
        description = medicine.Description,
        priceCents = medicine.PriceCents,
        imageUrl = medicine.ImageUrl,
        averageRating = medicine.AverageRating,
        reviewCount = medicine.ReviewCount,
    };

    private static object ToReviewResponse(ReviewDto review) => new
    {
        id = review.Id,
        reviewerDisplayName = review.ReviewerDisplayName,
        rating = review.Rating,
        comment = review.Comment,
        createdAtUtc = review.CreatedAtUtc,
    };

    private static object ToReviewResponse(Review review) => new
    {
        id = review.Id,
        reviewerDisplayName = review.ReviewerDisplayName,
        rating = review.Rating,
        comment = review.Comment,
        createdAtUtc = review.CreatedAtUtc,
    };
}
