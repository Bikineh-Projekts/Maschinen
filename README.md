# Maschinendaten – Digitalisierung von Produktionsprozessen

**🌐 [Deutsch](#deutsch) | [English](#english)**

---

<a name="deutsch"></a>
# 🇩🇪 Deutsch

> Masterarbeit im Rahmen der Zusammenarbeit mit der **Rostocker Wurst- und Schinkenspezialitäten GmbH**

## Über das Projekt

Dieses Repository enthält die Datenbankgrundlage eines Systems zur **Digitalisierung und Optimierung von Produktionsprozessen** mittels Maschinendatenerfassung. Produktionsmaschinen werden über **Softing OPC-Programme** angebunden und liefern standardisiert über das **OPC UA/DA-Protokoll** Prozess-, Zustands-, Alarm- und Leistungsdaten, die in einer zentralen SQL-Datenbank gespeichert werden. Auf dieser Datenbasis sollen anschließend effiziente Verfahren zur Datenanalyse und -visualisierung umgesetzt werden.

## Ziele

- Ablösung manueller/papierbasierter Erfassung durch automatisierte Maschinendatenerfassung
- Einheitliche, standardisierte Datenübertragung über OPC UA/DA
- Persistente, strukturierte Speicherung aller relevanten Maschinen-, Zustands-, Alarm- und Leistungsdaten
- Grundlage für Datenanalyse und -visualisierung (z. B. OEE-Kennzahlen, Stillstandsanalysen, Temperaturüberwachung)

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

## Datenfluss

```
Produktionsmaschine
      │  (OPC UA/DA)
      ▼
Softing OPC-Programm
      │
      ▼
.NET-Anbindung (ASP.NET Core)
      │
      ▼
SQL-Server-Datenbank  ──►  Python-Analyse & Visualisierung
```

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

## Status

Die Kernkomponenten – Datenbankschema, OPC-Datenerfassung über die .NET-Anwendung sowie die Weboberfläche zur Live-Auswertung – sind bereits produktiv im Einsatz (siehe [Live-Anwendung](#live-anwendung--demo)). Im Rahmen der laufenden Masterarbeit folgen weiterführende Auswertungen und Visualisierungen (u. a. Python-basierte Analysen, OEE-Kennzahlen).

## Autor

Masterarbeit in Kooperation mit der Rostocker Wurst- und Schinkenspezialitäten GmbH.

## Lizenz

Noch nicht festgelegt.

---

<a name="english"></a>
# 🇬🇧 English

**🌐 [Deutsch](#deutsch) | [English](#english)**

> Master's thesis in cooperation with **Rostocker Wurst- und Schinkenspezialitäten GmbH**

## About the project

This repository contains the database foundation of a system for the **digitalization and optimization of production processes** through machine data acquisition. Production machines are connected via **Softing OPC programs** and deliver process, state, alarm, and performance data in a standardized way via the **OPC UA/DA protocol**, which is stored in a central SQL database. This data forms the basis for subsequent efficient data analysis and visualization.

## Goals

- Replacing manual/paper-based logging with automated machine data acquisition
- Uniform, standardized data transfer via OPC UA/DA
- Persistent, structured storage of all relevant machine, state, alarm, and performance data
- Foundation for data analysis and visualization (e.g. OEE metrics, downtime analysis, temperature monitoring)

## Tech stack

| Area | Technology |
|---|---|
| Backend | .NET (C#, ASP.NET Core) |
| Data acquisition | Softing OPC UA/DA |
| Data analysis | Python |
| Data storage | SQL Server (database project, SSDT) |

## Project structure

```
Maschinendaten.slnx              # Visual Studio solution
└── Maschinendaten/               # SQL Server database project (.sqlproj)
    ├── maschinen.sql             # Machine master data
    ├── Abzugsdaten.sql           # Draw/packaging data
    ├── Leistungsdaten.sql        # Cycle and performance metrics
    ├── Temperaturdaten.sql       # Target/actual temperatures
    ├── Programme.sql             # Program parameters (pr00–pr09)
    ├── Alarmdaten.sql            # Alarm states (bit fields AM1–AM94)
    ├── Alarmmeldungen.sql        # Plain-text alarm messages
    ├── Stoerungsdaten.sql        # Fault events per machine
    ├── Stoerungsmeldung.sql      # Plain-text fault messages
    ├── Zustandsdaten.sql         # State changes per machine
    └── Zustandsmeldung.sql       # Plain-text state messages
```

The database project is a **SQL Server Database Project (SSDT)** and can be opened directly in Visual Studio via `Maschinendaten.slnx` and published to a SQL Server instance.

## Data model

The central reference point of all tables is `maschinen`, which every measurement table references via `MaschinenId`. Plain-text messages (alarms, faults, states) are stored in their own lookup tables.

```mermaid
erDiagram
    maschinen ||--o{ Abzugsdaten : records
    maschinen ||--o{ Leistungsdaten : records
    maschinen ||--o{ Temperaturdaten : records
    maschinen ||--o{ Programme : records
    maschinen ||--o{ Alarmdaten : records
    maschinen ||--o{ Stoerungsdaten : records
    maschinen ||--o{ Zustandsdaten : records
    Stoerungsmeldung ||--o{ Stoerungsdaten : references
    Zustandsmeldung ||--o{ Zustandsdaten : references

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

### Table overview

| Table | Purpose |
|---|---|
| `maschinen` | Master data per machine (name, IP address for OPC connection) |
| `Abzugsdaten` | Draw length and packages per draw, per machine/program |
| `Leistungsdaten` | Cycle counters, package counters, and machine cycles for performance tracking |
| `Temperaturdaten` | Target/actual temperatures for two measuring points |
| `Programme` | Active program parameters (`pr00`–`pr09`) per program number |
| `Alarmdaten` | Snapshot of up to 94 alarm indicators (`AM1`–`AM94`) as bit fields |
| `Alarmmeldungen` | Plain-text translation of alarm indicator codes |
| `Stoerungsdaten` | Timestamp and reference to a fault that occurred, per machine |
| `Stoerungsmeldung` | Plain-text fault messages |
| `Zustandsdaten` | Timestamp and reference to a machine state |
| `Zustandsmeldung` | Plain-text state messages |

## Data flow

```
Production machine
      │  (OPC UA/DA)
      ▼
Softing OPC program
      │
      ▼
.NET connector (ASP.NET Core)
      │
      ▼
SQL Server database  ──►  Python analysis & visualization
```

## Requirements

- SQL Server (local or a network instance)
- Visual Studio with SQL Server Data Tools (SSDT) to open `Maschinendaten.slnx`
- .NET SDK for the connector component
- Python environment for analysis/visualization

## Installation & setup

1. Clone the repository
2. Open `Maschinendaten.slnx` in Visual Studio
3. Publish the database project to the target SQL Server instance (right-click → *Publish*)
4. Configure the OPC server connection details and the database connection string in the .NET application
5. Start the .NET application to activate data acquisition

## Live application / demo

The web interface for the live evaluation of machine data is available at:

🔗 **[https://maschinen.onrender.com](https://maschinen.onrender.com/)**

The application (ASP.NET Core) reads the machine data stored in the SQL database and displays it continuously updated across several views. The home page ("Current overview") shows the latest records for each table live.

### Available views

| View | Route | Content |
|---|---|---|
| Current overview | `/` | Summary of all data areas on one page |
| Performance | `/LeistungsDaten` | Cycle counters, packages, and machine cycles per machine/program |
| Draw | `/Abzugsdaten` | Draw length and packages per draw, per machine/program |
| Temperature | `/TemperaturDaten` | Target/actual temperatures of lower and upper film |
| Alarm | `/AlarmDaten` | Active alarm indicators per machine |
| State | `/ZustandsDaten` | Current and historical machine states |
| Fault | `/StoerungsDaten` | Faults that occurred per machine |
| Frequent messages | `/Haeufigkeit` | Analysis of the most frequently occurring alarm/fault messages |
| Planning | `/Planung` | Production planning/overview view |

### Sample excerpt (live data)

**Performance data**

| Timestamp | Machine | Program | Daily cnt. | Cycles | Packages | Cycle bar |
|---|---|---|---|---|---|---|
| 19.02.2025 16:27:38 | Maschine-01 | 1KG VAC | 83 | 2,650,759 | 332 | 100% |

**Draw data**

| Timestamp | Machine | Program name | Packages per draw | Draw length |
|---|---|---|---|---|
| 19.02.2025 15:50:11 | Maschine-01 | 1KG VAC | 9 pcs | 400 mm |
| 19.02.2025 15:50:11 | Maschine-02 | SCHRAEGSCH | 9 pcs | 420 mm |

**Temperature data**

Shows target/actual temperatures for lower film (`Isstemp1`) and upper film (`Isstemp2`) per machine and program, including timestamp.

The page also offers a **Reload** function to manually refresh the live view.

> These views directly reflect the content of the database tables described above (`Leistungsdaten`, `Abzugsdaten`, `Temperaturdaten`, `Alarmdaten`, `Zustandsdaten`, `Stoerungsdaten`), confirming that the full pipeline machine → OPC → .NET → SQL Server → web interface is already in productive use.

## Status

The core components — the database schema, OPC data acquisition via the .NET application, and the web interface for live evaluation — are already in productive use (see [Live application](#live-application--demo)). Further analyses and visualizations (including Python-based analytics and OEE metrics) are planned as part of the ongoing master's thesis.

## Author

Master's thesis in cooperation with Rostocker Wurst- und Schinkenspezialitäten GmbH.

## License

Not yet defined.
