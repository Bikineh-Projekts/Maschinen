using System.Collections.Generic;
using System.Linq;
using MaschinenDataein.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MaschinenDataein.Helper
{
    public static class DropDownListHelper
    {
        /// <summary>
        /// Gibt eine Liste von Maschinen für DropDownLists zurück
        /// </summary>
        public static List<SelectListItem> GetMaschinen(MaschinenDbContext context)
        {
            return context.Maschinen
                .Select(m => new SelectListItem
                {
                    Value = m.Id.ToString(),
                    Text = m.Bezeichnung
                })
                .ToList();
        }

        /// <summary>
        /// Gibt eine Liste von Maschinen für DropDownLists zurück
        /// </summary>
        /// <param name="context">Der DbContext</param>
        /// <param name="addEmptyOption">Wenn true, wird eine leere Option "Bitte wählen..." hinzugefügt</param>
        public static List<SelectListItem> GetMaschinen(MaschinenDbContext context, bool addEmptyOption)
        {
            var items = GetMaschinen(context);

            if (addEmptyOption)
            {
                items.Insert(0, new SelectListItem
                {
                    Value = "",
                    Text = "-- Bitte wählen --"
                });
            }

            return items;
        }
    }
}