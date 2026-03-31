/* whatsapp-embedded-signup.V1.0.4.js */

/* ============================
   CONFIG
============================ */
window.fbSdkReady = false;
let AUTH_CODE = null;

//const API_BASE_URL = "https://localhost:7141";
const API_BASE_URL = "https://invboxv-app.com;

/* ============================
   SDK INIT
============================ */
window.initFacebookSdk = function (appId) {
    function init() {
        if (window.fbSdkReady) return;

        window.FB.init({
            appId: appId,
            autoLogAppEvents: true,
            xfbml: true,
            version: 'v25.0' // ✅ versión estable recomendada
        });

        window.fbSdkReady = true;
        console.log("✅ Facebook SDK inicializado");
    }

    if (typeof FB !== "undefined") {
        init();
    } else {
        window.fbAsyncInit = function () {
            init();
        };
    }
};

/* ============================
   EMBEDDED SIGNUP
============================ */
window.whatsappEmbeddedSignup = function (configId) {
    if (!window.fbSdkReady) {
        console.error("❌ Facebook SDK NO inicializado aún");
        return;
    }

    AUTH_CODE = null; // reset

    window.FB.login(function (response) {
        console.log("📦 FB.login response:", response);

        if (response.authResponse?.code) {
            AUTH_CODE = response.authResponse.code;
            console.log("✅ AUTH CODE recibido");
        } else if (response.status === "not_authorized") {
            console.warn("⚠️ Usuario no autorizó la app");
        } else {
            console.warn("⚠️ Login cancelado o sin código");
        }
    },
        {
            config_id: configId,
            response_type: 'code',
            override_default_response_type: true,
            scope: 'whatsapp_business_management,whatsapp_business_messaging', // ✅ IMPORTANTE
            // 🔥 AGREGA ESTO
            extras: {
                featureType: 'whatsapp_business_app_onboarding',
                sessionInfoVersion: '3'
            }
        });
};

/* ============================
   MESSAGE LISTENER
============================ */
// Helper: esperar AUTH_CODE
function waitForAuthCode(timeout = 5000) {
    return new Promise((resolve, reject) => {
        const start = Date.now();

        const interval = setInterval(() => {
            if (AUTH_CODE) {
                clearInterval(interval);
                resolve(AUTH_CODE);
            }

            if (Date.now() - start > timeout) {
                clearInterval(interval);
                reject("Timeout esperando AUTH_CODE");
            }
        }, 100);
    });
}

window.addEventListener("message", async (event) => {
    // 🔒 Validar origen
    if (!event.origin.includes("facebook.com")) return;

    try {
        let data = null;

        // ✅ Parse seguro (evita error "cb=...")
        if (typeof event.data === "string") {
            try {
                data = JSON.parse(event.data);
            } catch {
                return; // ignorar mensajes no JSON
            }
        } else {
            data = event.data;
        }

        if (!data || data.type !== "WA_EMBEDDED_SIGNUP") return;

        console.log("📨 Evento recibido:", data);

        /* ============================
           HANDLE EVENTS
        ============================ */

        if (data.event === "FINISH") {
            let code = AUTH_CODE || localStorage.getItem("WA_AUTH_CODE");

            // 🔥 Esperar si aún no llega
            if (!code) {
                try {
                    code = await waitForAuthCode();
                } catch {
                    console.error("❌ AUTH_CODE no llegó a tiempo");
                    return;
                }
            }

            const payload = data.data;

            // ✅ Validación fuerte
            if (
                !payload ||
                !payload.business_id ||
                !payload.waba_id ||
                !payload.phone_number_id
            ) {
                console.error("❌ Datos incompletos:", payload);
                return;
            }

            console.log("🚀 Enviando datos al backend...", {
                code,
                businessId: payload.business_id,
                wabaId: payload.waba_id,
                phoneNumberId: payload.phone_number_id
            });

            const token = localStorage.getItem("TOKEN_KEY");

            if (!token) {
                console.error("❌ No hay TOKEN_KEY en localStorage");
                return;
            }

            try {
                const response = await fetch(`${API_BASE_URL}/api/WhatsAppOnboarding/exchange-code`, {
                    method: "POST",
                    headers: {
                        "Content-Type": "application/json",
                        "Authorization": `Bearer ${token}`
                    },
                    body: JSON.stringify({
                        code: code,
                        businessId: payload.business_id,
                        wabaId: payload.waba_id,
                        phoneNumberId: payload.phone_number_id
                    })
                });

                if (!response.ok) {
                    throw new Error(`HTTP ${response.status}`);
                }

                const result = await response.json();

                console.log("✅ WhatsApp conectado correctamente:", result);

                // 🧹 Limpieza
                AUTH_CODE = null;
                localStorage.removeItem("WA_AUTH_CODE");
            } catch (err) {
                console.error("❌ Error en exchange-code:", err);
            }
        }

        else if (data.event === "CANCEL") {
            console.warn("⚠️ Usuario canceló el proceso");
            AUTH_CODE = null;
            localStorage.removeItem("WA_AUTH_CODE");
        }

        else if (data.event === "ERROR") {
            console.error("❌ Error en Embedded Signup:", data);
            AUTH_CODE = null;
            localStorage.removeItem("WA_AUTH_CODE");
        }
    } catch (e) {
        console.warn("⚠️ Error general:", e, event.data);
    }
});

/* ============================
   LOGOUT
============================ */
window.logoutFacebook = function () {
    if (typeof FB === "undefined") return;

    FB.getLoginStatus(res => {
        if (res.status === "connected") {
            FB.logout(() => console.log("✅ Logout OK"));
        }
    });
};