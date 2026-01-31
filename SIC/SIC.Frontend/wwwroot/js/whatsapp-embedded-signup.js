/* ============================
SDK INIT
============================ */
window.fbSdkReady = false;

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
    if (!event.origin.endsWith("facebook.com")) return;

    try {
        const data = typeof event.data === "string"
            ? JSON.parse(event.data)
            : event.data;

        if (data.type === "WA_EMBEDDED_SIGNUP") {
            console.log("📨 Embedded Signup data:", data);

            fetch("/api/whatsapp/exchange-code", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({
                    code: data.code,
                    wabaId: data.waba_id,
                    phoneNumberId: data.phone_number_id
                })
            });
        }
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