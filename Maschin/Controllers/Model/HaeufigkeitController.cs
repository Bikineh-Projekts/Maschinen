using MaschinenDataein.Models;
using MaschinenDataein.Models.Data;
using MaschinenDataein.Models.ModelView;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Collections.Generic;

namespace MaschinenDataein.Controllers
{
    public class HaeufigkeitController : Controller
    {
        private readonly MaschinenDbContext _context;

        public HaeufigkeitController(MaschinenDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(CancellationToken ct)
        {
            var viewModel = new HaeufigkeitModelView();

            // 1. Alarmmeldungen zählen
            //    Nur Datensätze mit aktivem Alarm, ohne Change-Tracking und gestreamt:
            //    Die Tabelle wird nicht mehr komplett in den Speicher geladen,
            //    und statt Reflection wird direkt auf AM1 … AM94 zugegriffen.
            var alarmCounts = new int[Alarmdaten.AnzahlAlarme + 1];

            var alarme = _context.Alarmdaten
                .AsNoTracking()
                .Where(Alarmdaten.HatAktivenAlarm)
                .AsAsyncEnumerable()
                .WithCancellation(ct);

            await foreach (var alarm in alarme)
            {
                foreach (var nummer in alarm.AktiveAlarmNummern())
                    alarmCounts[nummer]++;
            }

            viewModel.TopAlarmmeldungen = Enumerable.Range(1, Alarmdaten.AnzahlAlarme)
                .Where(i => alarmCounts[i] > 0) // Nur die Alarme zeigen, die tatsächlich vorkommen!
                .Select(i => new HaeufigMeldung { Meldung = $"AM{i}", Anzahl = alarmCounts[i] })
                .OrderByDescending(x => x.Anzahl)
                .ToList();

            // 2. Zustandsmeldungen zählen
            viewModel.TopZustandsmeldungen = await _context.Zustandsdaten
                .GroupBy(z => z.Zustandsmeldung.Meldung)
                .Select(g => new HaeufigMeldung { Meldung = g.Key ?? "", Anzahl = g.Count() })
                .OrderByDescending(x => x.Anzahl)
                .Take(20)
                .ToListAsync(ct);

            // 3. Störungsmeldungen zählen
            viewModel.TopStoerungsmeldungen = await _context.Stoerungsdaten
                .GroupBy(s => s.Stoerungsmeldung.Meldung)
                .Select(g => new HaeufigMeldung { Meldung = g.Key ?? "", Anzahl = g.Count() })
                .OrderByDescending(x => x.Anzahl)
                .Take(20)
                .ToListAsync(ct);

            return View(viewModel);
        }
    }
}
