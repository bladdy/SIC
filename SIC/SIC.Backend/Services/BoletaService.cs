using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using QRCoder;
using SkiaSharp;
using SIC.Shared.DTOs;

namespace SIC.Backend.Services
{
    public class BoletaService
    {
        public (byte[] pdfBytes, byte[] pngBytes) GenerarBoleta(BoletaInvitacionDto data)
        {
            // 🧩 Generar código QR
            var qrGenerator = new QRCodeGenerator();
            var qrCodeData = qrGenerator.CreateQrCode(data.CodigoQr, QRCodeGenerator.ECCLevel.Q);
            var qrCode = new PngByteQRCode(qrCodeData);
            var qrBytes = qrCode.GetGraphic(25); // más alto = QR más grande

            // 🖼️ Configurar tamaño base del QR y márgenes
            float qrSize = 600;
            float sideMargin = 20;
            float contentWidth = qrSize + sideMargin * 2;

            // 📏 Calcular alto dinámico según cantidad de líneas de texto
            int totalLines = 5; // título + líneas de info
            float lineHeight = 80;
            float topMargin = 60;
            float qrTop = topMargin + 100;
            float contentHeight = qrTop + qrSize + (lineHeight * totalLines) + 100;

            // 🖌️ Crear lienzo con fondo blanco
            using var surface = SKSurface.Create(new SKImageInfo((int)contentWidth, (int)contentHeight));
            var canvas = surface.Canvas;
            canvas.Clear(SKColors.White);

            // 🖋️ Fuentes principales
            using var fontTitle = new SKPaint
            {
                TextSize = 46,
                IsAntialias = true,
                Color = new SKColor(30, 30, 30),
                Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold)
            };

            using var fontSubtitle = new SKPaint
            {
                TextSize = 34,
                IsAntialias = true,
                Color = new SKColor(90, 90, 90),
                Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Italic)
            };

            using var fontText = new SKPaint
            {
                TextSize = 40,
                IsAntialias = true,
                Color = new SKColor(50, 50, 50),
                Typeface = SKTypeface.FromFamilyName("Arial")
            };

            // 🧾 Título centrado arriba
            var title = data.NombreEvento?.ToUpperInvariant() ?? "EVENTO";
            var titleWidth = fontTitle.MeasureText(title);
            float titleY = topMargin + 20;
            canvas.DrawText(title, (contentWidth - titleWidth) / 2, titleY, fontTitle);

            // ✳️ Subtítulo centrado debajo del título
            var subtitle = data.SubNombre?.Trim();
            if (!string.IsNullOrEmpty(subtitle))
            {
                var subtitleWidth = fontSubtitle.MeasureText(subtitle);
                float subtitleY = titleY + 50;
                canvas.DrawText(subtitle, (contentWidth - subtitleWidth) / 2, subtitleY, fontSubtitle);
            }

            // 🧩 QR centrado debajo del título
            using var qrImage = SKImage.FromEncodedData(qrBytes);
            float qrX = (contentWidth - qrSize) / 2;
            canvas.DrawImage(qrImage, new SKRect(qrX, qrTop, qrX + qrSize, qrTop + qrSize));

            // 🧠 Código QR debajo como texto (centrado)
            var codigoTexto = $"CÓDIGO: {data.CodigoQr}";
            var codigoWidth = fontText.MeasureText(codigoTexto);
            float codigoX = (contentWidth - codigoWidth) / 2;
            float codigoY = qrTop + qrSize + 40;
            canvas.DrawText(codigoTexto, codigoX, codigoY, fontText);

            // 📋 Información del evento
            float infoStartY = qrTop + qrSize + 120;
            canvas.DrawText($"Invitado: {data.NombreInvitado}", sideMargin, infoStartY, fontText);
            canvas.DrawText($"Fecha: {data.Fecha:dd/MM/yyyy}", sideMargin, infoStartY + lineHeight, fontText);
            canvas.DrawText($"Hora: {data.Hora:hh:mm tt}", sideMargin, infoStartY + lineHeight * 2, fontText);

            // 👥 Texto de invitados con pluralización
            var textoInvitados = $"{data.Adultos} {(data.Adultos == 1 ? "Adulto" : "Adultos")}";
            if (data.Niños > 0)
                textoInvitados += $"  {data.Niños} {(data.Niños == 1 ? "Niño" : "Niños")}";

            canvas.DrawText($"Invitado(s): {textoInvitados}", sideMargin, infoStartY + lineHeight * 3, fontText);

            canvas.Flush();

            // 📤 Exportar PNG
            using var image = surface.Snapshot();
            using var dataPng = image.Encode(SKEncodedImageFormat.Png, 100);
            var pngBytes = dataPng.ToArray();

            // 📄 Crear PDF ajustado al contenido real
            using var ms = new MemoryStream();
            var pdf = new PdfDocument();
            var page = pdf.AddPage();
            page.Width = contentWidth;
            page.Height = contentHeight;

            using (var gfx = XGraphics.FromPdfPage(page))
            {
                using var imgStream = new MemoryStream(pngBytes);
                var xImage = XImage.FromStream(() => imgStream);
                gfx.DrawImage(xImage, 0, 0, contentWidth, contentHeight);
            }

            pdf.Save(ms, false);
            var pdfBytes = ms.ToArray();

            return (pdfBytes, pngBytes);
        }
    }
}
