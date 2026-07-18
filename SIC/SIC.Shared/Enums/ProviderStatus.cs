using System.ComponentModel;

namespace SIC.Shared.Enums;

public enum ProviderStatus
{
    [Description("Confirmado")]
    Confirmado,

    [Description("Pendiente")]
    Pendiente,

    [Description("Cancelado")]
    Cancelado
}
