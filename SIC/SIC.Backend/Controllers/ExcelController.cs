using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using SIC.Backend.UnitOfWork.Interfaces;
using SIC.Shared.DTOs;
using SIC.Shared.Entities;
using SIC.Shared.Enums;

namespace SIC.Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExcelController : ControllerBase
    {
        private readonly IInvitationUnitOfWork _invitationUnitOfWork;

        public ExcelController(IInvitationUnitOfWork invitationUnitOfWork)
        {
            _invitationUnitOfWork = invitationUnitOfWork;
        }

        [HttpPost("ImportarExcel/{eventId}")]
        public async Task<IActionResult> ImportarExcel(int eventId, IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("El archivo no es válido.");
            }

            var invitations = new List<Invitation>();

            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);
            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheets.FirstOrDefault();

            if (worksheet == null)
            {
                return BadRequest("El archivo Excel no contiene hojas válidas.");
            }

            var rangeUsed = worksheet.RangeUsed();
            if (rangeUsed == null)
            {
                return BadRequest("El archivo Excel no contiene datos válidos.");
            }

            var rows = rangeUsed.RowsUsed().Skip(1); // Saltar encabezado

            foreach (var row in rows)
            {
                try
                {
                    var invitation = new Invitation
                    {
                        Code = row.Cell(1).GetString(),
                        Name = row.Cell(2).GetString(),
                        Email = row.Cell(3).GetString(),
                        PhoneNumber = row.Cell(4).GetString(),
                        EventId = eventId,
                        NumberAdults = row.Cell(5).GetValue<int>(),
                        NumberChildren = row.Cell(6).GetValue<int>(),
                        NumberConfirmedAdults = row.Cell(7).GetValue<int>(),
                        NumberConfirmedChildren = row.Cell(8).GetValue<int>(),
                        Status = Status.Active, //Enum.TryParse<Status>(row.Cell(9).GetString(), out var status) ? status : Status.Pending,
                        Table = row.Cell(10).GetString(),
                        Comments = row.Cell(11).GetString(),
                        SentDate = DateTime.Now, //row.Cell(12).GetDateTime(),
                        ConfirmationDate = null //row.Cell(13).IsEmpty() ? null : row.Cell(13).GetDateTime()
                    };

                    invitations.Add(invitation);
                }
                catch (Exception e)
                {
                    return BadRequest($"{e.Message.ToString()}");
                }
            }

            int added = 0;
            int updated = 0;
            int errors = 0;

            foreach (var inv in invitations)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(inv.Code))
                    {
                        errors++;
                        continue; // saltamos esta fila
                    }
                    //Verificar que busque la invitacion por
                    var response = await _invitationUnitOfWork.GetByCodeAsync(inv.Code);
                    var existing = response.Result;  // ejemplo: búsqueda por código

                    if (existing == null)
                    {
                        await _invitationUnitOfWork.AddFullAsync(inv);
                        added++;
                    }
                    else
                    {
                        existing.Name = inv.Name;
                        existing.Email = inv.Email;
                        existing.PhoneNumber = inv.PhoneNumber;
                        existing.NumberAdults = inv.NumberAdults;
                        existing.NumberChildren = inv.NumberChildren;
                        existing.NumberConfirmedAdults = inv.NumberConfirmedAdults;
                        existing.NumberConfirmedChildren = inv.NumberConfirmedChildren;
                        existing.Status = inv.Status;
                        existing.Table = inv.Table;
                        existing.Comments = inv.Comments;
                        existing.SentDate = inv.SentDate;
                        existing.ConfirmationDate = inv.ConfirmationDate;

                        await _invitationUnitOfWork.UpdateFullAsync(existing);
                        updated++;
                    }
                }
                catch
                {
                    errors++;
                }
            }

            return Ok(new ImportExcelResultDTO
            {
                Total = invitations.Count,
                Agregadas = added,
                Modificadas = updated,
                Errores = errors,
                Message = $"Procesadas {invitations.Count} invitaciones. Agregadas: {added}, Modificadas: {updated}, Errores: {errors}"
            });
        }

        [HttpGet("GenerarExcel/{EventId}")]
        public async Task<IActionResult> GenerarExcel(int EventId)
        {
            var response = await _invitationUnitOfWork.GetInivtationsByyEventIdAsync(EventId);
            var invitationsList = response?.Result?.ToList();

            // 🔹 Si la lista viene null o vacía, se crea un dummy
            if (invitationsList == null || !invitationsList.Any())
            {
                invitationsList = new List<Invitation>
                {
                    new Invitation
                    {
                        Code = "DUMMY001",
                        Name = "Invitado de Ejemplo",
                        Email = "ejemplo@correo.com",
                        PhoneNumber = "000-000-0000",
                        NumberAdults = 2,
                        NumberChildren = 1,
                        NumberConfirmedAdults = 1,
                        NumberConfirmedChildren = 0,
                        Status = Status.Pending, // 👈 ajusta al enum real que uses
                        Table = "Mesa 1",
                        Comments = "⚠️ Registro de ejemplo porque no hay invitaciones",
                        SentDate = DateTime.Now,
                        ConfirmationDate = null
                    }
                };
            }

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Invitaciones");

            // 🔹 Encabezados
            worksheet.Cell(1, 1).Value = "Código";
            worksheet.Cell(1, 2).Value = "Nombre";
            worksheet.Cell(1, 3).Value = "Correo Electrónico";
            worksheet.Cell(1, 4).Value = "Número de Teléfono";
            worksheet.Cell(1, 5).Value = "Número de Adultos";
            worksheet.Cell(1, 6).Value = "Número de Niños";
            worksheet.Cell(1, 7).Value = "Adultos Confirmados";
            worksheet.Cell(1, 8).Value = "Niños Confirmados";
            worksheet.Cell(1, 9).Value = "Estado";
            worksheet.Cell(1, 10).Value = "Mesa";
            worksheet.Cell(1, 11).Value = "Comentarios";
            worksheet.Cell(1, 12).Value = "Fecha Envío";
            worksheet.Cell(1, 13).Value = "Fecha Confirmación";

            var headerRange = worksheet.Range(1, 1, 1, 13);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
            headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            // 🔹 Contenido
            int row = 2;
            foreach (var invitation in invitationsList)
            {
                worksheet.Cell(row, 1).Value = invitation.Code;
                worksheet.Cell(row, 2).Value = invitation.Name;
                worksheet.Cell(row, 3).Value = invitation.Email;
                worksheet.Cell(row, 4).Value = invitation.PhoneNumber;
                worksheet.Cell(row, 5).Value = invitation.NumberAdults;
                worksheet.Cell(row, 6).Value = invitation.NumberChildren;
                worksheet.Cell(row, 7).Value = invitation.NumberConfirmedAdults;
                worksheet.Cell(row, 8).Value = invitation.NumberConfirmedChildren;
                worksheet.Cell(row, 9).Value = invitation.Status.ToString();
                worksheet.Cell(row, 10).Value = invitation.Table;
                worksheet.Cell(row, 11).Value = invitation.Comments;
                worksheet.Cell(row, 12).Value = invitation.SentDate.ToString("dd/MM/yyyy HH:mm");
                worksheet.Cell(row, 13).Value = invitation.ConfirmationDate?.ToString("dd/MM/yyyy HH:mm") ?? "—";

                row++;
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            var content = stream.ToArray();

            return File(content,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Invitaciones_Evento_{EventId}.xlsx");
        }
    }
}