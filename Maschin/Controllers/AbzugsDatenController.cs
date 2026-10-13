using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MaschinenDataein.Controllers.Model;
using MaschinenDataein.Helper;
using MaschinenDataein.Models;
using MaschinenDataein.Models.ModelView;
using MaschinenDataein.Models.PaginatedModel;
using X.PagedList;

namespace MaschinenDataein.Controllers
{
    public class AbzugsDatenController : ControllerModel
    {
        public AbzugsDatenController(MaschinenDbContext context)
            : base(context, "Abzugsdaten")
        {
        }

        public IActionResult Index()
        {
            _actionName = "Data";
            FilterModelView model = GetFilterModelViewCookie();
            SetViewBagFilter(model, "Data", "Abzugsdaten");
            return View();
        }

        public async Task<IActionResult> Data(int pageNumber, CancellationToken ct)
        {
            _actionName = "Data";
            FilterModelView model = GetFilterModelViewCookie();
            SetViewBagFilter(model, "Index", "Abzugsdaten");

            // Filter, Sortierung und Seitenaufteilung laufen in der Datenbank –
            // es werden nur die 50 Datensätze der aktuellen Seite geladen.
            var aktiveMaschinenIds = DropDownListHelper.GetAktiveMaschinen(_context).Select(m => m.Id).ToList();

            var query = _context.Abzugsdaten
                .Include(x => x.Maschine)
                .ImZeitraum(model.DatumVon, model.DatumBis)
                .FuerAktiveMaschinen(model.MaschinenId, aktiveMaschinenIds)
                .NeuesteZuerst();

            if (pageNumber <= 0)
                pageNumber = 1;

            var pagedList = query.ToPagedList(pageNumber, 50);

            List<AbzugsdatenModelView> modelView = pagedList
                .Select(x => new AbzugsdatenModelView(_context, x)
                {
                    MaschinenId = x.MaschinenId,
                    Name = ProgrammnamenHelper.GetName(x.MaschinenId, x.PRnummer)
                })
                .ToList();

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
            ViewBag.Analyse = await AnalyseHelper.AbzugAsync(_context, model, ct);

            return View("Index", modelView);
        }
    }
}
