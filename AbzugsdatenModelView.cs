using MaschinenDataein.Models.Data;
using MaschinenDataein.Helper;
using MaschinenDataein.Models.ModelView;
using MaschinenDataein.Models.PaginatedModel;
using System;
using System.Linq;

namespace MaschinenDataein.Models.ModelView
{
    public class AbzugsdatenModelView
    {
  
        public AbzugsdatenModelView(MaschinenDbContext context, AbzugsDaten AbzugsDaten)
        {
            Id = AbzugsDaten.Id;
            Timestamp = AbzugsDaten.Timestamp;
            Maschine = AbzugsDaten.Maschine?.Bezeichnung;       
            PackungenproAbzug = AbzugsDaten.PackungenproAbzug;
            Abzuglaenge = AbzugsDaten.Abzuglaenge;

        }
    

        public long Id { get; set; }
        public DateTime Timestamp { get; set; }
        public long MaschinenId { get; set; }
        public string? Maschine { get; set; }
        public string? Name{ get; set; }
        public long PackungenproAbzug { get; set; }
        public long Abzuglaenge { get; set; }
       
    }
}
