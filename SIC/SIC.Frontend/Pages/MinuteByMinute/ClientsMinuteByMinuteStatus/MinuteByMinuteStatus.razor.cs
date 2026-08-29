using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using SIC.Frontend.Repositories;
using SIC.Shared.Entities;
using SIC.Shared.Enums;

namespace SIC.Frontend.Pages.MinuteByMinute.ClientsMinuteByMinuteStatus
{
    public partial class MinuteByMinuteStatus
    {
        [Parameter] public string? Code { get; set; }

        [Inject] private IRepository Repository { get; set; } = default!;
        [Inject] private NavigationManager NavigationManager { get; set; } = default!;
        [Inject] private SweetAlertService SweetAlertService { get; set; } = default!;

        private MinuteByMinuteContainer? Container;
        private List<MbMActivity> Activities => Container?.Activities
            ?.OrderBy(a => a.StartTime)
            .ToList() ?? new List<MbMActivity>();

        private string EventName => Container?.Event?.Name ?? string.Empty;

        private int TotalActivities => Activities.Count;
        private int CompletedCount => Activities.Count(a => a.Status == ActivityStatus.Completada);
        private int InProgressCount => Activities.Count(a => a.Status == ActivityStatus.EnProgreso);
        private int PendingCount => Activities.Count(a => a.Status == ActivityStatus.Pendiente || a.Status == ActivityStatus.Cancelada);

        protected override async Task OnInitializedAsync()
        {
            await LoadMinuteByMinuteAsync();
        }

        private async Task LoadMinuteByMinuteAsync()
        {
            var result = await Repository.GetAsync<MinuteByMinuteContainer>($"api/MinuteByMinute/byEventCode/{Code}");
            if (result.Error)
            {
                var message = await result.GetErrorMessageAsync();
                await SweetAlertService.FireAsync("Error", message, SweetAlertIcon.Error);
                return;
            }
            Container = result.Response;
        }

        private static string GetResponsibleName(MbMActivity activity)
        {
            if (string.IsNullOrWhiteSpace(activity.Responsible)) return "Sin asignar";
            var open = activity.Responsible.IndexOf('(');
            return open >= 0 ? activity.Responsible.Substring(0, open).Trim() : activity.Responsible.Trim();
        }

        private static string GetResponsibleRole(MbMActivity activity)
        {
            if (!string.IsNullOrWhiteSpace(activity.ResponsibleRole)) return activity.ResponsibleRole.Trim();
            if (string.IsNullOrWhiteSpace(activity.Responsible)) return "";
            var open = activity.Responsible.IndexOf('(');
            var close = activity.Responsible.LastIndexOf(')');
            if (open < 0 || close < 0 || close <= open) return "";
            return activity.Responsible.Substring(open + 1, close - open - 1).Trim();
        }
    }

    public class MinuteByMinuteContainer
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public bool IsPublic { get; set; }
        public DateTime CreatedAt { get; set; }
        public int EventId { get; set; }
        public Event? Event { get; set; }
        public List<MbMActivity>? Activities { get; set; }
    }
}
