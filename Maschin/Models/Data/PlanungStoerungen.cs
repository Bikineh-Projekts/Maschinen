using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MaschinenDataein.Models.Data
{
    /// <summary>
    /// Manuell erfasste Störungen/Stillstände aus der Produktionsplanung.
    /// Neue Tabelle – Skript: Datenbank/PostgreSQL/01_PlanungStoerungen.sql
    /// </summary>
    [Table("PlanungStoerungen")]
    public class PlanungStoerungen
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

        [Column("Von")]
        public TimeSpan Von { get; set; }

        [Column("Bis")]
        public TimeSpan Bis { get; set; }

        [Column("Grund")]
        [StringLength(200)]
        public string? Grund { get; set; }

        [ForeignKey(nameof(MaschinenId))]
        public virtual Maschine? Maschine { get; set; }

        /// <summary>Dauer in Minuten, auch über Mitternacht.</summary>
        [NotMapped]
        public int DauerMinuten
        {
            get
            {
                var diff = Bis - Von;
                if (diff < TimeSpan.Zero) diff += TimeSpan.FromDays(1);
                return (int)diff.TotalMinutes;
            }
        }
    }
}
