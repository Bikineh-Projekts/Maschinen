using System;
using System.Linq;
using System.Globalization;
using System.Collections.Generic;
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
    public class LeistungsdatenController : ControllerModel   // ← ControllerModel, nicht Controller!
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

            var aktiveMaschinenIds = DropDownListHelper.GetAktiveMaschinen(_context).Select(m => m.Id).ToList();

            var query = _context.Leistungsdaten
                .Include(x => x.Maschine)
                .ImZeitraum(model.DatumVon, model.DatumBis)
                .FuerAktiveMaschinen(model.MaschinenId, aktiveMaschinenIds)
                .NeuesteZuerst();

            if (pageNumber <= 0) pageNumber = 1;

            // Nur die aktuelle Seite aus der Datenbank laden
            var pagedList = query.ToPagedList(pageNumber, 50);

            var modelView = pagedList
                .Select(x => new LeistungsdatenModelView(_context, x)
                {
                    Name = ProgrammnamenHelper.GetName(x.MaschinenId, x.PRnummer)
                })
                .ToList();

            ViewData["PaginatedListItem"] = new PaginatedListItem(
                pagedList.PageNumber, pagedList.PageCount,
                pagedList.IsFirstPage, pagedList.IsLastPage, "Data")
            {
                TotalItemCount  = pagedList.TotalItemCount,
                FirstItemOnPage = pagedList.FirstItemOnPage,
                LastItemOnPage  = pagedList.LastItemOnPage
            };
            ViewData["IsShowId"] = true;

            return View("Index", modelView);
        }

        // ─────────────────────────────────────────────────────────────────────
        // GET /Leistungsdaten/GetChartData
        // GET /Leistungsdaten/GetChartData?datumVon=2024-12-25&datumBis=2024-12-26&maschinenId=2
        // ─────────────────────────────────────────────────────────────────────
        [HttpGet]
        public IActionResult GetChartData(string? datumVon, string? datumBis, long? maschinenId)
        {
            try
            {
                // Das Dropdown sendet 0 für "Beide Maschinen". Auch diese Auswahl muss
                // den gespeicherten Filter überschreiben können.
                FilterModelView model = GetFilterModelViewCookie();

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

                if (maschinenId.HasValue && maschinenId.Value >= 0)
                    model.MaschinenId = maschinenId.Value;

                var vonLeer = model.DatumVon == default || model.DatumVon == DateTime.MinValue;
                var bisLeer = model.DatumBis == default || model.DatumBis == DateTime.MinValue;

                if (vonLeer || bisLeer)
                {
                    var letzteAbfrage = _context.Leistungsdaten.AsNoTracking();
                    if (model.MaschinenId > 0)
                        letzteAbfrage = letzteAbfrage.Where(x => x.MaschinenId == model.MaschinenId);

                    var letzterTimestamp = letzteAbfrage
                        .OrderByDescending(x => x.Timestamp)
                        .Select(x => (DateTime?)x.Timestamp)
                        .FirstOrDefault();

                    var basisDatum = letzterTimestamp?.Date ?? DateTime.Today;
                    if (vonLeer && bisLeer)
                    {
                        model.DatumBis = basisDatum;
                        model.DatumVon = basisDatum.AddDays(-7);
                    }
                    else if (vonLeer)
                    {
                        model.DatumVon = model.DatumBis.AddDays(-7);
                    }
                    else
                    {
                        model.DatumBis = basisDatum;
                    }
                }

                if (model.DatumVon > model.DatumBis)
                    (model.DatumVon, model.DatumBis) = (model.DatumBis, model.DatumVon);

                var vonStart = model.DatumVon.Date;
                var bisExklusiv = model.DatumBis.Date.AddDays(1);

                var aktiveIds = DropDownListHelper.GetAktiveMaschinen(_context).Select(m => m.Id).ToList();
                var query = _context.Leistungsdaten
                    .AsNoTracking()
                    .Where(x => x.Timestamp >= vonStart && x.Timestamp < bisExklusiv);

                if (model.MaschinenId > 0)
                    query = query.Where(x => x.MaschinenId == model.MaschinenId);
                else
                    query = query.Where(x => aktiveIds.Contains(x.MaschinenId));

                // Neueste 2.000 Datensätze für den Chart; danach wieder zeitlich aufsteigend.
                // Anders als vorher wird bei einem leeren Zeitraum NICHT stillschweigend auf
                // andere Tage gewechselt: der Filter muss das zeigen, was der Benutzer auswählte.
                var roheDaten = query
                    .OrderByDescending(x => x.Timestamp)
                    .ThenByDescending(x => x.Id)
                    .Take(2001)
                    .ToList();

                var mehrAlsLimit = roheDaten.Count > 2000;
                if (mehrAlsLimit) roheDaten.RemoveAt(roheDaten.Count - 1);
                roheDaten.Reverse();

                var maschinenIds = roheDaten.Select(x => x.MaschinenId).Distinct().ToList();
                var maschinenNamen = _context.Maschinen.AsNoTracking()
                    .Where(m => maschinenIds.Contains(m.Id))
                    .ToDictionary(m => m.Id, m => m.Bezeichnung);

                string MaschinenName(long id) =>
                    maschinenNamen.TryGetValue(id, out var name) && !string.IsNullOrWhiteSpace(name)
                        ? name
                        : $"Maschine {id}";

                string ProgrammName(long id, int pr) =>
                    string.IsNullOrWhiteSpace(ProgrammnamenHelper.GetName(id, pr))
                        ? $"PR {pr}"
                        : ProgrammnamenHelper.GetName(id, pr).Trim();

                string AnzeigeName(long id, int pr) =>
                    model.MaschinenId > 0
                        ? ProgrammName(id, pr)
                        : $"{ProgrammName(id, pr)} · {MaschinenName(id)}";

                // Die Datenbank speichert an dieser Stelle Zählerstände, keine pro Messung
                // produzierten Mengen. Darum berechnen wir Deltas je Maschine; bei einem
                // Rückgang behandeln wir den Wert als Zählerreset (neue Menge = aktueller Stand).
                var vorherigeZaehler = new Dictionary<long, (int Packungen, int Takte)>();
                foreach (var ersteZeile in roheDaten
                    .GroupBy(x => x.MaschinenId)
                    .Select(g => g.First()))
                {
                    var vorher = _context.Leistungsdaten.AsNoTracking()
                        .Where(x => x.MaschinenId == ersteZeile.MaschinenId &&
                                    x.Timestamp < ersteZeile.Timestamp)
                        .OrderByDescending(x => x.Timestamp)
                        .ThenByDescending(x => x.Id)
                        .Select(x => new { x.Packungszaeler, x.Maschinentakte })
                        .FirstOrDefault();

                    if (vorher != null)
                        vorherigeZaehler[ersteZeile.MaschinenId] = (vorher.Packungszaeler, vorher.Maschinentakte);
                }

                static long ZaehlerDelta(int? vorher, int aktuell)
                {
                    var neu = Math.Max(0, aktuell);
                    if (!vorher.HasValue) return neu;

                    var alt = Math.Max(0, vorher.Value);
                    return neu >= alt ? (long)neu - alt : neu;
                }

                var normalisiert = new List<(DateTime Timestamp, long MaschinenId, int PRnummer,
                    long PackungenDelta, long TakteDelta, int TagesZaehler)>();

                foreach (var x in roheDaten)
                {
                    vorherigeZaehler.TryGetValue(x.MaschinenId, out var vorher);
                    var hatVorher = vorherigeZaehler.ContainsKey(x.MaschinenId);

                    var deltaPackungen = ZaehlerDelta(hatVorher ? vorher.Packungen : null, x.Packungszaeler);
                    var deltaTakte = ZaehlerDelta(hatVorher ? vorher.Takte : null, x.Maschinentakte);

                    normalisiert.Add((x.Timestamp, x.MaschinenId, x.PRnummer,
                        deltaPackungen, deltaTakte, x.Tagestaktzaehler));

                    vorherigeZaehler[x.MaschinenId] = (x.Packungszaeler, x.Maschinentakte);
                }

                var zeitreihe = normalisiert
                    .Select(x => new
                    {
                        ts = x.Timestamp.ToString("dd.MM HH:mm:ss"),
                        maschinenId = x.MaschinenId,
                        maschine = MaschinenName(x.MaschinenId),
                        takte = x.TakteDelta,
                        packungen = x.PackungenDelta,
                        tagesZaehler = x.TagesZaehler,
                        prNummer = x.PRnummer,
                        programm = AnzeigeName(x.MaschinenId, x.PRnummer)
                    })
                    .ToList();

                var programme = normalisiert
                    .GroupBy(x => new { x.PRnummer, x.MaschinenId })
                    .Select(g => new
                    {
                        prNummer = g.Key.PRnummer,
                        maschinenId = g.Key.MaschinenId,
                        key = $"{g.Key.MaschinenId}:{g.Key.PRnummer}",
                        maschine = MaschinenName(g.Key.MaschinenId),
                        name = AnzeigeName(g.Key.MaschinenId, g.Key.PRnummer),
                        takteSumme = g.Sum(x => x.TakteDelta),
                        takteMax = g.Max(x => x.TagesZaehler),
                        takteMin = g.Min(x => x.TagesZaehler),
                        takteAvg = Math.Round(g.Average(x => (double)x.TagesZaehler), 1),
                        packungen = g.Sum(x => x.PackungenDelta),
                        eintraege = g.Count()
                    })
                    .OrderByDescending(x => x.packungen)
                    .ToList();

                var stundenkurve = normalisiert
                    .GroupBy(x => new
                    {
                        Datum = x.Timestamp.Date,
                        Stunde = x.Timestamp.Hour,
                        x.MaschinenId
                    })
                    .Select(g => new
                    {
                        label = g.Key.Datum.ToString("dd.MM") + " " + g.Key.Stunde.ToString("D2") + ":00",
                        datum = g.Key.Datum.ToString("yyyy-MM-dd"),
                        maschinenId = g.Key.MaschinenId,
                        maschine = MaschinenName(g.Key.MaschinenId),
                        takte = g.Sum(x => x.TakteDelta),
                        packungen = g.Sum(x => x.PackungenDelta)
                    })
                    .OrderBy(x => x.datum)
                    .ThenBy(x => x.label)
                    .ThenBy(x => x.maschinenId)
                    .ToList();

                var tagesPackungen = normalisiert
                    .GroupBy(x => new
                    {
                        Datum = x.Timestamp.Date,
                        x.PRnummer,
                        x.MaschinenId
                    })
                    .Select(g => new
                    {
                        datum = g.Key.Datum.ToString("dd.MM.yyyy"),
                        datumSort = g.Key.Datum.ToString("yyyy-MM-dd"),
                        prNummer = g.Key.PRnummer,
                        maschinenId = g.Key.MaschinenId,
                        maschine = MaschinenName(g.Key.MaschinenId),
                        key = $"{g.Key.MaschinenId}:{g.Key.PRnummer}",
                        name = AnzeigeName(g.Key.MaschinenId, g.Key.PRnummer),
                        packungen = g.Sum(x => x.PackungenDelta),
                        takte = g.Sum(x => x.TakteDelta),
                        tagesMax = g.Max(x => x.TagesZaehler)
                    })
                    .OrderBy(x => x.datumSort)
                    .ThenByDescending(x => x.packungen)
                    .ToList();

                // Relative Ausgabe je Maschinen-/Programmgruppe. Das ist KEIN OEE- oder
                // Wirkungsgradwert, weil Laufzeit und Soll-Takt im Schema fehlen.
                var maxPackungen = programme.Count > 0 ? programme.Max(x => x.packungen) : 0;
                var leistungProzent = programme
                    .Select(g => new
                    {
                        prNummer = g.prNummer,
                        maschinenId = g.maschinenId,
                        key = g.key,
                        maschine = g.maschine,
                        name = g.name,
                        packungen = g.packungen,
                        takteSumme = g.takteSumme,
                        takteMax = g.takteMax,
                        leistungPct = maxPackungen > 0
                            ? Math.Round(g.packungen * 100.0 / maxPackungen, 1)
                            : 0.0
                    })
                    .OrderByDescending(x => x.packungen)
                    .ToList();

                // Downsampling nur für die Zeitreihe; Summen und Ranglisten basieren weiter
                // auf allen bis zu 2.000 geladenen Messpunkten.
                var chartSchritt = Math.Max(1, (int)Math.Ceiling(zeitreihe.Count / 500.0));
                var zeitreiheChart = zeitreihe.Where((_, i) => i % chartSchritt == 0).ToList();

                return Json(new
                {
                    zeitreihe = zeitreiheChart,
                    programme,
                    stundenkurve,
                    tagesPackungen,
                    leistungProzent,
                    debug = new
                    {
                        datumVon = model.DatumVon.ToString("dd.MM.yyyy"),
                        datumBis = model.DatumBis.ToString("dd.MM.yyyy"),
                        maschinenId = model.MaschinenId,
                        gesamtDatensaetze = roheDaten.Count,
                        dargestelltePunkte = zeitreiheChart.Count,
                        anzahlProgramme = programme.Count,
                        mehrAlsLimit,
                        maschinenAnzahl = maschinenIds.Count
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    zeitreihe = new List<object>(),
                    programme = new List<object>(),
                    stundenkurve = new List<object>(),
                    tagesPackungen = new List<object>(),
                    leistungProzent = new List<object>(),
                    debug = new { error = ex.Message }
                });
            }
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
