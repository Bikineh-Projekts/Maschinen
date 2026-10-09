# Änderungen – Optimierung DbContext & Datenzugriff

Namen von Klassen, Tabellen, Controllern und Views sind unverändert. .NET 8 / EF Core 8 / **nur PostgreSQL**.

## Datenbank & DbContext
- Ausschließlich PostgreSQL (Npgsql). SQL Server, LocalDB und das SQL-Server-Projekt sind entfernt.
- Connection String als `Host=…;Database=…` oder als Render-URL `postgres://…` (wird automatisch umgewandelt).
- `MaschinenDbContext`: doppelte Konfiguration entfernt, Maschinen-Beziehungen über eine gemeinsame
  Methode, alle Datumsfelder zentral als `timestamp without time zone` (auch MHD).
- `Leistungsdaten`: `[Table("leistungsdaten")]` widersprach dem DbContext → vereinheitlicht.
- `Alarmdaten`: AM1–AM94 als `bool?` (NULL-Werte lösen keine Exception mehr aus).
- Startprüfung: Beim Start steht im Log, ob PostgreSQL erreichbar ist – bzw. die genaue Ursache.

## Leistung
- Leistung, Abzug, Temperatur, Alarm: Seitenaufteilung in der Datenbank (nur 50 Datensätze pro Seite).
- Alarm & Häufigkeit: Filter „Alarm aktiv“ in SQL, kein Reflection mehr.
- Zustand & Störung: Maschinenfilter in der Datenbank statt im Speicher.
- Startseite lädt nur noch die letzten 20 Planungen.
- Alle Leseabfragen ohne Change-Tracking (`AsNoTracking`).

## Fehler behoben
- `_ViewImports.cshtml`/`_ViewStart.cshtml` lagen in `Views/Shared` und galten nur dort
  → `asp-action`-Formulare (z. B. Planung speichern) funktionierten in anderen Views nicht.
- Filter „bis“ ergab einen Tag zu viel.
- Planung: Es wurde nur die erste Produktionszeile gespeichert – jetzt alle Zeilen plus Störungen.
- Drei Dateien waren ISO-8859-kodiert (Umlaute kaputt) → UTF-8.

## Datenbank-Skripte (Ordner `Datenbank/PostgreSQL`)
| Datei | Zweck |
|---|---|
| `00_Schema.sql` | Alle Tabellen (nur fehlende werden angelegt) + Beispiel-Maschinen 1, 2, 4 |
| `01_PlanungStoerungen.sql` | Neue Tabelle für Störungen aus der Planung – **auch auf Render einmal ausführen** |
| `02_Indizes.sql` | Indizes für schnelle Zeitraum-Abfragen |

## Lokal starten
1. PostgreSQL für Windows installieren (inkl. pgAdmin), Passwort für `postgres` merken.
2. Datenbank einrichten (fragt nach dem Passwort):
   `powershell -ExecutionPolicy Bypass -File .\Datenbank\Postgres_lokal_einrichten.ps1`
   Mit echter Sicherung: zusätzlich `-Backup C:\Pfad\Maschinen.backup`
3. In `Maschin/appsettings.Development.json` bei `Password=` dein Passwort eintragen.
4. Starten: `dotnet run --project .\Maschin\MaschinenDataein.csproj --launch-profile https`

Gegen die Render-Datenbank arbeiten: statt Schritt 2–3 den externen Connection String aus dem
Render-Dashboard per User Secrets setzen (Rechtsklick Projekt → „Geheime Benutzerschlüssel verwalten“):
```json
{ "ConnectionStrings": { "DefaultConnection": "postgres://benutzer:passwort@host.frankfurt-postgres.render.com/datenbank" } }
```

## Oberfläche (Update 2)
- **Seitennavigation wie bei Google:** ‹ Zurück · 1 … 4 5 **6** 7 8 … 42 · Weiter ›, dazu
  „Einträge 51–100 von 1.234“ und ab 10 Seiten ein Feld „Seite [ ] Los“. Blättern auch mit ← / →.
- Alarm-Seite zeigt jetzt ebenfalls die Seitennavigation (vorher nur die ersten 50 Einträge).
- Navigationsleiste mit Symbolen, aktuelle Seite hervorgehoben, auf dem Handy einklappbar.
- Filter als Karte, Tabellen im einheitlichen Kartenlook, Zahlen rechtsbündig mit Tausenderpunkt.
- Klare Hinweise: „Zeitraum wählen und filtern“ bzw. „Keine Daten im gewählten Zeitraum“.
- Ladebalken beim Seitenwechsel, „Nach oben“-Knopf, Druckansicht ohne Navigation und Filter.
- Aktive Alarme als farbige Markierungen; Startseite ohne Reflection.

## Render wach halten (Update 3)
- `/health`: antwortet sofort mit „OK“ (ohne Datenbank). `/health?db=1` prüft zusätzlich die Datenbank.
- `WachHalteDienst`: Die App pingt sich auf Render alle 10 Minuten selbst (über `RENDER_EXTERNAL_URL`).
- GitHub-Workflow pingt `https://maschinen.onrender.com/health` alle 5 Minuten von außen
  (vorher wurde dort eine andere App, `market-pipeline`, angepingt). Weitere Adressen über die
  Repository-Variable `PING_URLS`.
- Dockerfile mit ReadyToRun: schnellerer Start nach Neustarts und Deploys.
- Hinweis: Render gibt 750 freie Stunden pro Monat und Workspace. Ein dauerhaft wacher Dienst
  braucht ~744 – zwei dauerhaft wache kostenlose Dienste im selben Workspace gehen sich nicht aus.

## Smartphone-Ansicht (Update 4)
- Unter 768 px werden alle Tabellen automatisch zu Karten („Bezeichnung … Wert“); die
  Bezeichnungen setzt `site.js` aus dem Tabellenkopf – auch für später hinzugefügte Zeilen.
  Eine Tabelle mit Klasse `tabelle-quer` bleibt eine normale Tabelle.
- Planung: Eingabezeilen als zweispaltige Karten, Artikel und Störungsgrund über volle Breite.
- Seitennavigation passt auf 360-px-Bildschirme in eine Zeile, größere Tippflächen.
- Filter-Knöpfe nebeneinander in voller Breite, Eingabefelder mit 16 px (kein Auto-Zoom auf dem iPhone).
- Diagramm-Seiten mit kleineren Abständen, Kennzahlen zweispaltig, Leistungs-Raster bricht um.
- Häufige Meldungen: Überschrift war fast weiß auf hellem Hintergrund.
- PC: breite Tabellen verbreitern die Seite nicht mehr.
