using MaschinenDataein.Models.Data;
using System;
using System.Linq;

namespace MaschinenDataein.Models.ModelView
{
    public class LeistungsdatenModelView  
    {

        public LeistungsdatenModelView(MaschinenDbContext context, Leistungsdaten leistungsdaten)
        {
            Id = leistungsdaten.Id;
            Timestamp = leistungsdaten.Timestamp;
            MaschinenId = leistungsdaten.MaschinenId;
            Maschine = leistungsdaten.Maschine?.Bezeichnung;
            Tagestaktzaehler = leistungsdaten.Tagestaktzaehler;
            Packungszaehler = leistungsdaten.Packungszaeler;
            Maschinentakte = leistungsdaten.Maschinentakte;
        }

        public long Id { get; set; }
        public DateTime Timestamp { get; set; }
        public long MaschinenId { get; set; }
        public string? Maschine { get; set; }
        public string? Name { get; set; }
        public int Tagestaktzaehler { get; set; }
        public int Packungszaehler { get; set; }
        public int Maschinentakte { get; set; }
     
    }
}