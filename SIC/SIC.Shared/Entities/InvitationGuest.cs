using SIC.Shared.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIC.Shared.Entities
{
    public class InvitationGuest
    {
        public int Id { get; set; }

        // Ya NO van Required aquí
        public string? GuestName { get; set; }

        public GuestType GuestType { get; set; } = GuestType.Adult;

        public int InvitationId { get; set; }
        public Status Status { get; set; } = Status.Pending;

        public Invitation? Invitation { get; set; }

        public int? TablesEventsId { get; set; }
        public TablesEvents? TablesEvents { get; set; }

        [NotMapped]
        public int? EffectiveTablesEventsId => TablesEventsId ?? Invitation?.TablesEventsId;
    }
}
