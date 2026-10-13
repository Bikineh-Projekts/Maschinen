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
        // Die Suche läuft über alle sechs Messdatentabellen und wird pro Seitenaufruf
        // mehrfach gebraucht (Filter, Tabelle, Analyse). Ergebnis daher kurz merken.
        private static readonly object _sperre = new();
        private static List<long>? _aktiveIds;
        private static DateTime _aktiveIdsZeit;
        private static readonly TimeSpan _merkDauer = TimeSpan.FromSeconds(60);

        /// <summary>
        /// Liefert die zwei zuletzt aktiven Maschinen aus den tatsächlichen Messdaten.
        /// Leere Demo-/Stammdatensätze ohne Telemetrie erzeugen keine zusätzlichen Maschinen-Kacheln.
        /// Die Auswahl basiert nicht auf fest codierten Maschinen-IDs.
        /// </summary>
        public static List<Maschine> GetAktiveMaschinen(MaschinenDbContext context)
        {
            var aktiveIds = AktiveMaschinenIds(context);

            return context.Maschinen
                .AsNoTracking()
                .Where(m => aktiveIds.Contains(m.Id))
                .OrderBy(m => m.Id)
                .ToList();
        }

        private static List<long> AktiveMaschinenIds(MaschinenDbContext context)
        {
            lock (_sperre)
            {
                if (_aktiveIds != null && DateTime.UtcNow - _aktiveIdsZeit < _merkDauer)
                    return _aktiveIds;
            }

            var ids = BerechneAktiveMaschinenIds(context);

            lock (_sperre)
            {
                _aktiveIds = ids;
                _aktiveIdsZeit = DateTime.UtcNow;
            }
            return ids;
        }

        private static List<long> BerechneAktiveMaschinenIds(MaschinenDbContext context)
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

            return aktivitaeten
                .GroupBy(x => x.MaschinenId)
                .Select(g => new { MaschinenId = g.Key, LetzterStand = g.Max(x => x.Timestamp) })
                .OrderByDescending(x => x.LetzterStand)
                .ThenBy(x => x.MaschinenId)
                .Take(2)
                .Select(x => x.MaschinenId)
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
