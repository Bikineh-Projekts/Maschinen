using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Collections.Generic;
using MaschinenDataein.Helper;
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

        public async Task<IActionResult> Data(CancellationToken ct)
        {
            _actionName = "Data";
            FilterModelView model = GetFilterModelViewCookie();
            SetViewBagFilter(model, "Index", "ZustandsDaten");

            // Maschinenfilter jetzt in der Datenbank statt nachträglich im Speicher
            var list = await _context.Zustandsdaten
                .Include(x => x.Maschine)
                .Include(x => x.Zustandsmeldung)
                .ImZeitraum(model.DatumVon, model.DatumBis)
                .FuerMaschine(model.MaschinenId)
                .NeuesteZuerst()
                .ToListAsync(ct);

            return View("Index", list);
        }
    }
}
