using System.Collections.Generic;
using System.Linq;
using MaschinenDataein.Models;
using MaschinenDataein.Models.Data;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace MaschinenDataein.Helper
{
    public static class DropDownListHelper
    {
        /// <summary>
        /// Liefert die zwei zuletzt aktiven Maschinen aus den tatsächlichen Messdaten.
        /// Leere Demo-/Stammdatensätze ohne Telemetrie erzeugen keine zusätzlichen Maschinen-Kacheln.
        /// Die Auswahl basiert nicht auf fest codierten Maschinen-IDs.
        /// </summary>
        public static List<Maschine> GetAktiveMaschinen(MaschinenDbContext context)
        {
            var aktivitaeten = context.Leistungsdaten.AsNoTracking()
                .Select(x => new { x.MaschinenId, x.Timestamp })
                .Concat(context.Temperaturdaten.AsNoTracking()
                    .Select(x => new { x.MaschinenId, x.Timestamp }))
                .Concat(context.Abzugsdaten.AsNoTracking()
                    .Select(x => new { x.MaschinenId, x.Timestamp }))
                .Concat(context.Alarmdaten.AsNoTracking()
                    .Select(x => new { x.MaschinenId, x.Timestamp }))
                .Concat(context.Zustandsdaten.AsNoTracking()
                    .Select(x => new { x.MaschinenId, x.Timestamp }))
                .Concat(context.Stoerungsdaten.AsNoTracking()
                    .Select(x => new { x.MaschinenId, x.Timestamp }));

            var aktiveIds = aktivitaeten
                .GroupBy(x => x.MaschinenId)
                .Select(g => new { MaschinenId = g.Key, LetzterStand = g.Max(x => x.Timestamp) })
                .OrderByDescending(x => x.LetzterStand)
                .ThenBy(x => x.MaschinenId)
                .Take(2)
                .Select(x => x.MaschinenId)
                .ToList();

            return context.Maschinen
                .AsNoTracking()
                .Where(m => aktiveIds.Contains(m.Id))
                .OrderBy(m => m.Id)
                .ToList();
        }

        /// <summary>
        /// Gibt nur Maschinen zurück, für die tatsächlich Messdaten vorliegen.
        /// Es werden maximal die zwei zuletzt aktiven Maschinen angeboten.
        /// </summary>
        public static List<SelectListItem> GetMaschinen(MaschinenDbContext context)
        {
            return GetAktiveMaschinen(context)
                .OrderBy(m => m.Bezeichnung)
                .Select(m => new SelectListItem
                {
                    Value = m.Id.ToString(),
                    Text = m.Bezeichnung
                })
                .ToList();
        }

        public static List<SelectListItem> GetMaschinen(MaschinenDbContext context, bool addEmptyOption)
        {
            var items = GetMaschinen(context);

            if (addEmptyOption)
            {
                items.Insert(0, new SelectListItem
                {
                    Value = "0",
                    Text = "-- Beide Maschinen --"
                });
            }

            return items;
        }
    }
}
