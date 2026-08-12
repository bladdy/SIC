// ===============================================================
//  VARIABLES GLOBALES
// ===============================================================
let invitacion = null;
//const apiUrl = "https://localhost:7141/api/Invitations"; // Cambia si es necesario
const apiUrl = "https://invboxv-app.com/api/Invitations"; // Cambia si es necesario

// ===============================================================
//  ARRANQUE DEFENSIVO (funciona aunque DOMContentLoaded ya haya ocurrido)
// ===============================================================
function iniciar() {
    const params = new URLSearchParams(window.location.search);
    const code = params.get("codigo");
    if (!code) {
        mostrarNoInvitacion("SIN CÓDIGO");
        return;
    }

    cargarInvitacion(code);
}

if (document.readyState === "loading") {
    document.addEventListener("DOMContentLoaded", iniciar);
} else {
    iniciar();
}

window.addEventListener("error", function (event) {
    console.error("Error de carga:", event.message, "en", event.filename, "línea", event.lineno);
});

// ===============================================================
//  NORMALIZAR STATUS / GUEST TYPE (soporta strings y números)
// ===============================================================
function getStatusKey(status) {
    const st = String(status ?? "");
    if (st === "Attend" || st === "19") return "Attend";
    if (st === "NotAttend" || st === "20") return "NotAttend";
    if (st === "Pending" || st === "2") return "Pending";
    return st;
}

function getGuestTypeKey(type) {
    const t = String(type ?? "");
    if (t === "Adult" || t === "1") return "Adult";
    if (t === "Youth" || t === "2") return "Youth";
    if (t === "Children" || t === "3") return "Children";
    return t;
}

// ===============================================================
//  LLAMAR API PARA CARGAR INVITACIÓN
// ===============================================================
function cargarInvitacion(code) {
    const controller = new AbortController();
    const timer = setTimeout(() => controller.abort(), 15000);

    fetch(`${apiUrl}/byCode/${code}`, { signal: controller.signal })
        .then(response => {
            if (!response.ok) {
                mostrarNoInvitacion(code);
                throw new Error("Invitación no encontrada");
            }
            return response.json();
        })
        .then(data => {
            if (!data || Object.keys(data).length === 0) {
                mostrarNoInvitacion(code);
                return;
            }
            invitacion = data;
            const status = getStatusKey(invitacion.status);
            console.log("Status de la invitación:", invitacion.status, "→", status);

            if (status === "Attend") {
                mostrarGracias(data);
                if (invitacion.event?.code) {
                    cargarQRAndPdf(invitacion.code, invitacion.event.code);
                }
            }
            else if (status === "NotAttend") {
                mostrarGracias(data);
            }
            else if (status === "Pending") {
                renderFormulario();
            }
            else {
                console.error("Status inesperado:", invitacion.status);
                mostrarNoInvitacion(code);
            }
        })
        .catch(error => {
            if (error.name === "AbortError") {
                console.error("Tiempo de espera agotado al obtener la invitación:", error);
                mostrarErrorConexion();
            } else if (error instanceof TypeError) {
                console.error("Error de red al obtener la invitación:", error);
                mostrarErrorConexion();
            } else if (error instanceof SyntaxError) {
                console.error("Respuesta no válida del servidor:", error);
                mostrarErrorConexion();
            } else {
                console.error("Error al obtener la invitación:", error);
            }
        })
        .finally(() => clearTimeout(timer));
}

// ===============================================================
//  MOSTRAR FORMULARIO
// ===============================================================
function renderFormulario() {
    document.getElementById("rotulo_invitacion").innerText = invitacion.name;

    contarInvitados();

    generarListadoInvitados();

    document.getElementById("formulario_respuesta").style.display = "block";
}

// ===============================================================
//  CONTAR ADULTOS / JÓVENES / NIÑOS
// ===============================================================
function contarInvitados() {
    let adultos = invitacion.guests.filter(g => getGuestTypeKey(g.guestType) === "Adult").length;
    let jovenes = invitacion.guests.filter(g => getGuestTypeKey(g.guestType) === "Youth").length;
    let menores = invitacion.guests.filter(g => getGuestTypeKey(g.guestType) === "Children").length;

    const invAdutlos = document.getElementById("invitados_mayores")
    if (invAdutlos) {
        if (adultos) {
            invAdutlos.innerText = `Adultos invitados: ${adultos}`
        }
        else {
            invAdutlos.style.display = "none";
        }
    }
    const invJovenes = document.getElementById("invitados_jovenes");
    if (invJovenes) {
        if (jovenes) {
            invJovenes.innerText = `Jóvenes invitados: ${jovenes}`;
        }
        else {
            invJovenes.style.display = "none";
        }
    }
    const invMenores = document.getElementById("invitados_menores");
    if (invMenores) {
        invMenores.innerText = menores === 0
            ? "Respetuosamente NO NIÑOS"
            : `Niños invitados: ${menores}`;
    }
}

// ===============================================================
//  GENERA EL LISTADO DE INVITADOS CON RADIOS
// ===============================================================
function generarListadoInvitados() {
    const cont = document.getElementById("guestList");
    if (!cont) return;

    // --- Limpia el contenedor ---
    cont.innerHTML = "";

    // --- Agrega la CABECERA solo una vez ---
    cont.innerHTML += `
        <div class="row fw-bold border-bottom">
            <div class="col-6">Invitado</div>
            <div class="col-6">¿Asistirá?</div>
        </div>
    `;

    // --- Agrega cada invitado ---
    invitacion.guests.forEach((g, index) => {
        cont.innerHTML += `
            <div class="row align-items-center py-2 fw-bold">
                <div class="col-6">
                    ${g.guestName || "Invitado " + (index + 1)}
                </div>

                <div class="col-6 d-flex flex-row gap-4 justify-content-center">

                    <!-- Sí -->
                    <label class="form-check d-flex align-items-center gap-1">
                        <input class="form-check-input"
                               type="radio"
                               name="guest_${index}"
                               value="19"
                               onchange="marcarAsistencia(${index}, 'Attend')"
                               ${getStatusKey(g.status) === 'Attend' ? "checked" : ""}>
                        Sí
                    </label>

                    <!-- No -->
                    <label class="form-check d-flex align-items-center gap-1">
                        <input class="form-check-input"
                               type="radio"
                               name="guest_${index}"
                               value="20"
                               onchange="marcarAsistencia(${index}, 'NotAttend')"
                               ${getStatusKey(g.status) === 'NotAttend' ? "checked" : ""}>
                        No
                    </label>

                </div>
            </div>

        `;
    });
}

// ===============================================================
//  ASIGNAR ASISTENCIA A UN INVITADO
// ===============================================================
function marcarAsistencia(index, valor) {
    invitacion.guests[index].status = valor;
}

// ===============================================================
//  SELECCIÓN "SÍ ASISTIRÉ / NO ASISTIRÉ"
// ===============================================================
function fn_asistencia(tipo) {
    console.log(invitacion)
    if (tipo === "n") {
        // NO ASISTIRÁ → todos status = NotAttend
        invitacion.status = 'NotAttend';
        invitacion.guests.forEach(g => g.status = 'NotAttend');

        document.getElementById("guestListContainer").style.display = "none";
    }

    if (tipo === "s") {
        // SÍ ASISTIRÁ → todos status = Attend
        invitacion.status = 'Attend';
        invitacion.guests.forEach(g => g.status = 'Attend');

        document.getElementById("guestListContainer").style.display = "block";
    }

    // Re-render para actualizar radios
    generarListadoInvitados();

    document.getElementById("btnConfirmar").style.display = "block";
}

// ===============================================================
//  ENVIAR RESPUESTA COMPLETA
// ===============================================================
async function sendRespuesta() {
    invitacion.comments = document.getElementById("texto_respuesta").value;
    const respuesta = {
        guests: invitacion.guests,
        code: invitacion.code,
        status: invitacion.status,
        comments: invitacion.comments,
    }
    try {
        const response = await fetch(`${apiUrl}/update-invitation`, {
            method: "PUT",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify(invitacion)
        });
        if (!response.ok) {
            throw new Error("No se pudo guardar la respuesta");
        }
        const data = await response.json();
        mostrarGracias(data);
        if (getStatusKey(invitacion.status) === 'Attend' && invitacion.event?.code) {
            cargarQRAndPdf(invitacion.code, invitacion.event.code)
        }
    } catch (error) {
        console.log(error)
        alert("Error al conectar con el servidor");
    }
}

// ===============================================================
//  MOSTRAR MENSAJE DE GRACIAS + QR
// ===============================================================
function mostrarGracias(data) {
    document.getElementById("formulario_respuesta").style.display = "none";
    if (getStatusKey(data.status) === 'Attend') {
        document.getElementById("mensaje_gracias_asistire").style.display = "block";
    } else {
        document.getElementById("mensaje_gracias_noasistire").style.display = "block";
    }

    if (data.result && data.result.qrCodeBase64) {
        document.getElementById("cont_descargaqr").style.display = "block";
        document.getElementById("img_qr").src = "data:image/png;base64," + data.result.qrCodeBase64;
    }
}

// ===============================================================
//  MOSTRAR ERROR SI NO EXISTE INVITACIÓN
// ===============================================================
function mostrarNoInvitacion(code) {
    document.getElementById("formulario_respuesta").style.display = "none";
    document.getElementById("mensaje_no_invitacion").style.display = "block";
    document.getElementById("codigo_no_encontrado").innerText = code;
}

// ===============================================================
//  MOSTRAR ERROR DE CONEXIÓN / TIMEOUT (con reintentar)
// ===============================================================
function mostrarErrorConexion() {
    let bloque = document.getElementById("mensaje_error_conexion");
    if (!bloque) {
        bloque = document.createElement("div");
        bloque.id = "mensaje_error_conexion";
        bloque.className = "mt-4 text-center";
        bloque.style.display = "block";
        bloque.innerHTML = `
            <h5 class="fw-bold text-warning">Problema de conexión</h5>
            <p>No pudimos comunicarnos con el servidor. Verifica tu conexión e inténtalo de nuevo.</p>
            <button class="btn btn-primary" onclick="cargarInvitacionDeNuevo()">Reintentar</button>
        `;
        const contenedor = document.getElementById("formulario_respuesta");
        if (contenedor && contenedor.parentNode) {
            contenedor.parentNode.insertBefore(bloque, contenedor);
        } else {
            document.body.appendChild(bloque);
        }
    } else {
        bloque.style.display = "block";
    }

    const formulario = document.getElementById("formulario_respuesta");
    if (formulario) formulario.style.display = "none";
}

function cargarInvitacionDeNuevo() {
    const params = new URLSearchParams(window.location.search);
    const code = params.get("codigo");
    if (!code) {
        mostrarNoInvitacion("SIN CÓDIGO");
        return;
    }

    const bloque = document.getElementById("mensaje_error_conexion");
    if (bloque) bloque.style.display = "none";

    const formulario = document.getElementById("formulario_respuesta");
    if (formulario) formulario.style.display = "block";

    cargarInvitacion(code);
}

function cargarQRAndPdf(codigoInvitacion, codigoEvento) {
    const qrUrl = `${apiUrl}/qr?codigo=${codigoInvitacion}&evento=${codigoEvento}`;
    const controller = new AbortController();
    const timer = setTimeout(() => controller.abort(), 15000);

    fetch(qrUrl, { signal: controller.signal })
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
        .catch(error => {
            if (error.name === "AbortError") {
                console.error("Tiempo de espera agotado al generar el QR:", error);
            } else {
                console.error("Error al generar QR:", error);
            }
        })
        .finally(() => clearTimeout(timer));
}
function base64ToBlob(base64, mimeType) {
    const bytes = atob(base64);
    const len = bytes.length;
    const buffer = new Uint8Array(len);
    for (let i = 0; i < len; i++) {
        buffer[i] = bytes.charCodeAt(i);
    }
    return new Blob([buffer], { type: mimeType });
}
