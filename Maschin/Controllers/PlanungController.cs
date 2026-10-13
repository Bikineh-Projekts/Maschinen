using System;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MaschinenDataein.Models;
using MaschinenDataein.Models.Data;
using MaschinenDataein.Helper;

namespace MaschinenDataein.Controllers
{
    public class PlanungController : Controller
    {
        private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;
        private static readonly string[] ZeitFormate = { @"hh\:mm", @"hh\:mm\:ss" };

        private readonly MaschinenDbContext _context;
        private readonly ILogger<PlanungController> _logger;

        public PlanungController(MaschinenDbContext context, ILogger<PlanungController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // ─── INDEX (GET) ─────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Index(
            DateTime? datum = null,
            long? maschinenId = null,
            CancellationToken ct = default)
        {
            var tag = datum?.Date ?? DateTime.Today;
            var folgetag = tag.AddDays(1);
            var id = maschinenId ?? 0;

            // Dieselbe Zwei-Maschinen-Auswahl wie im Leitstand verwenden.
            // So tauchen Seed-/Demo-Maschinen ohne Telemetrie nicht im Formular auf.
            ViewBag.Maschinen = DropDownListHelper.GetMaschinen(_context);
            ViewBag.Datum = tag.ToString("yyyy-MM-dd", Inv);
            ViewBag.MaschinenId = id;

            // Bereichsabfrage statt p.Datum.Date == d -> kann einen Index nutzen
            var query = _context.Planung
                .Include(p => p.Maschine)
                .AsNoTracking()
                .Where(p => p.Datum >= tag && p.Datum < folgetag);

            if (id > 0)
                query = query.Where(p => p.MaschinenId == id);

            var items = await query
                .OrderBy(p => p.MaschinenId)
                .ThenBy(p => p.Starten)
                .Take(500)
                .ToListAsync(ct);

            // Gespeicherte Störungen – neue Tabelle, daher abgesichert
            try
            {
                var stoerungen = _context.PlanungStoerungen
                    .Include(s => s.Maschine)
                    .AsNoTracking()
                    .Where(s => s.Datum >= tag && s.Datum < folgetag);

                if (id > 0)
                    stoerungen = stoerungen.Where(s => s.MaschinenId == id);

                ViewBag.Stoerungen = await stoerungen.OrderBy(s => s.Von).ToListAsync(ct);
            }
            catch (DbException ex)
            {
                _logger.LogWarning(ex, "Tabelle PlanungStoerungen nicht verfügbar");
                ViewBag.Stoerungen = new List<PlanungStoerungen>();
                ViewBag.StoerungenFehlt = true;
            }

            return View(items);
        }

        // ─── SPEICHERN (POST) ─────────────────────────────────────────
        // Die View schickt jede Tabellenzeile als eigenes Array-Element.
        // Vorher wurde nur die erste Produktionszeile gespeichert.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(
            DateTime? datum,
            long maschinenId,
            string? personalsoll,
            string? personalnamen,
            string[]? artikel,
            string[]? sollmenge,
            string[]? starten,
            string[]? stoppen,
            string[]? pause,
            string[]? mhd,
            string[]? kartonsanzahl,
            string[]? personalIst,
            string[]? fertigware,
            string[]? stoerungVon,
            string[]? stoerungBis,
            string[]? stoerungGrund,
            CancellationToken ct)
        {
            if (datum is null)
                return Fehler("Bitte ein gültiges Datum angeben.", null, maschinenId);

            var tag = datum.Value.Date;

            if (maschinenId <= 0 || !await _context.Maschinen.AnyAsync(m => m.Id == maschinenId, ct))
                return Fehler("Bitte eine Maschine auswählen.", tag, maschinenId);

            var soll = ParseInt(personalsoll);
            var namen = Kuerzen(personalnamen, 500);

            // ── Produktionszeilen ──
            var neuePlanungen = new List<Planungs>();
            for (int i = 0; i < (artikel?.Length ?? 0); i++)
            {
                var zeile = new[] { At(artikel, i), At(sollmenge, i), At(starten, i), At(stoppen, i), At(pause, i),
                                    At(mhd, i), At(kartonsanzahl, i), At(personalIst, i), At(fertigware, i) };
                if (zeile.All(string.IsNullOrWhiteSpace)) continue;

                var art = Kuerzen(At(artikel, i), 100);
                if (art is null)
                    return Fehler($"Produktionszeile {i + 1}: Die Artikel-Nr. fehlt.", tag, maschinenId);

                var p = new Planungs
                {
                    Datum = tag,
                    MaschinenId = maschinenId,
                    Personalsoll = soll,
                    Personalnamen = namen,
                    Artikel = art,
                    Sollmenge = ParseInt(At(sollmenge, i)),
                    Starten = ParseZeit(At(starten, i)),
                    Stoppen = ParseZeit(At(stoppen, i)),
                    Pause = ParseInt(At(pause, i)),
                    MHD = ParseDatum(At(mhd, i)),
                    Kartonsanzahl = Kuerzen(At(kartonsanzahl, i), 100),
                    PersonalIst = ParseInt(At(personalIst, i)),
                    Fertigware = ParseInt(At(fertigware, i))
                };

                if (LaufzeitMinuten(p) < 0)
                    return Fehler($"Produktionszeile {i + 1}: Die Pause ist länger als die Laufzeit.", tag, maschinenId);

                neuePlanungen.Add(p);
            }

            if (neuePlanungen.Count == 0)
                return Fehler("Bitte mindestens eine Produktionszeile ausfüllen.", tag, maschinenId);

            // ── Störungen ──
            var neueStoerungen = new List<PlanungStoerungen>();
            for (int i = 0; i < (stoerungVon?.Length ?? 0); i++)
            {
                var von = ParseZeit(At(stoerungVon, i));
                var bis = ParseZeit(At(stoerungBis, i));
                var grund = Kuerzen(At(stoerungGrund, i), 200);

                if (von is null && bis is null && grund is null) continue;
                if (von is null || bis is null)
                    return Fehler($"Störung {i + 1}: Bitte Von- und Bis-Zeit angeben.", tag, maschinenId);

                neueStoerungen.Add(new PlanungStoerungen
                {
                    Datum = tag,
                    MaschinenId = maschinenId,
                    Von = von.Value,
                    Bis = bis.Value,
                    Grund = grund
                });
            }

            _context.Planung.AddRange(neuePlanungen);
            await _context.SaveChangesAsync(ct);

            var meldung = $"{neuePlanungen.Count} Produktionszeile(n) gespeichert.";

            if (neueStoerungen.Count > 0)
            {
                try
                {
                    _context.PlanungStoerungen.AddRange(neueStoerungen);
                    await _context.SaveChangesAsync(ct);
                    meldung += $" {neueStoerungen.Count} Störung(en) gespeichert.";
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogError(ex, "Störungen konnten nicht gespeichert werden");
                    TempData["Error"] = "Die Störungen konnten nicht gespeichert werden. " +
                                        "Bitte die Tabelle PlanungStoerungen in der Datenbank anlegen.";
                }
            }

            TempData["Success"] = meldung;
            return RedirectToAction(nameof(Index), new { datum = tag.ToString("yyyy-MM-dd", Inv), maschinenId });
        }

        // ─── DELETE (POST) ────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(long id, DateTime? datum, long? maschinenId, CancellationToken ct)
        {
            var anzahl = await _context.Planung.Where(p => p.Id == id).ExecuteDeleteAsync(ct);

            if (anzahl > 0) TempData["Success"] = "Eintrag gelöscht.";
            else TempData["Error"] = "Eintrag nicht gefunden.";

            // Nach dem Löschen auf demselben Datum und derselben Maschine bleiben
            return RedirectToAction(nameof(Index), new { datum = datum?.ToString("yyyy-MM-dd", Inv), maschinenId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteStoerung(long id, DateTime? datum, long? maschinenId, CancellationToken ct)
        {
            var anzahl = await _context.PlanungStoerungen.Where(s => s.Id == id).ExecuteDeleteAsync(ct);

            if (anzahl > 0) TempData["Success"] = "Störung gelöscht.";
            else TempData["Error"] = "Störung nicht gefunden.";

            return RedirectToAction(nameof(Index), new { datum = datum?.ToString("yyyy-MM-dd", Inv), maschinenId });
        }

        // ─── HILFEN ───────────────────────────────────────────────────
        /// <summary>Netto-Laufzeit in Minuten (Bis – Von – Pause), auch über Mitternacht.</summary>
        public static int? LaufzeitMinuten(Planungs p)
        {
            if (p.Starten is null || p.Stoppen is null) return null;
            var diff = p.Stoppen.Value - p.Starten.Value;
            if (diff < TimeSpan.Zero) diff += TimeSpan.FromDays(1);
            return (int)diff.TotalMinutes - (p.Pause ?? 0);
        }

        private IActionResult Fehler(string meldung, DateTime? datum, long maschinenId)
        {
            TempData["Error"] = meldung;
            return RedirectToAction(nameof(Index), new { datum = datum?.ToString("yyyy-MM-dd", Inv), maschinenId });
        }

        private static string? At(string[]? werte, int i)
            => werte is not null && i < werte.Length ? werte[i] : null;

        private static string? Kuerzen(string? s, int max)
        {
            s = s?.Trim();
            if (string.IsNullOrEmpty(s)) return null;
            return s.Length <= max ? s : s[..max];
        }

        private static int? ParseInt(string? s)
            => int.TryParse(s, NumberStyles.Integer, Inv, out var v) && v >= 0 ? v : null;

        private static DateTime? ParseDatum(string? s)
            => DateTime.TryParseExact(s, "yyyy-MM-dd", Inv, DateTimeStyles.None, out var d) ? d : null;

        private static TimeSpan? ParseZeit(string? s)
            => TimeSpan.TryParseExact(s, ZeitFormate, Inv, out var t) && t < TimeSpan.FromDays(1) ? t : null;
    }
}
