window.initPopovers = (durationMs = 4000) => {
    const triggers = document.querySelectorAll('[data-bs-toggle="popover"]');

    triggers.forEach(el => {
        // Crear instancia UNA SOLA VEZ
        let instance = bootstrap.Popover.getInstance(el);
        if (!instance) {
            instance = new bootstrap.Popover(el, {
                trigger: 'manual'
            });
        }

        // Evitar múltiples listeners
        if (el._popoverBound) return;
        el._popoverBound = true;

        el.addEventListener('click', () => {
            // Limpiar timeout previo
            if (el._popoverTimeout) {
                clearTimeout(el._popoverTimeout);
                el._popoverTimeout = null;
            }

            // TOGGLE REAL
            if (el.getAttribute('aria-describedby')) {
                instance.hide();
                return;
            }

            instance.show();

            // Auto close
            el._popoverTimeout = setTimeout(() => {
                instance.hide();
                el._popoverTimeout = null;
            }, durationMs);
        });
    });
};
window.scrollToBottom = (element) => {
    if (element)
        element.scrollTop = element.scrollHeight;
};
window.clampNumberInput = (el) => {
    const v = parseFloat(el.value);
    if (isNaN(v)) return;
    let min = -Infinity;
    let max = Infinity;
    if (el.getAttribute('min') !== null && el.getAttribute('min') !== '') {
        min = parseFloat(el.min);
    }
    if (el.getAttribute('max') !== null && el.getAttribute('max') !== '') {
        max = parseFloat(el.max);
    }
    const clamped = Math.min(Math.max(v, min), max);
    if (clamped !== v) {
        el.value = clamped;
        el.dispatchEvent(new Event('change', { bubbles: true }));
    }
};
document.addEventListener('input', (e) => {
    const el = e.target;
    if (el && el.type === 'number' && el.hasAttribute('data-maxmin-clamp')) {
        window.clampNumberInput(el);
    }
});
