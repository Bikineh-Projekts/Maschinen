using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
        public IActionResult Data(int pageNumber)
        {
            _actionName = "Data";
            FilterModelView model = GetFilterModelViewCookie();
            SetViewBagFilter(model, "Index", "AlarmDaten");

            var datumBis = model.DatumBis.AddHours(23).AddMinutes(59).AddSeconds(59);

            var query = _context.Alarmdaten
                .Include(a => a.Maschine)
                .Where(a => a.Timestamp >= model.DatumVon && a.Timestamp <= datumBis);

            if (model.MaschinenId > 0)
                query = query.Where(a => a.MaschinenId == model.MaschinenId);

            var list = query
                .OrderByDescending(a => a.Timestamp)
                .ToList();

            // Nur Einträge mit mindestens einem aktiven Alarm
            var mitAlarmen = list
                .Where(a => GetAktiveAlarme(a).Any())
                .ToList();

            if (pageNumber <= 0)
                pageNumber = 1;

            var pagedList = mitAlarmen.ToPagedList(pageNumber, 50);
            var pageItems = pagedList.ToList();

            PaginatedListItem paginatedListItem = new(
                pagedList.PageNumber,
                pagedList.PageCount,
                pagedList.IsFirstPage,
                pagedList.IsLastPage,
                "Data"
            );

            ViewData["PaginatedListItem"] = paginatedListItem;
            ViewData["IsShowId"] = true;

            return View("Index", pageItems);
        }

        // ─── HELPER: Aktive Alarme per Reflection ────────────────────
        // Sauberere Lösung als 94-facher switch
        public static List<AlarmInfo> GetAktiveAlarme(Alarmdaten alarmdaten)
        {
            var aktiveAlarme = new List<AlarmInfo>();
            var type = alarmdaten.GetType();

            for (int i = 1; i <= 94; i++)
            {
                PropertyInfo? prop = type.GetProperty($"AM{i}");
                if (prop == null) continue;

                var value = prop.GetValue(alarmdaten);
                if (value is bool istAktiv && istAktiv)
                {
                    aktiveAlarme.Add(new AlarmInfo
                    {
                        Nummer = i,
                        Beschreibung = AlarmnamenHelper.Alarmnamen.TryGetValue(i, out var name)
                                        ? name
                                        : $"Unbekannter Alarm {i}"
                    });
                }
            }

            return aktiveAlarme;
        }

        public class AlarmInfo
        {
            public int Nummer { get; set; }
            public string Beschreibung { get; set; } = string.Empty;
        }
    }
}
