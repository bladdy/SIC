// ===============================================================
//  VARIABLES GLOBALES
// ===============================================================
let invitacion = null;

//const apiUrl = "https://localhost:7141/api/Invitations";
const apiUrl = "https://invboxv-app.com/api/Invitations";

// ===============================================================
//  DETECTAR IDIOMA DEL NAVEGADOR
// ===============================================================
const browserLang = navigator.language.toLowerCase().startsWith("en")
    ? "en"
    : "es";

// ===============================================================
//  TRADUCCIONES
// ===============================================================
const translations = {
    es: {
        chooseResponse: "Elija su respuesta:",
        yesAttend: "Sí Asistiré",
        noAttend: "No Asistiré",

        dedication: "Dedicatoria",
        dedicationPlaceholder: "Escribe aquí tu dedicatoria...",

        sendResponse: "Enviar Respuesta",

        thankYou: "¡Gracias por confirmar tu asistencia!",

        invitationNotFound: "No se encontró la invitación",

        invitationNotFoundDesc:
            "No pudimos encontrar una invitación válida con el código:",

        invitationNotFoundContact:
            "Verifica que el enlace de la invitación sea correcto o contacta al organizador del evento.",

        qrMessage:
            "Recuerda presentar el siguiente QR en la entrada del evento.",

        downloadQr: "Descargar QR",
        downloadPdf: "Descargar boleta PDF",

        invitedAdults: "Adultos invitados",
        invitedYoung: "Jóvenes invitados",
        invitedChildren: "Niños invitados",

        noChildren: "Respetuosamente NO NIÑOS",

        guest: "Invitado",
        willAttend: "¿Asistirá?",

        yes: "Sí",
        no: "No",

        connectionError: "Error al conectar con el servidor",

        invitation: "Invitación"
    },

    en: {
        chooseResponse: "Choose your response:",
        yesAttend: "Yes, I will attend",
        noAttend: "No, I will not attend",

        dedication: "Dedication",
        dedicationPlaceholder: "Write your dedication here...",

        sendResponse: "Send Response",

        thankYou: "Thank you for confirming your attendance!",

        invitationNotFound: "Invitation not found",

        invitationNotFoundDesc:
            "We could not find a valid invitation with the code:",

        invitationNotFoundContact:
            "Please verify the invitation link or contact the event organizer.",

        qrMessage:
            "Remember to present the following QR code at the event entrance.",

        downloadQr: "Download QR",
        downloadPdf: "Download PDF Ticket",

        invitedAdults: "Invited adults",
        invitedYoung: "Invited teenagers",
        invitedChildren: "Invited children",

        noChildren: "Respectfully NO CHILDREN",

        guest: "Guest",
        willAttend: "Will attend?",

        yes: "Yes",
        no: "No",

        connectionError: "Error connecting to server",

        invitation: "Invitation"
    }
};

// ===============================================================
//  TRADUCCIÓN ACTIVA
// ===============================================================
const t = translations[browserLang];

// ===============================================================
//  CARGAR DOCUMENTO
// ===============================================================
document.addEventListener("DOMContentLoaded", function () {
    aplicarTraducciones();

    const params = new URLSearchParams(window.location.search);
    const code = params.get("codigo");

    if (!code) {
        mostrarNoInvitacion("SIN CÓDIGO");
        return;
    }

    cargarInvitacion(code);
});

// ===============================================================
//  APLICAR TRADUCCIONES AL HTML
// ===============================================================
function aplicarTraducciones() {
    document.title = t.invitation;

    setText("lbl_elegir_respuesta", t.chooseResponse);

    setText("lbl_si_asistire", t.yesAttend);

    setText("lbl_no_asistire", t.noAttend);

    setText("lbl_dedicatoria", t.dedication);

    setText("btnConfirmar", t.sendResponse);

    setText("lbl_gracias", t.thankYou);

    setText("lbl_no_invitacion", t.invitationNotFound);

    setText(
        "lbl_no_invitacion_desc",
        t.invitationNotFoundDesc
    );

    setText(
        "lbl_no_invitacion_contacto",
        t.invitationNotFoundContact
    );

    setText(
        "mensaje_qr_aviso",
        t.qrMessage
    );

    setText(
        "mi_enlace_descarga_qr",
        t.downloadQr
    );

    const textarea = document.getElementById("texto_respuesta");

    if (textarea) {
        textarea.placeholder = t.dedicationPlaceholder;
    }
}

// ===============================================================
//  HELPER PARA CAMBIAR TEXTO
// ===============================================================
function setText(id, value) {
    const el = document.getElementById(id);

    if (el) {
        el.innerText = value;
    }
}

// ===============================================================
//  CARGAR INVITACIÓN
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

                if (invitacion.status === 19) {
                    mostrarGracias(data);

                    cargarQRAndPdf(
                        invitacion.code,
                        invitacion.event.code
                    );
                }
                else if (invitacion.status === 20) {
                    mostrarGracias(data);
                }
                else if (data.status === 2) {
                    renderFormulario();
                }
            })
            .catch(error => {
                console.error(
                    "Error al obtener invitación:",
                    error
                );

                mostrarNoInvitacion(code);
            });
    } catch (e) {
        mostrarNoInvitacion(code);
    }
}

// ===============================================================
//  RENDER FORMULARIO
// ===============================================================
function renderFormulario() {
    document.getElementById(
        "rotulo_invitacion"
    ).innerText = invitacion.name;

    contarInvitados();

    generarListadoInvitados();

    document.getElementById(
        "formulario_respuesta"
    ).style.display = "block";
}

// ===============================================================
//  CONTAR INVITADOS
// ===============================================================
function contarInvitados() {
    let adultos =
        invitacion.guests.filter(
            g => g.guestType === 1
        ).length;

    let jovenes =
        invitacion.guests.filter(
            g => g.guestType === 2
        ).length;

    let menores =
        invitacion.guests.filter(
            g => g.guestType === 3
        ).length;

    document.getElementById(
        "invitados_mayores"
    ).innerText =
        `${t.invitedAdults}: ${adultos}`;

    const invJovenes =
        document.getElementById(
            "invitados_jovenes"
        );

    if (jovenes) {
        invJovenes.innerText =
            `${t.invitedYoung}: ${jovenes}`;
    }
    else {
        invJovenes.style.display = "none";
    }

    document.getElementById(
        "invitados_menores"
    ).innerText =
        menores === 0
            ? t.noChildren
            : `${t.invitedChildren}: ${menores}`;
}

// ===============================================================
//  GENERAR LISTADO DE INVITADOS
// ===============================================================
function generarListadoInvitados() {
    const cont =
        document.getElementById("guestList");

    cont.innerHTML = "";

    cont.innerHTML += `
        <div class="row fw-bold border-bottom">
            <div class="col-6">${t.guest}</div>
            <div class="col-6">${t.willAttend}</div>
        </div>
    `;

    invitacion.guests.forEach((g, index) => {
        cont.innerHTML += `
            <div class="row align-items-center py-2 fw-bold">

                <div class="col-6">
                    ${g.guestName || t.guest + " " + (index + 1)}
                </div>

                <div class="col-6 d-flex flex-row gap-4 justify-content-center">

                    <label class="form-check d-flex align-items-center gap-1">

                        <input
                            class="form-check-input"
                            type="radio"
                            name="guest_${index}"
                            value="19"
                            onchange="marcarAsistencia(${index}, 19)"
                            ${Number(g.status) === 19 ? "checked" : ""}
                        >

                        ${t.yes}

                    </label>

                    <label class="form-check d-flex align-items-center gap-1">

                        <input
                            class="form-check-input"
                            type="radio"
                            name="guest_${index}"
                            value="20"
                            onchange="marcarAsistencia(${index}, 20)"
                            ${Number(g.status) === 20 ? "checked" : ""}
                        >

                        ${t.no}

                    </label>

                </div>
            </div>
        `;
    });
}

// ===============================================================
//  MARCAR ASISTENCIA
// ===============================================================
function marcarAsistencia(index, valor) {
    invitacion.guests[index].status = valor;
}

// ===============================================================
//  SÍ / NO ASISTIRÁ
// ===============================================================
function fn_asistencia(tipo) {
    if (tipo === "n") {
        invitacion.status = 20;

        invitacion.guests.forEach(
            g => g.status = 20
        );

        document.getElementById(
            "guestListContainer"
        ).style.display = "none";
    }

    if (tipo === "s") {
        invitacion.status = 19;

        invitacion.guests.forEach(
            g => g.status = 19
        );

        document.getElementById(
            "guestListContainer"
        ).style.display = "block";
    }

    generarListadoInvitados();

    document.getElementById(
        "btnConfirmar"
    ).style.display = "block";
}

// ===============================================================
//  ENVIAR RESPUESTA
// ===============================================================
async function sendRespuesta() {
    invitacion.comments =
        document.getElementById(
            "texto_respuesta"
        ).value;

    try {
        const response =
            await fetch(
                `${apiUrl}/update-invitation`,
                {
                    method: "PUT",
                    headers: {
                        "Content-Type": "application/json"
                    },
                    body: JSON.stringify(invitacion)
                }
            );

        const data = await response.json();

        mostrarGracias(data);

        if (invitacion.status === 19) {
            cargarQRAndPdf(
                invitacion.code,
                invitacion.event.code
            );
        }
    } catch (error) {
        console.log(error);

        alert(t.connectionError);
    }
}

// ===============================================================
//  MOSTRAR GRACIAS
// ===============================================================
function mostrarGracias(data) {
    document.getElementById(
        "formulario_respuesta"
    ).style.display = "none";

    document.getElementById(
        "mensaje_gracias"
    ).style.display = "block";

    if (
        data.result &&
        data.result.qrCodeBase64
    ) {
        document.getElementById(
            "cont_descargaqr"
        ).style.display = "block";

        document.getElementById(
            "img_qr"
        ).src =
            "data:image/png;base64," +
            data.result.qrCodeBase64;
    }
}

// ===============================================================
//  MOSTRAR NO INVITACIÓN
// ===============================================================
function mostrarNoInvitacion(code) {
    document.getElementById(
        "formulario_respuesta"
    ).style.display = "none";

    document.getElementById(
        "mensaje_no_invitacion"
    ).style.display = "block";

    document.getElementById(
        "codigo_no_encontrado"
    ).innerText = code;
}

// ===============================================================
//  CARGAR QR Y PDF
// ===============================================================
function cargarQRAndPdf(
    codigoInvitacion,
    codigoEvento
) {
    const qrUrl =
        `${apiUrl}/qr?codigo=${codigoInvitacion}&evento=${codigoEvento}`;

    fetch(qrUrl)

        .then(response => {
            if (!response.ok) {
                throw new Error(
                    "Error al generar la boleta"
                );
            }

            return response.json();
        })

        .then(data => {
            const imgQr =
                document.getElementById("img_qr");

            if (
                !data.qrBase64 ||
                data.qrBase64 === "null"
            ) {
                console.error(
                    "El backend no devolvió un QR válido"
                );

                imgQr.src = "";

                return;
            }

            imgQr.src =
                `data:image/png;base64,${data.qrBase64}`;

            const enlaceDescarga =
                document.getElementById(
                    "mi_enlace_descarga_qr"
                );

            if (
                data.pdfBase64 &&
                data.pdfBase64 !== "null"
            ) {
                const pdfBlob =
                    base64ToBlob(
                        data.pdfBase64,
                        "application/pdf"
                    );

                const url =
                    URL.createObjectURL(pdfBlob);

                enlaceDescarga.href = url;

                enlaceDescarga.download =
                    `Boleta_${codigoInvitacion}.pdf`;

                enlaceDescarga.textContent =
                    t.downloadPdf;
            }

            document.getElementById(
                "cont_descargaqr"
            ).style.display = "block";
        })

        .catch(error =>
            console.error(
                "Error al generar QR:",
                error
            )
        );
}

// ===============================================================
//  BASE64 → BLOB
// ===============================================================
function base64ToBlob(base64, mimeType) {
    const bytes = atob(base64);

    const len = bytes.length;

    const buffer = new Uint8Array(len);

    for (let i = 0; i < len; i++) {
        buffer[i] = bytes.charCodeAt(i);
    }

    return new Blob(
        [buffer],
        { type: mimeType }
    );
}