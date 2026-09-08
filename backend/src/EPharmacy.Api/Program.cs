using EPharmacy.Api.Endpoints;
using EPharmacy.Application;
using EPharmacy.Domain;
using EPharmacy.Infrastructure;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("AppDb")));

builder.Services.AddScoped<IHealthCheckRepository, HealthCheckRepository>();
builder.Services.AddScoped<RecordHealthCheckHandler>();

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<RegisterUserHandler>();
builder.Services.AddScoped<AuthenticateUserHandler>();

builder.Services.AddScoped<IMedicineRepository, MedicineRepository>();
builder.Services.AddScoped<ListMedicinesHandler>();

builder.Services.AddScoped<ICartRepository, CartRepository>();
builder.Services.AddScoped<AddCartItemHandler>();
builder.Services.AddScoped<GetCartHandler>();
builder.Services.AddScoped<UpdateCartItemHandler>();
builder.Services.AddScoped<RemoveCartItemHandler>();

builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IPaymentGateway, FakePaymentGateway>();
builder.Services.AddScoped<PlaceOrderHandler>();

// Cookie auth, not JWT: a same-origin SPA (via the Vite dev proxy) doesn't
// need bearer tokens, and cookies let the browser handle session storage.
// API redirects (401/403) are returned instead of the default login-page
// redirects since this is a JSON API, not a server-rendered app.
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Events.OnRedirectToLogin = context =>
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        };
        options.Events.OnRedirectToAccessDenied = context =>
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        };
    });
builder.Services.AddAuthorization();

var app = builder.Build();

// Dev-only convenience: apply pending EF Core migrations at startup so a
// fresh SQLite database always has the current schema. Revisit before any
// production deployment (SQLite itself is a dev-only choice here).
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.Migrate();

    // Dev-only convenience: seed a fixed set of sample medicines if the
    // catalog is empty. There is no admin/catalog-management story yet, so
    // this is the only way sample data gets into a fresh database.
    if (!dbContext.Medicines.Any())
    {
        dbContext.Medicines.AddRange(
            Medicine.Create(Guid.NewGuid(), "Paracetamol 500mg", "Pain and fever relief tablets, 20 count.", 599, null),
            Medicine.Create(Guid.NewGuid(), "Ibuprofen 200mg", "Anti-inflammatory pain relief tablets, 24 count.", 749, null),
            Medicine.Create(Guid.NewGuid(), "Allergy Relief 10mg", "Non-drowsy antihistamine tablets, 30 count.", 899, null),
            Medicine.Create(Guid.NewGuid(), "Vitamin C 1000mg", "Immune support supplement, 60 tablets.", 1099, null),
            Medicine.Create(Guid.NewGuid(), "Cough Syrup 100ml", "Soothing relief for dry and chesty coughs.", 649, null),
            Medicine.Create(Guid.NewGuid(), "Multivitamin Daily", "Complete daily multivitamin, 90 tablets.", 1299, null));

        dbContext.SaveChanges();
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/health", async (RecordHealthCheckHandler handler, CancellationToken cancellationToken) =>
{
    var healthCheck = await handler.HandleAsync(cancellationToken);
    return Results.Ok(new { status = "healthy", checkedAtUtc = healthCheck.CheckedAtUtc });
})
.WithName("GetHealth");

app.MapAuthEndpoints();
app.MapCatalogEndpoints();
app.MapCartEndpoints();
app.MapOrderEndpoints();

app.Run();

public partial class Program;

