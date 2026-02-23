using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MaschinenDataein.Models.Data;

[Table("Abzugsdaten")]
public class AbzugsDaten
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
    [Column("PRnummer")]
    public int PRnummer { get; set; }

    [Required]
    [Column("PackungenproAbzug")]
    public long PackungenproAbzug { get; set; }

    [Required]
    [Column("Abzuglaenge")]
    public long Abzuglaenge { get; set; }
}