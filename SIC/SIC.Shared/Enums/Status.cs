using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIC.Shared.Enums
{
    public enum Status
    {
        [Description("Activo")]
        Active,
        [Description("Inactivo")]
        Inactive,
        [Description("Pendiente")]
        Pending,
        [Description("Confirmado")]
        Confirmed,
        [Description("Completado")]
        Completed,
        [Description("Cerrado")]
        Close,
        [Description("Abierto")]
        Open,
        [Description("Bloqueado")]
        Blocked,
        [Description("Registado")]
        Registered,
        [Description("Cancelado")]
        Cancelled,
        [Description("Enviado")]
        Sent,
        [Description("Recibido")]
        Received,
        [Description("Leído")]
        Read,
        [Description("No Leído")]
        Unread,
        [Description("Eliminado")]
        Deleted,
        [Description("Archivado")]
        Archived,
        [Description("Pagado")]
        Paid
    }
}
