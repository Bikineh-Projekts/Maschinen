using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MaschinenDataein.Models.Data;

[Table("Stoerungsmeldung")]
public class StoerungsMeldung
{
    [Key]
    [Column("Id")]
    public long Id { get; set; }

    [Required]
    [MaxLength(500)]
    [Column("Meldung")]
    public string? Meldung { get; set; }
}