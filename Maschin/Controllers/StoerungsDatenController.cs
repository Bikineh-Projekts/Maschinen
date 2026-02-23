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
    public class StoerungsDatenController : ControllerModel
    {
        public StoerungsDatenController(MaschinenDbContext context)
            : base(context, "StoerungsDaten") { }

        public IActionResult Index()
        {
            _actionName = "Data";

            // Standard-Filtermodell holen
            FilterModelView model = GetFilterModelViewCookie();

            // Maschinenliste usw. vorbereiten
            SetViewBagFilter(model, "Data", "StoerungsDaten");

            return View();
        }

        public IActionResult Data()
        {
            _actionName = "Data";
            FilterModelView model = GetFilterModelViewCookie();
            SetViewBagFilter(model, "Index", "StoerungsDaten");

            var datumbis = model.DatumBis.AddHours(23).AddMinutes(59).AddSeconds(59);

            // Störungsdaten abrufen inkl. Navigationseigenschaften
            var list = _context.Stoerungsdaten
                .Include(x => x.Maschine)
                .Include(x => x.Stoerungsmeldung)
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
