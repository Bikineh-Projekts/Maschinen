// MaschinenDataein – kleine Komfortfunktionen für alle Seiten
(function () {
    'use strict';

    const ladebalken = document.getElementById('ladebalken');

    function ladenAnzeigen() {
        if (ladebalken) ladebalken.classList.add('aktiv');
    }

    // Ladebalken bei Seitenwechsel über Links und Formulare
    document.addEventListener('click', function (e) {
        const link = e.target.closest('a[href]');
        if (!link || link.target === '_blank' || e.ctrlKey || e.metaKey || e.shiftKey) return;
        const href = link.getAttribute('href');
        if (!href || href.startsWith('#') || href.startsWith('javascript:') || link.hasAttribute('data-bs-toggle')) return;
        if (link.closest('.disabled')) { e.preventDefault(); return; }
        ladenAnzeigen();
    });

    document.addEventListener('submit', function (e) {
        if (!e.defaultPrevented) ladenAnzeigen();
    });

    // Beim Zurück-Navigieren aus dem Browser-Cache den Balken zurücksetzen
    window.addEventListener('pageshow', function () {
        if (ladebalken) ladebalken.classList.remove('aktiv');
    });

    // Blättern mit ← / → (nicht, wenn gerade in einem Eingabefeld getippt wird)
    document.addEventListener('keydown', function (e) {
        if (e.altKey || e.ctrlKey || e.metaKey) return;
        const ziel = e.target;
        if (ziel && (ziel.isContentEditable || /^(INPUT|TEXTAREA|SELECT)$/.test(ziel.tagName))) return;

        const richtung = e.key === 'ArrowLeft' ? 'zurueck' : e.key === 'ArrowRight' ? 'weiter' : null;
        if (!richtung) return;

        const link = document.querySelector('.page-item:not(.disabled) a[data-seite="' + richtung + '"]');
        if (link) {
            ladenAnzeigen();
            window.location.href = link.href;
        }
    });

    // "Nach oben"-Knopf
    const nachOben = document.getElementById('nachOben');
    if (nachOben) {
        window.addEventListener('scroll', function () {
            nachOben.classList.toggle('sichtbar', window.scrollY > 400);
        }, { passive: true });
        nachOben.addEventListener('click', function () {
            window.scrollTo({ top: 0, behavior: 'smooth' });
        });
    }
})();

// ── Tabellen fürs Smartphone vorbereiten ─────────────────────────────────────
// Schreibt die Spaltenüberschrift als data-label in jede Zelle. Das CSS zeigt sie
// unter 768 px als Bezeichnung in der Kartenansicht an. Funktioniert auch für
// Zeilen, die später per JavaScript hinzukommen (z. B. in der Planung).
(function () {
    'use strict';

    function spaltenNamen(tabelle) {
        const kopf = tabelle.tHead && tabelle.tHead.rows[0];
        if (!kopf) return null;

        const namen = [];
        for (const th of kopf.cells) {
            const versteckt = th.querySelector('.visually-hidden');
            const text = th.textContent.replace(/\s+/g, ' ').trim();
            // Nur unsichtbar beschriftete Spalten (z. B. "Aktionen") bekommen keine Bezeichnung
            const name = versteckt && versteckt.textContent.trim() === text ? '' : text;
            for (let i = 0; i < (th.colSpan || 1); i++) namen.push(name);
        }
        return namen;
    }

    function beschrifte(tabelle) {
        const namen = spaltenNamen(tabelle);
        if (!namen) return;

        const zeilen = [];
        for (const teil of tabelle.tBodies) zeilen.push(...teil.rows);
        if (tabelle.tFoot) zeilen.push(...tabelle.tFoot.rows);

        for (const zeile of zeilen) {
            let spalte = 0;
            for (const zelle of zeile.cells) {
                const name = zelle.colSpan > 1 ? '' : (namen[spalte] || '');
                if (zelle.getAttribute('data-label') !== name) zelle.setAttribute('data-label', name);
                zelle.classList.toggle('ohne-label', !name);
                zelle.classList.toggle('eingabe', !!zelle.querySelector('input:not([type=hidden]), select, textarea'));
                spalte += zelle.colSpan || 1;
            }
            const hatEingabe = !!zeile.querySelector('input:not([type=hidden]), select, textarea');
            zeile.classList.toggle('eingabe-zeile', hatEingabe);
            // Textfelder für Grund/Beschreibung über die ganze Kartenbreite
            for (const zelle of zeile.cells) {
                zelle.classList.toggle('breit', hatEingabe && !!zelle.querySelector('input[name*="Grund"], input[name="artikel"], textarea'));
            }
            const erste = zeile.cells[0];
            if (erste) erste.classList.toggle('kartenkopf', /^(id)$/i.test(erste.getAttribute('data-label') || ''));
        }
    }

    function alleBeschriften() {
        document.querySelectorAll('main table.table').forEach(beschrifte);
    }

    let geplant = false;
    function spaeterBeschriften() {
        if (geplant) return;
        geplant = true;
        requestAnimationFrame(function () { geplant = false; alleBeschriften(); });
    }

    document.addEventListener('DOMContentLoaded', function () {
        alleBeschriften();
        const main = document.querySelector('main');
        if (main && 'MutationObserver' in window) {
            new MutationObserver(function (aenderungen) {
                for (const a of aenderungen) {
                    for (const knoten of a.addedNodes) {
                        if (knoten.nodeType === 1 && (knoten.tagName === 'TR' || knoten.querySelector && knoten.querySelector('table'))) {
                            spaeterBeschriften();
                            return;
                        }
                    }
                }
            }).observe(main, { childList: true, subtree: true });
        }
    });
})();

// ── Hell/Dunkel umschalten ───────────────────────────────────────────────────
(function () {
    'use strict';
    const html = document.documentElement;
    const knopf = document.getElementById('themaKnopf');

    function istDunkel() { return html.getAttribute('data-bs-theme') === 'dark'; }

    function symbolSetzen() {
        if (!knopf) return;
        const i = knopf.querySelector('i');
        if (i) i.className = istDunkel() ? 'bi bi-sun' : 'bi bi-moon-stars';
        knopf.setAttribute('aria-pressed', istDunkel() ? 'true' : 'false');
    }

    symbolSetzen();
    if (!knopf) return;

    knopf.addEventListener('click', function () {
        const neu = istDunkel() ? 'light' : 'dark';
        html.setAttribute('data-theme', neu);
        html.setAttribute('data-bs-theme', neu);
        try { localStorage.setItem('md-thema', neu); } catch (e) { }
        symbolSetzen();

        // Diagramme zeichnen ihre Farben auf ein Canvas – dafür die Seite neu laden
        if (document.querySelector('main canvas')) window.location.reload();
    });
})();

// ── Reiter "Analyse | Tabelle" ───────────────────────────────────────────────
// Merkt die Auswahl je Seite. Beim Blättern in der Tabelle (pageNumber in der
// Adresse) öffnet sich automatisch der Tabellen-Reiter.
(function () {
    'use strict';
    document.addEventListener('DOMContentLoaded', function () {
        document.querySelectorAll('[data-reiter]').forEach(function (leiste) {
            const name = 'md-reiter-' + leiste.getAttribute('data-reiter');
            const knoepfe = leiste.querySelectorAll('[data-bs-toggle="tab"]');
            if (!knoepfe.length || !window.bootstrap) return;

            let ziel = null;
            if (/[?&]pageNumber=/.test(location.search)) ziel = 'tabelle';
            else { try { ziel = localStorage.getItem(name); } catch (e) { } }

            knoepfe.forEach(function (k) {
                k.addEventListener('shown.bs.tab', function () {
                    try { localStorage.setItem(name, k.getAttribute('data-reiter-id')); } catch (e) { }
                    // Seiten mit eigenem Diagramm-Lader (Leistung, Temperatur)
                    const lader = k.getAttribute('data-beim-zeigen');
                    if (lader && typeof window[lader] === 'function') window[lader]();
                    window.mdAnalyseZeichnen && window.mdAnalyseZeichnen();
                });
                if (ziel && k.getAttribute('data-reiter-id') === ziel) bootstrap.Tab.getOrCreateInstance(k).show();
            });
        });
        window.mdAnalyseZeichnen && window.mdAnalyseZeichnen();
    });
})();

// ── Analyse-Diagramme zeichnen (Partial _Analyse) ────────────────────────────
(function () {
    'use strict';

    function kurz(text, max) {
        text = String(text);
        return text.length > max ? text.slice(0, max - 1) + '…' : text;
    }
    function zahl(v) {
        return v == null ? '–' : Number(v).toLocaleString('de-DE', { maximumFractionDigits: 1 });
    }

    function zeichne(def) {
        const canvas = document.getElementById(def.id);
        if (!canvas || canvas.dataset.gezeichnet) return;
        if (!canvas.offsetParent) return;          // in verstecktem Reiter: später zeichnen
        canvas.dataset.gezeichnet = '1';

        const text2 = mdFarbe('md-text-2');
        const raster = mdFarbe('viz-raster');
        const flaeche = mdFarbe('md-flaeche');
        const waagrecht = def.art === 'BalkenWaagrecht' || def.art === 'GestapeltWaagrecht';
        const gestapelt = def.art === 'GestapeltWaagrecht';
        const linie = def.art === 'Linie';
        const mehrere = def.reihen.length > 1;

        Chart.defaults.font.family = getComputedStyle(document.body).fontFamily;
        Chart.defaults.color = text2;

        const datasets = def.reihen.map(function (r) {
            const farbe = mdFarbe(r.farbe) || mdFarbe('viz-1');
            const basis = { label: r.name, data: r.werte };
            if (linie) return Object.assign(basis, {
                borderColor: farbe, backgroundColor: farbe, borderWidth: 2,
                pointRadius: 0, pointHoverRadius: 5, pointHitRadius: 12, tension: .25, spanGaps: false   // Lücken = keine Daten, nicht verbinden
            });
            return Object.assign(basis, {
                backgroundColor: farbe, borderColor: flaeche,
                borderWidth: gestapelt ? { right: 2 } : 0,
                borderRadius: gestapelt ? 0 : 4, borderSkipped: 'start',
                maxBarThickness: waagrecht ? 22 : 28
            });
        });

        const wertAchse = {
            // Mengen beginnen bei 0; Verläufe (Linie) zoomen auf den Wertebereich
            beginAtZero: !linie, grace: linie ? '10%' : 0, stacked: gestapelt,
            grid: { color: raster, drawTicks: false },
            border: { display: false },
            ticks: { padding: 6, callback: function (v) { return zahl(v); } }
        };
        const kategorieAchse = {
            stacked: gestapelt,
            grid: { display: false },
            border: { color: mdFarbe('md-rand') },
            ticks: {
                autoSkip: !waagrecht, maxRotation: 0, padding: 4,
                // schmale Bildschirme: kürzere Namen, der volle Name steht im Tooltip
                callback: function (v) { return kurz(this.getLabelForValue(v), waagrecht ? (canvas.clientWidth < 520 ? 18 : 34) : 14); }
            }
        };

        new Chart(canvas, {
            type: linie ? 'line' : 'bar',
            data: { labels: def.beschriftungen, datasets: datasets },
            options: {
                indexAxis: waagrecht ? 'y' : 'x',
                responsive: true, maintainAspectRatio: false,
                animation: { duration: 400 },
                interaction: { mode: linie || gestapelt ? 'index' : 'nearest', axis: waagrecht ? 'y' : 'x', intersect: false },
                scales: waagrecht ? { x: wertAchse, y: kategorieAchse } : { x: kategorieAchse, y: wertAchse },
                plugins: {
                    legend: { display: mehrere, position: 'bottom', labels: { usePointStyle: true, pointStyle: 'rectRounded', boxWidth: 10, padding: 14 } },
                    tooltip: {
                        backgroundColor: mdFarbe('md-tooltip'), titleColor: mdFarbe('md-tooltip-text'), bodyColor: mdFarbe('md-tooltip-text'),
                        padding: 10, cornerRadius: 6, boxPadding: 4,
                        callbacks: {
                            label: function (c) {
                                const v = waagrecht ? c.parsed.x : c.parsed.y;
                                return ' ' + (mehrere ? c.dataset.label + ': ' : '') + zahl(v) + ' ' + def.einheit;
                            }
                        }
                    }
                }
            }
        });
    }

    window.mdAnalyseZeichnen = function () {
        if (!window.Chart) return;
        document.querySelectorAll('[data-analyse] .analyse-daten').forEach(function (s) {
            let defs = [];
            try { defs = JSON.parse(s.textContent); } catch (e) { return; }
            defs.forEach(zeichne);
        });
    };
})();
