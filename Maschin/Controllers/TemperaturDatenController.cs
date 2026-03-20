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
                    Id          = x.Id,
                    MaschinenId = x.MaschinenId,
                    Maschine    = x.Maschine?.Bezeichnung,
                    Timestamp   = x.Timestamp,
                    Name        = ProgrammnamenHelper.GetName(x.MaschinenId, x.PRnummer),
                    Solltemp1   = x.Solltemp1,
                    Isstemp1    = x.Isstemp1,
                    Solltemp2   = x.Solltemp2,
                    Isstemp2    = x.Isstemp2
                })
                .ToList();

            if (pageNumber <= 0)
                pageNumber = 1;

            var pagedList = modelView.ToPagedList(pageNumber, 50);
            modelView     = pagedList.ToList();

            PaginatedListItem paginatedListItem = new(
                pagedList.PageNumber,
                pagedList.PageCount,
                pagedList.IsFirstPage,
                pagedList.IsLastPage,
                "Data"
            );

            ViewData["PaginatedListItem"] = paginatedListItem;
            ViewData["IsShowId"]          = true;

            return View("Index", modelView);
        }

        // ─────────────────────────────────────────────────────────────────────
        // JSON-Endpoint fuer das Analyse-Dashboard
        //
        // GET /TemperaturDaten/GetChartData
        // GET /TemperaturDaten/GetChartData?datumVon=2024-01-25&datumBis=2026-02-25
        // GET /TemperaturDaten/GetChartData?datumVon=25.01.2024&datumBis=25.02.2026&maschinenId=3
        //
        // Akzeptiert: yyyy-MM-dd (ISO), dd.MM.yyyy (DE), dd/MM/yyyy, MM/dd/yyyy
        // ─────────────────────────────────────────────────────────────────────
        [HttpGet]
        public IActionResult GetChartData(string? datumVon, string? datumBis, long? maschinenId)
        {
            // ── 1. Cookie-Filter als Basis ─────────────────────────────────────
            FilterModelView model = GetFilterModelViewCookie();

            // ── 2. Query-String ueberschreibt Cookie wenn vorhanden ─────────────
            if (!string.IsNullOrWhiteSpace(datumVon))
            {
                var parsed = ParseDatum(datumVon);
                if (parsed.HasValue) model.DatumVon = parsed.Value;
            }

            if (!string.IsNullOrWhiteSpace(datumBis))
            {
                var parsed = ParseDatum(datumBis);
                if (parsed.HasValue) model.DatumBis = parsed.Value;
            }

            if (maschinenId.HasValue && maschinenId.Value > 0)
                model.MaschinenId = maschinenId.Value;

            // ── 3. Sicherheits-Check: Datum plausibel? ─────────────────────────
            //
            //  FIX: Alter Code hatte  DatumVon >= DatumBis als Fallback-Bedingung.
            //       Das hat 1-Tages-Filter (Von = Bis = heute) faelschlicherweise
            //       auf "letzte 30 Tage" umgestellt.
            //       Neu: nur Fallback wenn Datum wirklich ungesetzt (default) ist.
            //
            bool vonUngesetzt = model.DatumVon == default || model.DatumVon == DateTime.MinValue;
            bool bisUngesetzt = model.DatumBis == default || model.DatumBis == DateTime.MinValue;

            if (vonUngesetzt && bisUngesetzt)
            {
                // Noch gar kein Filter gesetzt -> letzte 30 Tage als Standardwert
                model.DatumBis = DateTime.Today;
                model.DatumVon = DateTime.Today.AddDays(-30);
            }
            else if (vonUngesetzt)
            {
                model.DatumVon = model.DatumBis.AddDays(-30);
            }
            else if (bisUngesetzt)
            {
                model.DatumBis = DateTime.Today;
            }

            // Wenn Von > Bis: tauschen statt Fehler
            if (model.DatumVon > model.DatumBis)
                (model.DatumVon, model.DatumBis) = (model.DatumBis, model.DatumVon);

            // Zeit-Anteil normalisieren (nur Datum zaehlt)
            var vonStart  = model.DatumVon.Date;
            var bisEnd    = model.DatumBis.Date.AddHours(23).AddMinutes(59).AddSeconds(59);

            // ── 4. Datenbankabfrage ────────────────────────────────────────────
            var query = _context.Temperaturdaten
                .Where(x => x.Timestamp >= vonStart && x.Timestamp <= bisEnd);

            if (model.MaschinenId > 0)
                query = query.Where(x => x.MaschinenId == model.MaschinenId);

            var list = query
                .OrderBy(x => x.Timestamp)
                .ToList();

            // ── 5. Downsampling: max. 500 Punkte fuer Chart-Performance ────────
            int step    = Math.Max(1, list.Count / 500);
            var sampled = list.Where((_, i) => i % step == 0).ToList();

            // ── 6. Chart-Datenpunkte ───────────────────────────────────────────
            var chartPoints = sampled.Select(x => new
            {
                t       = x.Timestamp.ToString("dd.MM HH:mm"),
                soll1   = x.Solltemp1,
                ist1    = x.Isstemp1,
                fehler1 = x.Isstemp1 - x.Solltemp1,
                soll2   = x.Solltemp2,
                ist2    = x.Isstemp2,
                fehler2 = x.Isstemp2 - x.Solltemp2
            }).ToList();

            // ── 7. Statistik ───────────────────────────────────────────────────
            var f1 = sampled.Select(x => (double)(x.Isstemp1 - x.Solltemp1)).ToList();
            var f2 = sampled.Select(x => (double)(x.Isstemp2 - x.Solltemp2)).ToList();

            return Json(new
            {
                punkte    = chartPoints,
                statistik = new
                {
                    kanal1 = BerechneStatistik(f1),
                    kanal2 = BerechneStatistik(f2)
                },
                debug = new
                {
                    datumVonVerwendet     = model.DatumVon.ToString("dd.MM.yyyy"),
                    datumBisVerwendet     = model.DatumBis.ToString("dd.MM.yyyy"),
                    maschinenIdFilter     = model.MaschinenId,
                    gesamtPunkteDatenbank = list.Count,
                    dargestelltePunkte    = sampled.Count,
                    empfangenDatumVon     = datumVon ?? "(aus Cookie)",
                    empfangenDatumBis     = datumBis ?? "(aus Cookie)"
                }
            });
        }

        // ─────────────────────────────────────────────────────────────────────
        // Robustes Datum-Parsing
        // Unterstuetzt: yyyy-MM-dd / dd.MM.yyyy / dd/MM/yyyy / MM/dd/yyyy
        // ─────────────────────────────────────────────────────────────────────
        private static DateTime? ParseDatum(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return null;
            input = input.Trim();

            string[] formate =
            [
                "yyyy-MM-dd",             // ISO  -- von JavaScript
                "dd.MM.yyyy",             // DE   -- von Dropdowns
                "d.M.yyyy",               // DE kurz (1.1.2024)
                "d.MM.yyyy",              // DE gemischt
                "dd.M.yyyy",              // DE gemischt
                "dd/MM/yyyy",             // AT
                "d/M/yyyy",
                "MM/dd/yyyy",             // US
                "M/d/yyyy",
                "yyyy.MM.dd",
                "dd-MM-yyyy",
                "yyyy-MM-ddTHH:mm:ss",    // ISO mit Zeit
                "yyyy-MM-ddTHH:mm:ssZ",
            ];

            // Erst Deutsch versuchen (dd.MM bevorzugt vor MM/dd)
            if (DateTime.TryParseExact(input, formate,
                    new CultureInfo("de-DE"), DateTimeStyles.None, out var deResult))
                return deResult.Date;

            // Dann InvariantCulture (deckt ISO ab)
            if (DateTime.TryParseExact(input, formate,
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out var invResult))
                return invResult.Date;

            // Generischer Fallback
            if (DateTime.TryParse(input, new CultureInfo("de-DE"),
                    DateTimeStyles.None, out var fallback))
                return fallback.Date;

            return null;
        }

        // ─────────────────────────────────────────────────────────────────────
        // Statistik-Kennzahlen berechnen
        // ─────────────────────────────────────────────────────────────────────
        private static object BerechneStatistik(List<double> fehler)
        {
            if (fehler.Count == 0)
                return new { bias = 0.0, rmse = 0.0, std = 0.0, maxFehler = 0.0, anzahl = 0 };

            int    n        = fehler.Count;
            double bias     = fehler.Average();
            double rmse     = Math.Sqrt(fehler.Select(f => f * f).Average());
            double variance = fehler.Select(f => Math.Pow(f - bias, 2)).Average();
            double std      = Math.Sqrt(variance);
            double maxAbs   = fehler.Select(Math.Abs).Max();

            return new
            {
                bias      = Math.Round(bias,   3),
                rmse      = Math.Round(rmse,   3),
                std       = Math.Round(std,    3),
                maxFehler = Math.Round(maxAbs, 3),
                anzahl    = n
            };
        }
    }
}
