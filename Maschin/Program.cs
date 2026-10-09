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

// ── Render wach halten (siehe Helper/WachHalteDienst.cs) ──
builder.Services.AddHttpClient();
builder.Services.AddHostedService<WachHalteDienst>();

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

// ── Health-Check: antwortet sofort, ohne Datenbank, Session oder Views ──
// Wird vom Wachhalte-Dienst, vom GitHub-Workflow und optional von UptimeRobot aufgerufen.
// /health?db=1 prüft zusätzlich die Datenbank (für Monitoring, nicht für den Ping).
app.MapMethods("/health", new[] { "GET", "HEAD" }, async (HttpContext ctx, MaschinenDbContext db) =>
{
    ctx.Response.Headers.CacheControl = "no-store";

    if (ctx.Request.Query.ContainsKey("db"))
    {
        var ok = await db.Database.CanConnectAsync(ctx.RequestAborted);
        return ok ? Results.Text("OK (Datenbank erreichbar)") : Results.Text("Datenbank nicht erreichbar", statusCode: 503);
    }

    return Results.Text("OK");
});

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
