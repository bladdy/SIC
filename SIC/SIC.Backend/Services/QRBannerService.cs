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

        // Posiciones + si va rotado
        List<(float x, float y, bool rotate)> positions =
        [
            (0, bannerHeight, true),            // 1 Arriba izquierda ROTADO
            (bannerWidth, bannerHeight, true),  // 2 Arriba derecha ROTADO
            (0, 0, false),                      // 3 Abajo izquierda NORMAL
            (bannerWidth, 0, false)             // 4 Abajo derecha NORMAL
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
                bannerHeight,
                position.rotate);
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
        float height,
        bool rotate)
    {
        if (rotate)
        {
            canvas.SaveState();

            // Rotar 180°
            canvas.ConcatCtm(-1, 0, 0, -1, x * 2 + width, y * 2 + height);
        }

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

        float qrSize = width * 0.55f;

        qrImage.ScaleAbsolute(qrSize, qrSize);

        float qrX = x + ((width - qrSize) / 2);
        float qrY = y + ((height - qrSize) / 2);

        qrImage.SetAbsolutePosition(qrX, qrY);

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

        ColumnText ct = new ColumnText(canvas);

        Paragraph p = new Paragraph
        {
            Alignment = Element.ALIGN_CENTER
        };

        p.Add(new Chunk("ESCANEA, TOMA Y COMPARTE\n", topTextFont));
        p.Add(new Chunk("¡ASÍ DE FÁCIL!", topTextFont));

        ct.SetSimpleColumn(
            p,
            x,
            y + height - 90,
            x + width,
            y + height - 20,
            20,
            Element.ALIGN_CENTER);

        ct.Go();

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

        //canvas.SetColorStroke(BaseColor.White);

        //canvas.SetLineWidth(2);

        /*canvas.Rectangle(
            x + 5,
            y + 5,
            width - 10,
            height - 10);

        canvas.Stroke();*/

        canvas.RestoreState();

        if (rotate)
        {
            canvas.RestoreState();
        }
    }
}