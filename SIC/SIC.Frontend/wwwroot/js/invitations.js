// ==============================
// invitations.js
// ==============================
// Pendiente: HTML agregarle la opciones de tipo de letra, color y tamaño
// Si los niños es = 0 que diga "Respetuosamente NO NIÑOS" y el select de niños no se muestre

//Select
// options 1 Adultos
// options 2 Adultos
// Igual para los niños

// ✅ Constante global accesible en todo el archivo
const apiUrl = "https://localhost:7141"; //🔹 Constante global
//const apiUrl = "https://invboxv-app.com"

let invitacionData = null;

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
            console.log("➡️ Estado de la respuesta:", res.status);
            if (!res.ok) throw new Error(`Error HTTP ${res.status}`);
            return res.json();
        })
        .then((data) => {
            console.log("📦 Datos recibidos:", data);

            if (!data || Object.keys(data).length === 0) {
                mostrarMensajeNoInvitacion(codigoInvitacion);
                return;
            }

            invitacionData = data;
            mostrarDatosInvitacion(data);

            // 🔹 Verificar estado
            if (data.status === 19) {
                mostrarMensajeConfirmacion(data, true);
                cargarQR(data.code, data.event.code);
            } else if (data.status === 20) {
                mostrarMensajeConfirmacion(data, false);
            } else if (data.status === 2) {
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
    document.getElementById("invitados_mayores").innerText = `${inv.numberAdults} Adultos`;
    document.getElementById("invitados_menores").innerText = `${inv.numberChildren} Niños`;

    llenarSelect("confirmadosadultos", inv.numberAdults, "adultos");
    llenarSelect("confirmadosmenores", inv.numberChildren, "niños");
}

// ✅ Llena los selects
function llenarSelect(selectId, cantidad, tipo) {
    const select = document.getElementById(selectId);
    if (!select) return;
    if (tipo === "niños")
    {
        select.innerHTML = `<option value="0">No asistirán ${tipo}</option>`;
    }
    for (let i = 1; i <= cantidad; i++) {
        const opt = document.createElement("option");
        opt.value = i;
        opt.text = `${i} ${tipo}`;
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
        selMenores.style.display = "block";
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

    const asistira = document.getElementById("siasistire").checked;
    const noAsistira = document.getElementById("noasistire").checked;

    if (!asistira && !noAsistira) {
        alert("Por favor selecciona si asistirás o no al evento.");
        return;
    }

    const respuesta = {
        codigoInvitacion: invitacionData.code,
        nombre: invitacionData.name,
        cantidadDeMayores: invitacionData.numberAdults,
        cantidadDeMenores: invitacionData.numberChildren,
        confirmacionAsistencia: asistira,
        confirmadosAdultos: parseInt(document.getElementById("confirmadosadultos").value),
        confirmadosMenores: parseInt(document.getElementById("confirmadosmenores").value),
        mensaje: document.getElementById("texto_respuesta").value
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
            } else {
                mostrarMensajeConfirmacion(data, false);
            }
        })
        .catch((err) => {
            console.error(err);
            alert("Ocurrió un error al enviar tu respuesta. Intenta de nuevo.");
        });
}

// ✅ Muestra el mensaje final (con o sin QR)
function mostrarMensajeConfirmacion(data, asistira) {
    const form = document.getElementById("formulario_respuesta");
    const mensajeGracias = document.getElementById("mensaje_gracias");
    const mensajeQR = mensajeGracias ? mensajeGracias.querySelector("p") : null;
    const contQR = document.getElementById("cont_descargaqr");

    if (form) form.style.display = "none";
    if (mensajeGracias) mensajeGracias.style.display = "block";

    if (asistira) {
        if (mensajeQR) mensajeQR.style.display = "block";
        if (contQR) contQR.style.display = "block";
        document.getElementById("mi_codigo_invitado").innerText = `Código: ${data.codigoInvitacion || data.code}`;
    } else {
        if (mensajeQR) mensajeQR.style.display = "none";
        if (contQR) contQR.style.display = "none";
    }
}

// ✅ Cargar el QR desde el backend
function cargarQR(codigoInvitacion, codigoEvento) {
    const imgQR = document.getElementById("mi_acceso_qr");
    const enlaceQR = document.getElementById("mi_enlace_descarga_qr");
    const contQR = document.getElementById("cont_descargaqr");

    const qrUrl = `${apiUrl}/api/Invitations/qr?codigo=${codigoInvitacion}&evento=${codigoEvento}`;

    fetch(qrUrl)
        .then(response => response.json())
        .then(data => {
            console.log("Respuesta QR:", data);
            if (data && data.qrCodeBase64) {
                let qrImage = document.getElementById("mi_acceso_qr");
                qrImage.src = "data:image/png;base64," + data.qrCodeBase64;

                let enlaceDescarga = document.getElementById("mi_enlace_descarga_qr");
                enlaceDescarga.href = qrImage.src;

                // NO tocar display aquí
                // document.getElementById("cont_descargaqr").style.display = "block";
            } else {
                console.error("No se recibió base64 del QR");
            }
        })
        .catch(error => console.error("Error al generar QR:", error));
}

// ✅ Descargar QR
function descargarQR() {
    const qrImage = document.getElementById("mi_acceso_qr");
    if (!qrImage.src) {
        alert("El QR aún no está disponible");
        return;
    }

    // Crear un enlace temporal
    const enlace = document.createElement("a");
    enlace.href = qrImage.src;
    enlace.download = "INV - " + `${invitacionData.event.name}.png`; // Nombre del archivo
    document.body.appendChild(enlace);
    enlace.click();
    document.body.removeChild(enlace);
}
