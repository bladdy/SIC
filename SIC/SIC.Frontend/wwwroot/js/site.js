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
