using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MaschinenDataein.Models.Data
{
    [Table("Stoerungsdaten")]
    public class StoerungsDaten
    {
        [Key]
        [Column("Id")]
        public long Id { get; set; }

        [Required]
        [Column("Timestamp")]
        public DateTime Timestamp { get; set; }

        [Required]
        [Column("MaschinenId")]
        public long MaschinenId { get; set; }

        [ForeignKey(nameof(MaschinenId))]
        public virtual Maschine? Maschine { get; set; }

        [Required]
        [Column("StoerungsmeldungId")]
        public long StoerungsmeldungId { get; set; }

        [ForeignKey(nameof(StoerungsmeldungId))]
        public virtual StoerungsMeldung Stoerungsmeldung { get; set; } = null!;
    }
}