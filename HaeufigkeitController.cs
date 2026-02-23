using MaschinenDataein.Models;
using MaschinenDataein.Models.Data;
using MaschinenDataein.Models.ModelView;
using Microsoft.AspNetCore.Mvc;
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

        public IActionResult Index()
        {
            var viewModel = new HaeufigkeitModelView();

            // 1. Alarmmeldungen zählen
            var alarmCounts = new Dictionary<int, int>();

            for (int i = 1; i <= 94; i++)
            {
                alarmCounts[i] = 0;
            }

            foreach (var alarm in _context.Alarmdaten.ToList())
            {
                for (int i = 1; i <= 94; i++)
                {
                    var prop = typeof(Alarmdaten).GetProperty($"AM{i}");
                    if (prop != null)
                    {
                        var value = prop.GetValue(alarm);

                        bool isActive = false;

                        if (value is bool boolValue)
                        {
                            isActive = boolValue;
                        }
                        else if (value is int intValue)
                        {
                            isActive = intValue != 0;
                        }

                        if (isActive)
                        {
                            alarmCounts[i]++;
                        }
                    }
                }
            }

            viewModel.TopAlarmmeldungen = alarmCounts
                .Where(x => x.Value > 0) // Nur die Alarme zeigen, die tatsächlich vorkommen!
                .Select(x => new HaeufigMeldung { Meldung = $"AM{x.Key}", Anzahl = x.Value })
                .OrderByDescending(x => x.Anzahl)
                .ToList();

            // 2. Zustandsmeldungen zählen
            viewModel.TopZustandsmeldungen = _context.Zustandsdaten
                .GroupBy(z => z.Zustandsmeldung.Meldung)
                .Select(g => new HaeufigMeldung { Meldung = g.Key, Anzahl = g.Count() })
                .OrderByDescending(x => x.Anzahl)
                .Take(20)
                .ToList();

            // 3. Störungsmeldungen zählen
            viewModel.TopStoerungsmeldungen = _context.Stoerungsdaten
                .GroupBy(s => s.Stoerungsmeldung.Meldung)
                .Select(g => new HaeufigMeldung { Meldung = g.Key, Anzahl = g.Count() })
                .OrderByDescending(x => x.Anzahl)
                .Take(20)
                .ToList();

            return View(viewModel);
        }
    }
}
