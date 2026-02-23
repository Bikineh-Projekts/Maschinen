using MaschinenDataein.Models.Data;
using Microsoft.EntityFrameworkCore;

namespace MaschinenDataein.Models
{
    public class MaschinenDbContext : DbContext
    {
        public MaschinenDbContext(DbContextOptions<MaschinenDbContext> options)
            : base(options)
        {
        }

        public DbSet<Planungs> Planung { get; set; } = null!;
        public DbSet<Leistungsdaten> Leistungsdaten { get; set; } = null!;
        public DbSet<TemperaturDaten> Temperaturdaten { get; set; } = null!;
        public DbSet<AbzugsDaten> Abzugsdaten { get; set; } = null!;
        public DbSet<Alarmdaten> Alarmdaten { get; set; } = null!;
        public DbSet<ZustandsDaten> Zustandsdaten { get; set; } = null!;
        public DbSet<ZustandsMeldung> Zustandsmeldung { get; set; } = null!;
        public DbSet<StoerungsDaten> Stoerungsdaten { get; set; } = null!;
        public DbSet<StoerungsMeldung> Stoerungsmeldung { get; set; } = null!;
        public DbSet<Maschine> Maschinen { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Primärschlüssel explizit definieren
            modelBuilder.Entity<Maschine>().HasKey(m => m.Id);
            modelBuilder.Entity<Planungs>().HasKey(p => p.Id);
            modelBuilder.Entity<Leistungsdaten>().HasKey(l => l.Id);
            modelBuilder.Entity<TemperaturDaten>().HasKey(t => t.Id);
            modelBuilder.Entity<AbzugsDaten>().HasKey(a => a.Id);
            modelBuilder.Entity<Alarmdaten>().HasKey(a => a.Id);
            modelBuilder.Entity<ZustandsDaten>().HasKey(z => z.Id);
            modelBuilder.Entity<ZustandsMeldung>().HasKey(z => z.Id);
            modelBuilder.Entity<StoerungsDaten>().HasKey(s => s.Id);
            modelBuilder.Entity<StoerungsMeldung>().HasKey(s => s.Id);

            // Tabellennamen (✅ Planungs mit uppercase P wie in der DB)
            modelBuilder.Entity<Maschine>().ToTable("maschinen");
            modelBuilder.Entity<Planungs>().ToTable("Planungs");
            modelBuilder.Entity<Leistungsdaten>().ToTable("Leistungsdaten");
            modelBuilder.Entity<TemperaturDaten>().ToTable("Temperaturdaten");
            modelBuilder.Entity<AbzugsDaten>().ToTable("Abzugsdaten");
            modelBuilder.Entity<Alarmdaten>().ToTable("Alarmdaten");
            modelBuilder.Entity<ZustandsDaten>().ToTable("Zustandsdaten");
            modelBuilder.Entity<ZustandsMeldung>().ToTable("Zustandsmeldung");
            modelBuilder.Entity<StoerungsDaten>().ToTable("Stoerungsdaten");
            modelBuilder.Entity<StoerungsMeldung>().ToTable("Stoerungsmeldung");

            // Timestamp Konfigurationen
            modelBuilder.Entity<Leistungsdaten>()
                .Property(x => x.Timestamp)
                .HasColumnType("timestamp without time zone");

            modelBuilder.Entity<TemperaturDaten>()
                .Property(x => x.Timestamp)
                .HasColumnType("timestamp without time zone");

            modelBuilder.Entity<Alarmdaten>()
                .Property(x => x.Timestamp)
                .HasColumnType("timestamp without time zone");

            modelBuilder.Entity<ZustandsDaten>()
                .Property(x => x.Timestamp)
                .HasColumnType("timestamp without time zone");

            modelBuilder.Entity<StoerungsDaten>()
                .Property(x => x.Timestamp)
                .HasColumnType("timestamp without time zone");

            modelBuilder.Entity<AbzugsDaten>()
                .Property(x => x.Timestamp)
                .HasColumnType("timestamp without time zone");

            modelBuilder.Entity<Planungs>()
                .Property(x => x.Datum)
                .HasColumnType("timestamp without time zone");

            // Beziehungen konfigurieren
            ConfigureRelationships(modelBuilder);
        }

        private void ConfigureRelationships(ModelBuilder modelBuilder)
        {
            // Leistungsdaten -> Maschine
            modelBuilder.Entity<Leistungsdaten>()
                .HasOne(x => x.Maschine)
                .WithMany()
                .HasForeignKey(x => x.MaschinenId)
                .OnDelete(DeleteBehavior.Restrict);

            // TemperaturDaten -> Maschine
            modelBuilder.Entity<TemperaturDaten>()
                .HasOne(x => x.Maschine)
                .WithMany()
                .HasForeignKey(x => x.MaschinenId)
                .OnDelete(DeleteBehavior.Restrict);

            // AbzugsDaten -> Maschine
            modelBuilder.Entity<AbzugsDaten>()
                .HasOne(x => x.Maschine)
                .WithMany()
                .HasForeignKey(x => x.MaschinenId)
                .OnDelete(DeleteBehavior.Restrict);

            // Alarmdaten -> Maschine
            modelBuilder.Entity<Alarmdaten>()
                .HasOne(x => x.Maschine)
                .WithMany()
                .HasForeignKey(x => x.MaschinenId)
                .OnDelete(DeleteBehavior.Restrict);

            // ZustandsDaten -> Maschine
            modelBuilder.Entity<ZustandsDaten>()
                .HasOne(x => x.Maschine)
                .WithMany()
                .HasForeignKey(x => x.MaschinenId)
                .OnDelete(DeleteBehavior.Restrict);

            // ZustandsDaten -> ZustandsMeldung
            modelBuilder.Entity<ZustandsDaten>()
                .HasOne(x => x.Zustandsmeldung)
                .WithMany()
                .HasForeignKey(x => x.ZustandsmeldungId)
                .OnDelete(DeleteBehavior.Restrict);

            // StoerungsDaten -> Maschine
            modelBuilder.Entity<StoerungsDaten>()
                .HasOne(x => x.Maschine)
                .WithMany()
                .HasForeignKey(x => x.MaschinenId)
                .OnDelete(DeleteBehavior.Restrict);

            // StoerungsDaten -> StoerungsMeldung
            modelBuilder.Entity<StoerungsDaten>()
                .HasOne(x => x.Stoerungsmeldung)
                .WithMany()
                .HasForeignKey(x => x.StoerungsmeldungId)
                .OnDelete(DeleteBehavior.Restrict);

             modelBuilder.Entity<Planungs>()
            .Property(x => x.MaschinenId)
            .HasColumnName("MaschinenId");
        }
    }
}