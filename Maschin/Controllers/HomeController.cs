using System.Diagnostics;
using MaschinenDataein.Models;
using MaschinenDataein.Models.Data;
using MaschinenDataein.Models.ModelView;
using MaschinenDataein.Helper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MaschinenDataein.Controllers
{
    public class HomeController : Controller
    {
        private readonly MaschinenDbContext _context;
        private readonly ILogger<HomeController> _logger;

        public HomeController(MaschinenDbContext context, ILogger<HomeController> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        public async Task<IActionResult> Index(CancellationToken ct)
        {
            var model = new DashboardModelView();

            try
            {
                // Die Startseite soll genau die zwei zuletzt aktiven Maschinen zeigen.
                // So erscheinen leere Demo-Stammdatensätze nicht als zusätzliche Maschine.
                var maschinen = DropDownListHelper.GetAktiveMaschinen(_context);
                var maschinenIds = maschinen.Select(m => m.Id).ToList();

                foreach (var maschine in maschinen)
                    model.Kacheln.Add(await BaueKachelAsync(maschine, ct));

                model.Ereignisse = await LadeEreignisseAsync(maschinenIds, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fehler beim Laden der Startseite");
                ViewBag.Fehler = "Die Daten konnten nicht geladen werden. Bitte später erneut versuchen.";
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public Task<IActionResult> Index(string? dummy, CancellationToken ct) => Index(ct);

        // ─────────────────────────────────────────────────────────────────────
        // Kachel einer Maschine: jeweils der neueste Datensatz je Tabelle
        // ─────────────────────────────────────────────────────────────────────
        private async Task<MaschinenKachel> BaueKachelAsync(Maschine maschine, CancellationToken ct)
        {
            var id = maschine.Id;
            var kachel = new MaschinenKachel { MaschinenId = id, Bezeichnung = maschine.Bezeichnung };

            var leistung = await _context.Leistungsdaten.AsNoTracking()
                .Where(x => x.MaschinenId == id).OrderByDescending(x => x.Timestamp).FirstOrDefaultAsync(ct);
            var alarm = await _context.Alarmdaten.AsNoTracking()
                .Where(x => x.MaschinenId == id).OrderByDescending(x => x.Timestamp).FirstOrDefaultAsync(ct);
            var zustand = await _context.Zustandsdaten.AsNoTracking().Include(x => x.Zustandsmeldung)
                .Where(x => x.MaschinenId == id).OrderByDescending(x => x.Timestamp).FirstOrDefaultAsync(ct);
            var stoerung = await _context.Stoerungsdaten.AsNoTracking().Include(x => x.Stoerungsmeldung)
                .Where(x => x.MaschinenId == id).OrderByDescending(x => x.Timestamp).FirstOrDefaultAsync(ct);
            var temperatur = await _context.Temperaturdaten.AsNoTracking()
                .Where(x => x.MaschinenId == id).OrderByDescending(x => x.Timestamp).FirstOrDefaultAsync(ct);

            // Produktion am Tag des neuesten Leistungsdatensatzes
            if (leistung != null)
            {
                kachel.Programm = LeerZuNull(ProgrammnamenHelper.GetName(id, leistung.PRnummer)) ?? $"Programm {leistung.PRnummer}";

                var tag = leistung.Timestamp.Date;
                var folgetag = tag.AddDays(1);
                kachel.ProduktionsTag = tag;

                var tagesWerte = await _context.Leistungsdaten.AsNoTracking()
                    .Where(x => x.MaschinenId == id && x.Timestamp >= tag && x.Timestamp < folgetag)
                    .OrderBy(x => x.Timestamp).ThenBy(x => x.Id)
                    .Select(x => new { x.Timestamp, x.Packungszaeler, x.Maschinentakte })
                    .ToListAsync(ct);

                // Packungszaeler und Maschinentakte sind Zählerstände, keine Ereigniswerte.
                // Sie dürfen daher nicht über sämtliche Mess-Snapshots aufsummiert werden.
                // Der letzte Stand vor Tagesbeginn hilft, Tagesgrenzen und Zähler-Resets korrekt
                // zu behandeln. Bei einem Reset wird der neue Zählerstand als Delta gezählt.
                var letzterStandVorTag = await _context.Leistungsdaten.AsNoTracking()
                    .Where(x => x.MaschinenId == id && x.Timestamp < tag)
                    .OrderByDescending(x => x.Timestamp).ThenByDescending(x => x.Id)
                    .Select(x => new { x.Packungszaeler, x.Maschinentakte })
                    .FirstOrDefaultAsync(ct);

                int? vorherPackungen = letzterStandVorTag?.Packungszaeler;
                int? vorherTakte = letzterStandVorTag?.Maschinentakte;

                foreach (var w in tagesWerte)
                {
                    var deltaPackungen = ZaehlerDelta(vorherPackungen, w.Packungszaeler);
                    var deltaTakte = ZaehlerDelta(vorherTakte, w.Maschinentakte);

                    kachel.Packungen += deltaPackungen;
                    kachel.Takte += deltaTakte;
                    kachel.TakteProStunde[w.Timestamp.Hour] += deltaTakte;

                    vorherPackungen = w.Packungszaeler;
                    vorherTakte = w.Maschinentakte;
                }

                var soll = await _context.Planung.AsNoTracking()
                    .Where(p => p.MaschinenId == id && p.Datum >= tag && p.Datum < folgetag)
                    .SumAsync(p => (int?)p.Sollmenge, ct);
                kachel.SollMenge = soll is > 0 ? soll : null;
            }

            if (alarm != null)
            {
                kachel.AktiveAlarme = alarm.AktiveAlarmNummern()
                    .Select(n => AlarmnamenHelper.Alarmnamen.TryGetValue(n, out var name) ? name.TrimEnd('!') : $"Alarm {n}")
                    .Distinct()
                    .ToList();
            }

            if (zustand != null)
            {
                kachel.Zustand = zustand.Zustandsmeldung?.Meldung;
                kachel.ZustandSeit = zustand.Timestamp;
            }

            if (stoerung != null)
            {
                kachel.LetzteStoerung = stoerung.Stoerungsmeldung?.Meldung;
                kachel.LetzteStoerungZeit = stoerung.Timestamp;
            }

            if (temperatur != null)
            {
                // Maschine 4 liefert ganze Grad, alle anderen Zehntelgrad
                decimal Grad(int wert) => id == 4 ? wert : SiteHelper.ConvertTemperatur(wert);
                kachel.Soll1 = Grad(temperatur.Solltemp1);
                kachel.Ist1 = Grad(temperatur.Isstemp1);
                kachel.Soll2 = Grad(temperatur.Solltemp2);
                kachel.Ist2 = Grad(temperatur.Isstemp2);
            }

            kachel.Stand = new DateTime?[] { leistung?.Timestamp, alarm?.Timestamp, zustand?.Timestamp, stoerung?.Timestamp, temperatur?.Timestamp }
                .Where(t => t.HasValue).Max();

            BestimmeStatus(kachel, alarm, stoerung);
            return kachel;
        }

        /// <summary>
        /// Differenz zweier SPS-Zählerstände. Wenn der neue Wert kleiner ist, wurde
        /// der Zähler vermutlich zurückgesetzt; der neue Stand ist dann die Menge seit Reset.
        /// Wenn es keinen Vorwert gibt, kann nur der aktuelle Stand als Näherung dienen.
        /// </summary>
        private static long ZaehlerDelta(int? vorher, int aktuell)
        {
            var neu = Math.Max(0, aktuell);
            if (!vorher.HasValue) return neu;

            var alt = Math.Max(0, vorher.Value);
            return neu >= alt ? (long)neu - alt : neu;
        }

        /// <summary>
        /// Status aus dem letzten bekannten Stand:
        /// aktiver Alarm im neuesten Alarmdatensatz → kritisch,
        /// Störung kurz vor dem letzten Stand → Warnung, sonst gut (mit Zustandstext).
        /// </summary>
        private static void BestimmeStatus(MaschinenKachel k, Alarmdaten? alarm, StoerungsDaten? stoerung)
        {
            if (k.Stand is null)
            {
                k.Status = KachelStatus.Unbekannt;
                k.StatusText = "Keine Daten";
                return;
            }

            var stand = k.Stand.Value;

            if (k.AktiveAlarme.Count > 0 && alarm != null && alarm.Timestamp >= stand.AddMinutes(-15))
            {
                k.Status = KachelStatus.Kritisch;
                k.StatusText = k.AktiveAlarme.Count == 1 ? "Alarm" : $"{k.AktiveAlarme.Count} Alarme";
                return;
            }

            if (stoerung != null && stoerung.Timestamp >= stand.AddMinutes(-15))
            {
                k.Status = KachelStatus.Warnung;
                k.StatusText = "Störung";
                return;
            }

            k.Status = KachelStatus.Gut;
            k.StatusText = LeerZuNull(k.Zustand) ?? "In Betrieb";
        }

        // ─────────────────────────────────────────────────────────────────────
        // Ereignisverlauf: Alarme, Störungen und Zustandswechsel zusammen
        // ─────────────────────────────────────────────────────────────────────
        private async Task<List<Ereignis>> LadeEreignisseAsync(List<long> maschinenIds, CancellationToken ct)
        {
            const int anzahl = 15;
            if (maschinenIds.Count == 0) return new List<Ereignis>();

            var alarme = await _context.Alarmdaten.AsNoTracking().Include(a => a.Maschine)
                .Where(a => maschinenIds.Contains(a.MaschinenId))
                .Where(Alarmdaten.HatAktivenAlarm)
                .OrderByDescending(a => a.Timestamp).Take(anzahl).ToListAsync(ct);

            var stoerungen = await _context.Stoerungsdaten.AsNoTracking()
                .Include(s => s.Maschine).Include(s => s.Stoerungsmeldung)
                .Where(s => maschinenIds.Contains(s.MaschinenId))
                .OrderByDescending(s => s.Timestamp).Take(anzahl).ToListAsync(ct);

            var zustaende = await _context.Zustandsdaten.AsNoTracking()
                .Include(z => z.Maschine).Include(z => z.Zustandsmeldung)
                .Where(z => maschinenIds.Contains(z.MaschinenId))
                .OrderByDescending(z => z.Timestamp).Take(anzahl).ToListAsync(ct);

            var liste = new List<Ereignis>();

            liste.AddRange(alarme.Select(a => new Ereignis
            {
                Zeit = a.Timestamp,
                Maschine = a.Maschine?.Bezeichnung ?? $"Maschine {a.MaschinenId}",
                Art = "Alarm",
                Stufe = KachelStatus.Kritisch,
                Text = string.Join(" · ", a.AktiveAlarmNummern()
                    .Select(n => AlarmnamenHelper.Alarmnamen.TryGetValue(n, out var name) ? name.TrimEnd('!') : $"Alarm {n}")
                    .Distinct())
            }));

            liste.AddRange(stoerungen.Select(s => new Ereignis
            {
                Zeit = s.Timestamp,
                Maschine = s.Maschine?.Bezeichnung ?? $"Maschine {s.MaschinenId}",
                Art = "Störung",
                Stufe = KachelStatus.Warnung,
                Text = s.Stoerungsmeldung?.Meldung ?? "Störung"
            }));

            liste.AddRange(zustaende.Select(z => new Ereignis
            {
                Zeit = z.Timestamp,
                Maschine = z.Maschine?.Bezeichnung ?? $"Maschine {z.MaschinenId}",
                Art = "Zustand",
                Stufe = KachelStatus.Gut,
                Text = z.Zustandsmeldung?.Meldung ?? "Zustandswechsel"
            }));

            return liste.OrderByDescending(e => e.Zeit).Take(anzahl).ToList();
        }

        private static string? LeerZuNull(string? s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}
