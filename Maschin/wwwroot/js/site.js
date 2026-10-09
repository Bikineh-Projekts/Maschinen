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
