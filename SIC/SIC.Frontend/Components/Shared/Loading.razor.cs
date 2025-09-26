using Microsoft.AspNetCore.Components;

namespace SIC.Frontend.Components.Shared;

public partial class Loading
{
    [Parameter] public string? Label { get; set; }

    protected override void OnParametersSet()
    {
        base.OnParametersSet();
        if (string.IsNullOrEmpty(Label))
        {
            Label = "Por Favor Espere...";
        }
    }
}