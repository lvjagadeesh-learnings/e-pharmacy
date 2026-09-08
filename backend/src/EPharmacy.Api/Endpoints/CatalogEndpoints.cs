using EPharmacy.Application;
using EPharmacy.Domain;

namespace EPharmacy.Api.Endpoints;

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

        return app;
    }

    private static object ToMedicineResponse(Medicine medicine) => new
    {
        id = medicine.Id,
        name = medicine.Name,
        description = medicine.Description,
        priceCents = medicine.PriceCents,
        imageUrl = medicine.ImageUrl,
    };
}
