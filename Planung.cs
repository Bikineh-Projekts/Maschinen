using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MaschinenDataein.Models.Data
{
    [Table("Planungs")]
    public class Planungs
    {
        [Key]
        [Column("Id")]
        public long Id { get; set; }

        [Column("Datum")]
        [Required]
        public DateTime Datum { get; set; }


        [Column("MaschinenId")]
        [Required]
        public long MaschinenId { get; set; }

        [Column("Personalsoll")]
        public int? Personalsoll { get; set; }

        [Column("Personalnamen")]
        [StringLength(500)]
        public string? Personalnamen { get; set; }

        // Produktionserfassung-Felder
        [Column("Artikel")]
        [StringLength(100)]
        public string? Artikel { get; set; }

        [Column("Sollmenge")]
        public int? Sollmenge { get; set; }

        [Column("MHD")]
        public DateTime? MHD { get; set; }

        [Column("Kartonsanzahl")]
        [StringLength(100)]
        public string? Kartonsanzahl { get; set; }

        [Column("PersonalIst")]
        public int? PersonalIst { get; set; }

        [Column("Fertigware")]
        public int? Fertigware { get; set; }

        [Column("Starten")]
        public TimeSpan? Starten { get; set; }

        [Column("Stoppen")]
        public TimeSpan? Stoppen { get; set; }

        [Column("Pause")]
        public int? Pause { get; set; }

        // Navigation Property
        [ForeignKey(nameof(MaschinenId))]
        public virtual Maschine? Maschine { get; set; }
    }
}