namespace MaschinenDataein.Models.Data
{
    /// <summary>
    /// Gemeinsame Felder aller Messdaten-Tabellen (Leistung, Abzug, Temperatur, Alarm, Zustand, Störung).
    /// Ermöglicht eine einheitliche Konfiguration im DbContext und gemeinsame Filter
    /// (siehe <see cref="MaschinenDataein.Helper.MaschinenDatenFilter"/>).
    /// </summary>
    public interface IMaschinenDaten
    {
        long Id { get; set; }
        DateTime Timestamp { get; set; }
        long MaschinenId { get; set; }
        Maschine? Maschine { get; set; }
    }
}
