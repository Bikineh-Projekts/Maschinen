using System.Diagnostics;
using MaschinenDataein.Models;
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
        public async Task<IActionResult> Index()
        {
            try
            {
                // Nur neueste 10 Einträge pro Tabelle laden
                var leistungsdaten = await _context.Leistungsdaten
                    .Include(l => l.Maschine)
                    .OrderByDescending(l => l.Timestamp)
                    .Take(10)
                    .AsNoTracking()
                    .ToListAsync();

                var abzugsdaten = await _context.Abzugsdaten
                    .Include(a => a.Maschine)
                    .OrderByDescending(a => a.Timestamp)
                    .Take(10)
                    .AsNoTracking()
                    .ToListAsync();

                var temperaturdaten = await _context.Temperaturdaten
                    .Include(t => t.Maschine)
                    .OrderByDescending(t => t.Timestamp)
                    .Take(10)
                    .AsNoTracking()
                    .ToListAsync();

                var model = new DashboardModelView
                {
                    // Neuestes Programm pro Maschine
                    ProgrammenList = await _context.Planung
                        .Include(p => p.Maschine)
                        .OrderByDescending(p => p.Id)
                        .AsNoTracking()
                        .ToListAsync(),

                    LeistungsDatenList = leistungsdaten
                        .Select(l => new LeistungsdatenModelView(_context, l)
                        {
                            Name = ProgrammnamenHelper.GetName(l.MaschinenId, l.PRnummer)
                        })
                        .ToList(),

                    AbzugsDatenList = abzugsdaten
                        .Select(a => new AbzugsdatenModelView(_context, a)
                        {
                            Name = ProgrammnamenHelper.GetName(a.MaschinenId, a.PRnummer)
                        })
                        .ToList(),

                    TemperaturDatenList = temperaturdaten
                        .Select(t => new TemperaturdatenModelView
                        {
                            Id = t.Id,
                            MaschinenId = t.MaschinenId,
                            Maschine = t.Maschine?.Bezeichnung,
                            Timestamp = t.Timestamp,
                            Name = ProgrammnamenHelper.GetName(t.MaschinenId, t.PRnummer),
                            Solltemp1 = t.Solltemp1,
                            Isstemp1 = t.Isstemp1,
                            Solltemp2 = t.Solltemp2,
                            Isstemp2 = t.Isstemp2
                        })
                        .ToList(),

                    AlarmDatenList = await _context.Alarmdaten
                        .Include(a => a.Maschine)
                        .OrderByDescending(a => a.Timestamp)
                        .Take(10)
                        .AsNoTracking()
                        .ToListAsync(),

                    ZustandsDatenList = await _context.Zustandsdaten
                        .Include(z => z.Maschine)
                        .Include(z => z.Zustandsmeldung)
                        .OrderByDescending(z => z.Timestamp)
                        .Take(10)
                        .AsNoTracking()
                        .ToListAsync(),

                    StoerungsDatenList = await _context.Stoerungsdaten
                        .Include(s => s.Maschine)
                        .Include(s => s.Stoerungsmeldung)
                        .OrderByDescending(s => s.Timestamp)
                        .Take(10)
                        .AsNoTracking()
                        .ToListAsync()
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fehler beim Laden der Dashboard-Daten");
                return View(new DashboardModelView());
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(string? dummy)
        {
            return await Index();
        }

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