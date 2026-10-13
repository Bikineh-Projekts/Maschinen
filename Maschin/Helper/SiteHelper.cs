namespace MaschinenDataein.Helper
{
    /// <summary>
    /// ZENTRALE REGEL FÜR ALLE TEMPERATURWERTE (Solltemp1/2 und Isstemp1/2):
    /// Die SPS speichert Temperaturen in Zehntelgrad. JEDER Temperaturwert MUSS
    /// IMMER durch 10 geteilt werden, bevor er angezeigt oder berechnet wird –
    /// für ALLE Maschinen, ohne Ausnahme. Beispiel: 1000 → 100,0 °C.
    /// Die Rohwerte in der Datenbank bleiben unverändert.
    /// Temperaturen deshalb NIE direkt aus der Datenbank verwenden, sondern immer
    /// über diese Methoden umrechnen.
    /// </summary>
    public static class SiteHelper
    {
        /// <summary>Teiler für alle Temperaturwerte (Zehntelgrad → °C).</summary>
        public const int TemperaturTeiler = 10;

        /// <summary>Rohwert ÷ 10 → °C (für Anzeige und Berechnungen).</summary>
        public static decimal ConvertTemperatur(int temp)
            => (decimal)temp / TemperaturTeiler;

        /// <summary>Rohwert ÷ 10 → °C als double (für Diagramme und Statistik).</summary>
        public static double TemperaturGrad(int temp)
            => (double)temp / TemperaturTeiler;

        /// <summary>Rohwert ÷ 10 → Text mit einer Nachkommastelle, z. B. "100,0".</summary>
        public static string ConvertTemperaturToString(int temp)
            => ConvertTemperatur(temp).ToString("0.0", System.Globalization.CultureInfo.GetCultureInfo("de-DE"));
    }
}
