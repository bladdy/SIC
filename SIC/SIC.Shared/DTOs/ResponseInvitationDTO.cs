using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIC.Shared.DTOs
{
    public class ResponseInvitationDTO
    {
        public List<GuestDTO> Guests { get; set; } = null!;
        public string Code { get; set; } = null!;
        public int Status { get; set; }
        public string? Comments { get; set; }
    }

    public class GuestDTO
    {
        public int Id { get; set; }
        public string? GuestName { get; set; }
        public int GuestType { get; set; }
        public int InvitationId { get; set; }
        public int Status { get; set; }
        public object? Invitation { get; set; }
    }
}