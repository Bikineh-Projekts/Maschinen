namespace MaschinenDataein.Models.ModelView
{
    /// <summary>Zustand einer Maschine auf der Startseite – bestimmt Farbe und Symbol der Kachel.</summary>
    public enum KachelStatus
    {
        Gut,
        Warnung,
        Kritisch,
        Unbekannt
    }

    /// <summary>Eine Kachel der Startseite: das Wichtigste zu einer Maschine auf einen Blick.</summary>
    public class MaschinenKachel
    {
        public long MaschinenId { get; set; }
        public string Bezeichnung { get; set; } = "";

        public KachelStatus Status { get; set; } = KachelStatus.Unbekannt;
        public string StatusText { get; set; } = "Keine Daten";

        /// <summary>Zeitpunkt des neuesten Datensatzes dieser Maschine.</summary>
        public DateTime? Stand { get; set; }

        public string? Programm { get; set; }

        // Produktion am Tag des neuesten Datensatzes
        public DateTime? ProduktionsTag { get; set; }
        public int Packungen { get; set; }
        public int Takte { get; set; }

        /// <summary>Takte je Stunde (0–23) für den Verlauf in der Kachel.</summary>
        public int[] TakteProStunde { get; set; } = new int[24];

        /// <summary>Summe der Soll-Mengen aus der Produktionsplanung für diesen Tag (falls geplant).</summary>
        public int? SollMenge { get; set; }

        public List<string> AktiveAlarme { get; set; } = new();

        public string? Zustand { get; set; }
        public DateTime? ZustandSeit { get; set; }

        public string? LetzteStoerung { get; set; }
        public DateTime? LetzteStoerungZeit { get; set; }

        public decimal? Ist1 { get; set; }
        public decimal? Soll1 { get; set; }
        public decimal? Ist2 { get; set; }
        public decimal? Soll2 { get; set; }

        /// <summary>Anteil der Soll-Menge in Prozent (nur mit Planung).</summary>
        public int? SollProzent => SollMenge is > 0 ? (int)Math.Round(Packungen * 100.0 / SollMenge.Value) : null;
    }

    /// <summary>Ein Eintrag im Ereignisverlauf der Startseite (Alarm, Störung oder Zustandswechsel).</summary>
    public class Ereignis
    {
        public DateTime Zeit { get; set; }
        public string Maschine { get; set; } = "";
        public string Art { get; set; } = "";
        public string Text { get; set; } = "";
        public KachelStatus Stufe { get; set; }
    }
}
