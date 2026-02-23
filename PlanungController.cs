using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MaschinenDataein.Models;
using MaschinenDataein.Models.Data;

namespace MaschinenDataein.Controllers
{
    public class PlanungController : Controller
    {
        private readonly MaschinenDbContext _context;

        public PlanungController(MaschinenDbContext context)
        {
            _context = context;
        }

        // ─── INDEX (GET) ─────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Index(
            DateTime? datum = null,
            long? maschinenId = null,
            CancellationToken ct = default)
        {
            var d = datum?.Date ?? DateTime.Today;

            // Maschinen-Dropdown
            var maschinen = await _context.Maschinen
                .AsNoTracking()
                .OrderBy(m => m.Bezeichnung)
                .Select(m => new SelectListItem
                {
                    Value = m.Id.ToString(),
                    Text = m.Bezeichnung
                })
                .ToListAsync(ct);

            maschinen.Insert(0, new SelectListItem { Value = "0", Text = "— Maschine wählen —" });
            ViewBag.Maschinen = maschinen;
            ViewBag.Datum = d.ToString("yyyy-MM-dd");
            ViewBag.MaschinenId = maschinenId ?? 0;

            // Liste filtern
            var query = _context.Planung
                .Include(p => p.Maschine)
                .AsNoTracking()
                .Where(p => p.Datum.Date == d);

            if (maschinenId.HasValue && maschinenId.Value > 0)
                query = query.Where(p => p.MaschinenId == maschinenId.Value);

            var items = await query
                .OrderByDescending(p => p.Id)
                .Take(500)
                .ToListAsync(ct);

            return View(items);
        }

        // ─── SPEICHERN (POST) ─────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(
            DateTime datum,
            long maschinenId,
            int? personalsoll,
            string? personalnamen,
            string? artikel,
            int? sollmenge,
            DateTime? mhd,
            string? kartonsanzahl,
            int? personalIst,
            int? fertigware,
            TimeSpan? starten,
            TimeSpan? stoppen,
            int? pause,
            CancellationToken ct)
        {
            if (maschinenId <= 0)
                maschinenId = await _context.Maschinen
                    .AsNoTracking()
                    .Select(m => m.Id)
                    .FirstOrDefaultAsync(ct);

            if (maschinenId <= 0)
            {
                TempData["Error"] = "Keine Maschine vorhanden. Bitte zuerst Maschinen anlegen.";
                return RedirectToAction(nameof(Index));
            }

            var entity = new Planungs
            {
                Datum = datum.Date,
                MaschinenId = maschinenId,
                Personalsoll = personalsoll,
                Personalnamen = personalnamen,
                Artikel = artikel,
                Sollmenge = sollmenge,
                MHD = mhd,
                Kartonsanzahl = kartonsanzahl,
                PersonalIst = personalIst,
                Fertigware = fertigware,
                Starten = starten,
                Stoppen = stoppen,
                Pause = pause
            };

            _context.Planung.Add(entity);
            await _context.SaveChangesAsync(ct);

            TempData["Success"] = "Planung erfolgreich gespeichert.";
            return RedirectToAction(nameof(Index), new
            {
                datum = datum.Date.ToString("yyyy-MM-dd"),
                maschinenId
            });
        }

        // ─── DELETE (POST) ────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(long id, CancellationToken ct)
        {
            var entity = await _context.Planung.FindAsync(new object[] { id }, ct);
            if (entity != null)
            {
                _context.Planung.Remove(entity);
                await _context.SaveChangesAsync(ct);
                TempData["Success"] = "Eintrag gelöscht.";
            }
            else
            {
                TempData["Error"] = "Eintrag nicht gefunden.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}