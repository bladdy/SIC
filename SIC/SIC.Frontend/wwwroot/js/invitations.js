// ==============================
// invitations.js
// ==============================
// ToDo: validacion visual que sino encuentra la invitacion, 
//  que le diga al invitado que esa invitacion no existe, favor de comunicarse con el Host o El Planner
// Función para obtener los parámetros de la URL
function getQueryVariable(variable) {
    var query = window.location.search.substring(1); // Obtener parte de la URL después del "?"
    var vars = query.split("&");
    for (var i = 0; i < vars.length; i++) {
        var pair = vars[i].split("=");
        if (pair[0] == variable) return pair[1];
    }
    return null; // Retorna null si no encuentra el parámetro
}

// Variable global para almacenar la invitación
let invitacionData = null;

// Obtener el código de la invitación de la URL
var codigo = getQueryVariable("codigo");
const boton = document.getElementById("btnConfirmar");

if (codigo) {
    console.log("Código:", codigo);
    obtenerDatosInvitacion(codigo);
} else {
    console.log("Parámetro 'codigo' no encontrado en la URL.");
}

// ==============================
// Función para mostrar u ocultar selects según asistencia
// ==============================
function fn_asistencia(respuesta) {

    if (respuesta === 's') {
        document.getElementById("seladultos").style.display = "block";
        document.getElementById("selmenores").style.display = "block";

        // Deshabilitar inicialmente el botón
        boton.disabled = true;

        // Escuchar cambios en los selects
        document.getElementById("confirmadosadultos").addEventListener("change", validarBotonEnviar);
        document.getElementById("confirmadosmenores").addEventListener("change", validarBotonEnviar);
    } else {
        document.getElementById("seladultos").style.display = "none";
        document.getElementById("selmenores").style.display = "none";

        // Habilitar el botón para "No asistiré"
        boton.disabled = false;
    }
}


// ==============================
// Validación del botón Enviar
// ==============================
function validarBotonEnviar() {
    const siAsistire = document.getElementById("siasistire").checked;
    const adultos = parseInt(document.getElementById("confirmadosadultos").value) || 0;
    const menores = parseInt(document.getElementById("confirmadosmenores").value) || 0;
    const boton = document.getElementById("btnConfirmar");

    if (siAsistire) {
        // Si elige "Sí asistiré", debe confirmar al menos 1 adulto o 1 menor
        if (adultos > 0 || menores > 0) {
            boton.disabled = false;
        } else {
            boton.disabled = true;
        }
    } else {
        // Si elige "No asistiré", puede enviar sin problema
        boton.disabled = false;
    }
}

// ==============================
// Función para generar QR
// ==============================
function fillQRCodeImage(codigo_inv, codigo_evento) {
    var qrUrl = `https://localhost:7141/api/Invitations/qr?codigo=${codigo_inv}&evento=${codigo_evento}`;
    //var qrUrl = `http://invboxv-app.com/api/Invitations/qr?codigo=${codigo_inv}&evento=${codigo_evento}`;

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

// ==============================
// Función para obtener los datos de la invitación
// ==============================
function obtenerDatosInvitacion(codigo) {
    // Deshabilitar inicialmente el botón
    boton.disabled = true;
    var apiUrl = `https://localhost:7141/api/Invitations/byCode/${codigo}`;
    //var apiUrl = `http://invboxv-app.com/api/Invitations/byCode/${codigo}`;


    fetch(apiUrl)
        .then(response => {
            if (!response.ok) throw new Error('Error al obtener los datos de la invitación');
            return response.json();
        })
        .then(data => {
            invitacionData = data; // Guardamos globalmente

            // Mostrar datos en HTML
            document.getElementById("rotulo_invitacion").textContent = data.name || "Sin Nombre";
            document.getElementById("invitados_mayores").textContent = "Cantidad de adultos: " + (data.numberAdults || 0);
            document.getElementById("invitados_menores").textContent = "Cantidad de menores: " + (data.numberChildren || 0);
            document.getElementById("mi_codigo_invitado").textContent = "Código Invitado: " + codigo;

            // Evento
            document.getElementById("evento_nombre").textContent = data.event.name || "Nombre Evento";
            document.getElementById("evento_subtitulo").textContent = data.event.subTitle || "";

            if (data.event) {
                let fechaEvento = new Date(data.event.date);
                document.getElementById("evento_fecha").textContent = "Fecha: " + fechaEvento.toLocaleDateString('es-MX', {
                    weekday: 'long',
                    year: 'numeric',
                    month: 'long',
                    day: 'numeric'
                });

                let horaFormateada = data.event.time;
                if (horaFormateada) {
                    let [hora, min, seg] = horaFormateada.split(':');
                    let fechaHora = new Date();
                    fechaHora.setHours(parseInt(hora), parseInt(min), parseInt(seg));
                    document.getElementById("evento_hora").textContent = "Hora: " + fechaHora.toLocaleTimeString('es-MX', {
                        hour: '2-digit',
                        minute: '2-digit'
                    });
                }
            }

            // Llenar selects según cantidad de invitados
            let selAdultos = document.getElementById("confirmadosadultos");
            selAdultos.innerHTML = '';
            for (let i = 0; i <= (data.numberAdults || 0); i++) {
                let opt = document.createElement("option");
                opt.value = i;
                opt.textContent = i === 0 ? "No asistiré" : i;
                selAdultos.appendChild(opt);
            }

            let selMenores = document.getElementById("confirmadosmenores");
            selMenores.innerHTML = '';
            for (let i = 0; i <= (data.numberChildren || 0); i++) {
                let opt = document.createElement("option");
                opt.value = i;
                opt.textContent = i === 0 ? "No asistirán menores" : i;
                selMenores.appendChild(opt);
            }

            // Mostrar botón confirmar
            document.getElementById("btnConfirmar").style.display = "block";

            // Generar QR inicial
            //fillQRCodeImage(codigo, data.event.code);
        })
        .catch(error => console.error("Error al cargar los datos de la invitación:", error));
}

// ==============================
// Función para enviar confirmación
// ==============================
function sendRespuesta() {
    boton.disabled = false;
    if (!invitacionData) {
        console.error("Datos de invitación no cargados");
        return;
    }

    var codigo = invitacionData.code;
    var asistencia = document.getElementById("siasistire").checked;
    var adultosConfirmados = parseInt(document.getElementById("confirmadosadultos").value) || 0;
    var menoresConfirmados = parseInt(document.getElementById("confirmadosmenores").value) || 0;
    var mensaje = document.getElementById("texto_respuesta").value;

    var payload = {
        CodigoInvitacion: codigo,
        Nombre: invitacionData.name,
        CantidadDeMayores: invitacionData.numberAdults,
        CantidadDeMenores: invitacionData.numberChildren,
        ConfirmacionAsistencia: asistencia,
        ConfirmadosAdultos: adultosConfirmados,
        ConfirmadosMenores: menoresConfirmados,
        Mensaje: mensaje
    };
    var apiUrl = "https://localhost:7141/api/Invitations/confirm";
    //var apiUrl = "http://invboxv-app.com/api/Invitations/confirm";
    fetch(apiUrl, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(payload)
    })
        .then(response => {
            if (!response.ok) throw new Error("Error al enviar confirmación");
            return response.json();
        })
        .then(data => {
            console.log("Confirmación enviada:", data);

            // Ocultar formulario
            document.getElementById("formulario_respuesta").style.display = "none";

            // Mostrar mensaje de gracias
            let mensajeHtml = document.getElementById("mensaje_gracias");
            mensajeHtml.style.display = "block";

            // Generar y mostrar QR **solo después de enviar**
            fillQRCodeImage(codigo, invitacionData.event.code);
            document.getElementById("cont_descargaqr").style.display = "block";
        })
        .catch(error => {
            console.error("Error al enviar la confirmación:", error);
            alert("Ocurrió un error al enviar tu respuesta. Intenta de nuevo.");
        });
}

function descargarQR() {
    const qrImage = document.getElementById("mi_acceso_qr");
    if (!qrImage.src) {
        alert("El QR aún no está disponible");
        return;
    }

    // Crear un enlace temporal
    const enlace = document.createElement("a");
    enlace.href = qrImage.src;
    enlace.download = "INV - " +`${invitacionData.event.name}.png`; // Nombre del archivo
    document.body.appendChild(enlace);
    enlace.click();
    document.body.removeChild(enlace);
}