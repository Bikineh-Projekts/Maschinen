using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MaschinenDataein.Models.Data
{
    [Table("Zustandsdaten")]
    public class ZustandsDaten
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
        [Column("ZustandsmeldungId")]
        public long ZustandsmeldungId { get; set; }

        [ForeignKey(nameof(ZustandsmeldungId))]
        public virtual ZustandsMeldung Zustandsmeldung { get; set; } = null!;
    }
}