window.qrScanner = {
    html5QrCode: null,
    isRunning: false,
    stopping: false,  // <--- NUEVO

    start: function (dotnetRef) {
        if (this.isRunning) return;

        if (!this.html5QrCode) {
            this.html5QrCode = new Html5Qrcode("reader");
        }

        const config = { fps: 10, qrbox: { width: 450, height: 450 } };

        this.html5QrCode.start(
            { facingMode: "environment" },
            config,
            qrCodeMessage => {
                if (!this.isRunning) return;
                this.stopping = true;  // indicamos que vamos a detener
                this.html5QrCode.stop()
                    .then(() => {
                        this.isRunning = false;
                        this.stopping = false;
                        dotnetRef.invokeMethodAsync("OnQrCodeScanned", qrCodeMessage);
                    })
                    .catch(err => {
                        console.error("Error al detener:", err);
                        this.stopping = false;
                    });
            }
        ).then(() => this.isRunning = true)
            .catch(err => console.error("Error al iniciar cámara:", err));
    },

    stop: function () {
        if (this.html5QrCode && this.isRunning && !this.stopping) {
            this.stopping = true;
            return this.html5QrCode.stop()
                .then(() => {
                    this.isRunning = false;
                    this.stopping = false;
                })
                .catch(err => {
                    console.warn("No se pudo detener:", err);
                    this.stopping = false;
                });
        }
        return Promise.resolve();
    }
};