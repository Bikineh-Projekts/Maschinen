using MaschinenDataein.Models.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MaschinenDataein.Models.ModelView
{
    public class TemperaturdatenModelView
    {
        public TemperaturdatenModelView() { }

        public long Id { get; set; }
        public DateTime Timestamp { get; set; }
        public string? Maschine { get; set; }
        public long MaschinenId { get; set; }
        public string? Name { get; set; }
        public int Solltemp1 { get; set; }
        public int Isstemp1 { get; set; }
        public int Solltemp2 { get; set; }
        public int Isstemp2 { get; set; }

        
    }
}
