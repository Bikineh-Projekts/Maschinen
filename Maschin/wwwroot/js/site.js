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
