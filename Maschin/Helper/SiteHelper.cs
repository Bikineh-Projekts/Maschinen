namespace MaschinenDataein.Helper
{
    /// <summary>
    /// ZENTRALE REGEL FÜR ALLE TEMPERATURWERTE (Solltemp1/2, Isstemp1/2):
    ///
    ///   1. Isstemp2 wird IMMER zuerst durch 10 geteilt (alle Maschinen, ohne Ausnahme).
    ///   2. Danach wird JEDER Temperaturwert so lange durch 10 geteilt, bis er realistisch
    ///      ist, also höchstens 250 °C beträgt. Form- und Siegeltemperaturen der
    ///      Verpackungsmaschinen liegen etwa zwischen 20 und 200 °C; größere Werte sind
    ///      Zehntel- oder Hundertstelgrad aus der SPS. So passen Soll und Ist zusammen.
    ///
    ///   Beispiele:   900 → 90,0 °C     1200 → 120,0 °C    10000 → 100,0 °C
    ///                450 → 45,0 °C     20000 → 200,0 °C   120 → 120,0 °C
    ///
    ///   Obergrenze anpassen: nur RealistischeObergrenze ändern.
    ///
    /// Die Rohwerte in der Datenbank bleiben unverändert. Temperaturen deshalb NIE direkt
    /// aus der Datenbank anzeigen oder berechnen, sondern immer über diese Methoden.
    /// </summary>
    public static class SiteHelper
    {
        /// <summary>Feste Teilung NUR für Isstemp2 (Schritt 1).</summary>
        public const int Isstemp2Teiler = 10;

        /// <summary>Höchste realistische Temperatur in °C. Darüber wird weiter ÷ 10 geteilt.</summary>
        public const decimal RealistischeObergrenze = 250m;

        // ── Isstemp2: erst ÷ 10, dann auf realistischen Bereich bringen ───────
        public static decimal Isstemp2(int rohwert) => Realistisch((decimal)rohwert / Isstemp2Teiler);
        public static double Isstemp2Grad(int rohwert) => (double)Isstemp2(rohwert);

        // ── Solltemp1, Isstemp1, Solltemp2: auf realistischen Bereich bringen ─
        public static decimal Temperatur(int rohwert) => Realistisch(rohwert);
        public static double TemperaturGrad(int rohwert) => (double)Temperatur(rohwert);

        /// <summary>Teilt durch 10, bis der Betrag höchstens 250 °C ist (900 → 90, 20000 → 200).</summary>
        public static decimal Realistisch(decimal wert)
        {
            while (Math.Abs(wert) > RealistischeObergrenze)
                wert /= 10m;
            return wert;
        }

        /// <summary>Temperatur als Text mit einer Nachkommastelle, z. B. "100,0".</summary>
        public static string Text(decimal grad)
            => grad.ToString("0.0", System.Globalization.CultureInfo.GetCultureInfo("de-DE"));
    }
}
