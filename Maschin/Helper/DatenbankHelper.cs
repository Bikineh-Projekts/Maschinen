using Microsoft.Extensions.Configuration;

namespace MaschinenDataein.Helper
{
    /// <summary>
    /// Liest den PostgreSQL-Connection-String.
    /// Akzeptiert sowohl das Npgsql-Format (Host=…;Database=…) als auch
    /// die URL-Form postgres://user:passwort@host:port/datenbank, wie Render sie liefert.
    /// </summary>
    public static class DatenbankHelper
    {
        public static string LeseConnectionString(IConfiguration config)
        {
            var cs = config.GetConnectionString("DefaultConnection");

            if (string.IsNullOrWhiteSpace(cs))
                cs = config["DATABASE_URL"];

            if (string.IsNullOrWhiteSpace(cs))
                throw new InvalidOperationException(
                    "Kein Connection String gefunden. Bitte 'ConnectionStrings:DefaultConnection' in " +
                    "appsettings.Development.json, in den User Secrets oder als Umgebungsvariable " +
                    "'ConnectionStrings__DefaultConnection' setzen.");

            cs = cs.Trim();

            return cs.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase) ||
                   cs.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase)
                ? PostgresUrlZuConnectionString(cs)
                : cs;
        }

        /// <summary>Server und Datenbank für Log-Ausgaben – ohne Passwort.</summary>
        public static string Beschreibung(string connectionString)
        {
            var teile = connectionString.Split(';', StringSplitOptions.RemoveEmptyEntries)
                .Select(t => t.Split('=', 2))
                .Where(t => t.Length == 2)
                .GroupBy(t => t[0].Trim(), StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First()[1].Trim(), StringComparer.OrdinalIgnoreCase);

            string Wert(string key) => teile.TryGetValue(key, out var v) ? v : "?";

            return $"Host '{Wert("Host")}', Datenbank '{Wert("Database")}', Benutzer '{Wert("Username")}'";
        }

        private static string PostgresUrlZuConnectionString(string url)
        {
            var uri = new Uri(url);
            var user = uri.UserInfo.Split(':', 2);
            var port = uri.Port > 0 ? uri.Port : 5432;

            return $"Host={uri.Host};Port={port};Database={uri.AbsolutePath.TrimStart('/')};" +
                   $"Username={Uri.UnescapeDataString(user[0])};" +
                   $"Password={(user.Length > 1 ? Uri.UnescapeDataString(user[1]) : "")};" +
                   "SSL Mode=Prefer";
        }
    }
}
