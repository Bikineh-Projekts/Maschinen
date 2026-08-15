# Maschinendaten – Digitalisierung von Produktionsprozessen

🌐 **Sprache:** Deutsch | [English](README.en.md)

> Masterarbeit *„Analysetechniken zur Steigerung der Betriebseffizienz von Verpackungsmaschinen“* im Rahmen der Zusammenarbeit mit der **Rostocker Wurst- und Schinkenspezialitäten GmbH**
> Universität Rostock, Fakultät für Informatik und Elektrotechnik (IEF), Studiengang Master ITTI

## Über das Projekt

Dieses Repository enthält die Datenbankgrundlage eines Systems zur **Digitalisierung und Optimierung von Produktionsprozessen** mittels Maschinendatenerfassung im Sinne der **Industrie 4.0**. Im Werk Rostock kommen **VARIOVAC**-Verpackungsmaschinen (u. a. *Primus* und *Multipower*) mit **Siemens-S7-Steuerungen** zum Einsatz. Die Maschinen liefern Prozess-, Zustands-, Alarm- und Leistungsdaten über das **OPC-DA-Protokoll** (COM/DCOM-basiert, RFC1006), das mithilfe der Software **Softing dataFEED OPC Suite** ausgelesen und über eine **ODBC-Schnittstelle** in eine zentrale SQL-Datenbank geschrieben wird – ein Aufbau, der konzeptionell einer klassischen **SCADA**-Anbindung entspricht. Damit legt das System die Datenbasis für typische **MES**-Funktionen (Betriebs- und Maschinendatenerfassung, Störungsverfolgung) sowie für die Ableitung von Kennzahlen wie der **OEE (Overall Equipment Effectiveness)**. Eine Migration auf das modernere, plattformunabhängige **OPC-UA**-Protokoll wird in der zugrunde liegenden Masterarbeit als sinnvoller nächster Schritt empfohlen (siehe [Empfehlungen](#empfehlungen--ausblick)).

## Ziele

- Ablösung manueller/papierbasierter Erfassung durch automatisierte Maschinendatenerfassung (Kern eines MES)
- Einheitliche, standardisierte Datenübertragung über OPC UA/DA, wie sie auch in SCADA-Systemen üblich ist
- Persistente, strukturierte Speicherung aller relevanten Maschinen-, Zustands-, Alarm- und Leistungsdaten
- Grundlage für Datenanalyse und -visualisierung, u. a. zur Berechnung der **OEE**, für Stillstandsanalysen und Temperaturüberwachung
- Beitrag zur Vernetzung der Produktion im Sinne der Industrie 4.0

## Technologiestack

| Bereich | Technologie |
|---|---|
| Backend | .NET (C#, ASP.NET Core) |
| Datenanbindung | Softing OPC UA/DA |
| Datenanalyse | Python |
| Datenhaltung | SQL Server (Datenbankprojekt, SSDT) |

## Projektstruktur

```
Maschinendaten.slnx              # Visual-Studio-Solution
└── Maschinendaten/               # SQL-Server-Datenbankprojekt (.sqlproj)
    ├── maschinen.sql             # Stammdaten der Maschinen
    ├── Abzugsdaten.sql           # Abzugs-/Verpackungsdaten
    ├── Leistungsdaten.sql        # Takt- und Leistungskennzahlen
    ├── Temperaturdaten.sql       # Soll-/Ist-Temperaturen
    ├── Programme.sql             # Programmparameter (pr00–pr09)
    ├── Alarmdaten.sql            # Alarmzustände (Bitfelder AM1–AM94)
    ├── Alarmmeldungen.sql        # Klartext-Alarmmeldungen
    ├── Stoerungsdaten.sql        # Störungsereignisse je Maschine
    ├── Stoerungsmeldung.sql      # Klartext-Störungsmeldungen
    ├── Zustandsdaten.sql         # Zustandswechsel je Maschine
    └── Zustandsmeldung.sql       # Klartext-Zustandsmeldungen
```

Das Datenbankprojekt liegt als **SQL Server Database Project (SSDT)** vor und kann direkt in Visual Studio über `Maschinendaten.slnx` geöffnet und gegen eine SQL-Server-Instanz veröffentlicht werden.

## Datenmodell

Zentraler Bezugspunkt aller Tabellen ist `maschinen`, auf die alle Messwert-Tabellen per `MaschinenId` referenzieren. Klartext-Meldungen (Alarme, Störungen, Zustände) sind in eigenen Nachschlagetabellen ausgelagert.

```mermaid
erDiagram
    maschinen ||--o{ Abzugsdaten : erfasst
    maschinen ||--o{ Leistungsdaten : erfasst
    maschinen ||--o{ Temperaturdaten : erfasst
    maschinen ||--o{ Programme : erfasst
    maschinen ||--o{ Alarmdaten : erfasst
    maschinen ||--o{ Stoerungsdaten : erfasst
    maschinen ||--o{ Zustandsdaten : erfasst
    Stoerungsmeldung ||--o{ Stoerungsdaten : referenziert
    Zustandsmeldung ||--o{ Zustandsdaten : referenziert

    maschinen {
        BIGINT Id PK
        NVARCHAR Bezeichnung
        NVARCHAR IpAdresse
    }
    Abzugsdaten {
        BIGINT Id PK
        DATETIME2 Tiemstamp
        BIGINT MaschinenId FK
        INT PRnummer
        BIGINT PackungenproAbzug
        BIGINT Abzuglaenge
    }
    Leistungsdaten {
        BIGINT Id PK
        DATETIME2 Timestamp
        BIGINT MaschinenId FK
        INT PRnummer
        INT Tagestaktzaehler
        INT Packungszaeler
        INT Maschinentakte
    }
    Temperaturdaten {
        BIGINT Id PK
        DATETIME2 Timestamp
        BIGINT MaschinenId FK
        INT PRnummer
        INT Solltemp1
        INT Isstemp1
        INT Solltemp2
        INT Isstemp2
    }
    Programme {
        BIGINT Id PK
        DATETIME2 Timestamp
        BIGINT MaschinenId FK
        INT PRnummer
        INT pr00
    }
    Alarmdaten {
        BIGINT Id PK
        DATETIME2 Timestamp
        BIGINT MaschinenId FK
        BIT AM1
    }
    Stoerungsdaten {
        BIGINT Id PK
        DATETIME2 Timestamp
        BIGINT MaschinenId FK
        BIGINT StoerungsmeldungId FK
    }
    Stoerungsmeldung {
        BIGINT Id PK
        NVARCHAR Meldung
    }
    Zustandsdaten {
        BIGINT Id PK
        DATETIME2 Timestamp
        BIGINT MaschinenId FK
        BIGINT ZustandsmeldungId FK
    }
    Zustandsmeldung {
        BIGINT Id PK
        NVARCHAR Meldung
    }
```

### Tabellenübersicht

| Tabelle | Zweck |
|---|---|
| `maschinen` | Stammdaten je Maschine (Bezeichnung, IP-Adresse für OPC-Anbindung) |
| `Abzugsdaten` | Abzugslänge und Packungsanzahl pro Abzug je Maschine/Programm |
| `Leistungsdaten` | Taktzähler, Packungszähler und Maschinentakte zur Leistungsermittlung |
| `Temperaturdaten` | Soll-/Ist-Temperaturen zweier Messstellen |
| `Programme` | Aktive Programmparameter (`pr00`–`pr09`) je Programmnummer |
| `Alarmdaten` | Momentaufnahme von bis zu 94 Alarmmeldern (`AM1`–`AM94`) als Bitfelder |
| `Alarmmeldungen` | Klartext-Übersetzung der Alarmmelder-Codes |
| `Stoerungsdaten` | Zeitpunkt und Referenz auf eine aufgetretene Störung je Maschine |
| `Stoerungsmeldung` | Klartext-Störungsmeldungen |
| `Zustandsdaten` | Zeitpunkt und Referenz auf einen Maschinenzustand |
| `Zustandsmeldung` | Klartext-Zustandsmeldungen |

## Herkunft der Maschinendaten

Die erfassten Werte stammen unmittelbar aus dem Speicher der Siemens-S7-Steuerung der VARIOVAC-Maschinen und werden als OPC-Tags über feste Datenbausteinadressen (DB-Adressen) angesprochen, z. B.:

| OPC-Tag | SPS-Adresse | Einheit | Beschreibung |
|---|---|---|---|
| `OPC_Daten/T_SOLL2` | `DB85.DBW30:INT` | °C (1/10) | Solltemperatur Regelkreis 2, Formwerkzeug Oberteil |
| `OPC_Daten/Isttemp_FWZ_Ob1` | `DB85.DBW494:INT` | °C (1/10) | Isttemperatur obere Heizeinheit Formwerkzeug |
| `OPC_Daten/T_SOLL4` | `DB85.DBW34:INT` | °C (1/10) | Solltemperatur Regelkreis 4, Siegelwerkzeug |
| `OPC_Daten/Isttemp_SWZ` | `DB85.DBW490:INT` | °C (1/10) | Isttemperatur Siegelwerkzeug |

**Skalierung:** Temperatur- und Längenwerte werden in der SPS als Ganzzahlen (`INT`) im Format 1/10 gespeichert – der Rohwert `432` entspricht z. B. `43,2 °C`. Binäre Alarm- und Zustandsmelder werden als `BOOL` (0/1) übertragen. Diese Skalierung wird beim Einlesen in die Datenbank berücksichtigt.

Zustands- und Störungsmeldungen (`Zustandsmeldung`, `Stoerungsmeldung`) sind Klartext-Übersetzungen numerischer Codes, die die Steuerung an definierten Byte-Adressen bereitstellt (z. B. `DB85.DBB530:BYTE` für Zustandsmeldungen, `DB85.DBB453:BYTE` für Störungsmeldungen), und wurden anhand der Herstellerdokumentation der VARIOVAC-Maschinen katalogisiert.

## Datenfluss

```
VARIOVAC-Verpackungsmaschine (Siemens S7)
      │  OPC-DA-Protokoll (COM/DCOM, RFC1006)
      ▼
Softing dataFEED OPC Suite  (OPC-DA-Server)
      │  ODBC-Treiber (32-Bit)
      ▼
SQL-Server-Datenbank
      │  Entity Framework Core
      ▼
ASP.NET Core MVC – Webanwendung
      │  Chart.js / Highcharts
      ▼
Browserbasierte Live-Übersicht & Analyse
```

Die Übertragung von der Maschine in die Datenbank erfolgt durch Konfiguration eines Datenziels vom Typ „SQL-Datenbank“ in der Softing dataFEED OPC Suite: Jedem OPC-Tag wird dort ein Datenbankfeld zugeordnet und die Übertragung per `INSERT INTO`-Anweisung zeitgesteuert ausgelöst.

## Voraussetzungen

- SQL Server (lokal oder Instanz im Netzwerk)
- Visual Studio mit SQL Server Data Tools (SSDT) zum Öffnen von `Maschinendaten.slnx`
- .NET SDK für die Anbindungskomponente
- Python-Umgebung für Analyse/Visualisierung

## Installation & Setup

1. Repository klonen
2. `Maschinendaten.slnx` in Visual Studio öffnen
3. Datenbankprojekt gegen die Ziel-SQL-Server-Instanz veröffentlichen (Rechtsklick → *Publish*)
4. Verbindungsdaten der OPC-Server sowie die Datenbank-Connection-String in der .NET-Anwendung hinterlegen
5. .NET-Anwendung starten, um die Datenerfassung zu aktivieren

## Live-Anwendung / Demo

Die Weboberfläche zur Live-Auswertung der Maschinendaten ist unter folgender Adresse erreichbar:

🔗 **[https://maschinen.onrender.com](https://maschinen.onrender.com/)**

Die Anwendung (ASP.NET Core) liest die in der SQL-Datenbank gespeicherten Maschinendaten aus und stellt sie fortlaufend aktualisiert in mehreren Übersichten dar. Auf der Startseite ("Aktuelle Übersicht") werden die jeweils letzten Datensätze je Tabelle live angezeigt.

### Verfügbare Ansichten

| Ansicht | Route | Inhalt |
|---|---|---|
| Aktuelle Übersicht | `/` | Zusammenfassung aller Datenbereiche auf einer Seite |
| Leistung | `/LeistungsDaten` | Taktzähler, Packungen und Maschinentakte je Maschine/Programm |
| Abzug | `/Abzugsdaten` | Abzuglänge und Packungen pro Abzug je Maschine/Programm |
| Temperatur | `/TemperaturDaten` | Soll-/Ist-Temperaturen von Unter- und Oberfolie |
| Alarm | `/AlarmDaten` | Aktive Alarmmelder je Maschine |
| Zustand | `/ZustandsDaten` | Aktuelle und historische Maschinenzustände |
| Störung | `/StoerungsDaten` | Aufgetretene Störungen je Maschine |
| Häufige Meldungen | `/Haeufigkeit` | Auswertung der am häufigsten auftretenden Alarm-/Störungsmeldungen |
| Planung | `/Planung` | Produktionsplanungs-/-übersichtsansicht |

### Beispielhafter Ausschnitt (Live-Daten)

**Leistungsdaten**

| Zeitstempel | Maschine | Programm | Tagesz. | Takte | Packungen | Takte-Balken |
|---|---|---|---|---|---|---|
| 19.02.2025 16:27:38 | Maschine-01 | 1KG VAC | 83 | 2.650.759 | 332 | 100 % |

**Abzugsdaten**

| Zeitstempel | Maschine | PR-Name | Packungen pro Abzug | Abzuglänge |
|---|---|---|---|---|
| 19.02.2025 15:50:11 | Maschine-01 | 1KG VAC | 9 Stück | 400 mm |
| 19.02.2025 15:50:11 | Maschine-02 | SCHRAEGSCH | 9 Stück | 420 mm |

**Temperaturdaten**

Anzeige der Soll-/Ist-Temperaturen für Unterfolie (`Isstemp1`) und Oberfolie (`Isstemp2`) je Maschine und Programm, inkl. Zeitstempel.

Die Seite bietet zudem eine **Reload**-Funktion zum manuellen Aktualisieren der Live-Ansicht.

> Die Ansichten spiegeln direkt den Inhalt der oben beschriebenen Datenbanktabellen (`Leistungsdaten`, `Abzugsdaten`, `Temperaturdaten`, `Alarmdaten`, `Zustandsdaten`, `Stoerungsdaten`) wider und bestätigen, dass die Pipeline Maschine → OPC → .NET → SQL-Server → Weboberfläche bereits produktiv im Einsatz ist.

## Softwarearchitektur

Die ASP.NET-Core-MVC-Anwendung folgt konsequent dem MVC-Muster mit klarer Trennung von Controllern, Datenmodellen und ViewModels:

- **Controller** (je Datenbereich, z. B. `AbzugsDatenController`, `AlarmDatenController`, `LeistungsDatenController`, `StoerungsDatenController`, `ZustandsDatenController`, `ProgrammenController`) lesen Filtereinstellungen aus Cookies, filtern Daten nach Zeitraum und Maschinen-ID und übergeben sie an die View-Schicht. Ein `TableSelectionController` übernimmt das Routing zwischen den Tabellenansichten.
- **`MaschinenDbContext`** (Entity Framework Core) definiert alle Tabellen als `DbSet<T>` und legt die Primärschlüssel in `OnModelCreating` fest.
- **ViewModel-Schicht** (u. a. `AbzugsdatenModelView`, `DashboardModelView`, `LeistungsdatenModelView`, `TemperaturdatenModelView`, `HaeufigkeitModelView`) bereitet die Rohdaten für die Anzeige auf, kombiniert z. B. Programmname und Messwerte und speist das Dashboard.
- **`PaginatedList<T>`** implementiert generische serverseitige Seitennummerierung für alle Datentabellen der Anwendung.
- Die Klasse `Programmen` wandelt die numerischen Felder `pr00`–`pr09` automatisch per ASCII-Konvertierung in einen lesbaren Programmnamen (z. B. „1KG VAC“) um.

## Analysemethoden zur Fehlererkennung

Auf Basis der erfassten Temperaturdaten wurden im Rahmen der Masterarbeit mehrere Analyseverfahren zur Bewertung der Betriebsstabilität und Fehlererkennung untersucht:

- **Stückweise Regression / stückweise polynomiale Regression**: Segmentierung des Temperaturverlaufs in charakteristische Prozessphasen (Aufheizen, Halten, Abkühlen) mit jeweils separatem quadratischem Regressionsmodell je Segment.
- **Polynomiale Regression 10. Grades**: globale Modellierung des Temperaturverlaufs als Vergleichsbasis zur stückweisen Regression.
- **Fehlerwahrscheinlichkeitsmodell**: Schätzung der Fehlerwahrscheinlichkeit pro Tag auf Basis mehrerer unabhängiger Einflussfaktoren – Temperaturabweichung, Programmwechsel, Produktsensitivität, Abzugsgeschwindigkeit sowie die besonders fehleranfällige Anlaufphase der Schicht (erste 15 Minuten).
- **Fehlerflussdiagramm**: strukturierter Entscheidungsbaum zur Unterscheidung von Anwender- und Gerätefehlern bei Temperaturabweichungen sowie zur Verifikation der Fehlerbehebung.
- **Explorative Diagramme**: Liniendiagramm, Histogramm, Boxplot und Balkendiagramm der Temperaturabweichungen je Sensor zur Identifikation von Ausreißern, Streuung und wiederkehrenden Mustern.

## Empfehlungen & Ausblick

Aus der Masterarbeit ergeben sich folgende Empfehlungen für die Weiterentwicklung des Systems:

1. **Schulung der Anwender** zur korrekten Zuordnung von Werkzeug, Folie und Programmnummer, um Bedienfehler zu vermeiden.
2. **Sperrung von Änderungen während laufender Produktion** (Programmwechsel, Solltemperatur-Anpassung), um ungewollte Temperaturabweichungen zu verhindern.
3. **Anzeige der Temperaturabweichung im Bedienpanel** inkl. programmspezifisch dokumentierter Toleranzbereiche.
4. **Migration von OPC DA zu OPC UA**, um Plattformunabhängigkeit, höhere Sicherheit (Verschlüsselung, Zertifikate) und eine einfachere Cloud-Anbindung (z. B. Siemens MindSphere, AWS IoT, Azure IoT Hub) zu erreichen.

## Zugehörige Masterarbeit

Dieses Repository bildet die technische Grundlage der folgenden Masterarbeit:

> Bikineh, M. (2025). *Analysetechniken zur Steigerung der Betriebseffizienz von Verpackungsmaschinen.* Masterarbeit, Universität Rostock, Fakultät für Informatik und Elektrotechnik (IEF), Studiengang Master ITTI.
> 1. Gutachter: Dr. Holger Meyer · 2. Gutachter: M.Sc. Daniel Tempelmann · Abgabedatum: 22.07.2025

## Status

Die Kernkomponenten – Datenbankschema, OPC-DA-Datenerfassung über die Softing dataFEED OPC Suite sowie die ASP.NET-Core-MVC-Webanwendung zur Live-Auswertung – sind bereits produktiv im Einsatz (siehe [Live-Anwendung](#live-anwendung--demo)). Die im Rahmen der Masterarbeit entwickelten Analysemethoden (Regressionsmodelle, Fehlerwahrscheinlichkeitsmodell) liegen als Auswertungen vor; eine Integration als feste Bestandteile der Weboberfläche sowie eine Migration auf OPC UA sind als nächste Schritte vorgesehen.

## Autor

Masterarbeit von Mohammadhossein Bikineh (Master ITTI, Universität Rostock) in Kooperation mit der Rostocker Wurst- und Schinkenspezialitäten GmbH.

## Lizenz

Noch nicht festgelegt.
