namespace MaschinenDataein.Helper
{
    /// <summary>
    /// ZENTRALE REGEL FÜR TEMPERATURWERTE:
    ///   - Isstemp2 MUSS IMMER durch 10 geteilt werden (alle Maschinen, ohne Ausnahme).
    ///     Beispiel: 1000 → 100,0 °C.
    ///   - Solltemp1, Isstemp1 und Solltemp2 werden NICHT geteilt, sondern so verwendet,
    ///     wie sie in der Datenbank stehen.
    /// Die Rohwerte in der Datenbank bleiben unverändert. Temperaturen deshalb immer über
    /// diese Methoden umrechnen und nie direkt aus der Datenbank anzeigen.
    /// </summary>
    public static class SiteHelper
    {
        /// <summary>Teiler NUR für Isstemp2.</summary>
        public const int Isstemp2Teiler = 10;

        // ── Isstemp2: immer ÷ 10 ──────────────────────────────────────────────
        public static decimal Isstemp2(int rohwert) => (decimal)rohwert / Isstemp2Teiler;
        public static double Isstemp2Grad(int rohwert) => (double)rohwert / Isstemp2Teiler;

        // ── Solltemp1, Isstemp1, Solltemp2: unverändert ───────────────────────
        public static decimal Temperatur(int rohwert) => rohwert;
        public static double TemperaturGrad(int rohwert) => rohwert;

        /// <summary>Temperatur als Text mit einer Nachkommastelle, z. B. "100,0".</summary>
        public static string Text(decimal grad)
            => grad.ToString("0.0", System.Globalization.CultureInfo.GetCultureInfo("de-DE"));
    }
}
