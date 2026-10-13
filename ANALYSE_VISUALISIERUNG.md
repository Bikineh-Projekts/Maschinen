# Dashboard- und Analyseverbesserungen – MaschinenDataein

## Grundsatz
Grafana dient hier ausschließlich als gestalterische Vorlage. Grafana wird nicht als zusätzliche Laufzeit oder Import-Dashboard in das Projekt eingebaut. Alle Ansichten bleiben in der vorhandenen ASP.NET-Core-MVC-Anwendung mit Chart.js und der bestehenden PostgreSQL-Datenbank.

## Begrenzung auf zwei Maschinen
Die Startseite, Maschinenfilter und Analyseansichten verwenden höchstens die zwei Maschinen mit den aktuellsten Mess-/Ereignisdaten aus den Tabellen `Leistungsdaten`, `Temperaturdaten`, `Abzugsdaten`, `Alarmdaten`, `Zustandsdaten` und `Stoerungsdaten`. Leere Demo-Stammdatensätze ohne Telemetrie werden nicht als zusätzliche Maschinen angezeigt. Gespeicherte Filter auf eine nicht mehr ausgewählte Maschine werden auf „Beide Maschinen“ zurückgesetzt.

## Gestalterische Muster nach Grafana-Art
- **Stat-/Kennzahlenkarten:** aktuelle Produktion, Zählerdeltas, Soll/Ist-Temperaturen und Datenaktualität.
- **Zeitreihen:** Temperaturkanal 1/2 sowie Produktionsverlauf pro Maschine. Wenn beide Maschinen gewählt sind, werden die Linien nicht zu einer falschen gemeinsamen Maschine vermischt.
- **Statusfarben und Schwellen:** Rot/Gelb/Grün werden für Alarm-/Warn-/Gut-Zustände verwendet; Farben sind nicht die einzige Unterscheidung, es gibt zusätzlich Texte und Symbole.
- **Balkendiagramme:** Top-Alarme und -Störungen, Zustandsdauer sowie Pakete pro Tag und Programm.
- **Ereignisverlauf:** zeitlich sortierte Alarm-, Störungs- und Zustandsmeldungen.
- **Smartphone-Layout:** Kennzahlen in einem kompakten Raster, Diagramme untereinander, Tabellen in horizontal scrollbaren Bereichen und berührungsfreundliche Umschalter.

## Zählerauswertung
`Packungszaeler` und `Maschinentakte` werden als SPS-Zählerstände behandelt, nicht als unabhängige Produktionsmengen pro Datenbankzeile. Die Analyse berechnet deshalb Differenzen zwischen aufeinanderfolgenden Werten je Maschine. Sinkt der Zählerstand, wird ein Reset angenommen und der neue Stand als Menge seit Reset gewertet. Für den ersten Wert ohne Vorgänger wird der aktuelle Stand als Näherung verwendet.

Bei sehr großen Zeiträumen lädt das Leistungsdiagramm maximal 2.000 jüngste Datensätze; die sichtbare Zeitreihe wird zusätzlich auf ungefähr 500 Punkte reduziert. Aggregationen basieren auf den geladenen Datensätzen. Der ausgewählte Zeitraum wird nicht stillschweigend durch einen anderen Zeitraum ersetzt, wenn keine Daten gefunden wurden.

## Grenzen der Kennzahlen
- „Relativer Paketausstoß“ vergleicht die Paketzahl je Maschine/Programm mit der größten Paketzahl im gewählten Zeitraum. Das ist kein OEE- oder Wirkungsgradwert. Dafür fehlen im aktuellen Schema insbesondere Laufzeit, geplante Taktzeit und definierte Produktions-/Schichtgrenzen.
- Alarmtabellen enthalten Momentaufnahmen der Bits `AM1`–`AM94`. Ein aktiver Alarm in einem Snapshot ist nicht automatisch ein neuer, eindeutiger Alarmstart.
- Zustandsdauer wird aus Zustandswechseln geschätzt; lange Datenlücken werden nicht vollständig als Betriebs- oder Stillstandszeit gewertet.
- Die vorhandene Temperaturkonvention bleibt erhalten: Maschinen-ID `4` wird als Ganzgradwert behandelt, andere IDs als Zehntelgrad. Das muss anhand der realen SPS-/OPC-Rohwerte bestätigt werden. Rohwerte werden nicht in der Datenbank verändert.
- `Timestamp` ist als Datum/Uhrzeit modelliert und wird nicht pauschal durch 10 oder 1.000 geteilt.

## Testhinweis
In dieser Umgebung ist kein .NET-SDK installiert. Daher wurde kein `dotnet build` durchgeführt. Vor dem Deployment sollte das Projekt lokal mit .NET 8 gebaut und mit der tatsächlichen PostgreSQL-Datenbank geprüft werden.
