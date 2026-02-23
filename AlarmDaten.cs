using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MaschinenDataein.Models.Data

{
    [Table("Alarmdaten")]
 public class Alarmdaten
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

        [Column("AM1")] public bool AM1 { get; set; }
        [Column("AM2")] public bool AM2 { get; set; }
        [Column("AM3")] public bool AM3 { get; set; }
        [Column("AM4")] public bool AM4 { get; set; }
        [Column("AM5")] public bool AM5 { get; set; }
        [Column("AM6")] public bool AM6 { get; set; }
        [Column("AM7")] public bool AM7 { get; set; }
        [Column("AM8")] public bool AM8 { get; set; }
        [Column("AM9")] public bool AM9 { get; set; }
        [Column("AM10")] public bool AM10 { get; set; }
        [Column("AM11")] public bool AM11 { get; set; }
        [Column("AM12")] public bool AM12 { get; set; }
        [Column("AM13")] public bool AM13 { get; set; }
        [Column("AM14")] public bool AM14 { get; set; }
        [Column("AM15")] public bool AM15 { get; set; }
        [Column("AM16")] public bool AM16 { get; set; }
        [Column("AM17")] public bool AM17 { get; set; }
        [Column("AM18")] public bool AM18 { get; set; }
        [Column("AM19")] public bool AM19 { get; set; }
        [Column("AM20")] public bool AM20 { get; set; }
        [Column("AM21")] public bool AM21 { get; set; }
        [Column("AM22")] public bool AM22 { get; set; }
        [Column("AM23")] public bool AM23 { get; set; }
        [Column("AM24")] public bool AM24 { get; set; }
        [Column("AM25")] public bool AM25 { get; set; }
        [Column("AM26")] public bool AM26 { get; set; }
        [Column("AM27")] public bool AM27 { get; set; }
        [Column("AM28")] public bool AM28 { get; set; }
        [Column("AM29")] public bool AM29 { get; set; }
        [Column("AM30")] public bool AM30 { get; set; }
        [Column("AM31")] public bool AM31 { get; set; }
        [Column("AM32")] public bool AM32 { get; set; }
        [Column("AM33")] public bool AM33 { get; set; }
        [Column("AM34")] public bool AM34 { get; set; }
        [Column("AM35")] public bool AM35 { get; set; }
        [Column("AM36")] public bool AM36 { get; set; }
        [Column("AM37")] public bool AM37 { get; set; }
        [Column("AM38")] public bool AM38 { get; set; }
        [Column("AM39")] public bool AM39 { get; set; }
        [Column("AM40")] public bool AM40 { get; set; }
        [Column("AM41")] public bool AM41 { get; set; }
        [Column("AM42")] public bool AM42 { get; set; }
        [Column("AM43")] public bool AM43 { get; set; }
        [Column("AM44")] public bool AM44 { get; set; }
        [Column("AM45")] public bool AM45 { get; set; }
        [Column("AM46")] public bool AM46 { get; set; }
        [Column("AM47")] public bool AM47 { get; set; }
        [Column("AM48")] public bool AM48 { get; set; }
        [Column("AM49")] public bool AM49 { get; set; }
        [Column("AM50")] public bool AM50 { get; set; }
        [Column("AM51")] public bool AM51 { get; set; }
        [Column("AM52")] public bool AM52 { get; set; }
        [Column("AM53")] public bool AM53 { get; set; }
        [Column("AM54")] public bool AM54 { get; set; }
        [Column("AM55")] public bool AM55 { get; set; }
        [Column("AM56")] public bool AM56 { get; set; }
        [Column("AM57")] public bool AM57 { get; set; }
        [Column("AM58")] public bool AM58 { get; set; }
        [Column("AM59")] public bool AM59 { get; set; }
        [Column("AM60")] public bool AM60 { get; set; }
        [Column("AM61")] public bool AM61 { get; set; }
        [Column("AM62")] public bool AM62 { get; set; }
        [Column("AM63")] public bool AM63 { get; set; }
        [Column("AM64")] public bool AM64 { get; set; }
        [Column("AM65")] public bool AM65 { get; set; }
        [Column("AM66")] public bool AM66 { get; set; }
        [Column("AM67")] public bool AM67 { get; set; }
        [Column("AM68")] public bool AM68 { get; set; }
        [Column("AM69")] public bool AM69 { get; set; }
        [Column("AM70")] public bool AM70 { get; set; }
        [Column("AM71")] public bool AM71 { get; set; }
        [Column("AM72")] public bool AM72 { get; set; }
        [Column("AM73")] public bool AM73 { get; set; }
        [Column("AM74")] public bool AM74 { get; set; }
        [Column("AM75")] public bool AM75 { get; set; }
        [Column("AM76")] public bool AM76 { get; set; }
        [Column("AM77")] public bool AM77 { get; set; }
        [Column("AM78")] public bool AM78 { get; set; }
        [Column("AM79")] public bool AM79 { get; set; }
        [Column("AM80")] public bool AM80 { get; set; }
        [Column("AM81")] public bool AM81 { get; set; }
        [Column("AM82")] public bool AM82 { get; set; }
        [Column("AM83")] public bool AM83 { get; set; }
        [Column("AM84")] public bool AM84 { get; set; }
        [Column("AM85")] public bool AM85 { get; set; }
        [Column("AM86")] public bool AM86 { get; set; }
        [Column("AM87")] public bool AM87 { get; set; }
        [Column("AM88")] public bool AM88 { get; set; }
        [Column("AM89")] public bool AM89 { get; set; }
        [Column("AM90")] public bool AM90 { get; set; }
        [Column("AM91")] public bool AM91 { get; set; }
        [Column("AM92")] public bool AM92 { get; set; }
        [Column("AM93")] public bool AM93 { get; set; }
        [Column("AM94")] public bool AM94 { get; set; }
    }
}