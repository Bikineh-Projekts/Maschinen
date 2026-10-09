namespace MaschinenDataein.Models.ModelView
{
    /// <summary>
    /// Analyse-Reiter einer Datenseite: Kennzahlen oben, darunter Diagramme.
    /// Wird vom Partial _Analyse gezeichnet; Farben sind Namen aus dem Farbsystem (z. B. "viz-1").
    /// </summary>
    public class AnalyseModel
    {
        public string Zeitraum { get; set; } = "";
        public List<Kennzahl> Kennzahlen { get; set; } = new();
        public List<Diagramm> Diagramme { get; set; } = new();
        public bool HatDaten => Kennzahlen.Count > 0 || Diagramme.Count > 0;
    }

    public class Kennzahl
    {
        public string Titel { get; set; } = "";
        public string Wert { get; set; } = "";
        public string? Hinweis { get; set; }
        public string? Icon { get; set; }
    }

    public enum DiagrammArt
    {
        /// <summary>Waagrechte Balken – für Ranglisten mit langen Namen.</summary>
        BalkenWaagrecht,
        /// <summary>Senkrechte Säulen – für Werte je Tag oder Stunde.</summary>
        Saeulen,
        /// <summary>Linie – für Verläufe über die Zeit.</summary>
        Linie,
        /// <summary>Gestapelte waagrechte Balken – Anteile je Kategorie.</summary>
        GestapeltWaagrecht
    }

    public class Diagramm
    {
        public string Id { get; set; } = "d" + Guid.NewGuid().ToString("N")[..8];
        public string Titel { get; set; } = "";
        public string? Hinweis { get; set; }
        public DiagrammArt Art { get; set; }
        public string Einheit { get; set; } = "";
        public bool Breit { get; set; }
        public List<string> Beschriftungen { get; set; } = new();
        public List<Reihe> Reihen { get; set; } = new();
    }

    public class Reihe
    {
        public string Name { get; set; } = "";
        public string Farbe { get; set; } = "viz-1";
        public List<double?> Werte { get; set; } = new();
    }
}
