using MaschinenDataein.Helper;
using MaschinenDataein.Models;
using MaschinenDataein.Models.Data;
using MaschinenDataein.Models.ModelView;
using Microsoft.AspNetCore.Mvc;

namespace MaschinenDataein.Controllers.Model
{
    public class ControllerModel : Controller
    {
        protected MaschinenDbContext _context;
        protected string _searchString = "Search";
        protected string _controllerName = "";
        protected string _actionName = "";

        public ControllerModel(MaschinenDbContext context, string controllerName)
        {
            _context = context;
            _controllerName = controllerName;
            _actionName = "";
        }

        /// <summary>
        /// Speichert das übergebene Filtermodell in der Session
        /// und leitet zur gewünschten Action weiter.
        /// </summary>

        public IActionResult Filter(FilterModelView filterModelView)
        {
            SessionHelper.SetObjectInSession(
                HttpContext.Session,
                $"{_searchString}{filterModelView.ControllerName}{filterModelView.ActionForm}",
                filterModelView
            );

            return RedirectToAction(filterModelView.ActionForm);
        }
       
        /// <summary>
        /// Setzt Maschinenliste und Filterdaten für die ViewBag
        /// </summary>
        public void SetViewBagFilter(FilterModelView filterModelView, string lastPage, string moduleName)
        {
            ViewBag.FilterView = filterModelView;
            ViewBag.Maschinen = DropDownListHelper.GetMaschinen(_context, true);
        }

        public void SetViewBagFilter(FilterModelView filterModelView)
        {
            filterModelView.ActionForm = _actionName;
            filterModelView.ControllerName = _controllerName;

            ViewBag.FilterView = filterModelView;
            ViewBag.Maschinen = DropDownListHelper.GetMaschinen(_context, true);
        }


        /// <summary>
        /// Lädt das Filtermodell aus der Session oder erstellt ein neues.
        /// </summary>
        public FilterModelView GetFilterModelViewCookie()
        {
            var filterModelView = SessionHelper.GetCustomObjectFromSession<FilterModelView>(
                HttpContext.Session,
                $"{_searchString}{_controllerName}{_actionName}"
            );

            filterModelView ??= new FilterModelView(_actionName, _controllerName);
            return filterModelView;
        }
       
    }
}
