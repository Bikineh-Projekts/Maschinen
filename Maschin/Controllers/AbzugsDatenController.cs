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

        public IActionResult Data(int pageNumber)
        {
            _actionName = "Data";
            FilterModelView model = GetFilterModelViewCookie();
            SetViewBagFilter(model, "Index", "Abzugsdaten");

            var datumBis = model.DatumBis.AddHours(23).AddMinutes(59).AddSeconds(59);

            // ✅ .Include(x => x.Maschine) damit Navigation-Property geladen wird
            var query = _context.Abzugsdaten
                .Include(x => x.Maschine)
                .Where(x => x.Timestamp >= model.DatumVon && x.Timestamp <= datumBis);

            if (model.MaschinenId > 0)
                query = query.Where(x => x.MaschinenId == model.MaschinenId);

            var list = query.ToList();

            // ✅ Name aus ProgrammnamenHelper befüllen
            List<AbzugsdatenModelView> modelView = list
                .Select(x => new AbzugsdatenModelView(_context, x)
                {
                    MaschinenId = x.MaschinenId,
                    Name = ProgrammnamenHelper.GetName(x.MaschinenId, x.PRnummer)
                })
                .ToList();

            if (pageNumber <= 0)
                pageNumber = 1;

            var pagedList = modelView.ToPagedList(pageNumber, 50);
            modelView = pagedList.ToList();

            PaginatedListItem paginatedListItem = new(
                pagedList.PageNumber,
                pagedList.PageCount,
                pagedList.IsFirstPage,
                pagedList.IsLastPage,
                "Data"
            );

            ViewData["PaginatedListItem"] = paginatedListItem;
            ViewData["IsShowId"] = true;

            return View("Index", modelView);
        }
    }
}
