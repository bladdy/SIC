using System.ComponentModel;

namespace SIC.Shared.Enums;

public enum ActivityStatus
{
    [Description("Pendiente")]
    Pendiente,

    [Description("En Progreso")]
    EnProgreso,

    [Description("Completada")]
    Completada,

    [Description("Cancelada")]
    Cancelada
}
