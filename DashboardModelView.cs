using MaschinenDataein.Models.Data;
using MaschinenDataein.Models.ModelView;

namespace MaschinenDataein.Models.ModelView
{
    public class DashboardModelView
    {
        public List<Planungs>? ProgrammenList { get; set; } = new();
        public List<LeistungsdatenModelView>? LeistungsDatenList { get; set; } = new();
        public List<AbzugsdatenModelView>? AbzugsDatenList { get; set; } = new();
        public List<TemperaturdatenModelView>? TemperaturDatenList { get; set; } = new();
        public List<Alarmdaten>? AlarmDatenList { get; set; } = new();
        public List<ZustandsDaten>? ZustandsDatenList { get; set; } = new();
        public List<StoerungsDaten>? StoerungsDatenList { get; set; } = new();
    }
}