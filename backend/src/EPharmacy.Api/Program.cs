using EPharmacy.Api.Endpoints;
using EPharmacy.Application;
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

app.Run();

public partial class Program;

