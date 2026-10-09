using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MaschinenDataein.Controllers.Model;
using MaschinenDataein.Helper;
using MaschinenDataein.Models;
using MaschinenDataein.Models.Data;
using MaschinenDataein.Models.ModelView;
using MaschinenDataein.Models.PaginatedModel;
using X.PagedList;

namespace MaschinenDataein.Controllers
{
    public class AlarmdatenController : ControllerModel
    {
        public AlarmdatenController(MaschinenDbContext context)
            : base(context, "AlarmDaten")
        {
        }

        // ─── INDEX (GET) ─────────────────────────────────────────────
        public IActionResult Index()
        {
            _actionName = "Data";
            FilterModelView model = GetFilterModelViewCookie();
            SetViewBagFilter(model, "Data", "AlarmDaten");
            ViewData["IsShowId"] = true;
            return View();
        }

        // ─── DATA (GET + POST) ────────────────────────────────────────
        public async Task<IActionResult> Data(int pageNumber, CancellationToken ct)
        {
            _actionName = "Data";
            FilterModelView model = GetFilterModelViewCookie();
            SetViewBagFilter(model, "Index", "AlarmDaten");

            // "Mindestens ein Alarm aktiv" wird jetzt in SQL geprüft.
            // Vorher wurde der ganze Zeitraum geladen und per Reflection durchsucht.
            var query = _context.Alarmdaten
                .Include(a => a.Maschine)
                .ImZeitraum(model.DatumVon, model.DatumBis)
                .FuerMaschine(model.MaschinenId)
                .Where(Alarmdaten.HatAktivenAlarm)
                .NeuesteZuerst();

            if (pageNumber <= 0)
                pageNumber = 1;

            var pagedList = query.ToPagedList(pageNumber, 50);
            var pageItems = pagedList.ToList();

            PaginatedListItem paginatedListItem = new(
                pagedList.PageNumber,
                pagedList.PageCount,
                pagedList.IsFirstPage,
                pagedList.IsLastPage,
                "Data"
            )
            {
                TotalItemCount  = pagedList.TotalItemCount,
                FirstItemOnPage = pagedList.FirstItemOnPage,
                LastItemOnPage  = pagedList.LastItemOnPage
            };

            ViewData["PaginatedListItem"] = paginatedListItem;
            ViewData["IsShowId"] = true;
            ViewBag.Analyse = await AnalyseHelper.AlarmAsync(_context, model, ct);

            return View("Index", pageItems);
        }

        // ─── HELPER: Aktive Alarme mit Klartext ──────────────────────
        public static List<AlarmInfo> GetAktiveAlarme(Alarmdaten alarmdaten)
        {
            return alarmdaten.AktiveAlarmNummern()
                .Select(i => new AlarmInfo
                {
                    Nummer = i,
                    Beschreibung = AlarmnamenHelper.Alarmnamen.TryGetValue(i, out var name)
                        ? name
                        : $"Unbekannter Alarm {i}"
                })
                .ToList();
        }

        public class AlarmInfo
        {
            public int Nummer { get; set; }
            public string Beschreibung { get; set; } = string.Empty;
        }
    }
}
