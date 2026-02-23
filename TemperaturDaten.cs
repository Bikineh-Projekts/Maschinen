using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MaschinenDataein.Models.Data
{
    [Table("Temperaturdaten")]
    public class TemperaturDaten
    {
        [Key]
        [Column("Id")]
        public long Id { get; set; }

        [Column("Timestamp")]
        [Required]
        public DateTime Timestamp { get; set; }

        [Column("MaschinenId")]
        [Required]
        public long MaschinenId { get; set; }

        [ForeignKey("MaschinenId")]
        public virtual Maschine? Maschine { get; set; }

        [Column("PRnummer")]
        [Required]
        public int PRnummer { get; set; }

        [Column("Solltemp1")]
        [Required]
        public int Solltemp1 { get; set; }

        [Column("Isstemp1")]
        [Required]
        public int Isstemp1 { get; set; }

        [Column("Solltemp2")]
        [Required]
        public int Solltemp2 { get; set; }

        [Column("Isstemp2")]
        [Required]
        public int Isstemp2 { get; set; }
    }
}