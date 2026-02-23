using System;

namespace MaschinenDataein.Models.ModelView
{
    public class GrunddatenModelView
    {
        public DateTime? Datum { get; set; }

     
        public long MaschinenId { get; set; }

        public int? Personalsoll { get; set; }
        public string? Personalnamen { get; set; }
    }
}