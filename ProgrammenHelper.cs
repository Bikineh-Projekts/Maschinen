using System.Collections.Generic;

namespace MaschinenDataein.Helper
{
    public static class ProgrammnamenHelper
    {
        private static readonly Dictionary<(long MaschinenId, int PRnummer), string> _mapping = new()
        {
            { (1, 0), "" },
            { (1, 1), "1KG VAC" },
            { (1, 2), "3.2 VAC" },
            { (1, 3), "3.2 VAC" },
            { (1, 4), "3.2 ASP" },
            { (1, 9), "GRILLFLEI" },
            { (1, 12), "LEBERWURST" },
            { (1, 13), "ZEBERKA" },
            { (1, 75), "3.2 VAC" },

            { (2, 2), "6.2 GAS HF" },
            { (2, 3), "SCHRAEGSCH" },
            { (2, 5), "SCHRAEGSCH" },
            { (2, 6), "KOCHSCHINK" },
            { (2, 10), "CKER 8X25G" },

            { (4, 0), "Werksprogramm" },
            { (4, 1), "2.2 Packung 500 gr." },
            { (4, 2), "3.1 HF Gas" },
            { (4, 3), "3.2 Scheiblet. 100gr klar" },
            { (4, 4), "3.3 HF Gas" },
            { (4, 5), "4.2 HF Gas (duo)" },
            { (4, 6), "4.3 HF Gas (duo)" },
            { (4, 7), "3.2Scheiblet. 100gr. bunt" },
            { (4, 8), "2.2 200g" },
            { (4, 9), "3.2 Scheiblet. 200g" },
            { (4, 10), "2.2 Scheibl. 250g" },
            { (4, 11), "3.1 Salami.Mantel 500g" },
            { (4, 12), "3.1 500gr" },
            { (4, 13), "3.3 vakuum" },
            { (4, 14), "3.3 Gas Scheiben" },
            { (4, 17), "332.33" },
            { (4, 19), "RIPPCHENFOLIE" },
            { (4, 20), "Fruhstuck knacker" },
            { (4, 81), "3.2 Scheiblet. 100g bunt" },
            { (4, 82), "3.2 Scheiblet. 100g klar" },
            { (4, 83), "3.2 Scheiblet. 200g" },
            { (4, 85), "2.2 Scheiblet. 500g" },
            { (4, 88), "wildfolie test" },
            { (4, 89), "3.2 250g Hartfolie" },
            { (4, 90), "gas test" },
            { (4, 91), "332 3.2 bunt 100g" },
            { (4, 92), "332 3.2 klar 100g" },
            { (4, 93), "331 3.3 STAPELFACH" },
            { (4, 94), "332 3.2 200g" },
            { (4, 95), "333 3.3 StapelDuo" },
            { (4, 96), "331 3.3 Stapel Scheiben" },
            { (4, 98), "3.2 331 100g scheibl. not" },
            { (4, 100), "4.2 HF Gas duo" },
        };

        public static string GetName(long maschinenId, int prnummer)
        {
            return _mapping.TryGetValue((maschinenId, prnummer), out var name)
                ? name
                : string.Empty;
        }
    }
}
