using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MaschinenDataein.Models.Data
{
    [Table("leistungsdaten")]
    public class Leistungsdaten  
    {
        [Key]
        [Column("Id")]
        public long Id { get; set; }

        [Column("MaschinenId")]
        [ForeignKey("Maschine")]
        public long MaschinenId { get; set; }

        [Column("Timestamp")]
        [Required]
        public DateTime Timestamp { get; set; }

        [Column("PRnummer")]
        public int PRnummer { get; set; }

        [Column("Tagestaktzaehler")]
        public int Tagestaktzaehler { get; set; }

        [Column("Packungszaeler")]
        public int Packungszaeler { get; set; }

        [Column("Maschinentakte")]
        public int Maschinentakte { get; set; }

        // Navigation Property
        [ForeignKey("MaschinenId")]
        public virtual Maschine? Maschine { get; set; }
    }
}