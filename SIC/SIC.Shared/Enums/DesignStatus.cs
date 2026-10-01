using System.ComponentModel;

namespace SIC.Shared.Enums;

public enum DesignStatus
{
    [Description("Pendiente")]
    Pending = 0,

    [Description("En proceso")]
    InProgress = 1,

    [Description("Realizado")]
    Completed = 2
}
