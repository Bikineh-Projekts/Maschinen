using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MaschinenDataein.Models.Data
{
    [Table("maschinen")]
    public class Maschine
    {
        [Key]
        [Column("Id")]
        public long Id { get; set; }

        [Required]
        [Column("Bezeichnung")]
        [StringLength(200)]
        public string Bezeichnung { get; set; } = string.Empty;

        [Column("IpAdresse")]  
        [StringLength(50)]
        public string IpAdresse { get; set; } = string.Empty;
    }
}