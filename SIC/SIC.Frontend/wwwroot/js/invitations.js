// ==============================
// invitations.js
// ==============================
// Pendiente: HTML agregarle la opciones de tipo de letra, color y tamaño
// ToDo: Validar cuando el inv.numberChildren sea 0 no mostrar el select de niños
// ✅ Constante global accesible en todo el archivo
//const apiUrl = "https://localhost:7141"; //🔹 Constante global
const apiUrl = "https://invboxv-app.com"

let invitacionData = null;
let hayNiños = false;
let tipoEvent = "";

document.addEventListener("DOMContentLoaded", function () {
    obtenerDatosInvitacion();
});

// ✅ Obtiene los datos de la invitación por código
function obtenerDatosInvitacion() {
    const codigoInvitacion = new URLSearchParams(window.location.search).get("codigo");
    if (!codigoInvitacion) {
        mostrarMensajeNoInvitacion(null);
        return;
    }

    fetch(`${apiUrl}/api/invitations/byCode/${codigoInvitacion}`)
        .then((res) => {
            if (!res.ok) throw new Error(`Error HTTP ${res.status}`);
            return res.json();
        })
        .then((data) => {
            if (!data || Object.keys(data).length === 0) {
                mostrarMensajeNoInvitacion(codigoInvitacion);
                return;
            }
            invitacionData = data;
            tipoEvent = data.event.eventType.name;
            const btnConfirmar = document.getElementById("btnConfirmar");
            if (tipoEvent === "Save the Date") {
                btnConfirmar.style.display = "block";
            }

            mostrarDatosInvitacion(data);

            // 🔹 Verificar estado
            if (data.status === 19) {//Asistire
                mostrarMensajeConfirmacion(data, true);
                if (tipoEvent !== "Save the Date") {
                    cargarQR(data.code, data.event.code);
                }
            } else if (data.status === 20) {//No Asistire
                mostrarMensajeConfirmacion(data, false);
            } else if (data.status === 2) {//Pendiente
                const form = document.getElementById("formulario_respuesta");
                if (form) form.style.display = "block";
            } else {
                console.warn("⚠️ Estado desconocido:", data.status);
            }
        })
        .catch((err) => {
            console.error("❌ Error al cargar invitación:", err);
            mostrarMensajeNoInvitacion(codigoInvitacion);
        });
}

// 🆕 Mostrar mensaje si no se encuentra la invitación
function mostrarMensajeNoInvitacion(codigoInvitacion) {
    // Oculta el formulario y otros elementos
    const form = document.getElementById("formulario_respuesta");
    const mensajeGracias = document.getElementById("mensaje_gracias");
    const contQR = document.getElementById("cont_descargaqr");
    const mensajeNoInvitacion = document.getElementById("mensaje_no_invitacion");

    if (form) form.style.display = "none";
    if (mensajeGracias) mensajeGracias.style.display = "none";
    if (contQR) contQR.style.display = "none";

    // Muestra el mensaje de error
    if (mensajeNoInvitacion) {
        mensajeNoInvitacion.style.display = "block";

        const codigoSpan = document.getElementById("codigo_no_encontrado");
        if (codigoSpan && codigoInvitacion) {
            codigoSpan.innerText = codigoInvitacion;
        }
    }
}

// ✅ Muestra los datos en el HTML
function mostrarDatosInvitacion(inv) {
    document.getElementById("rotulo_invitacion").innerText = inv.name;
    console.log(tipoEvent)
    if (tipoEvent !== "Save the Date") {
        document.getElementById("invitados_mayores").innerText = `${inv.numberAdults} Adulto(s)`;
        document.getElementById("invitados_menores").innerText = inv.numberChildren === 0
            ? "Respetuosamente NO NIÑOS"
            : `${inv.numberChildren} Niño(s)`;

        if (inv.numberChildren > 0) hayNiños = true;

        llenarSelect("confirmadosadultos", inv.numberAdults, "Adulto");
        llenarSelect("confirmadosmenores", inv.numberChildren, "Niño");
    }
}

// ✅ Llena los selects
function llenarSelect(selectId, cantidad, tipo) {
    const select = document.getElementById(selectId);
    if (!select) return;
    if (tipo === "Niño") {
        select.innerHTML = `<option value="0">No asistirán ${tipo}(s)</option>`;
    }
    for (let i = 1; i <= cantidad; i++) {
        const opt = document.createElement("option");
        opt.value = i;
        // Verifica si i es mayor que 1 y concatena "s" a tipo
        opt.text = `${i} ${tipo}${i > 1 ? 's' : ''}`;
        select.appendChild(opt);
    }
}

// ✅ Maneja el cambio de asistencia
function fn_asistencia(valor) {
    const selAdultos = document.getElementById("seladultos");
    const selMenores = document.getElementById("selmenores");
    const btnConfirmar = document.getElementById("btnConfirmar");

    if (valor === "s") {
        selAdultos.style.display = "block";
        if (hayNiños) selMenores.style.display = "block";
        btnConfirmar.style.display = "block";
    } else {
        selAdultos.style.display = "none";
        selMenores.style.display = "none";
        btnConfirmar.style.display = "block";
    }
}

// ✅ Envía la confirmación
function sendRespuesta() {
    if (!invitacionData) {
        alert("No se ha cargado la invitación correctamente.");
        return;
    }
    const asistira = document.getElementById("siasistire")
        ? document.getElementById("siasistire").checked
        : null;

    const noAsistira = document.getElementById("noasistire")
        ? document.getElementById("noasistire").checked
        : null;

    if (!asistira && !noAsistira && (tipoEvent !== "Save the Date")) {
        alert("Por favor selecciona si asistirás o no al evento.");
        return;
    }

    const respuesta = {
        codigoInvitacion: invitacionData.code,
        nombre: invitacionData.name,
        cantidadDeMayores: invitacionData.numberAdults,
        cantidadDeMenores: invitacionData.numberChildren,
        confirmacionAsistencia: tipoEvent === "Save the Date" ? true : asistira,
        confirmadosAdultos: parseInt(document.getElementById("confirmadosadultos")?.value || "0"),
        confirmadosMenores: parseInt(document.getElementById("confirmadosmenores")?.value || "0"),
        mensaje: document.getElementById("texto_respuesta")?.value || ""
    };

    fetch(`${apiUrl}/api/invitations/confirm`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(respuesta)
    })
        .then((res) => {
            if (!res.ok) throw new Error("Error al enviar la confirmación.");
            return res.json();
        })
        .then((data) => {
            if (asistira) {
                mostrarMensajeConfirmacion(data, true);
                cargarQR(invitacionData.code, invitacionData.event.code);
                //ToDo: abrir whatsapp con mensaje predefinido, para avisar al que envio el mensaje que ya confirmo
                var numero = invitacionData.event.hostPhone;
                if (invitacionData.event.plannerPhone !== null) {
                    numero = invitacionData.event.plannerPhone;
                }

                const mensaje = `Ya confirmé mi asistencia, gracias por la invitación.`;

                abrirWhatsApp(numero, mensaje);
            } else {
                mostrarMensajeConfirmacion(data, false);
            }
        })
        .catch((err) => {
            console.error(err);
            alert("Ocurrió un error al enviar tu respuesta. Intenta de nuevo.");
        });
}
function abrirWhatsApp(numero, mensaje) {
    const url = `https://api.whatsapp.com/send?phone=${numero}&text=${encodeURIComponent(mensaje)}&app_absent=0`;
    window.open(url, "_blank");
}

// ✅ Muestra el mensaje final (con o sin QR)
function mostrarMensajeConfirmacion(data, asistira) {
    const form = document.getElementById("formulario_respuesta");
    const mensajeGracias = document.getElementById("mensaje_gracias");
    const mensajeQR = mensajeGracias ? mensajeGracias.querySelector("p") : null;
    const contQR = document.getElementById("cont_descargaqr");

    if (form) form.style.display = "none";
    if (mensajeGracias) mensajeGracias.style.display = "block";

    if (asistira && tipoEvent !== "Save the Date") {
        if (mensajeQR) mensajeQR.style.display = "block";
        if (contQR) contQR.style.display = "block";
        document.getElementById("mi_codigo_invitado").innerText = `Código: ${data.codigoInvitacion || data.code}`;
    } else {
        if (mensajeQR) mensajeQR.style.display = "none";
        if (contQR) contQR.style.display = "none";
    }
}

// ✅ Cargar QR / Boleta PDF desde el backend
function cargarQR(codigoInvitacion, codigoEvento) {
    const qrUrl = `${apiUrl}/api/Invitations/qr?codigo=${codigoInvitacion}&evento=${codigoEvento}`;

    fetch(qrUrl)
        .then(response => {
            if (!response.ok) throw new Error("Error al generar la boleta");
            return response.json(); // API debe devolver { qrBase64: "...", pdfBase64: "..." }
        })
        .then(data => {
            // ==========================
            // 1️⃣ CARGAR QR EN <img>
            // ==========================

            const imgQr = document.getElementById("img_qr");

            if (!data.qrBase64 || data.qrBase64 === "null") {
                console.error("El backend no devolvió un QR válido");
                imgQr.src = "";
                return;
            }

            imgQr.src = `data:image/png;base64,${data.qrBase64}`;

            // ==========================
            // 2️⃣ GENERAR PDF PARA DESCARGA
            // ==========================
            const enlaceDescarga = document.getElementById("mi_enlace_descarga_qr");

            if (data.pdfBase64 && data.pdfBase64 !== "null") {
                const pdfBlob = base64ToBlob(data.pdfBase64, "application/pdf");
                const url = URL.createObjectURL(pdfBlob);

                enlaceDescarga.href = url;
                enlaceDescarga.download = `Boleta_${codigoInvitacion}.pdf`;
                enlaceDescarga.textContent = "Descargar boleta PDF";
            }

            // Mostrar bloque
            document.getElementById("cont_descargaqr").style.display = "block";
        })
        .catch(error => console.error("Error al generar QR:", error));
}
// ==============================
// Función auxiliar
// ==============================
function base64ToBlob(base64, mimeType) {
    const bytes = atob(base64);
    const len = bytes.length;
    const buffer = new Uint8Array(len);
    for (let i = 0; i < len; i++) {
        buffer[i] = bytes.charCodeAt(i);
    }
    return new Blob([buffer], { type: mimeType });
}