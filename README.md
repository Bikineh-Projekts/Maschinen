

# **MaschinenDataein – Digitale Maschinendatenerfassung & Produktionsplanung**

*ASP.NET Core MVC • OPC UA/DA • SQL Server • EF Core • Produktionsanalyse*

## 🚀 Überblick

**MaschinenDataein** ist ein vollständiges System zur **Erfassung, Speicherung, Analyse und Visualisierung** von Maschinendaten in der Lebensmittelproduktion.
Es wurde im Rahmen eines realen Industrieprojekts entwickelt („Die Rostocker Wurst & Schinkenspezialitäten GmbH“) und ermöglicht:

* Auslesen von Maschinendaten über **Softing OPC UA/DA**
* Speicherung in **SQL Server** über **EF Core**
* Web-Dashboard für Echtzeit-Zustände
* Planung & Erfassung von Produktionsdaten
* Visualisierung von Temperatur-, Leistungs- und Störungsdaten

---

## 🏗️ Architektur

**Backend:**

* ASP.NET Core MVC (C#)
* Entity Framework Core (SQL Server)
* Repository & Model-View-Pattern
* Razor Views + jQuery für dynamische Tabellen
* Session-Handling (JSON via Newtonsoft)

**OPC-Anbindung:**

* Softing OPC UA/DA
* Standardisierte Übertragung
* NodeIDs für Temperatur, Leistung, Alarme, Zustände

**Datenbankstruktur (Auszug):**

* `Maschine`
* `MaschinenProgrammen`
* `LeistungsDaten`
* `TemperaturDaten`
* `ZustandsDaten / ZustandsMeldung`
* `StoerungsDaten / StoerungsMeldung`
* `Planungs` (Produktionsplanung)

---

## 📊 Features

### 🔹 **Dashboard**

* Maschinenübersicht
* Letzte Meldungen & Störungen
* Temperatur & Leistung in Echtzeit
* Produktionsstatus je Maschine

### 🔹 **Produktionsplanung**

* Grunddaten + dynamische Produktionszeilen
* JSON-Mapping von Frontend → Backend
* Speichern in Einzeltabelle `Planungs`
* Validierung & Error-Handling (TempData)

### 🔹 **Auswertung**

* Filterbare Temperatur- & Leistungsdaten
* Störungsanalyse
* Programmdaten Übersicht
* Pagination & Suchfunktionen

---

## 📂 Projektstruktur

```
MaschinenDataein/
│
├── Controllers/
│   ├── DashboardController.cs
│   ├── TemperaturDatenController.cs
│   ├── LeistungsDatenController.cs
│   ├── ZustandsDatenController.cs
│   ├── PlanungController.cs
│
├── Models/
│   ├── DbContext/
│   ├── Entity-Modelle
│   ├── ModelView/
│
├── Views/
│   ├── Dashboard/
│   ├── Planung/
│   ├── Temperatur/
│   ├── Leistungs/
│
├── Helper/
│   └── SessionHelper.cs
│
└── wwwroot/
    └── JS, CSS, Bilder
```
## ⚙️ Installation

### 1️⃣ Repository klonen

```
git clone https://github.com/DEIN-USERNAME/MaschinenDataein.git
```

### 2️⃣ SQL Server Connection ändern

In **appsettings.json**:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=.;Database=MaschinenDataein;Trusted_Connection=True;"
}
```

### 3️⃣ Datenbank migrieren

```
dotnet ef database update
```

### 4️⃣ Starten

```
dotnet run
```

---

## 🧩 OPC UA/DA Integration

Das System liest Maschinendaten über **Softing OPC**:

* Temperatur
* Leistung
* Zustandscodes
* Störungsnummern
* Laufzeiten / Programme

Die Daten werden standardisiert in SQL gespeichert und im Dashboard dargestellt.



---

## 👤 Autor

**Milan Bikineh**
M.Sc. Technische Informatik – Produktion, Datenanalyse, OPC UA, .NET, SQL
📍 Rostock / Deutschland

---

## ⭐ Unterstützung

Gib dem Projekt einen **Star**, wenn du es hilfreich findest ⭐

