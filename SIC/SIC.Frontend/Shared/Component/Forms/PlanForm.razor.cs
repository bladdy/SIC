using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Routing;
using SIC.Shared.Entities;

namespace SIC.Frontend.Shared.Component.Forms
{
    public partial class PlanForm
    {
        private EditContext editContext = null!;
        [Parameter, EditorRequired] public Plan Plan { get; set; } = null!;
        [Parameter, EditorRequired] public EventCallback OnValidSubmit { get; set; }
        [Parameter, EditorRequired] public EventCallback ReturnAction { get; set; }
        [Inject] public SweetAlertService SweetAlertService { get; set; } = null!;
        public bool FormPostedSuccessfully { get; set; }

        private async Task OnBeforeInternalNavigation(LocationChangingContext context)
        {
            var formWasEdited = editContext.IsModified();
            if (!formWasEdited || FormPostedSuccessfully)
            {
                return;
            }
            var result = await SweetAlertService.FireAsync(new SweetAlertOptions
            {
                AllowEnterKey = true,
                Title = "¿Estás seguro?",
                Text = "Hay cambios sin guardar. Si continúas, se perderán los cambios.",
                Icon = SweetAlertIcon.Warning,
                ShowCancelButton = true,
                ConfirmButtonText = "Sí, salir",
                CancelButtonText = "No, quedarme"
            });
            var confirmed = !string.IsNullOrEmpty(result.Value);
            if (!confirmed)
            {
                return;
            }
            context.PreventNavigation();
        }
    }
}