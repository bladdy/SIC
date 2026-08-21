using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIC.Shared.DTOs
{
    public class TablesDto
    {
    }
    public class CreateOrEditTablesDto
    {
        public int Id { get; set; }
        public int EventoId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Seats { get; set; }
    }
    public class GenerateTablesDto
    {
        public int EventoId { get; set; }
        public int NumbersTables { get; set; }
        public int NumberOfSeats { get; set; }
    }
    public class AssignTablesDto
    {
        public int Id { get; set; } = 0;
        public int InvitationId { get; set; }
        public int TableId { get; set; }
    }
    public class AssignGuestTableDto
    {
        public int GuestId { get; set; }
        public int? TablesEventsId { get; set; }
    }
    public class AssignBulkResultDto
    {
        public int Assigned { get; set; }
        public List<string> Skipped { get; set; } = [];
    }
}
