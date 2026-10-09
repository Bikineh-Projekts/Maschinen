namespace MaschinenDataein.Helper
{
    /// <summary>
    /// Hält die App auf Render wach: ruft alle paar Minuten die eigene öffentliche Adresse auf.
    /// Render lässt einen kostenlosen Dienst nach 15 Minuten ohne eingehende Anfragen einschlafen.
    ///
    /// Läuft nur, wenn eine öffentliche Adresse bekannt ist:
    ///   - auf Render automatisch über die Umgebungsvariable RENDER_EXTERNAL_URL
    ///   - sonst über "WachHalten:Url" in der Konfiguration
    ///
    /// Wichtig: Ein schlafender Dienst kann sich nicht selbst wecken. Deshalb pingt zusätzlich
    /// der GitHub-Workflow (.github/workflows/ping-render.yml) von außen.
    /// </summary>
    public sealed class WachHalteDienst : BackgroundService
    {
        private readonly ILogger<WachHalteDienst> _logger;
        private readonly HttpClient _http;
        private readonly Uri? _ziel;
        private readonly TimeSpan _abstand;

        public WachHalteDienst(IConfiguration config, ILogger<WachHalteDienst> logger, IHttpClientFactory httpFactory)
        {
            _logger = logger;
            _http = httpFactory.CreateClient(nameof(WachHalteDienst));
            _http.Timeout = TimeSpan.FromSeconds(30);

            var basis = config["WachHalten:Url"] ?? config["RENDER_EXTERNAL_URL"];
            if (!string.IsNullOrWhiteSpace(basis) && Uri.TryCreate(basis.TrimEnd('/') + "/health", UriKind.Absolute, out var uri))
                _ziel = uri;

            // Standard 10 Minuten – deutlich unter der Render-Grenze von 15 Minuten
            var minuten = config.GetValue("WachHalten:Minuten", 10);
            _abstand = TimeSpan.FromMinutes(Math.Clamp(minuten, 1, 14));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (_ziel is null)
            {
                _logger.LogInformation("Wachhalten aus (keine öffentliche Adresse – lokal normal)");
                return;
            }

            _logger.LogInformation("Wachhalten aktiv: {Ziel} alle {Minuten} Minuten", _ziel, _abstand.TotalMinutes);

            using var timer = new PeriodicTimer(_abstand);
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                try
                {
                    using var antwort = await _http.GetAsync(_ziel, stoppingToken);
                    if (!antwort.IsSuccessStatusCode)
                        _logger.LogWarning("Wachhalten: {Ziel} antwortet mit {Status}", _ziel, (int)antwort.StatusCode);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    // Nie abstürzen – beim nächsten Durchlauf erneut versuchen
                    _logger.LogWarning("Wachhalten fehlgeschlagen: {Fehler}", ex.Message);
                }
            }
        }
    }
}
