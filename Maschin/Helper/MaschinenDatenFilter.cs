using System.Collections.Generic;
using MaschinenDataein.Models.Data;
using Microsoft.EntityFrameworkCore;

namespace MaschinenDataein.Helper
{
    /// <summary>
    /// Gemeinsame Filter für alle Messdaten – ersetzt die in jedem Controller wiederholten Where-Klauseln.
    /// </summary>
    public static class MaschinenDatenFilter
    {
        /// <summary>
        /// Zeitraum von DatumVon (00:00) bis einschließlich DatumBis (ganzer Tag).
        /// Nur das Datum zählt – eine Uhrzeit im Filter wird ignoriert.
        /// </summary>
        public static IQueryable<T> ImZeitraum<T>(this IQueryable<T> query, DateTime von, DateTime bis)
            where T : class, IMaschinenDaten
        {
            var start = von.Date;
            var ende = bis.Date.AddDays(1);
            return query.Where(x => x.Timestamp >= start && x.Timestamp < ende);
        }

        /// <summary>Filtert auf eine Maschine; 0 bedeutet die zwei zuletzt aktiven Maschinen.</summary>
        public static IQueryable<T> FuerAktiveMaschinen<T>(
            this IQueryable<T> query,
            long maschinenId,
            IReadOnlyCollection<long> aktiveMaschinenIds)
            where T : class, IMaschinenDaten
            => maschinenId > 0
                ? query.Where(x => x.MaschinenId == maschinenId)
                : query.Where(x => aktiveMaschinenIds.Contains(x.MaschinenId));

        /// <summary>Kompatibilitätsmethode für Stellen, die explizit alle Daten benötigen.</summary>
        public static IQueryable<T> FuerMaschine<T>(this IQueryable<T> query, long maschinenId)
            where T : class, IMaschinenDaten
            => maschinenId > 0 ? query.Where(x => x.MaschinenId == maschinenId) : query;

        /// <summary>Nur lesen (kein Change-Tracking), neueste zuerst.</summary>
        public static IQueryable<T> NeuesteZuerst<T>(this IQueryable<T> query)
            where T : class, IMaschinenDaten
            => query.AsNoTracking().OrderByDescending(x => x.Timestamp);
    }
}
