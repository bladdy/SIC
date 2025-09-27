using Microsoft.AspNetCore.Components;

namespace SIC.Frontend.Shared.Component
{
    public partial class GenericList<Titem>
    {
        [Parameter] public RenderFragment? Loading { get; set; }
        [Parameter] public RenderFragment? NoRecords { get; set; }
        [EditorRequired, Parameter] public RenderFragment? Body { get; set; }
        [EditorRequired, Parameter] public List<Titem>? MyList { get; set; }
    }
}