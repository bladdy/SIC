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
        // DESCARGAR LOGO UNA SOLA VEZ
        // =====================================================

        byte[]? logoBytes = null;

        try
        {
            string logoUrl = "https://invboxv-app.com/logo.png";

            using HttpClient httpClient = new();

            logoBytes = await httpClient.GetByteArrayAsync(logoUrl);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }

        // =====================================================
        // DESCARGAR FONDO UNA SOLA VEZ
        // =====================================================

        byte[]? backgroundBytes = null;

        try
        {
            string backgroundUrl = "https://invboxv-app.com/banner_bg.jpeg";

            using HttpClient httpClient = new();

            backgroundBytes = await httpClient.GetByteArrayAsync(backgroundUrl);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }

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
        // TAMAÑO PAGINA
        // =====================================================

        float pageWidth = PageSize.A4.Width;
        float pageHeight = PageSize.A4.Height;

        // =====================================================
        // FONDO PARA TODA LA PAGINA
        // =====================================================

        try
        {
            if (backgroundBytes != null)
            {
                Image backgroundImage = Image.GetInstance(backgroundBytes);

                backgroundImage.ScaleAbsolute(pageWidth, pageHeight);

                backgroundImage.SetAbsolutePosition(0, 0);

                canvas.AddImage(backgroundImage);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }

        // =====================================================
        // CONFIGURACION 4 POR PAGINA
        // =====================================================

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
            await DrawBannerAsync(
                document,
                writer,
                canvas,
                data,
                position.x,
                position.y,
                bannerWidth,
                bannerHeight,
                position.rotate,
                logoBytes);
        }

        document.Close();

        return stream.ToArray();
    }

    private async Task DrawBannerAsync(
        Document document,
        PdfWriter writer,
        PdfContentByte canvas,
        Event data,
        float x,
        float y,
        float width,
        float height,
        bool rotate,
        byte[]? logoBytes
        )
    {
        if (rotate)
        {
            canvas.SaveState();

            canvas.ConcatCtm(
                -1,
                0,
                0,
                -1,
                x * 2 + width,
                y * 2 + height);
        }
        // =====================================================
        // FONDO
        // =====================================================

        /*canvas.SaveState();

        BaseColor bgColor = new BaseColor(
            System.Drawing.ColorTranslator.FromHtml("#3C6A79"));

        canvas.SetColorFill(bgColor);

        canvas.Rectangle(x, y, width, height);

        canvas.Fill();

        canvas.RestoreState();*/

        // =====================================================
        // OVERLAY
        // =====================================================

        /*canvas.SaveState();

        PdfGState state = new()
        {
            FillOpacity = 0.35f
        };

        canvas.SetGState(state);

        canvas.SetColorFill(BaseColor.Black);

        canvas.Rectangle(x, y, width, height);

        canvas.Fill();

        canvas.RestoreState();*/

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

        float qrSize = width * 0.40f;

        qrImage.ScaleAbsolute(qrSize, qrSize);

        float qrX = x + ((width - qrSize) / 2);
        float qrY = y + 170;

        qrImage.SetAbsolutePosition(qrX, qrY);

        canvas.AddImage(qrImage);

        // =====================================================
        // ICONO WEB DESDE URL
        // =====================================================

        try
        {
            if (logoBytes != null)
            {
                Image webIcon = Image.GetInstance(logoBytes);

                webIcon.ScaleAbsolute(18, 18);

                webIcon.SetAbsolutePosition(
                    x + width / 2 - 45,
                    y + 63);

                canvas.AddImage(webIcon);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }

        // =====================================================
        // FUENTES
        // =====================================================

        string fontsPath = Path.Combine(
            AppContext.BaseDirectory,
            "Fonts");

        BaseFont glacial = BaseFont.CreateFont(
            BaseFont.TIMES_ROMAN,//Path.Combine(fontsPath, "GlacialIndifference-Regular.otf"),
            BaseFont.CP1252,
            false);

        BaseFont heyGotcha = BaseFont.CreateFont(
            BaseFont.TIMES_ROMAN, //Path.Combine(fontsPath, "HeyGotcha-Regular.ttf"),
            BaseFont.CP1252,
            false);

        BaseFont quicksand = BaseFont.CreateFont(
            BaseFont.TIMES_ROMAN, //Path.Combine(fontsPath, "Quicksand-Regular.ttf"),
            BaseFont.CP1252,
            false);

        BaseFont times = BaseFont.CreateFont(
            BaseFont.TIMES_ROMAN,
            BaseFont.CP1252,
            false);

        Font titleFont = new Font(
            heyGotcha,
            30,
            Font.BOLD,
            BaseColor.Black);

        Font subTitleFont = new Font(
            glacial,
            16,
            Font.NORMAL,
            BaseColor.Black);

        Font middleTextFont = new Font(
            heyGotcha,
            12,
            Font.NORMAL,
            BaseColor.Black);

        Font bottomTextFont = new Font(
            times,
            20,
            Font.NORMAL,
            BaseColor.Black);

        Font linkFont = new Font(
            quicksand,
            9,
            Font.NORMAL,
            BaseColor.Black);

        // =====================================================
        // TITULO
        // =====================================================

        ColumnText titleColumn = new(canvas);

        Paragraph titleParagraph = new(
            data.Name.ToUpper(),
            titleFont);

        titleParagraph.Alignment = Element.ALIGN_CENTER;

        titleColumn.SetSimpleColumn(
            titleParagraph,
            x + 20,
            y + height - 150,
            x + width - 20,
            y + height - 50,
            30,
            Element.ALIGN_CENTER);

        titleColumn.Go();

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
            y + height - 40,
            0);

        // =====================================================
        // TEXTO MEDIO
        // =====================================================

        ColumnText middleText = new(canvas);

        Paragraph middleParagraph = new()
        {
            Alignment = Element.ALIGN_CENTER
        };

        middleParagraph.Add(
            new Chunk(
                "Escanea el Código QR, comparte y sube\n",
                middleTextFont));

        middleParagraph.Add(
            new Chunk(
                "tus mejores fotos a mi álbum digital",
                middleTextFont));

        middleText.SetSimpleColumn(
            middleParagraph,
            x + 20,
            y + 30,
            x + width - 20,
            y + 120,
            15,
            Element.ALIGN_CENTER);

        middleText.Go();

        // =====================================================
        // TEXTO INFERIOR
        // =====================================================

        ColumnText.ShowTextAligned(
            canvas,
            Element.ALIGN_CENTER,
            new Phrase(
                "ESCANÉAME",
                bottomTextFont),
            x + width / 2,
            y + 135,
            0);

        // =====================================================
        // LINK APP
        // =====================================================

        ColumnText.ShowTextAligned(
            canvas,
            Element.ALIGN_CENTER,
            new Phrase(
                "www.invboxv.com",
                linkFont),
            x + width / 2 + 10,
            y + 70,
            0);

        // =====================================================
        // BORDE
        // =====================================================

        canvas.SaveState();

        /*canvas.SetLineWidth(1f);

        canvas.SetColorStroke(new BaseColor(255, 255, 255, 40));

        canvas.Rectangle(
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