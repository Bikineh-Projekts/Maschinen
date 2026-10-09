using MaschinenDataein.Models.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MaschinenDataein.Models
{
    public class MaschinenDbContext : DbContext
    {
        public MaschinenDbContext(DbContextOptions<MaschinenDbContext> options)
            : base(options)
        {
        }

        // ── Stammdaten ──
        public DbSet<Maschine> Maschinen { get; set; } = null!;
        public DbSet<ZustandsMeldung> Zustandsmeldung { get; set; } = null!;
        public DbSet<StoerungsMeldung> Stoerungsmeldung { get; set; } = null!;

        // ── Messdaten (werden von der OPC-Anbindung geschrieben, hier nur gelesen) ──
        public DbSet<Leistungsdaten> Leistungsdaten { get; set; } = null!;
        public DbSet<TemperaturDaten> Temperaturdaten { get; set; } = null!;
        public DbSet<AbzugsDaten> Abzugsdaten { get; set; } = null!;
        public DbSet<Alarmdaten> Alarmdaten { get; set; } = null!;
        public DbSet<ZustandsDaten> Zustandsdaten { get; set; } = null!;
        public DbSet<StoerungsDaten> Stoerungsdaten { get; set; } = null!;

        // ── Produktionsplanung (manuelle Erfassung) ──
        public DbSet<Planungs> Planung { get; set; } = null!;
        public DbSet<PlanungStoerungen> PlanungStoerungen { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Tabellennamen, Spalten und Schlüssel kommen aus den Attributen der Modelle
            // ([Table], [Column], [Key]). Hier stehen nur noch Beziehungen und Datentypen.

            // ───────────── Stammdaten ─────────────
            modelBuilder.Entity<Maschine>().ToTable("maschinen");

            // ───────────── Messdaten → Maschine ─────────────
            MitMaschine(modelBuilder.Entity<Leistungsdaten>());
            MitMaschine(modelBuilder.Entity<TemperaturDaten>());
            MitMaschine(modelBuilder.Entity<AbzugsDaten>());
            MitMaschine(modelBuilder.Entity<Alarmdaten>());
            MitMaschine(modelBuilder.Entity<ZustandsDaten>());
            MitMaschine(modelBuilder.Entity<StoerungsDaten>());
            MitMaschine(modelBuilder.Entity<Planungs>());
            MitMaschine(modelBuilder.Entity<PlanungStoerungen>());

            // ───────────── Meldungen ─────────────
            modelBuilder.Entity<ZustandsDaten>()
                .HasOne(x => x.Zustandsmeldung)
                .WithMany()
                .HasForeignKey(x => x.ZustandsmeldungId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<StoerungsDaten>()
                .HasOne(x => x.Stoerungsmeldung)
                .WithMany()
                .HasForeignKey(x => x.StoerungsmeldungId)
                .OnDelete(DeleteBehavior.Restrict);

            // ───────────── Uhrzeiten der Planung ─────────────
            // In der Datenbank "time without time zone" (Standard wäre "interval")
            modelBuilder.Entity<Planungs>(e =>
            {
                e.Property(x => x.Starten).HasColumnType("time without time zone");
                e.Property(x => x.Stoppen).HasColumnType("time without time zone");
            });

            // ───────────── Datumsfelder ─────────────
            // Alle DateTime-Spalten als "timestamp without time zone".
            // Ersetzt die einzelnen HasColumnType-Aufrufe und deckt auch MHD ab –
            // sonst lehnt Npgsql das Speichern von Datumswerten ohne Zeitzone ab.
            var datumsFelder = modelBuilder.Model.GetEntityTypes()
                .SelectMany(t => t.GetProperties())
                .Where(p => p.ClrType == typeof(DateTime) || p.ClrType == typeof(DateTime?));

            foreach (var feld in datumsFelder)
                feld.SetColumnType("timestamp without time zone");
        }

        /// <summary>
        /// Beziehung "Datensatz → Maschine" über MaschinenId.
        /// Restrict: eine Maschine mit Daten kann nicht versehentlich mitgelöscht werden.
        /// </summary>
        private static void MitMaschine<T>(EntityTypeBuilder<T> entity) where T : class
        {
            entity.HasOne<Maschine>("Maschine")
                  .WithMany()
                  .HasForeignKey("MaschinenId")
                  .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
