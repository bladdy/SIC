using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Extensions;
using SIC.Backend.UnitOfWork.Interfaces;
using SIC.Shared.DTOs;
using SIC.Shared.Entities;
using SIC.Shared.Enums;
using SIC.Shared.Helpers;

//ToDO: Validar que las invitaciones importadas pertenezcan al eventId
//ToDO: Manejar mejor los errores (ej. código duplicado)
//ToDo: Refactorizar el codigo
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

        //Validar si sean invitaciones que pertenecen al eventId
        [HttpPost("ImportarExcel/{eventId}/{DeleteRegister}")]
        public async Task<IActionResult> ImportarExcel(
            int eventId,
            IFormFile file,
            bool DeleteRegister)
        {
            //try catch general para capturar cualquier error inesperado durante el proceso de importación

            try
            {

            
            if (file == null || file.Length == 0)
                return BadRequest("El archivo no es válido.");

            var invitations = new List<Invitation>();

            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);
            using var workbook = new XLWorkbook(stream);

            //Validacion del nombre de la hoja
            var worksheet = workbook.Worksheets.FirstOrDefault();

            if (worksheet == null)
                return BadRequest("El archivo Excel no contiene hojas válidas.");

            var rangeUsed = worksheet.RangeUsed();
            if (rangeUsed == null)
                return BadRequest("El archivo Excel no contiene datos válidos.");

            var rows = rangeUsed.RowsUsed().Skip(1); // encabezado

            foreach (var row in rows)
            {
                try
                {
                    var code = row.Cell(1).GetString();

                    var invitation = new Invitation
                    {
                        Code = code,
                        Name = row.Cell(2).GetString(),
                        Email = row.Cell(3).GetString(),
                        PhoneNumber = row.Cell(4).GetString(),
                        EventId = eventId,

                        NumberAdults = row.Cell(5).GetValue<int>(),
                        NumberYouths = row.Cell(6).GetValue<int>(),
                        NumberChildren = row.Cell(7).GetValue<int>(),

                        NumberConfirmedAdults = row.Cell(8).GetValue<int>(),
                        NumberConfirmedYouths = row.Cell(9).GetValue<int>(),
                        NumberConfirmedChildren = row.Cell(10).GetValue<int>(),

                        Table = row.Cell(12).GetString(),
                        Comments = row.Cell(13).GetString(),
                        SentDate = DateTime.Now,

                        Guests = new List<InvitationGuest>()
                    };

                    invitations.Add(invitation);
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }
            }

            int added = 0;
            int updated = 0;
            int deleted = 0;
            int errors = 0;

            // 🔴 ELIMINAR los que no vienen en el Excel
            if (DeleteRegister)
            {
                var response = await _invitationUnitOfWork.GetInivtationsByyEventIdAsync(eventId);
                var existingInvitations = response?.Result?.ToList() ?? new List<Invitation>();

                var excelCodes = invitations.Select(i => i.Code).ToHashSet();

                var toDelete = existingInvitations
                    .Where(i => !excelCodes.Contains(i.Code))
                    .ToList();

                foreach (var inv in toDelete)
                {
                    try
                    {
                        await _invitationUnitOfWork.DeleteAsync(inv);
                        deleted++;
                    }
                    catch
                    {
                        errors++;
                    }
                }
            }

            // 🟢 AGREGAR / ACTUALIZAR
            foreach (var inv in invitations)
            {
                try
                {
                    //ToDo: revisar el cargado del excel con los guest
                    var response = await _invitationUnitOfWork.GetByCodeAsync(inv.Code!);
                    var existing = response.Result;

                    // ➕ NUEVA
                    if (existing == null)
                    {
                        inv.Status = Status.Pending;
                        inv.ConfirmationDate = null;

                        // 👥 Adultos
                        for (int i = 0; i < inv.NumberAdults; i++)
                        {
                            inv.Guests.Add(new InvitationGuest
                            {
                                GuestName = $"Adulto {i + 1}",
                                GuestType = GuestType.Adult,
                                Status = Status.Pending
                            });
                        }

                        // 👥 Jóvenes
                        for (int i = 0; i < inv.NumberYouths; i++)
                        {
                            inv.Guests.Add(new InvitationGuest
                            {
                                GuestName = $"Joven {i + 1}",
                                GuestType = GuestType.Youth,
                                Status = Status.Pending
                            });
                        }

                        // 👥 Niños
                        for (int i = 0; i < inv.NumberChildren; i++)
                        {
                            inv.Guests.Add(new InvitationGuest
                            {
                                GuestName = $"Niño {i + 1}",
                                GuestType = GuestType.Children,
                                Status = Status.Pending
                            });
                        }

                        await _invitationUnitOfWork.AddFullAsync(inv);
                        added++;
                    }
                    // ✏️ EXISTENTE
                    else
                    {
                        existing.Name = inv.Name;
                        existing.Email = inv.Email;
                        existing.PhoneNumber = inv.PhoneNumber;
                        existing.NumberAdults = inv.NumberAdults;
                        existing.NumberYouths = inv.NumberYouths;
                        existing.NumberChildren = inv.NumberChildren;
                        existing.NumberConfirmedAdults = inv.NumberConfirmedAdults;
                        existing.NumberConfirmedYouths = inv.NumberConfirmedYouths;
                        existing.NumberConfirmedChildren = inv.NumberConfirmedChildren;
                        existing.Table = inv.Table;
                        existing.Comments = inv.Comments;
                        existing.SentDate = inv.SentDate;

                        // 🔒 NO tocar estado ni invitados si ya existe
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
                    Eliminadas = deleted,
                    Errores = errors,
                    Message =
                        $"Procesadas {invitations.Count} invitaciones. " +
                        $"Agregadas: {added}, " +
                        $"Modificadas: {updated}, " +
                        $"Eliminadas: {deleted}, " +
                        $"Errores: {errors}"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new {error = ex.Message, detalle = ex.StackTrace });
            }
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
                        PhoneNumber = "0000000000",
                        NumberAdults = 2,
                        NumberYouths = 1,
                        NumberChildren = 1,
                        NumberConfirmedAdults = 0,
                        NumberConfirmedYouths = 0,
                        NumberConfirmedChildren = 0,
                        Status = Status.Pending, // 👈 ajusta al enum real que uses
                        Table = "Mesa 1",
                        Comments = "Registro de ejemplo porque no hay invitaciones",
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
            worksheet.Cell(1, 6).Value = "Número de Jóvenes";
            worksheet.Cell(1, 7).Value = "Número de Niños";
            worksheet.Cell(1, 8).Value = "Adultos Confirmados";
            worksheet.Cell(1, 9).Value = "Jóvenes Confirmados";
            worksheet.Cell(1, 10).Value = "Niños Confirmados";
            worksheet.Cell(1, 11).Value = "Estado";
            worksheet.Cell(1, 12).Value = "Mesa";
            worksheet.Cell(1, 13).Value = "Comentarios";
            /*worksheet.Cell(1, 12).Value = "Fecha Envío";
            worksheet.Cell(1, 13).Value = "Fecha Confirmación";*/

            var headerRange = worksheet.Range(1, 1, 1, 13);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
            headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            worksheet.SheetView.Freeze(1, 1);

            // 🔹 Contenido
            int row = 2;
            foreach (var invitation in invitationsList)
            {
                worksheet.Cell(row, 1).Value = invitation.Code;
                worksheet.Cell(row, 2).Value = invitation.Name;
                worksheet.Cell(row, 3).Value = invitation.Email;
                worksheet.Cell(row, 4).Value = invitation.PhoneNumber;
                worksheet.Cell(row, 5).Value = invitation.NumberAdults;
                worksheet.Cell(row, 6).Value = invitation.NumberConfirmedYouths;
                worksheet.Cell(row, 7).Value = invitation.NumberChildren;
                worksheet.Cell(row, 8).Value = invitation.NumberConfirmedAdults;
                worksheet.Cell(row, 9).Value = invitation.NumberConfirmedYouths;
                worksheet.Cell(row, 10).Value = invitation.NumberConfirmedChildren;
                worksheet.Cell(row, 11).Value = invitation.Status.GetDescription();
                worksheet.Cell(row, 12).Value = invitation.Table;
                worksheet.Cell(row, 13).Value = invitation.Comments;
                /*worksheet.Cell(row, 12).Value = invitation.SentDate.ToString("dd/MM/yyyy HH:mm");
                worksheet.Cell(row, 13).Value = invitation.ConfirmationDate?.ToString("dd/MM/yyyy HH:mm") ?? "—";*/

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