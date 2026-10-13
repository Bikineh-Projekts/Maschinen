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
using MaschinenDataein.Models.PaginatedModel;
using X.PagedList;

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

        public async Task<IActionResult> Data(int pageNumber, CancellationToken ct)
        {
            _actionName = "Data";
            FilterModelView model = GetFilterModelViewCookie();
            SetViewBagFilter(model, "Index", "StoerungsDaten");

            // Filter und Seitenaufteilung in der Datenbank – nur 50 Einträge pro Seite
            var aktiveMaschinenIds = DropDownListHelper.GetAktiveMaschinen(_context).Select(m => m.Id).ToList();

            var query = _context.Stoerungsdaten
                .Include(x => x.Maschine)
                .Include(x => x.Stoerungsmeldung)
                .ImZeitraum(model.DatumVon, model.DatumBis)
                .FuerAktiveMaschinen(model.MaschinenId, aktiveMaschinenIds)
                .NeuesteZuerst();

            if (pageNumber <= 0) pageNumber = 1;
            var pagedList = query.ToPagedList(pageNumber, 50);

            ViewData["PaginatedListItem"] = new PaginatedListItem(
                pagedList.PageNumber, pagedList.PageCount,
                pagedList.IsFirstPage, pagedList.IsLastPage, "Data")
            {
                TotalItemCount  = pagedList.TotalItemCount,
                FirstItemOnPage = pagedList.FirstItemOnPage,
                LastItemOnPage  = pagedList.LastItemOnPage
            };
            ViewBag.Analyse = await AnalyseHelper.StoerungAsync(_context, model, ct);

            return View("Index", pagedList.ToList());
        }
    }
}
