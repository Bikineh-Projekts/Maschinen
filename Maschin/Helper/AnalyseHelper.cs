using System.Globalization;
using MaschinenDataein.Models;
using MaschinenDataein.Models.Data;
using MaschinenDataein.Models.ModelView;
using Microsoft.EntityFrameworkCore;

namespace MaschinenDataein.Helper
{
    /// <summary>
    /// Berechnet die Analyse-Reiter für Abzug, Alarm, Störung und Zustand.
    /// Gezählt und gruppiert wird in der Datenbank; nur Alarme (94 Bit-Spalten) und
    /// Zustandsdauern werden gestreamt im Speicher ausgewertet.
    /// </summary>
    public static class AnalyseHelper
    {
        private static readonly CultureInfo De = CultureInfo.GetCultureInfo("de-DE");

        /// <summary>Serienfarben in fester Reihenfolge (geprüft), danach neutral.</summary>
        private static string SerienFarbe(int i) => i < 8 ? $"viz-{i + 1}" : "viz-weitere";

        // ── Zeitachse: bis 2 Tage je Stunde, sonst je Tag ────────────────────────
        private sealed record Zeitachse(bool JeStunde, List<DateTime> Punkte, List<string> Beschriftungen)
        {
            public int Index(DateTime t)
            {
                var schluessel = JeStunde ? new DateTime(t.Year, t.Month, t.Day, t.Hour, 0, 0) : t.Date;
                return Punkte.BinarySearch(schluessel);
            }
        }

        private static Zeitachse BaueZeitachse(DateTime von, DateTime bis)
        {
            var start = von.Date;
            var ende = bis.Date.AddDays(1);
            var jeStunde = (ende - start).TotalDays <= 2;
            var punkte = new List<DateTime>();
            var namen = new List<string>();

            for (var t = start; t < ende; t = jeStunde ? t.AddHours(1) : t.AddDays(1))
            {
                punkte.Add(t);
                namen.Add(jeStunde
                    ? ((ende - start).TotalDays > 1 ? t.ToString("dd.MM. HH:00", De) : t.ToString("HH:00", De))
                    : t.ToString("dd.MM.", De));
            }
            return new Zeitachse(jeStunde, punkte, namen);
        }

        private static string ZeitraumText(DateTime von, DateTime bis) =>
            von.Date == bis.Date ? von.ToString("dd.MM.yyyy", De) : $"{von:dd.MM.yyyy} – {bis:dd.MM.yyyy}";

        private static string Zahl(double wert, int stellen = 0) => wert.ToString("N" + stellen, De);

        // ═════════════════════════════════════════════════════════════════════
        // ABZUG
        // ═════════════════════════════════════════════════════════════════════
        public static async Task<AnalyseModel> AbzugAsync(MaschinenDbContext db, FilterModelView f, CancellationToken ct = default)
        {
            var aktiveMaschinenIds = DropDownListHelper.GetAktiveMaschinen(db).Select(m => m.Id).ToList();
            var a = new AnalyseModel { Zeitraum = ZeitraumText(f.DatumVon, f.DatumBis) };
            var daten = await db.Abzugsdaten.AsNoTracking()
                .ImZeitraum(f.DatumVon, f.DatumBis).FuerAktiveMaschinen(f.MaschinenId, aktiveMaschinenIds)
                .Select(x => new { x.Timestamp, x.MaschinenId, x.PRnummer, x.PackungenproAbzug, x.Abzuglaenge, Maschine = x.Maschine!.Bezeichnung })
                .ToListAsync(ct);
            if (daten.Count == 0) return a;

            a.Kennzahlen.Add(new Kennzahl { Titel = "Datensätze", Wert = Zahl(daten.Count), Icon = "bi-database" });
            a.Kennzahlen.Add(new Kennzahl { Titel = "Ø Packungen pro Abzug", Wert = Zahl(daten.Average(x => (double)x.PackungenproAbzug), 1), Icon = "bi-box-seam" });
            a.Kennzahlen.Add(new Kennzahl { Titel = "Ø Abzuglänge", Wert = Zahl(daten.Average(x => x.Abzuglaenge / 10.0), 1) + " mm", Icon = "bi-rulers" });
            a.Kennzahlen.Add(new Kennzahl
            {
                Titel = "Programme",
                Wert = Zahl(daten.Select(x => (x.MaschinenId, x.PRnummer)).Distinct().Count()),
                Hinweis = "unterschiedliche im Zeitraum",
                Icon = "bi-list-ol"
            });

            // Abzuglänge im Verlauf – je Maschine eine Linie (Mittelwert je Zeitschritt)
            var achse = BaueZeitachse(f.DatumVon, f.DatumBis);
            var verlauf = new Diagramm
            {
                Titel = "Abzuglänge im Verlauf",
                Hinweis = achse.JeStunde ? "Mittelwert je Stunde" : "Mittelwert je Tag",
                Art = DiagrammArt.Linie, Einheit = "mm", Breit = true,
                Beschriftungen = achse.Beschriftungen
            };
            var maschinen = daten.GroupBy(x => x.Maschine).OrderBy(g => g.Key).ToList();
            for (int i = 0; i < maschinen.Count; i++)
            {
                var werte = new double?[achse.Punkte.Count];
                foreach (var g in maschinen[i].GroupBy(x => achse.Index(x.Timestamp)).Where(g => g.Key >= 0))
                    werte[g.Key] = Math.Round(g.Average(x => x.Abzuglaenge / 10.0), 1);
                verlauf.Reihen.Add(new Reihe { Name = maschinen[i].Key, Farbe = SerienFarbe(i), Werte = werte.ToList() });
            }
            a.Diagramme.Add(verlauf);

            // Ø Packungen pro Abzug je Programm
            var programme = daten
                .GroupBy(x => (x.MaschinenId, x.PRnummer, x.Maschine))
                .Select(g => new
                {
                    Name = (LeerZuNull(ProgrammnamenHelper.GetName(g.Key.MaschinenId, g.Key.PRnummer)) ?? $"PR {g.Key.PRnummer}")
                           + (maschinen.Count > 1 ? $" ({g.Key.Maschine})" : ""),
                    Wert = Math.Round(g.Average(x => (double)x.PackungenproAbzug), 1)
                })
                .OrderByDescending(x => x.Wert).Take(12).ToList();
            a.Diagramme.Add(new Diagramm
            {
                Titel = "Ø Packungen pro Abzug je Programm", Art = DiagrammArt.BalkenWaagrecht, Einheit = "Stück",
                Beschriftungen = programme.Select(x => x.Name).ToList(),
                Reihen = { new Reihe { Name = "Ø Packungen", Farbe = "viz-1", Werte = programme.Select(x => (double?)x.Wert).ToList() } }
            });

            // Abzüge je Maschine
            a.Diagramme.Add(new Diagramm
            {
                Titel = "Datensätze je Maschine", Art = DiagrammArt.BalkenWaagrecht, Einheit = "Datensätze",
                Beschriftungen = maschinen.Select(g => g.Key).ToList(),
                Reihen = { new Reihe { Name = "Datensätze", Farbe = "viz-1", Werte = maschinen.Select(g => (double?)g.Count()).ToList() } }
            });
            return a;
        }

        // ═════════════════════════════════════════════════════════════════════
        // ALARM
        // ═════════════════════════════════════════════════════════════════════
        public static async Task<AnalyseModel> AlarmAsync(MaschinenDbContext db, FilterModelView f, CancellationToken ct = default)
        {
            var aktiveMaschinenIds = DropDownListHelper.GetAktiveMaschinen(db).Select(m => m.Id).ToList();
            var a = new AnalyseModel { Zeitraum = ZeitraumText(f.DatumVon, f.DatumBis) };
            var achse = BaueZeitachse(f.DatumVon, f.DatumBis);

            var jeAlarm = new int[Alarmdaten.AnzahlAlarme + 1];
            var jeZeit = new int[achse.Punkte.Count];
            var jeMaschine = new Dictionary<string, int>();
            int datensaetze = 0;

            var strom = db.Alarmdaten.AsNoTracking().Include(x => x.Maschine)
                .ImZeitraum(f.DatumVon, f.DatumBis).FuerAktiveMaschinen(f.MaschinenId, aktiveMaschinenIds)
                .Where(Alarmdaten.HatAktivenAlarm)
                .AsAsyncEnumerable().WithCancellation(ct);

            await foreach (var x in strom)
            {
                datensaetze++;
                var anzahl = 0;
                foreach (var n in x.AktiveAlarmNummern()) { jeAlarm[n]++; anzahl++; }
                var i = achse.Index(x.Timestamp);
                if (i >= 0) jeZeit[i] += anzahl;
                var m = x.Maschine?.Bezeichnung ?? $"Maschine {x.MaschinenId}";
                jeMaschine[m] = jeMaschine.GetValueOrDefault(m) + anzahl;
            }
            if (datensaetze == 0) return a;

            var rangliste = Enumerable.Range(1, Alarmdaten.AnzahlAlarme)
                .Where(n => jeAlarm[n] > 0)
                .Select(n => new { Name = AlarmName(n), Anzahl = jeAlarm[n] })
                .OrderByDescending(x => x.Anzahl).ToList();

            a.Kennzahlen.Add(new Kennzahl { Titel = "Alarm-Meldungen", Wert = Zahl(rangliste.Sum(x => x.Anzahl)), Hinweis = $"in {Zahl(datensaetze)} Datensätzen", Icon = "bi-bell" });
            a.Kennzahlen.Add(new Kennzahl { Titel = "Verschiedene Alarme", Wert = Zahl(rangliste.Count), Icon = "bi-list-ul" });
            a.Kennzahlen.Add(new Kennzahl { Titel = "Häufigster Alarm", Wert = rangliste[0].Name, Hinweis = $"{Zahl(rangliste[0].Anzahl)}-mal", Icon = "bi-exclamation-octagon" });
            a.Kennzahlen.Add(new Kennzahl { Titel = "Betroffene Maschinen", Wert = Zahl(jeMaschine.Count), Icon = "bi-cpu" });

            var top = rangliste.Take(10).ToList();
            a.Diagramme.Add(new Diagramm
            {
                Titel = "Häufigste Alarme", Hinweis = "Top 10 im Zeitraum", Art = DiagrammArt.BalkenWaagrecht, Einheit = "mal",
                Beschriftungen = top.Select(x => x.Name).ToList(),
                Reihen = { new Reihe { Name = "Anzahl", Farbe = "status-kritisch", Werte = top.Select(x => (double?)x.Anzahl).ToList() } }
            });
            a.Diagramme.Add(ZeitDiagramm("Alarme im Verlauf", achse, jeZeit, "status-kritisch", "Alarme"));
            a.Diagramme.Add(MaschinenDiagramm("Alarme je Maschine", jeMaschine, "status-kritisch", "Alarme"));
            return a;
        }

        // ═════════════════════════════════════════════════════════════════════
        // STÖRUNG
        // ═════════════════════════════════════════════════════════════════════
        public static async Task<AnalyseModel> StoerungAsync(MaschinenDbContext db, FilterModelView f, CancellationToken ct = default)
        {
            var aktiveMaschinenIds = DropDownListHelper.GetAktiveMaschinen(db).Select(m => m.Id).ToList();
            var a = new AnalyseModel { Zeitraum = ZeitraumText(f.DatumVon, f.DatumBis) };
            var daten = await db.Stoerungsdaten.AsNoTracking()
                .ImZeitraum(f.DatumVon, f.DatumBis).FuerAktiveMaschinen(f.MaschinenId, aktiveMaschinenIds)
                .Select(x => new { x.Timestamp, Maschine = x.Maschine!.Bezeichnung, Meldung = x.Stoerungsmeldung.Meldung })
                .ToListAsync(ct);
            if (daten.Count == 0) return a;

            var rangliste = daten.GroupBy(x => x.Meldung ?? "Unbekannt")
                .Select(g => new { Name = g.Key, Anzahl = g.Count() })
                .OrderByDescending(x => x.Anzahl).ToList();

            a.Kennzahlen.Add(new Kennzahl { Titel = "Störungen", Wert = Zahl(daten.Count), Icon = "bi-exclamation-triangle" });
            a.Kennzahlen.Add(new Kennzahl { Titel = "Verschiedene Meldungen", Wert = Zahl(rangliste.Count), Icon = "bi-list-ul" });
            a.Kennzahlen.Add(new Kennzahl { Titel = "Häufigste Störung", Wert = rangliste[0].Name, Hinweis = $"{Zahl(rangliste[0].Anzahl)}-mal", Icon = "bi-exclamation-triangle" });
            a.Kennzahlen.Add(new Kennzahl { Titel = "Betroffene Maschinen", Wert = Zahl(daten.Select(x => x.Maschine).Distinct().Count()), Icon = "bi-cpu" });

            var top = rangliste.Take(10).ToList();
            a.Diagramme.Add(new Diagramm
            {
                Titel = "Häufigste Störungen", Hinweis = "Top 10 im Zeitraum", Art = DiagrammArt.BalkenWaagrecht, Einheit = "mal",
                Beschriftungen = top.Select(x => x.Name).ToList(),
                Reihen = { new Reihe { Name = "Anzahl", Farbe = "status-ernst", Werte = top.Select(x => (double?)x.Anzahl).ToList() } }
            });

            var achse = BaueZeitachse(f.DatumVon, f.DatumBis);
            var jeZeit = new int[achse.Punkte.Count];
            foreach (var x in daten) { var i = achse.Index(x.Timestamp); if (i >= 0) jeZeit[i]++; }
            a.Diagramme.Add(ZeitDiagramm("Störungen im Verlauf", achse, jeZeit, "status-ernst", "Störungen"));

            var jeMaschine = daten.GroupBy(x => x.Maschine ?? "–").ToDictionary(g => g.Key, g => g.Count());
            a.Diagramme.Add(MaschinenDiagramm("Störungen je Maschine", jeMaschine, "status-ernst", "Störungen"));
            return a;
        }

        // ═════════════════════════════════════════════════════════════════════
        // ZUSTAND – Dauer je Zustand aus den Zeitpunkten der Zustandswechsel
        // ═════════════════════════════════════════════════════════════════════
        public static async Task<AnalyseModel> ZustandAsync(MaschinenDbContext db, FilterModelView f, CancellationToken ct = default)
        {
            var aktiveMaschinenIds = DropDownListHelper.GetAktiveMaschinen(db).Select(m => m.Id).ToList();
            var a = new AnalyseModel { Zeitraum = ZeitraumText(f.DatumVon, f.DatumBis) };
            var daten = await db.Zustandsdaten.AsNoTracking()
                .ImZeitraum(f.DatumVon, f.DatumBis).FuerAktiveMaschinen(f.MaschinenId, aktiveMaschinenIds)
                .OrderBy(x => x.MaschinenId).ThenBy(x => x.Timestamp)
                .Select(x => new { x.Timestamp, x.MaschinenId, Maschine = x.Maschine!.Bezeichnung, Meldung = x.Zustandsmeldung.Meldung })
                .ToListAsync(ct);
            if (daten.Count == 0) return a;

            // Ein Zustand gilt bis zum nächsten Wechsel derselben Maschine.
            // Lücken über 4 Stunden (z. B. Datenausfall, Wochenende) werden nicht mitgezählt.
            var maxDauer = TimeSpan.FromHours(4);
            var periodeEnde = f.DatumBis.Date.AddDays(1);
            var jetzt = DateTime.Now;
            var dauern = new List<(string Maschine, string Zustand, double Stunden)>();

            foreach (var maschine in daten.GroupBy(x => x.MaschinenId))
            {
                var liste = maschine.ToList();
                for (int i = 0; i < liste.Count; i++)
                {
                    var ende = i + 1 < liste.Count ? liste[i + 1].Timestamp : (periodeEnde < jetzt ? periodeEnde : jetzt);
                    var d = ende - liste[i].Timestamp;
                    if (d <= TimeSpan.Zero) continue;
                    if (d > maxDauer) d = maxDauer;
                    dauern.Add((liste[i].Maschine ?? $"Maschine {liste[i].MaschinenId}", liste[i].Meldung ?? "Unbekannt", d.TotalHours));
                }
            }

            var jeZustand = dauern.GroupBy(x => x.Zustand)
                .Select(g => new { Name = g.Key, Stunden = g.Sum(x => x.Stunden), Anzahl = g.Count() })
                .OrderByDescending(x => x.Stunden).ToList();
            var gesamt = jeZustand.Sum(x => x.Stunden);

            a.Kennzahlen.Add(new Kennzahl { Titel = "Zustandswechsel", Wert = Zahl(daten.Count), Icon = "bi-arrow-left-right" });
            a.Kennzahlen.Add(new Kennzahl { Titel = "Verschiedene Zustände", Wert = Zahl(jeZustand.Count), Icon = "bi-list-ul" });
            if (jeZustand.Count > 0)
                a.Kennzahlen.Add(new Kennzahl
                {
                    Titel = "Längster Zustand", Wert = jeZustand[0].Name,
                    Hinweis = $"{Zahl(jeZustand[0].Stunden, 1)} Std. · {Zahl(gesamt > 0 ? jeZustand[0].Stunden * 100 / gesamt : 0)} % der Zeit",
                    Icon = "bi-hourglass-split"
                });
            a.Kennzahlen.Add(new Kennzahl { Titel = "Erfasste Zeit", Wert = Zahl(gesamt, 1) + " Std.", Hinweis = "Summe aller Maschinen", Icon = "bi-clock" });

            // Gestapelt: Stunden je Zustand je Maschine (7 größte Zustände + "Weitere")
            var hauptZustaende = jeZustand.Take(7).Select(x => x.Name).ToList();
            var maschinen = dauern.Select(x => x.Maschine).Distinct().OrderBy(x => x).ToList();
            var gestapelt = new Diagramm
            {
                Titel = "Zeit je Zustand und Maschine", Hinweis = "Stunden im Zeitraum",
                Art = DiagrammArt.GestapeltWaagrecht, Einheit = "Std.", Breit = true, Beschriftungen = maschinen
            };
            for (int i = 0; i < hauptZustaende.Count; i++)
            {
                var z = hauptZustaende[i];
                gestapelt.Reihen.Add(new Reihe
                {
                    Name = z, Farbe = SerienFarbe(i),
                    Werte = maschinen.Select(m => (double?)Math.Round(dauern.Where(x => x.Maschine == m && x.Zustand == z).Sum(x => x.Stunden), 2)).ToList()
                });
            }
            if (jeZustand.Count > hauptZustaende.Count)
                gestapelt.Reihen.Add(new Reihe
                {
                    Name = "Weitere", Farbe = "viz-weitere",
                    Werte = maschinen.Select(m => (double?)Math.Round(dauern.Where(x => x.Maschine == m && !hauptZustaende.Contains(x.Zustand)).Sum(x => x.Stunden), 2)).ToList()
                });
            a.Diagramme.Add(gestapelt);

            var top = jeZustand.Take(10).ToList();
            a.Diagramme.Add(new Diagramm
            {
                Titel = "Dauer je Zustand", Hinweis = "alle Maschinen zusammen", Art = DiagrammArt.BalkenWaagrecht, Einheit = "Std.",
                Beschriftungen = top.Select(x => x.Name).ToList(),
                Reihen = { new Reihe { Name = "Stunden", Farbe = "viz-1", Werte = top.Select(x => (double?)Math.Round(x.Stunden, 1)).ToList() } }
            });
            a.Diagramme.Add(new Diagramm
            {
                Titel = "Häufigste Zustände", Hinweis = "Anzahl Wechsel in den Zustand", Art = DiagrammArt.BalkenWaagrecht, Einheit = "mal",
                Beschriftungen = jeZustand.OrderByDescending(x => x.Anzahl).Take(10).Select(x => x.Name).ToList(),
                Reihen = { new Reihe { Name = "Anzahl", Farbe = "viz-1", Werte = jeZustand.OrderByDescending(x => x.Anzahl).Take(10).Select(x => (double?)x.Anzahl).ToList() } }
            });
            return a;
        }

        // ── Gemeinsame Bausteine ─────────────────────────────────────────────────
        private static Diagramm ZeitDiagramm(string titel, Zeitachse achse, int[] werte, string farbe, string name) => new()
        {
            Titel = titel, Hinweis = achse.JeStunde ? "je Stunde" : "je Tag", Art = DiagrammArt.Saeulen, Einheit = name, Breit = true,
            Beschriftungen = achse.Beschriftungen,
            Reihen = { new Reihe { Name = name, Farbe = farbe, Werte = werte.Select(x => (double?)x).ToList() } }
        };

        private static Diagramm MaschinenDiagramm(string titel, Dictionary<string, int> jeMaschine, string farbe, string name)
        {
            var liste = jeMaschine.OrderByDescending(x => x.Value).ToList();
            return new Diagramm
            {
                Titel = titel, Art = DiagrammArt.BalkenWaagrecht, Einheit = name,
                Beschriftungen = liste.Select(x => x.Key).ToList(),
                Reihen = { new Reihe { Name = name, Farbe = farbe, Werte = liste.Select(x => (double?)x.Value).ToList() } }
            };
        }

        private static string AlarmName(int n) =>
            AlarmnamenHelper.Alarmnamen.TryGetValue(n, out var name) ? name.TrimEnd('!') : $"Alarm {n}";

        private static string? LeerZuNull(string? s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();
    }
}
