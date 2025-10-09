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
            // Generar código QR
            var qrGenerator = new QRCodeGenerator();
            var qrCodeData = qrGenerator.CreateQrCode(data.CodigoQr, QRCodeGenerator.ECCLevel.Q);
            var qrCode = new PngByteQRCode(qrCodeData);
            var qrBytes = qrCode.GetGraphic(20);

            // Crear lienzo de imagen formal
            using var surface = SKSurface.Create(new SKImageInfo(800, 500));
            var canvas = surface.Canvas;
            canvas.Clear(SKColors.White);

            // Tipografías
            using var fontTitle = new SKPaint
            {
                TextSize = 36,
                IsAntialias = true,
                Color = SKColors.Black,
                Typeface = SKTypeface.FromFamilyName("Times New Roman", SKFontStyle.Bold)
            };

            using var fontText = new SKPaint
            {
                TextSize = 22,
                IsAntialias = true,
                Color = SKColors.DarkSlateGray,
                Typeface = SKTypeface.FromFamilyName("Georgia")
            };

            // Marco dorado elegante
            var border = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = SKColors.Goldenrod,
                StrokeWidth = 6
            };
            canvas.DrawRoundRect(new SKRect(20, 20, 780, 480), 20, 20, border);

            // Título
            canvas.DrawText("BOLETA DE INVITACIÓN", 200, 70, fontTitle);

            // Texto principal
            canvas.DrawText($"Evento: {data.NombreEvento}", 60, 130, fontText);
            canvas.DrawText($"Invitado: {data.NombreInvitado}", 60, 170, fontText);
            canvas.DrawText($"Fecha: {data.Fecha:dd/MM/yyyy}", 60, 210, fontText);
            canvas.DrawText($"Hora: {data.Hora}", 60, 250, fontText);
            canvas.DrawText($"Lugar: {data.Lugar}", 60, 290, fontText);
            canvas.DrawText($"Cantidad de Personas: {data.CantidadPersonas}", 60, 330, fontText);
            canvas.DrawText($"Mesa Asignada: {data.MesaAsignada}", 60, 370, fontText);

            // QR en esquina inferior derecha
            using var qrImage = SKImage.FromEncodedData(qrBytes);
            canvas.DrawImage(qrImage, new SKRect(620, 320, 760, 460));

            canvas.Flush();

            // Exportar a PNG
            using var image = surface.Snapshot();
            using var dataPng = image.Encode(SKEncodedImageFormat.Png, 100);
            var pngBytes = dataPng.ToArray();

            // Crear PDF con la imagen PNG
            using var ms = new MemoryStream();
            var pdf = new PdfDocument();
            var page = pdf.AddPage();
            page.Width = 800;
            page.Height = 500;

            using (var gfx = XGraphics.FromPdfPage(page))
            {
                using var imgStream = new MemoryStream(pngBytes);
                var xImage = XImage.FromStream(() => imgStream);
                gfx.DrawImage(xImage, 0, 0, 800, 500);
            }

            pdf.Save(ms, false);
            var pdfBytes = ms.ToArray();

            return (pdfBytes, pngBytes);
        }
    }
}