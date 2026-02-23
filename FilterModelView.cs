using System;

namespace MaschinenDataein.Models.ModelView
{
    /// <summary>
    /// Modell für Filterdaten wie Datum, Maschine und Zielseite.
    /// Wird für Such-/Filter-Funktionalität in mehreren Modulen verwendet.
    /// </summary>
    public class FilterModelView
    {
        /// <summary>
        /// Standard-Konstruktor
        /// </summary>
        public FilterModelView()
        {
            InitDefaultDates();
        }

        /// <summary>
        /// Konstruktor mit Action und Modulname
        /// </summary>
        public FilterModelView(string actionForm, string moduleName)
        {
            InitDefaultDates();
            ActionForm = actionForm;
            ControllerName = moduleName;
        }

        /// <summary>
        /// Maschinen-ID für die Filterung (0 = alle)
        /// </summary>
        public long MaschinenId { get; set; }

        /// <summary>
        /// Startdatum für den Filterbereich
        /// </summary>
        public DateTime DatumVon { get; set; }

        /// <summary>
        /// Enddatum für den Filterbereich
        /// </summary>
        public DateTime DatumBis { get; set; }

        /// <summary>
        /// Action-Name der Zielseite (z. B. "Data")
        /// </summary>
        public string ActionForm { get; set; } = string.Empty;

        /// <summary>
        /// Modulname (z. B. "Programme", "Leistung", ...)
        /// </summary>
        public string ControllerName { get; set; } = string.Empty;

        /// <summary>
        /// Initialisiert die Standarddaten für heute
        /// </summary>
        private void InitDefaultDates()
        {
            var now = DateTime.Now;
            DatumVon = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0);
            DatumBis = new DateTime(now.Year, now.Month, now.Day, 23, 59, 59);
        }
    }
}
