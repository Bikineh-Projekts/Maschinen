using MaschinenDataein.Helper;
using MaschinenDataein.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// ── Datenbank: PostgreSQL ──
var connectionString = DatenbankHelper.LeseConnectionString(builder.Configuration);
var istEntwicklung = builder.Environment.IsDevelopment();

builder.Services.AddDbContext<MaschinenDbContext>(options =>
    options.UseNpgsql(connectionString, npgsql =>
    {
        // Beim Entwickeln keine Wiederholungen -> echte Fehlermeldung sofort sichtbar
        if (!istEntwicklung) npgsql.EnableRetryOnFailure(maxRetryCount: 3);
    }));

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// ── Verbindung beim Start prüfen: klare Meldung statt Fehler beim ersten Seitenaufruf ──
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<MaschinenDbContext>();
    var ziel = DatenbankHelper.Beschreibung(connectionString);

    try
    {
        await db.Database.OpenConnectionAsync();
        await db.Database.CloseConnectionAsync();
        app.Logger.LogInformation("PostgreSQL-Verbindung OK ({Ziel})", ziel);
    }
    catch (Exception ex)
    {
        // Genaue Ursache ausgeben (falsches Passwort, Datenbank fehlt, Server aus …)
        app.Logger.LogError("Keine Verbindung zu PostgreSQL ({Ziel}): {Fehler}", ziel, ex.GetBaseException().Message);
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();
app.UseRouting();

app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
