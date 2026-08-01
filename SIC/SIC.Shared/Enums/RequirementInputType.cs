using System.ComponentModel;

namespace SIC.Shared.Enums;

public enum RequirementInputType
{
    [Description("Texto")]
    Text,

    [Description("Texto largo")]
    MultilineText,

    [Description("Número")]
    Number,

    [Description("Fecha")]
    Date,

    [Description("Hora")]
    Time,

    [Description("URL")]
    Url,

    [Description("Imagen")]
    Image
}
