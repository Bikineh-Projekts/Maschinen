# Richtet die lokale PostgreSQL-Datenbank "Maschinendaten" ein.
# Ausführen im Solution-Ordner:
#   powershell -ExecutionPolicy Bypass -File .\Datenbank\Postgres_lokal_einrichten.ps1
#
# Optional mit echter Sicherung (pg_dump, Format "Custom"):
#   powershell -ExecutionPolicy Bypass -File .\Datenbank\Postgres_lokal_einrichten.ps1 -Backup C:\Pfad\Maschinen.backup

param(
    [string]$Backup = "",
    [string]$Benutzer = "postgres",
    [string]$Datenbank = "Maschinendaten"
)

# psql / pg_restore suchen (Standard-Installationsort von PostgreSQL unter Windows)
$bin = Get-ChildItem "C:\Program Files\PostgreSQL\*\bin" -Directory -ErrorAction SilentlyContinue |
       Sort-Object FullName -Descending | Select-Object -First 1
if (-not $bin) { Write-Error "PostgreSQL nicht gefunden unter C:\Program Files\PostgreSQL"; exit 1 }
$psql      = Join-Path $bin.FullName "psql.exe"
$pgRestore = Join-Path $bin.FullName "pg_restore.exe"
Write-Host "PostgreSQL gefunden: $($bin.FullName)"

$env:PGPASSWORD = Read-Host "Passwort für Benutzer '$Benutzer'"
$env:PGOPTIONS  = "-c client_min_messages=warning"   # keine "already exists"-Hinweise

# Datenbank anlegen, falls sie fehlt
$vorhanden = & $psql -U $Benutzer -h localhost -tAc "SELECT 1 FROM pg_database WHERE datname = '$Datenbank'"
if ($vorhanden -ne "1") {
    & $psql -U $Benutzer -h localhost -c "CREATE DATABASE `"$Datenbank`""
}

if ($Backup) {
    # Echte Daten aus einer pg_dump-Sicherung einspielen
    & $pgRestore -U $Benutzer -h localhost -d $Datenbank --no-owner --no-privileges $Backup
}

# Schema ergänzen (legt nur fehlende Tabellen an), dann neue Tabelle und Indizes
Get-ChildItem "$PSScriptRoot\PostgreSQL\*.sql" | Sort-Object Name | ForEach-Object {
    Write-Host $_.Name
    & $psql -U $Benutzer -h localhost -d $Datenbank -v ON_ERROR_STOP=1 -q -f $_.FullName
}

& $psql -U $Benutzer -h localhost -d $Datenbank -c "\dt"
Remove-Item Env:PGPASSWORD, Env:PGOPTIONS
