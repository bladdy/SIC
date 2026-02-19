using Microsoft.AspNetCore.Components;
using SIC.Frontend.Repositories;
using SIC.Frontend.Services;
using SIC.Shared.DTOs;

namespace SIC.Frontend.Pages.Whatsapp;

public partial class WhatsappInboxPage : IAsyncDisposable
{
    [Inject] private IRepository Repository { get; set; } = default!;
    [Inject] private SignalRService SignalR { get; set; } = default!;

    private List<InboxConversationDto> Inbox = new();
    private string? SelectedEventCode;
    private string? OwnerPhone;

    protected override async Task OnInitializedAsync()
    {
        var response = await Repository.GetAsync<WhatsAppManualConfigDto>(
                "/api/whatsapp/configurar"
            );
        if (response.Response != null)
        {
            OwnerPhone = response.Response!.PhoneNumber;
            SignalR.OnInboxUpdated += UpdateInbox;
            await SignalR.StartAsync("https://invboxv-app.com/hubs/whatsapp-chat");
            await LoadInbox();
            
        }
    }

    private async Task LoadInbox()
    {
        var response = await Repository.GetAsync<List<InboxConversationDto>>(
            "/api/whatsapp/webhook/whatsapp/inbox"
        );

        Inbox = response.Response ?? new();
    }

    private async Task SelectEvent(string eventCode)
    {
        if (SelectedEventCode == eventCode)
            return;

        // salir del evento anterior
        if (!string.IsNullOrEmpty(SelectedEventCode))
        {
            await SignalR.LeaveEventInbox(OwnerPhone!, SelectedEventCode);
        }

        SelectedEventCode = eventCode;

        // 🔥 unirse al evento correcto
        await SignalR.JoinEventInbox(OwnerPhone!, eventCode);
    }

    /// <summary>
    /// 🔥 Actualiza SOLO el inbox de eventos
    /// </summary>
    private void UpdateInbox(InboxConversationDto item)
    {
        if (string.IsNullOrWhiteSpace(item.EventCode))
            return;

        _ = InvokeAsync(() =>
        {
            var ev = Inbox.FirstOrDefault(x => x.EventCode == item.EventCode);

            if (ev == null)
            {
                Inbox.Insert(0, item);
            }
            else
            {
                ev.LastMessage = item.LastMessage;
                ev.LastMessageAt = item.LastMessageAt;
                ev.UnreadCount++;

                Inbox.Remove(ev);
                Inbox.Insert(0, ev);
            }

            StateHasChanged();
        });
    }

    public async ValueTask DisposeAsync()
    {
        SignalR.OnInboxUpdated -= UpdateInbox;
    }
}