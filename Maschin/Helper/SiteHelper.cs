namespace MaschinenDataein.Helper
{
    /// <summary>
    /// ZENTRALE REGEL FÜR ALLE TEMPERATURWERTE (Solltemp1/2, Isstemp1/2):
    ///
    ///   1. Isstemp2 wird IMMER zuerst durch 10 geteilt (alle Maschinen, ohne Ausnahme).
    ///   2. Danach wird JEDER Temperaturwert so lange durch 10 geteilt, bis er realistisch
    ///      ist, also unter 1000 °C liegt. Vierstellige und größere Werte gibt es in der
    ///      Produktion nicht – sie sind Zehntel- oder Hundertstelgrad aus der SPS.
    ///
    ///   Beispiele:   1000 → 100,0 °C    10000 → 100,0 °C    20000 → 200,0 °C
    ///                 450 → 450,0 °C    (Isstemp2: 1200 → 120,0 °C)
    ///
    /// Die Rohwerte in der Datenbank bleiben unverändert. Temperaturen deshalb NIE direkt
    /// aus der Datenbank anzeigen oder berechnen, sondern immer über diese Methoden.
    /// </summary>
    public static class SiteHelper
    {
        /// <summary>Feste Teilung NUR für Isstemp2 (Schritt 1).</summary>
        public const int Isstemp2Teiler = 10;

        /// <summary>Ab diesem Betrag gilt ein Wert als unrealistisch und wird weiter ÷ 10 geteilt.</summary>
        public const decimal RealistischeObergrenze = 1000m;

        // ── Isstemp2: erst ÷ 10, dann auf realistischen Bereich bringen ───────
        public static decimal Isstemp2(int rohwert) => Realistisch((decimal)rohwert / Isstemp2Teiler);
        public static double Isstemp2Grad(int rohwert) => (double)Isstemp2(rohwert);

        // ── Solltemp1, Isstemp1, Solltemp2: auf realistischen Bereich bringen ─
        public static decimal Temperatur(int rohwert) => Realistisch(rohwert);
        public static double TemperaturGrad(int rohwert) => (double)Temperatur(rohwert);

        /// <summary>Teilt durch 10, bis der Betrag unter 1000 liegt (1000 → 100, 20000 → 200).</summary>
        public static decimal Realistisch(decimal wert)
        {
            while (Math.Abs(wert) >= RealistischeObergrenze)
                wert /= 10m;
            return wert;
        }

        /// <summary>Temperatur als Text mit einer Nachkommastelle, z. B. "100,0".</summary>
        public static string Text(decimal grad)
            => grad.ToString("0.0", System.Globalization.CultureInfo.GetCultureInfo("de-DE"));
    }
}
