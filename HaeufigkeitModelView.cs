namespace MaschinenDataein.Models.ModelView
{
    public class HaeufigkeitModelView
    {
        public List<HaeufigMeldung> TopAlarmmeldungen { get; set; } = new();
        public List<HaeufigMeldung> TopZustandsmeldungen { get; set; } = new();
        public List<HaeufigMeldung> TopStoerungsmeldungen { get; set; } = new();
    }

    public class HaeufigMeldung
    {
        public string Meldung { get; set; } = string.Empty;
        public int Anzahl { get; set; }
    }
}
