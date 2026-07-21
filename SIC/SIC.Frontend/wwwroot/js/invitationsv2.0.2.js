// ===============================================================
//  VARIABLES GLOBALES
// ===============================================================
let invitacion = null;
//const apiUrl = "https://localhost:7141/api/Invitations"; // Cambia si es necesario
const apiUrl = "https://invboxv-app.com/api/Invitations"; // Cambia si es necesario

// ===============================================================
//  CARGAR INVITACIÓN POR CÓDIGO EN LA URL
// ===============================================================
document.addEventListener("DOMContentLoaded", function () {
    const params = new URLSearchParams(window.location.search);
    const code = params.get("codigo");
    if (!code) {
        mostrarNoInvitacion("SIN CÓDIGO");
        return;
    }

    cargarInvitacion(code);
});

// ===============================================================
//  LLAMAR API PARA CARGAR INVITACIÓN
// ===============================================================
function cargarInvitacion(code) {
    try {
        fetch(`${apiUrl}/byCode/${code}`)
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
                if (invitacion.status === "Attend") {
                    mostrarGracias(data);
                    cargarQRAndPdf(invitacion.code, invitacion.event.code)
                }
                else if (invitacion.status === "NotAttend") {
                    mostrarGracias(data);
                }
                else if (data.status === "Pending") {
                    renderFormulario();
                }
            })
            .catch(error => {
                console.error("Error al obtener invitación:", error);
                mostrarNoInvitacion(code);
            });
    } catch (e) {
        mostrarNoInvitacion(code);
    }
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
    let adultos = invitacion.guests.filter(g => g.guestType === 1).length;
    let jovenes = invitacion.guests.filter(g => g.guestType === 2).length;
    let menores = invitacion.guests.filter(g => g.guestType === 3).length;

    document.getElementById("invitados_mayores").innerText = `Adultos invitados: ${adultos}`;

    const invJovenes = document.getElementById("invitados_jovenes");
    if (jovenes) {
        invJovenes.innerText = `Jóvenes invitados: ${jovenes}`;
    }
    else {
        invJovenes.style.display = "none";
    }
    document.getElementById("invitados_menores").innerText = menores === 0
        ? "Respetuosamente NO NIÑOS"
        : `Niños invitados: ${menores}`;
}

// ===============================================================
//  GENERA EL LISTADO DE INVITADOS CON RADIOS
// ===============================================================
function generarListadoInvitados() {
    const cont = document.getElementById("guestList");

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
                               onchange="marcarAsistencia(${index}, 19)"
                               ${Number(g.status) === 19 ? "checked" : ""}>
                        Sí
                    </label>

                    <!-- No -->
                    <label class="form-check d-flex align-items-center gap-1">
                        <input class="form-check-input"
                               type="radio"
                               name="guest_${index}"
                               value="20"
                               onchange="marcarAsistencia(${index}, 20)"
                               ${Number(g.status) === 20 ? "checked" : ""}>
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
    if (tipo === "n") {
        // NO ASISTIRÁ → todos status = 20
        invitacion.status = 20;
        invitacion.guests.forEach(g => g.status = 20);

        document.getElementById("guestListContainer").style.display = "none";
    }

    if (tipo === "s") {
        // SÍ ASISTIRÁ → todos status = 19
        invitacion.status = 19;
        invitacion.guests.forEach(g => g.status = 19);

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
        const data = await response.json();
        mostrarGracias(data);
        if (invitacion.status === 19) {
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
    if (data.status == 19) {
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

function cargarQRAndPdf(codigoInvitacion, codigoEvento) {
    console.log(codigoInvitacion, codigoEvento)
    const qrUrl = `${apiUrl}/qr?codigo=${codigoInvitacion}&evento=${codigoEvento}`;

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
function base64ToBlob(base64, mimeType) {
    const bytes = atob(base64);
    const len = bytes.length;
    const buffer = new Uint8Array(len);
    for (let i = 0; i < len; i++) {
        buffer[i] = bytes.charCodeAt(i);
    }
    return new Blob([buffer], { type: mimeType });
}