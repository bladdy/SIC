using iTextSharp.text;
using iTextSharp.text.pdf;
using QRCoder;
using SIC.Frontend.Helpers;
using SIC.Shared.DTOs;

namespace SIC.Backend.Services
{
    public class BoletaService
    {
        public (byte[] pdfBytes, byte[] pngBytes) GenerarBoleta(BoletaInvitacionDto data)
        {
            // ============================================================
            // 1️⃣ GENERAR QR COMO PNG
            // ============================================================
            var qrGenerator = new QRCodeGenerator();
            var qrCodeData = qrGenerator.CreateQrCode(data.CodigoQr, QRCodeGenerator.ECCLevel.Q);
            var qrCode = new PngByteQRCode(qrCodeData);
            var qrBytes = qrCode.GetGraphic(25); // tamaño del QR

            // ============================================================
            // 2️⃣ CREAR DOCUMENTO PDF
            // ============================================================
            using var msPdf = new MemoryStream();
            using var document = new Document(PageSize.Letter, 40, 40, 40, 40);
            PdfWriter.GetInstance(document, msPdf);

            document.Open();

            // Fuente principal
            var fontTitle = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 22, BaseColor.Black);
            var fontSubtitle = FontFactory.GetFont(FontFactory.HELVETICA_OBLIQUE, 18, BaseColor.DarkGray);
            var fontText = FontFactory.GetFont(FontFactory.HELVETICA, 16, BaseColor.Black);

            // ============================================================
            // 3️⃣ AGREGAR TÍTULO CENTRADO
            // ============================================================
            var title = new Paragraph(data.NombreEvento?.ToUpper() ?? "EVENTO", fontTitle)
            {
                Alignment = Element.ALIGN_CENTER,
                SpacingAfter = 10
            };
            document.Add(title);

            if (!string.IsNullOrWhiteSpace(data.SubNombre))
            {
                var subtitle = new Paragraph(data.SubNombre, fontSubtitle)
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingAfter = 20
                };
                document.Add(subtitle);
            }

            // ============================================================
            // 4️⃣ INSERTAR QR CENTRADO
            // ============================================================
            var qrImage = Image.GetInstance(qrBytes);
            qrImage.Alignment = Image.ALIGN_CENTER;
            qrImage.ScaleAbsolute(250, 250); // tamaño QR
            document.Add(qrImage);

            // Código QR debajo del QR
            var codeText = new Paragraph($"CÓDIGO: {data.CodigoQr}", fontText)
            {
                Alignment = Element.ALIGN_CENTER,
                SpacingAfter = 25
            };
            document.Add(codeText);

            // ============================================================
            // 5️⃣ INFORMACIÓN DEL EVENTO
            // ============================================================
            var info = new PdfPTable(1)
            {
                WidthPercentage = 100
            };

            void AddInfo(string text)
            {
                var cell = new PdfPCell(new Phrase(text, fontText))
                {
                    Border = Rectangle.NO_BORDER,
                    PaddingBottom = 10
                };
                info.AddCell(cell);
            }

            AddInfo($"Invitado: {data.NombreInvitado}");
            AddInfo($"Fecha: {data.Fecha:dd/MM/yyyy}");
            //AddInfo($"Hora: {data.Hora:hh:mm tt}");

            var invitados = $"{data.Adultos} {(data.Adultos == 1 ? "Adulto" : "Adultos")}";
            if (data.Jovenes > 0)
                invitados += $"  {data.Jovenes} {(data.Jovenes == 1 ? "Joven" : "Jovenes")}";
            if (data.Niños > 0)
                invitados += $"  {data.Niños} {(data.Niños == 1 ? "Niño" : "Niños")}";

            AddInfo($"Invitado(s): {invitados}");

            document.Add(info);

            // ============================================================
            // 6️⃣ CERRAR PDF Y RETORNAR BYTES
            // ============================================================
            document.Close();

            var pdfBytes = msPdf.ToArray();
            return (pdfBytes, qrBytes);
        }

        public async Task<byte[]> GenerarBoletaEstiloCard(BoletaInvitacionDto data, byte[] qrBytes)
        {
            byte[]? coverBytes = null;
            using var ms = new MemoryStream();
            using var document = new Document(PageSize.A4, 40, 40, 40, 40);
            PdfWriter.GetInstance(document, ms);
            document.Open();

            // ==============================
            // 🎨 COLORES tipo Bootstrap
            // ==============================
            var darkColor = new BaseColor(33, 37, 41);      // btn-dark
            var mutedColor = new BaseColor(108, 117, 125);  // text-muted
            var borderColor = new BaseColor(222, 226, 230); // border

            // ==============================
            // 🔠 FUENTES
            // ==============================
            var fontTitle = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14, BaseColor.Black);
            var fontMuted = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 11, mutedColor);
            var fontNormal = FontFactory.GetFont(FontFactory.HELVETICA, 12, BaseColor.Black);
            var fontSmall = FontFactory.GetFont(FontFactory.HELVETICA, 9, mutedColor);

            // ==============================
            // 📦 CONTENEDOR CARD
            // ==============================
            var cardTable = new PdfPTable(1)
            {
                WidthPercentage = 60
            };

            var cardCell = new PdfPCell
            {
                BorderColor = borderColor,
                BorderWidth = 1,
                Padding = 15,
                BackgroundColor = BaseColor.White
            };

            // ==============================
            // 🖼 IMAGEN SUPERIOR
            // ==============================
            if (data.CoverImageBytes != null)
            {
                coverBytes = await DescargarImagenAsync(data.CoverImageBytes);
                var coverImg = Image.GetInstance(coverBytes);
                coverImg.ScaleToFit(400, 200);
                coverImg.Alignment = Element.ALIGN_CENTER;
                cardCell.AddElement(coverImg);
                cardCell.AddElement(new Paragraph(" "));
            }

            // ==============================
            // 📝 TEXTOS CENTRADOS
            // ==============================
            void AddCentered(string text, Font font, int spacing = 5)
            {
                var p = new Paragraph(text, font)
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingAfter = spacing
                };
                cardCell.AddElement(p);
            }

            AddCentered("GRACIAS POR CONFIRMAR TU ASISTENCIA AL EVENTO DE:", fontMuted, 10);
            AddCentered(data.SubNombre?.ToUpper() ?? "", fontTitle, 0);
            AddCentered(data.NombreEvento?.ToUpper() ?? "", fontTitle, 0);
            AddCentered(data.Fecha.FormatearFechaLargaEspanol(), fontNormal, 15);

            // ==============================
            // 🔳 QR CENTRADO
            // ==============================
            var qrImage = Image.GetInstance(qrBytes);
            qrImage.ScaleAbsolute(120, 120);
            qrImage.Alignment = Element.ALIGN_CENTER;
            cardCell.AddElement(qrImage);

            AddCentered(" ", fontNormal, 5);

            // ==============================
            // 👥 LISTA DE INVITADOS
            // ==============================
            AddCentered("LISTA DE INVITADOS", fontMuted, 0);
            AddCentered(data.NombreInvitado, fontTitle, 0);

            foreach (var item in data.Guests)
            {
                AddCentered(item, fontTitle,0);
            }

            // ==============================
            // 📄 TEXTO INFORMATIVO
            // ==============================
            var infoText = new Paragraph(
                "Este código QR es tu acceso al evento.\nDescárgalo y preséntalo en la entrada.\nEste QR solo se escanea con nuestra App.",
                fontSmall)
            {
                Alignment = Element.ALIGN_CENTER,
                SpacingBefore = 10
            };

            cardCell.AddElement(infoText);

            cardTable.AddCell(cardCell);
            document.Add(cardTable);

            document.Close();
            return ms.ToArray();
        }

        private async Task<byte[]?> DescargarImagenAsync(string url)
        {
            try
            {
                using var httpClient = new HttpClient();
                return await httpClient.GetByteArrayAsync(url);
            }
            catch
            {
                return null; // si falla, no rompe el PDF
            }
        }

        public byte[] GenerarPdfQrs(string evento, List<string> codigosQr)
        {
            using var ms = new MemoryStream();
            using var document = new Document(PageSize.Letter, 25, 25, 25, 25);
            PdfWriter.GetInstance(document, ms);

            document.Open();

            var fontTitle = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16);
            var fontCode = FontFactory.GetFont(FontFactory.HELVETICA, 8);

            // 🔹 Título
            document.Add(new Paragraph(evento.ToUpper(), fontTitle)
            {
                Alignment = Element.ALIGN_CENTER,
                SpacingAfter = 15
            });

            int index = 0;

            while (index < codigosQr.Count)
            {
                // 🧱 4 columnas x 5 filas = 20 QR
                var table = new PdfPTable(4)
                {
                    WidthPercentage = 100
                };

                table.SetWidths(new float[] { 1, 1, 1, 1 });

                for (int i = 0; i < 20 && index < codigosQr.Count; i++, index++)
                {
                    var urlBase = "https://invboxv-app.com/photo-event";

                    var contenidoQr = $"{urlBase}/{evento}/{codigosQr[index]}";

                    var qrBytes = GenerateQrPng(contenidoQr);

                    var qrImage = Image.GetInstance(qrBytes);
                    qrImage.ScaleAbsolute(100, 100);
                    qrImage.Alignment = Element.ALIGN_CENTER;

                    var cell = new PdfPCell
                    {
                        Border = Rectangle.NO_BORDER,
                        HorizontalAlignment = Element.ALIGN_CENTER,
                        PaddingBottom = 5
                    };

                    cell.AddElement(qrImage);
                    cell.AddElement(new Paragraph(codigosQr[index], fontCode)
                    {
                        Alignment = Element.ALIGN_CENTER,
                    });

                    table.AddCell(cell);
                }

                document.Add(table);

                if (index < codigosQr.Count)
                    document.NewPage();
            }

            document.Close();
            return ms.ToArray();
        }

        // ============================
        // 🔹 QR PNG
        // ============================
        private static byte[] GenerateQrPng(string codigo)
        {
            using var generator = new QRCodeGenerator();
            using var data = generator.CreateQrCode(codigo, QRCodeGenerator.ECCLevel.Q);
            using var qr = new PngByteQRCode(data);

            return qr.GetGraphic(20);
        }

        internal object GenerarBoletaEstiloCard(BoletaInvitacionDto dto, string v)
        {
            throw new NotImplementedException();
        }
    }
}