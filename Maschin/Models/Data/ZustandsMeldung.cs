using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MaschinenDataein.Models.Data;

[Table("Zustandsmeldung")]
public class ZustandsMeldung
{
    [Key]
    [Column("Id")]
    public long Id { get; set; }

    [Required]
    [MaxLength(500)]
    [Column("Meldung")]
    public string? Meldung { get; set; }
}