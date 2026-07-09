using Budgexa.API;
using Budgexa.API.Endpoints;
using Budgexa.Application;
using Budgexa.Infrastructure;
using Budgexa.Infrastructure.Persistence;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Infrastructure;
using Scalar.AspNetCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();

builder.Services.AddPresentation();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy.WithOrigins(builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [])
            .AllowCredentials()
            .AllowAnyHeader()
            .AllowAnyMethod()
            .WithExposedHeaders("WWW-Authenticate");
    });
});

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddFixedWindowLimiter("PublicBudgetLimit", cfg =>
    {
        cfg.Window = TimeSpan.FromHours(1);
        cfg.PermitLimit = 10;
        cfg.QueueLimit = 0;
    });
    options.AddFixedWindowLimiter("PublicItemSearchLimit", cfg =>
    {
        cfg.Window = TimeSpan.FromMinutes(1);
        cfg.PermitLimit = 30;
        cfg.QueueLimit = 0;
    });
});

QuestPDF.Settings.License = LicenseType.Community;

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.EnableDarkMode();
        options.WithTheme(ScalarTheme.BluePlanet);
        options.AddPreferredSecuritySchemes("Bearer");
    });
}

app.UseSerilogRequestLogging();

app.UseExceptionHandler();

app.UseCors("Frontend");

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

app.MapAuthEndpoints();
app.MapUsersEndpoints();
app.MapRoleEndpoints();
app.MapCustomersEndpoints();
app.MapItemsEndpoints();
app.MapBudgetsEndpoints();
app.MapInvoicesEndpoints();
app.MapLanguagesEndpoints();
app.MapStatusEndpoints();
app.MapPublicBudgetsEndpoints();

// Automatically apply pending EF Core migrations at startup.
// This ensures the database schema is always up to date with the latest model changes.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    var retry = 30;

    while (true)
    {
        try
        {
            db.Database.Migrate();
            break;
        }
        catch
        {
            retry--;
            if (retry == 0) throw;

            Thread.Sleep(2000);
        }
    }
}

app.Run();
