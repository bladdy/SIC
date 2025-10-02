using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using SIC.Backend.UnitOfWork.Interfaces;
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