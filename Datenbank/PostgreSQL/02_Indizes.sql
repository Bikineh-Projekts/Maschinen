-- Optionale Indizes: beschleunigen die Abfragen "Maschine X im Zeitraum Y" deutlich.
-- Ändern keine Daten; mehrfaches Ausführen schadet nicht.

CREATE INDEX IF NOT EXISTS "IX_Leistungsdaten_Maschine_Zeit"  ON "Leistungsdaten"  ("MaschinenId", "Timestamp");
CREATE INDEX IF NOT EXISTS "IX_Abzugsdaten_Maschine_Zeit"     ON "Abzugsdaten"     ("MaschinenId", "Timestamp");
CREATE INDEX IF NOT EXISTS "IX_Temperaturdaten_Maschine_Zeit" ON "Temperaturdaten" ("MaschinenId", "Timestamp");
CREATE INDEX IF NOT EXISTS "IX_Alarmdaten_Maschine_Zeit"      ON "Alarmdaten"      ("MaschinenId", "Timestamp");
CREATE INDEX IF NOT EXISTS "IX_Zustandsdaten_Maschine_Zeit"   ON "Zustandsdaten"   ("MaschinenId", "Timestamp");
CREATE INDEX IF NOT EXISTS "IX_Stoerungsdaten_Maschine_Zeit"  ON "Stoerungsdaten"  ("MaschinenId", "Timestamp");
CREATE INDEX IF NOT EXISTS "IX_Planungs_Datum_Maschine"       ON "Planungs"        ("Datum", "MaschinenId");
