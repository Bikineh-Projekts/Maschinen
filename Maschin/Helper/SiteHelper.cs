namespace MaschinenDataein.Helper
{
    /// <summary>
    /// Konvertiert Temperaturwerte, die als Zehntelgrad gespeichert sind, in °C.
    /// Negative Werte werden ebenfalls korrekt konvertiert; 0 °C bleibt 0 °C.
    /// Für Maschine 4 (Ganzgrad-Rohwerte) diese Methode nicht verwenden.
    /// </summary>
    public static class SiteHelper
    {
        public static decimal ConvertTemperatur(int temp)
            => temp / 10m;

        public static string ConvertTemperaturToString(int temp)
            => ConvertTemperatur(temp).ToString("0.00", System.Globalization.CultureInfo.GetCultureInfo("de-DE"));
    }
}
