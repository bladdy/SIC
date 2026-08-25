using iTextSharp.text;
using iTextSharp.text.pdf;
using QRCoder;
using SIC.Shared.DTOs;
using SIC.Shared.Entities;
using SIC.Shared.Enums;
using SIC.Shared.Helpers;

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
            AddCentered(data.NombreInvitado, fontTitle, 5);

            if (data.IsIndividualAssignment && data.GuestsWithMesa.Count > 0)
            {
                foreach (var item in data.GuestsWithMesa)
                {
                    AddCentered(item, fontMuted, 0);
                }
            }
            else
            {
                foreach (var item in data.Guests)
                {
                    AddCentered(item, fontMuted, 0);
                }

                // ==============================
                // 📄 TEXTO INFORMATIVO
                // ==============================
                AddCentered($"MESA: {data.MesaAsignada}", fontTitle, 5);
            }

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

        public async Task<byte[]> GenerarListaPdf(List<Invitation> invitaciones)
        {
            using var ms = new MemoryStream();

            var document = new Document(PageSize.A4, 40, 40, 40, 40);
            PdfWriter.GetInstance(document, ms);
            document.Open();
            invitaciones = [.. invitaciones.OrderBy(i => i.Name)];
            // ==============================
            // 🎨 COLORES
            // ==============================
            var borderColor = new BaseColor(222, 226, 230);
            var headerColor = new BaseColor(33, 37, 41);
            var footerColor = new BaseColor(211, 211, 211);
            // ==============================
            // 🔠 FUENTES
            // ==============================
            var fontTitle = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16);
            var fontHeader = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10, BaseColor.White);
            var fontCell = FontFactory.GetFont(FontFactory.HELVETICA, 10);
            var fontFotter = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 11);
            // ==============================
            // 📝 TÍTULO
            // ==============================
            var title = new Paragraph($"LISTA DE INVITADOS CONFIRMADOS: {invitaciones.FirstOrDefault()?.Event?.Name}", fontTitle)
            {
                Alignment = Element.ALIGN_CENTER,
                SpacingAfter = 10
            };
            document.Add(title);
            document.Add(new Paragraph($"Fecha generación: {DateTime.Now:dd/MM/yyyy HH:mm}")
            {
                Alignment = Element.ALIGN_RIGHT,
                SpacingAfter = 8
            });

            // ==============================
            // 📋 TABLA
            // ==============================
            var table = new PdfPTable(6)
            {
                WidthPercentage = 100
            };

            table.SetWidths(new float[] { 4, 1, 1, 1, 1, 1 });

            void AddHeader(string text)
            {
                var cell = new PdfPCell(new Phrase(text, fontHeader))
                {
                    BackgroundColor = headerColor,
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    Padding = 2,
                    BorderColor = borderColor
                };

                table.AddCell(cell);
            }

            AddHeader("Nombre");
            AddHeader("Adul.");
            AddHeader("Jov.");
            AddHeader("Niñ.");
            AddHeader("Mesa");
            AddHeader("Ok");

            // ==============================
            // 👥 FILAS
            // ==============================
            foreach (var item in invitaciones)
            {
                table.AddCell(new PdfPCell(new Phrase(item.Name ?? "", fontCell)) { Padding = 5 });
                table.AddCell(new PdfPCell(new Phrase(item.NumberConfirmedAdults.ToString() ?? "", fontCell)) { Padding = 5, HorizontalAlignment = Element.ALIGN_CENTER });
                table.AddCell(new PdfPCell(new Phrase(item.NumberConfirmedYouths.ToString() ?? "", fontCell)) { Padding = 5, HorizontalAlignment = Element.ALIGN_CENTER });
                table.AddCell(new PdfPCell(new Phrase(item.NumberConfirmedChildren.ToString() ?? "", fontCell)) { Padding = 5, HorizontalAlignment = Element.ALIGN_CENTER });
                table.AddCell(
                    new PdfPCell(
                        new Phrase(
                            item.TablesEvents?.Name
                            ?? item.Guests?.FirstOrDefault(g => g.TablesEventsId.HasValue)?.TablesEvents?.Name
                            ?? "",
                            fontCell)
                    )
                    {
                        Padding = 5,
                        HorizontalAlignment = Element.ALIGN_CENTER
                    }
                );
                table.AddCell(new PdfPCell(new Phrase("", fontCell)) { Padding = 5 });
            }

            int totalAdultos = invitaciones.Sum(x => x.NumberConfirmedAdults);
            int totalJovenes = invitaciones.Sum(x => x.NumberConfirmedYouths);
            int totalNiños = invitaciones.Sum(x => x.NumberConfirmedChildren);
            int totales = totalAdultos + totalJovenes + totalNiños;

            table.AddCell(new PdfPCell(new Phrase("Total" ?? "", fontFotter)) { Padding = 5, BackgroundColor = footerColor });
            table.AddCell(new PdfPCell(new Phrase(totalAdultos.ToString() ?? "", fontFotter)) { Padding = 5, BackgroundColor = footerColor, HorizontalAlignment = Element.ALIGN_CENTER });
            table.AddCell(new PdfPCell(new Phrase(totalJovenes.ToString() ?? "", fontFotter)) { Padding = 5, BackgroundColor = footerColor, HorizontalAlignment = Element.ALIGN_CENTER });
            table.AddCell(new PdfPCell(new Phrase(totalNiños.ToString() ?? "", fontFotter)) { Padding = 5, BackgroundColor = footerColor, HorizontalAlignment = Element.ALIGN_CENTER });
            table.AddCell(new PdfPCell(new Phrase("", fontFotter)) { Padding = 5, BackgroundColor = footerColor });
            table.AddCell(new PdfPCell(new Phrase(totales.ToString(), fontFotter)) { Padding = 5, BackgroundColor = footerColor, HorizontalAlignment = Element.ALIGN_CENTER });

            document.Add(table);

            document.Close();

            return await Task.FromResult(ms.ToArray());
        }

        public byte[] GenerarRegistroEntradasPdf(List<InvitationEntry> entries)
        {
            using var ms = new MemoryStream();

            var document = new Document(PageSize.A4, 40, 40, 40, 40);
            PdfWriter.GetInstance(document, ms);
            document.Open();
            entries = [.. entries.OrderBy(e => e.Invitation!.Name)];
            // ==============================
            // 🎨 COLORES
            // ==============================
            var borderColor = new BaseColor(222, 226, 230);
            var headerColor = new BaseColor(33, 37, 41);
            var footerColor = new BaseColor(211, 211, 211);
            // ==============================
            // 🔠 FUENTES
            // ==============================
            var fontTitle = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16);
            var fontHeader = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 9, BaseColor.White);
            var fontCell = FontFactory.GetFont(FontFactory.HELVETICA, 10);
            var fontFooter = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 11);
            // ==============================
            // 📝 TÍTULO
            // ==============================
            var title = new Paragraph($"REGISTRO DE INVITADOS: {entries.FirstOrDefault()?.Event?.Name}", fontTitle)
            {
                Alignment = Element.ALIGN_CENTER,
                SpacingAfter = 10
            };
            document.Add(title);
            document.Add(new Paragraph($"Fecha generación: {DateTime.Now:dd/MM/yyyy HH:mm}")
            {
                Alignment = Element.ALIGN_RIGHT,
                SpacingAfter = 8
            });

            // ==============================
            // 📋 TABLA
            // ==============================
            var table = new PdfPTable(8)
            {
                WidthPercentage = 100
            };

            table.SetWidths(new float[] { 4, 1, 1, 1, 1, 1, 1, 2 });

            void AddHeader(string text)
            {
                var cell = new PdfPCell(new Phrase(text, fontHeader))
                {
                    BackgroundColor = headerColor,
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    Padding = 2,
                    BorderColor = borderColor
                };

                table.AddCell(cell);
            }

            AddHeader("Nombre");
            AddHeader("Conf. Adul.");
            AddHeader("Conf. Jov.");
            AddHeader("Conf. Niñ.");
            AddHeader("Asist. Adul.");
            AddHeader("Asist. Jov.");
            AddHeader("Asist. Niñ.");
            AddHeader("Mesa");

            // ==============================
            // 👥 FILAS
            // ==============================
            foreach (var item in entries)
            {
                var invitation = item.Invitation;

                string mesas;
                if (invitation?.Guests?.Any(g => g.TablesEventsId.HasValue) == true)
                {
                    mesas = string.Join("\n", invitation.Guests
                        .Where(g => g.Status == Status.Attend)
                        .Select(g => $"{g.GuestName}: {g.TablesEvents?.Name ?? "Sin asignar"}"));
                }
                else
                {
                    mesas = invitation?.TablesEvents?.Name ?? "Sin asignar";
                }

                table.AddCell(new PdfPCell(new Phrase(invitation?.Name ?? "", fontCell)) { Padding = 5 });
                table.AddCell(new PdfPCell(new Phrase(invitation?.NumberConfirmedAdults.ToString() ?? "0", fontCell)) { Padding = 5, HorizontalAlignment = Element.ALIGN_CENTER });
                table.AddCell(new PdfPCell(new Phrase(invitation?.NumberConfirmedYouths.ToString() ?? "0", fontCell)) { Padding = 5, HorizontalAlignment = Element.ALIGN_CENTER });
                table.AddCell(new PdfPCell(new Phrase(invitation?.NumberConfirmedChildren.ToString() ?? "0", fontCell)) { Padding = 5, HorizontalAlignment = Element.ALIGN_CENTER });
                table.AddCell(new PdfPCell(new Phrase(item.AdultsEntered.ToString(), fontCell)) { Padding = 5, HorizontalAlignment = Element.ALIGN_CENTER });
                table.AddCell(new PdfPCell(new Phrase(item.YouthsEntered.ToString(), fontCell)) { Padding = 5, HorizontalAlignment = Element.ALIGN_CENTER });
                table.AddCell(new PdfPCell(new Phrase(item.ChildrenEntered.ToString(), fontCell)) { Padding = 5, HorizontalAlignment = Element.ALIGN_CENTER });
                table.AddCell(new PdfPCell(new Phrase(mesas, fontCell)) { Padding = 5, HorizontalAlignment = Element.ALIGN_CENTER });
            }

            int totalConfAdultos = entries.Sum(x => x.Invitation?.NumberConfirmedAdults ?? 0);
            int totalConfJovenes = entries.Sum(x => x.Invitation?.NumberConfirmedYouths ?? 0);
            int totalConfNiños = entries.Sum(x => x.Invitation?.NumberConfirmedChildren ?? 0);
            int totalAsistAdultos = entries.Sum(x => x.AdultsEntered);
            int totalAsistJovenes = entries.Sum(x => x.YouthsEntered);
            int totalAsistNiños = entries.Sum(x => x.ChildrenEntered);
            int totalRegistrados = totalAsistAdultos + totalAsistJovenes + totalAsistNiños;

            table.AddCell(new PdfPCell(new Phrase("Total", fontFooter)) { Padding = 5, BackgroundColor = footerColor });
            table.AddCell(new PdfPCell(new Phrase(totalConfAdultos.ToString(), fontFooter)) { Padding = 5, BackgroundColor = footerColor, HorizontalAlignment = Element.ALIGN_CENTER });
            table.AddCell(new PdfPCell(new Phrase(totalConfJovenes.ToString(), fontFooter)) { Padding = 5, BackgroundColor = footerColor, HorizontalAlignment = Element.ALIGN_CENTER });
            table.AddCell(new PdfPCell(new Phrase(totalConfNiños.ToString(), fontFooter)) { Padding = 5, BackgroundColor = footerColor, HorizontalAlignment = Element.ALIGN_CENTER });
            table.AddCell(new PdfPCell(new Phrase(totalAsistAdultos.ToString(), fontFooter)) { Padding = 5, BackgroundColor = footerColor, HorizontalAlignment = Element.ALIGN_CENTER });
            table.AddCell(new PdfPCell(new Phrase(totalAsistJovenes.ToString(), fontFooter)) { Padding = 5, BackgroundColor = footerColor, HorizontalAlignment = Element.ALIGN_CENTER });
            table.AddCell(new PdfPCell(new Phrase(totalAsistNiños.ToString(), fontFooter)) { Padding = 5, BackgroundColor = footerColor, HorizontalAlignment = Element.ALIGN_CENTER });
            table.AddCell(new PdfPCell(new Phrase($"Registrados: {totalRegistrados}", fontFooter)) { Padding = 5, BackgroundColor = footerColor, HorizontalAlignment = Element.ALIGN_CENTER });

            document.Add(table);

            document.Close();

            return ms.ToArray();
        }

        public byte[] GenerarMesasPdf(string evento, List<TablesEvents> mesas)
        {
            using var ms = new MemoryStream();

            var document = new Document(PageSize.A4, 40, 40, 40, 40);
            PdfWriter.GetInstance(document, ms);
            document.Open();
            mesas = [.. mesas.OrderBy(m => m.Number)];
            // ==============================
            // 🎨 COLORES
            // ==============================
            var borderColor = new BaseColor(60, 106, 121);
            var headerColor = new BaseColor(33, 37, 41);
            // ==============================
            // 🔠 FUENTES
            // ==============================
            var fontTitle = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16);
            var fontMesaHeader = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12, BaseColor.White);
            var fontMesaInfo = FontFactory.GetFont(FontFactory.HELVETICA, 9, new BaseColor(222, 226, 230));
            var fontInvitacion = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10);
            var fontGuest = FontFactory.GetFont(FontFactory.HELVETICA, 9);
            var fontSmall = FontFactory.GetFont(FontFactory.HELVETICA, 8, BaseColor.Gray);
            // ==============================
            // 📝 TÍTULO
            // ==============================
            var title = new Paragraph($"DISTRIBUCIÓN DE MESAS: {evento}", fontTitle)
            {
                Alignment = Element.ALIGN_CENTER,
                SpacingAfter = 10
            };
            document.Add(title);
            document.Add(new Paragraph($"Fecha generación: {DateTime.Now:dd/MM/yyyy HH:mm}")
            {
                Alignment = Element.ALIGN_RIGHT,
                SpacingAfter = 12
            });

            // ==============================
            // 🪑 UN BLOQUE POR MESA (como las tarjetas del HTML)
            // ==============================
            var layout = new PdfPTable(2)
            {
                WidthPercentage = 100
            };

            foreach (var mesa in mesas)
            {
                var mesaTable = new PdfPTable(1)
                {
                    WidthPercentage = 100,
                    SpacingAfter = 15
                };

                // Encabezado de la mesa
                var headerCell = new PdfPCell
                {
                    BackgroundColor = headerColor,
                    BorderColor = borderColor,
                    Padding = 6
                };
                headerCell.AddElement(new Paragraph($"Mesa: {mesa.Name}", fontMesaHeader));
                if (!string.IsNullOrWhiteSpace(mesa.Description))
                {
                    headerCell.AddElement(new Paragraph(mesa.Description, fontMesaInfo));
                }
                headerCell.AddElement(new Paragraph($"Lugares: {mesa.Seats}   |   Disponibles: {mesa.Seats - mesa.OccupiedSeats}", fontMesaInfo));
                mesaTable.AddCell(headerCell);

                // Cuerpo: invitaciones asignadas + invitados individuales agrupados por invitación
                var bodyCell = new PdfPCell
                {
                    BorderColor = borderColor,
                    Padding = 6
                };

                bool hasContent = false;

                foreach (var invitacion in mesa.Invitations.OrderBy(i => i.Name))
                {
                    hasContent = true;

                    var guests = invitacion.Guests?
                        .Where(g => g.Status == Status.Attend && (g.TablesEventsId == null || g.TablesEventsId == mesa.Id))
                        .OrderBy(g => g.GuestName)
                        .ToList() ?? [];

                    bodyCell.AddElement(new Paragraph(invitacion.Name ?? "", fontInvitacion));

                    if (guests.Count == 0)
                    {
                        bodyCell.AddElement(new Paragraph("Sin invitados confirmados", fontSmall));
                    }
                    else
                    {
                        foreach (var guest in guests)
                        {
                            var tag = guest.TablesEventsId.HasValue ? "(Individual)" : "(Invitación)";
                            bodyCell.AddElement(new Paragraph($"• {guest.GuestName}   {tag}", fontGuest));
                        }
                    }

                    bodyCell.AddElement(new Paragraph(" ", fontSmall));
                }

                var directGroups = mesa.Guests?
                    .Where(g => g.TablesEventsId == mesa.Id && g.Status == Status.Attend && g.Invitation != null && !mesa.Invitations.Any(i => i.Id == g.InvitationId))
                    .GroupBy(g => g.InvitationId)
                    .ToList();

                if (directGroups != null)
                {
                    foreach (var group in directGroups)
                    {
                        hasContent = true;

                        bodyCell.AddElement(new Paragraph(group.First().Invitation!.Name ?? "", fontInvitacion));

                        foreach (var guest in group.OrderBy(g => g.GuestName))
                        {
                            bodyCell.AddElement(new Paragraph($"• {guest.GuestName}   (Individual)", fontGuest));
                        }

                        bodyCell.AddElement(new Paragraph(" ", fontSmall));
                    }
                }

                if (!hasContent)
                {
                    bodyCell.AddElement(new Paragraph("Sin invitados confirmados", fontSmall));
                }

                mesaTable.AddCell(bodyCell);

                layout.AddCell(new PdfPCell(mesaTable)
                {
                    Border = Rectangle.NO_BORDER,
                    PaddingLeft = 3,
                    PaddingRight = 3,
                    PaddingBottom = 10
                });
            }

            if (mesas.Count % 2 == 1)
            {
                layout.AddCell(new PdfPCell
                {
                    Border = Rectangle.NO_BORDER
                });
            }

            document.Add(layout);

            document.Close();

            return ms.ToArray();
        }
    }
}