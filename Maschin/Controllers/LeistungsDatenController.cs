using System;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MaschinenDataein.Controllers.Model;
using MaschinenDataein.Helper;
using MaschinenDataein.Models;
using MaschinenDataein.Models.ModelView;
using MaschinenDataein.Models.PaginatedModel;
using X.PagedList;
using X.PagedList.Extensions;

namespace MaschinenDataein.Controllers
{
    public class LeistungsdatenController : ControllerModel
    {
        public LeistungsdatenController(MaschinenDbContext context)
            : base(context, "Leistungsdaten")
        {
        }

        public IActionResult Index()
        {
            _actionName = "Data";
            FilterModelView model = GetFilterModelViewCookie();
            SetViewBagFilter(model, "Data", "Leistungsdaten");
            ViewData["IsShowId"] = true;
            return View();
        }

        public IActionResult Data(int pageNumber)
        {
            _actionName = "Data";
            FilterModelView model = GetFilterModelViewCookie();
            SetViewBagFilter(model, "Index", "Leistungsdaten");

            var datumBis = model.DatumBis.Date.AddHours(23).AddMinutes(59).AddSeconds(59);

            var query = _context.Leistungsdaten
                .Include(x => x.Maschine)
                .Where(x => x.Timestamp >= model.DatumVon.Date && x.Timestamp <= datumBis);

            if (model.MaschinenId > 0)
                query = query.Where(x => x.MaschinenId == model.MaschinenId);

            var list = query.ToList();

            var modelView = list
                .Select(x => new LeistungsdatenModelView(_context, x)
                {
                    Name = ProgrammnamenHelper.GetName(x.MaschinenId, x.PRnummer)
                })
                .ToList();

            if (pageNumber <= 0) pageNumber = 1;

            var pagedList = modelView.ToPagedList(pageNumber, 50);
            modelView = pagedList.ToList();

            ViewData["PaginatedListItem"] = new PaginatedListItem(
                pagedList.PageNumber, pagedList.PageCount,
                pagedList.IsFirstPage, pagedList.IsLastPage, "Data");
            ViewData["IsShowId"] = true;

            return View("Index", modelView);
        }

        // ─────────────────────────────────────────────────────────────────────
        // JSON-Endpoint für das Leistungs-Chart-Dashboard
        //
        // GET /Leistungsdaten/GetChartData
        // GET /Leistungsdaten/GetChartData?datumVon=2024-12-25&datumBis=2024-12-26
        // GET /Leistungsdaten/GetChartData?datumVon=2024-12-25&datumBis=2024-12-26&maschinenId=2
        //
        // Gibt zurück:
        //   - punkte:    Zeitreihe mit Maschinentakten pro Timestamp
        //   - programme: Gruppierung nach Programm (PRnummer) mit Tages-Summe
        //   - tagesKurve: Maschinentakte stündlich aggregiert für Zoom-Chart
        // ─────────────────────────────────────────────────────────────────────
        [HttpGet]
        public IActionResult GetChartData(string? datumVon, string? datumBis, long? maschinenId)
        {
            // ── 1. Filter aus hidden fields (bereits yyyy-MM-dd) ──────────────
            FilterModelView model = GetFilterModelViewCookie();

            if (!string.IsNullOrWhiteSpace(datumVon))
            {
                var p = ParseDatum(datumVon);
                if (p.HasValue) model.DatumVon = p.Value;
            }
            if (!string.IsNullOrWhiteSpace(datumBis))
            {
                var p = ParseDatum(datumBis);
                if (p.HasValue) model.DatumBis = p.Value;
            }
            if (maschinenId.HasValue && maschinenId.Value > 0)
                model.MaschinenId = maschinenId.Value;

            // ── 2. Fallback ───────────────────────────────────────────────────
            bool vonLeer = model.DatumVon == default || model.DatumVon == DateTime.MinValue;
            bool bisLeer = model.DatumBis == default || model.DatumBis == DateTime.MinValue;
            if (vonLeer && bisLeer) { model.DatumBis = DateTime.Today; model.DatumVon = DateTime.Today.AddDays(-7); }
            else if (vonLeer)       model.DatumVon = model.DatumBis.AddDays(-7);
            else if (bisLeer)       model.DatumBis = DateTime.Today;

            if (model.DatumVon > model.DatumBis)
                (model.DatumVon, model.DatumBis) = (model.DatumBis, model.DatumVon);

            var vonStart = model.DatumVon.Date;
            var bisEnd   = model.DatumBis.Date.AddHours(23).AddMinutes(59).AddSeconds(59);

            // ── 3. Datenbankabfrage ───────────────────────────────────────────
            var query = _context.Leistungsdaten
                .Where(x => x.Timestamp >= vonStart && x.Timestamp <= bisEnd);

            if (model.MaschinenId > 0)
                query = query.Where(x => x.MaschinenId == model.MaschinenId);

            var list = query
                .OrderBy(x => x.Timestamp)
                .ToList();

            // ── 4. Zeitreihe: jeder Datenpunkt mit Uhrzeit ───────────────────
            //    Für den Zoom-Chart: Maschinentakte über Zeit
            var zeitreihe = list.Select(x => new
            {
                ts            = x.Timestamp.ToString("dd.MM HH:mm"),
                takte         = x.Maschinentakte,
                tagesZaehler  = x.Tagestaktzaehler,
                packungen     = x.Packungszaeler,
                prNummer      = x.PRnummer,
                programm      = ProgrammnamenHelper.GetName(x.MaschinenId, x.PRnummer)
            }).ToList();

            // ── 5. Programme: Tages-Summe Maschinentakte pro Programm ─────────
            var programme = list
                .GroupBy(x => new { x.PRnummer, x.MaschinenId })
                .Select(g => new
                {
                    prNummer    = g.Key.PRnummer,
                    name        = ProgrammnamenHelper.GetName(g.Key.MaschinenId, g.Key.PRnummer),
                    takteSumme  = g.Sum(x => x.Maschinentakte),
                    takteMax    = g.Max(x => x.Maschinentakte),
                    takteMin    = g.Min(x => x.Maschinentakte),
                    takteAvg    = Math.Round(g.Average(x => x.Maschinentakte), 1),
                    packungen   = g.Sum(x => x.Packungszaeler),
                    eintraege   = g.Count()
                })
                .OrderByDescending(x => x.takteSumme)
                .ToList();

            // ── 6. Stündliche Aggregation für Tages-Übersicht ─────────────────
            var stundenkurve = list
                .GroupBy(x => new
                {
                    Datum  = x.Timestamp.Date,
                    Stunde = x.Timestamp.Hour
                })
                .Select(g => new
                {
                    label       = g.Key.Datum.ToString("dd.MM") + " " + g.Key.Stunde.ToString("D2") + ":00",
                    takte       = g.Sum(x => x.Maschinentakte),
                    packungen   = g.Sum(x => x.Packungszaeler)
                })
                .OrderBy(x => x.label)
                .ToList();

            // ── 7. Maschinen-Liste für Dropdown ──────────────────────────────
            var maschinen = list
                .Where(x => x.Maschine != null)
                .Select(x => new { id = x.MaschinenId, name = x.Maschine!.Bezeichnung })
                .DistinctBy(x => x.id)
                .OrderBy(x => x.name)
                .ToList();

            return Json(new
            {
                zeitreihe,
                programme,
                stundenkurve,
                maschinen,
                debug = new
                {
                    datumVon          = model.DatumVon.ToString("dd.MM.yyyy"),
                    datumBis          = model.DatumBis.ToString("dd.MM.yyyy"),
                    maschinenId       = model.MaschinenId,
                    gesamtDatensaetze = list.Count,
                    anzahlProgramme   = programme.Count
                }
            });
        }

        // ─────────────────────────────────────────────────────────────────────
        private static DateTime? ParseDatum(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return null;
            input = input.Trim();
            string[] formate =
            [
                "yyyy-MM-dd", "dd.MM.yyyy", "d.M.yyyy", "d.MM.yyyy",
                "dd.M.yyyy",  "dd/MM/yyyy", "d/M/yyyy", "MM/dd/yyyy",
                "yyyy.MM.dd", "dd-MM-yyyy",
            ];
            if (DateTime.TryParseExact(input, formate, new CultureInfo("de-DE"),
                    DateTimeStyles.None, out var r1)) return r1.Date;
            if (DateTime.TryParseExact(input, formate, CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out var r2)) return r2.Date;
            if (DateTime.TryParse(input, new CultureInfo("de-DE"),
                    DateTimeStyles.None, out var r3)) return r3.Date;
            return null;
        }
    }
}
