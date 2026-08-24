using Budgexa.API;
using Budgexa.API.Endpoints;
using Budgexa.Application;
using Budgexa.Infrastructure;
using Budgexa.Infrastructure.Persistence;
using Budgexa.Infrastructure.Services.FileStorage;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
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
        if (builder.Configuration.GetValue<bool>("Scalar:ShowProductionServer"))
        {
            options.AddServer("https://budgexaclient.duckdns.org", "Production");
        }
    });
}

app.UseSerilogRequestLogging();

app.UseExceptionHandler();

app.UseCors("Frontend");

// Serve uploaded profile images and signature images as static files
var fileStorageSettings = builder.Configuration
    .GetSection(FileStorageSettings.SectionName)
    .Get<FileStorageSettings>() ?? new FileStorageSettings();

var profileImagesPath = Path.GetFullPath(fileStorageSettings.ProfileImagesPath);
Directory.CreateDirectory(profileImagesPath);

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(profileImagesPath),
    RequestPath = "/profile-images"
});

var signatureImagesPath = Path.GetFullPath(fileStorageSettings.SignatureImagesPath);
Directory.CreateDirectory(signatureImagesPath);

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(signatureImagesPath),
    RequestPath = "/signature-images"
});

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
app.MapCompanyEndpoints();

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
