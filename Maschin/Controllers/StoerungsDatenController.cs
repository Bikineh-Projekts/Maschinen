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

        public async Task<IActionResult> Data(CancellationToken ct)
        {
            _actionName = "Data";
            FilterModelView model = GetFilterModelViewCookie();
            SetViewBagFilter(model, "Index", "StoerungsDaten");

            // Maschinenfilter jetzt in der Datenbank statt nachträglich im Speicher
            var list = await _context.Stoerungsdaten
                .Include(x => x.Maschine)
                .Include(x => x.Stoerungsmeldung)
                .ImZeitraum(model.DatumVon, model.DatumBis)
                .FuerMaschine(model.MaschinenId)
                .NeuesteZuerst()
                .ToListAsync(ct);

            return View("Index", list);
        }
    }
}
