using System;
using System.Linq;
using System.Collections.Generic;
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
    public class TemperaturDatenController : ControllerModel
    {
        public TemperaturDatenController(MaschinenDbContext context)
            : base(context, "TemperaturDaten")
        {
        }

        public IActionResult Index()
        {
            _actionName = "Data";
            FilterModelView model = GetFilterModelViewCookie();
            SetViewBagFilter(model, "Data", "TemperaturDaten");
            ViewData["IsShowId"] = true;
            return View();
        }

        public IActionResult Data(int pageNumber)
        {
            _actionName = "Data";
            FilterModelView model = GetFilterModelViewCookie();
            SetViewBagFilter(model, "Index", "TemperaturDaten");

            var datumBis = model.DatumBis.AddHours(23).AddMinutes(59).AddSeconds(59);

            var query = _context.Temperaturdaten
                .Include(x => x.Maschine)
                .Where(x => x.Timestamp >= model.DatumVon && x.Timestamp <= datumBis);

            if (model.MaschinenId > 0)
                query = query.Where(x => x.MaschinenId == model.MaschinenId);

            var list = query.ToList();

            List<TemperaturdatenModelView> modelView = list
                .Select(x => new TemperaturdatenModelView
                {
                    Id = x.Id,
                    MaschinenId = x.MaschinenId,
                    Maschine = x.Maschine?.Bezeichnung,
                    Timestamp = x.Timestamp,
                    Name = ProgrammnamenHelper.GetName(x.MaschinenId, x.PRnummer),
                    Solltemp1 = x.Solltemp1,
                    Isstemp1 = x.Isstemp1,
                    Solltemp2 = x.Solltemp2,
                    Isstemp2 = x.Isstemp2
                })
                .ToList();

            if (pageNumber <= 0)
                pageNumber = 1;

            var pagedList = modelView.ToPagedList(pageNumber, 50);
            modelView = pagedList.ToList();

            PaginatedListItem paginatedListItem = new(
                pagedList.PageNumber,
                pagedList.PageCount,
                pagedList.IsFirstPage,
                pagedList.IsLastPage,
                "Data"
            );

            ViewData["PaginatedListItem"] = paginatedListItem;
            ViewData["IsShowId"] = true;

            return View("Index", modelView);
        }

        // ─────────────────────────────────────────────────────────────────────
        // JSON-Endpoint für das Analyse-Dashboard
        //
        // Aufruf-Varianten:
        //   GET /TemperaturDaten/GetChartData
        //   GET /TemperaturDaten/GetChartData?datumVon=01.01.2024&datumBis=31.12.2024
        //   GET /TemperaturDaten/GetChartData?datumVon=01.01.2024&datumBis=31.12.2024&maschinenId=3
        //
        // Priorität: Query-String > Cookie > Fallback (letzte 30 Tage)
        // ─────────────────────────────────────────────────────────────────────
        [HttpGet]
        public IActionResult GetChartData(string? datumVon, string? datumBis, long? maschinenId)
        {
            // ── 1. Basis: Cookie-Filter ────────────────────────────────────────
            FilterModelView model = GetFilterModelViewCookie();

            // ── 2. Query-String überschreibt Cookie wenn vorhanden ─────────────
            if (!string.IsNullOrWhiteSpace(datumVon) &&
                DateTime.TryParse(datumVon, System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None, out var parsedVon))
                model.DatumVon = parsedVon;

            if (!string.IsNullOrWhiteSpace(datumBis) &&
                DateTime.TryParse(datumBis, System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None, out var parsedBis))
                model.DatumBis = parsedBis;

            if (maschinenId.HasValue && maschinenId.Value > 0)
                model.MaschinenId = maschinenId.Value;

            // ── 3. Fallback: wenn Datum nie gesetzt wurde → letzte 30 Tage ─────
            bool datumLeerOderGleich = model.DatumVon == default
                                    || model.DatumBis == default
                                    || model.DatumVon >= model.DatumBis;
            if (datumLeerOderGleich)
            {
                model.DatumBis = DateTime.Today;
                model.DatumVon = DateTime.Today.AddDays(-30);
            }

            var datumBisEnd = model.DatumBis.AddHours(23).AddMinutes(59).AddSeconds(59);

            // ── 4. Datenbankabfrage ───────────────────────────────────────────
            var query = _context.Temperaturdaten
                .Where(x => x.Timestamp >= model.DatumVon && x.Timestamp <= datumBisEnd);

            if (model.MaschinenId > 0)
                query = query.Where(x => x.MaschinenId == model.MaschinenId);

            var list = query
                .OrderBy(x => x.Timestamp)
                .ToList();

            // ── 5. Downsampling: max. 500 Punkte für Performance ──────────────
            int step = Math.Max(1, list.Count / 500);
            var sampled = list.Where((_, i) => i % step == 0).ToList();

            // ── 6. Chart-Datenpunkte aufbauen ─────────────────────────────────
            var chartPoints = sampled.Select(x => new
            {
                t = x.Timestamp.ToString("dd.MM HH:mm"),
                soll1 = x.Solltemp1,
                ist1 = x.Isstemp1,
                fehler1 = x.Isstemp1 - x.Solltemp1,
                soll2 = x.Solltemp2,
                ist2 = x.Isstemp2,
                fehler2 = x.Isstemp2 - x.Solltemp2
            }).ToList();

            // ── 7. Statistik berechnen ────────────────────────────────────────
            var f1 = sampled.Select(x => (double)(x.Isstemp1 - x.Solltemp1)).ToList();
            var f2 = sampled.Select(x => (double)(x.Isstemp2 - x.Solltemp2)).ToList();

            return Json(new
            {
                punkte = chartPoints,
                statistik = new
                {
                    kanal1 = BerechneStatistik(f1),
                    kanal2 = BerechneStatistik(f2)
                },
                // Debug-Info damit man im Browser sehen kann was abgefragt wurde
                debug = new
                {
                    datumVonVerwendet = model.DatumVon.ToString("dd.MM.yyyy"),
                    datumBisVerwendet = model.DatumBis.ToString("dd.MM.yyyy"),
                    maschinenIdFilter = model.MaschinenId,
                    gesamtPunkteDatenbank = list.Count,
                    dargestelltePunkte = sampled.Count
                }
            });
        }

        // ─────────────────────────────────────────────────────────────────────
        // Statistik-Kennzahlen berechnen
        // ─────────────────────────────────────────────────────────────────────
        private static object BerechneStatistik(List<double> fehler)
        {
            if (fehler.Count == 0)
                return new { bias = 0.0, rmse = 0.0, std = 0.0, maxFehler = 0.0, anzahl = 0 };

            int n = fehler.Count;
            double bias = fehler.Average();
            double rmse = Math.Sqrt(fehler.Select(f => f * f).Average());
            double variance = fehler.Select(f => Math.Pow(f - bias, 2)).Average();
            double std = Math.Sqrt(variance);
            double maxAbs = fehler.Select(Math.Abs).Max();

            return new
            {
                bias = Math.Round(bias, 3),
                rmse = Math.Round(rmse, 3),
                std = Math.Round(std, 3),
                maxFehler = Math.Round(maxAbs, 3),
                anzahl = n
            };
        }
    }
}
