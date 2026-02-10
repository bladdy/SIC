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

    protected override async Task OnInitializedAsync()
    {
        SignalR.OnInboxUpdated += UpdateInbox;
        await SignalR.StartAsync("https://invboxv-app.com/hubs/whatsapp-chat");
        await LoadInbox();
    }

    private async Task LoadInbox()
    {
        var response = await Repository.GetAsync<List<InboxConversationDto>>(
            "/api/whatsapp/webhook/whatsapp/inbox"
        );

        Inbox = response.Response ?? new();
    }

    private void SelectEvent(string eventCode)
    {
        SelectedEventCode = eventCode;
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