/* whatsapp-embedded-signup.V1.0.4.js */

/* ============================
   CONFIG
============================ */
window.fbSdkReady = false;
let AUTH_CODE = null;

//const API_BASE_URL = "https://localhost:7141";
const API_BASE_URL = "https://invboxv-app.com";
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
    console.log("===== MESSAGE =====");
    console.log("Origin:", event.origin);
    console.log("Data:", event.data);

    // Solo aceptar mensajes de Facebook
    const allowedOrigins = [
        "https://www.facebook.com",
        "https://web.facebook.com",
        "https://business.facebook.com"
    ];

    if (!allowedOrigins.includes(event.origin))
        return;

    try {
        let data = null;

        // Parse seguro
        if (typeof event.data === "string") {
            try {
                data = JSON.parse(event.data);
            }
            catch {
                // Ignorar mensajes que no sean JSON
                return;
            }
        }
        else {
            data = event.data;
        }

        if (!data || data.type !== "WA_EMBEDDED_SIGNUP")
            return;

        console.log("📨 Evento recibido:", data);

        switch (data.event) {
            //====================================================
            // Embedded Signup normal
            //====================================================

            case "FINISH":

            //====================================================
            // Embedded Signup Coexistence
            //====================================================

            case "FINISH_WHATSAPP_BUSINESS_APP_ONBOARDING":

                let code = AUTH_CODE || localStorage.getItem("WA_AUTH_CODE");

                if (!code) {
                    try {
                        console.log("⌛ Esperando AUTH_CODE...");
                        code = await waitForAuthCode();
                    }
                    catch (err) {
                        console.error("❌ AUTH_CODE no llegó:", err);
                        return;
                    }
                }

                const payload = data.data;

                if (
                    !payload ||
                    !payload.business_id ||
                    !payload.waba_id ||
                    !payload.phone_number_id
                ) {
                    console.error("❌ Datos incompletos:", payload);
                    return;
                }

                console.log("🚀 Enviando datos al backend...");

                const token = localStorage.getItem("TOKEN_KEY");

                if (!token) {
                    console.error("❌ TOKEN_KEY no encontrado");
                    return;
                }

                try {
                    const response = await fetch(
                        `${API_BASE_URL}/api/WhatsAppOnboarding/exchange-code`,
                        {
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

                    console.log("📡 Status:", response.status);

                    if (!response.ok) {
                        const error = await response.text();

                        console.error("❌ Backend:", error);

                        throw new Error(error);
                    }

                    const result = await response.json();

                    console.log("✅ WhatsApp conectado:", result);

                    AUTH_CODE = null;

                    localStorage.removeItem("WA_AUTH_CODE");
                }
                catch (err) {
                    console.error("❌ Error exchange-code:", err);
                }

                break;

            case "CANCEL":

                console.warn("⚠️ Usuario canceló el proceso");

                AUTH_CODE = null;

                localStorage.removeItem("WA_AUTH_CODE");

                break;

            case "ERROR":

                console.error("❌ Embedded Signup Error:", data);

                AUTH_CODE = null;

                localStorage.removeItem("WA_AUTH_CODE");

                break;

            default:

                console.log("Evento no manejado:", data.event);

                break;
        }
    }
    catch (e) {
        console.error("❌ Error general:", e);
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