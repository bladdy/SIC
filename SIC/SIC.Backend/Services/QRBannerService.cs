using iTextSharp.text;
using iTextSharp.text.pdf;
using QRCoder;
using SIC.Shared.Entities;

using Image = iTextSharp.text.Image;
using Font = iTextSharp.text.Font;

namespace SIC.Backend.Services;

public class QRBannerService
{
    //private const string BASE_URL = "https://localhost:7174/";
    private const string BASE_URL = "https://invboxv-app.com/";

    public async Task<byte[]> QRBanner(Event data)
    {
        using MemoryStream stream = new();

        // =====================================================
        // A4
        // =====================================================

        Document document = new(
            PageSize.A4,
            0,
            0,
            0,
            0);

        PdfWriter writer = PdfWriter.GetInstance(document, stream);

        document.Open();

        PdfContentByte canvas = writer.DirectContent;

        // =====================================================
        // CONFIGURACION 4 POR PAGINA
        // =====================================================

        float pageWidth = PageSize.A4.Width;
        float pageHeight = PageSize.A4.Height;

        // 2 columnas x 2 filas
        float bannerWidth = pageWidth / 2;
        float bannerHeight = pageHeight / 2;

        // Posiciones
        List<(float x, float y)> positions =
        [
            (0, bannerHeight),                 // Arriba izquierda
            (bannerWidth, bannerHeight),       // Arriba derecha
            (0, 0),                            // Abajo izquierda
            (bannerWidth, 0)                   // Abajo derecha
        ];

        foreach (var position in positions)
        {
            DrawBanner(
                document,
                writer,
                canvas,
                data,
                position.x,
                position.y,
                bannerWidth,
                bannerHeight);
        }

        document.Close();

        return stream.ToArray();
    }

    private void DrawBanner(
        Document document,
        PdfWriter writer,
        PdfContentByte canvas,
        Event data,
        float x,
        float y,
        float width,
        float height)
    {
        // =====================================================
        // FONDO
        // =====================================================

        canvas.SaveState();

        BaseColor bgColor = new BaseColor(
            System.Drawing.ColorTranslator.FromHtml("#3C6A79"));

        canvas.SetColorFill(bgColor);

        canvas.Rectangle(x, y, width, height);

        canvas.Fill();

        canvas.RestoreState();

        // =====================================================
        // OVERLAY OSCURO
        // =====================================================

        canvas.SaveState();

        PdfGState state = new()
        {
            FillOpacity = 0.45f
        };

        canvas.SetGState(state);

        canvas.SetColorFill(BaseColor.Black);

        canvas.Rectangle(x, y, width, height);

        canvas.Fill();

        canvas.RestoreState();

        // =====================================================
        // URL QR
        // =====================================================

        string qrUrl = $"{BASE_URL}/upload-photo/{data.Code}";

        // =====================================================
        // QR
        // =====================================================

        using QRCodeGenerator qrGenerator = new();

        QRCodeData qrCodeData = qrGenerator.CreateQrCode(
            qrUrl,
            QRCodeGenerator.ECCLevel.Q);

        using PngByteQRCode qrCode = new(qrCodeData);

        byte[] qrBytes = qrCode.GetGraphic(20);

        Image qrImage = Image.GetInstance(qrBytes);
        //Tamaño
        float qrSize = width * 0.55f;

        qrImage.ScaleAbsolute(qrSize, qrSize);

        // CENTRADO PERFECTO
        float qrX = x + ((width - qrSize) / 2);
        float qrY = y + ((height - qrSize) / 2);

        qrImage.SetAbsolutePosition(qrX, qrY);

        // IMPORTANTE
        canvas.AddImage(qrImage);

        // =====================================================
        // FUENTES
        // =====================================================

        Font titleFont = new Font(
            Font.HELVETICA,
            22,
            Font.BOLD,
            BaseColor.White);

        Font subTitleFont = new Font(
            Font.HELVETICA,
            12,
            Font.NORMAL,
            BaseColor.White);

        Font topTextFont = new Font(
            Font.HELVETICA,
            14,
            Font.BOLD,
            BaseColor.White);

        Font bottomTextFont = new Font(
            Font.HELVETICA,
            10,
            Font.NORMAL,
            BaseColor.White);

        // =====================================================
        // TEXTO SUPERIOR
        // =====================================================

        ColumnText.ShowTextAligned(
            canvas,
            Element.ALIGN_CENTER,
            new Phrase(
                "ESCANEA, TOMA Y COMPARTE\n¡ASÍ DE FÁCIL!",
                topTextFont),
            x + width / 2,
            y + height - 60,
            0);

        // =====================================================
        // TITULO
        // =====================================================

        ColumnText.ShowTextAligned(
            canvas,
            Element.ALIGN_CENTER,
            new Phrase(
                data.Name.ToUpper(),
                titleFont),
            x + width / 2,
            y + 70,
            0);

        // =====================================================
        // SUBTITLE
        // =====================================================

        ColumnText.ShowTextAligned(
            canvas,
            Element.ALIGN_CENTER,
            new Phrase(
                data.SubTitle.ToUpper(),
                subTitleFont),
            x + width / 2,
            y + 45,
            0);

        // =====================================================
        // TEXTO INFERIOR
        // =====================================================

        ColumnText.ShowTextAligned(
            canvas,
            Element.ALIGN_CENTER,
            new Phrase(
                "ESCANEA EL QR Y SUBE TUS FOTOS",
                bottomTextFont),
            x + width / 2,
            y + 20,
            0);

        // =====================================================
        // BORDE
        // =====================================================

        canvas.SaveState();

        canvas.SetColorStroke(BaseColor.White);

        canvas.SetLineWidth(2);

        canvas.Rectangle(
            x + 5,
            y + 5,
            width - 10,
            height - 10);

        canvas.Stroke();

        canvas.RestoreState();
    }
}