/* ============================
SDK INIT
============================ */
window.fbSdkReady = false;

const API_BASE_URL = "https://localhost:7141";

window.initFacebookSdk = function (appId) {
    function init() {
        if (window.fbSdkReady) return;

        FB.init({
            appId: appId,
            autoLogAppEvents: true,
            xfbml: true,
            version: 'v24.0'
        });

        window.fbSdkReady = true;
        console.log("✅ Facebook SDK inicializado");
    }

    // ✅ SDK ya cargado
    if (typeof FB !== "undefined") {
        init();
    }
    // ⏳ SDK aún no cargado
    else {
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

    FB.login(function (response) {
        console.log("📦 FB.login response:", response);

        if (response.authResponse?.code) {
            console.log("✅ AUTH CODE:", response.authResponse.code);
            // ⚠️ NO enviar aún, esperar message event
        }
    }, {
        config_id: configId, // 🔴 OBLIGATORIO
        response_type: 'code',
        override_default_response_type: true,
        extras: {
            setup: {},
            featureType: 'whatsapp_business_app_onboarding',
            sessionInfoVersion: '3'
        }
    });
};

/* ============================
MESSAGE LISTENER
============================ */
window.addEventListener("message", (event) => {
    if (!event.origin.includes("facebook.com")) return;

    try {
        const data =
            typeof event.data === "string" && event.data.trim().startsWith("{")
                ? JSON.parse(event.data)
                : event.data;

        if (data?.type !== "WA_EMBEDDED_SIGNUP") return;

        console.log("📨 Embedded Signup data:", data);

        // 🔑 Obtener JWT del localStorage
        const token = localStorage.getItem("TOKEN_KEY");

        if (!token) {
            console.error("❌ No hay TOKEN_KEY en localStorage");
            return;
        }

        fetch(`${API_BASE_URL}/api/WhatsAppOnboarding/exchange-code`, {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
                "Authorization": `Bearer ${token}` // 🔥 CLAVE
            },
            body: JSON.stringify({
                code: data.code,
                businessId: data.business_id,     // 🔥 ESTE FALTABA
                wabaId: data.waba_id,
                phoneNumberId: data.phone_number_id
            })
        })
            .then(res => {
                if (!res.ok) throw new Error(`HTTP ${res.status}`);
                return res.json();
            })
            .then(result => {
                console.log("✅ WhatsApp conectado:", result);
            })
            .catch(err => {
                console.error("❌ Error exchange-code:", err);
            });
    } catch (e) {
        console.warn("⚠️ message parse error", e, event.data);
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