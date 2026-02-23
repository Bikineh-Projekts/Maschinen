using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Collections.Generic;
using MaschinenDataein.Models;
using MaschinenDataein.Models.Data;
using MaschinenDataein.Models.ModelView;
using MaschinenDataein.Controllers.Model;

namespace MaschinenDataein.Controllers
{
    public class ZustandsDatenController : ControllerModel
    {
        public ZustandsDatenController(MaschinenDbContext context)
            : base(context, "ZustandsDaten") { }

        public IActionResult Index()
        {
            _actionName = "Data";

            // Standard-Filtermodell holen
            FilterModelView model = GetFilterModelViewCookie();

            // Maschinenliste usw. vorbereiten
            SetViewBagFilter(model, "Data", "ZustandsDaten");

            return View();
        }

        public IActionResult Data()
        {
            _actionName = "Data";
            FilterModelView model = GetFilterModelViewCookie();
            SetViewBagFilter(model, "Index", "ZustandsDaten");

            var datumbis = model.DatumBis.AddHours(23).AddMinutes(59).AddSeconds(59);

            // Zustandsdaten abrufen inkl. Navigationseigenschaften
            var list = _context.Zustandsdaten
                .Include(x => x.Maschine)
                .Include(x => x.Zustandsmeldung)
                .Where(x => x.Timestamp >= model.DatumVon && x.Timestamp <= datumbis)
                .ToList();

            // Maschinenfilter anwenden
            if (model.MaschinenId > 0)
            {
                list = list
                    .Where(x => x.MaschinenId == model.MaschinenId)
                    .ToList();
            }
            return View("Index", list);
        }
    }
}
