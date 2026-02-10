using Microsoft.AspNetCore.Components;
using SIC.Frontend.Repositories;
using SIC.Frontend.Services;
using SIC.Shared.DTOs;

namespace SIC.Frontend.Pages.Whatsapp.Components;

public partial class WhatsappInbox : IAsyncDisposable
{
    [Inject] private IRepository Repository { get; set; } = default!;
    [Inject] private SignalRService SignalR { get; set; } = default!;

    [Parameter] public string? EventCode { get; set; } // 🔑 CLAVE

    private List<InboxConversationDto> Inbox = new();
    private string? SelectedPhone;

    protected override async Task OnInitializedAsync()
    {
        SignalR.OnInboxUpdated += UpdateInbox;
        await SignalR.StartAsync("https://invboxv-app.com/hubs/whatsapp-chat");
    }

    protected override async Task OnParametersSetAsync()
    {
        Inbox = new();
        SelectedPhone = null;

        var response = await Repository.GetAsync<List<InboxConversationDto>>(
            $"/api/whatsapp/webhook/whatsapp/inbox/{EventCode}"
        );

        Inbox = response.Response ?? new();

        StateHasChanged();
    }

    private void SelectChat(string phone)
    {
        SelectedPhone = phone;

        var chat = Inbox.FirstOrDefault(x => x.PhoneNumber == phone);
        if (chat != null)
            chat.UnreadCount = 0;
    }

    private void BackToInbox()
    {
        SelectedPhone = null;
    }

    private void UpdateInbox(InboxConversationDto item)
    {
        // 🔐 Si viene EventCode, ignorar otros eventos
        if (!string.IsNullOrWhiteSpace(EventCode) &&
            item.EventCode != EventCode)
            return;

        var chat = Inbox.FirstOrDefault(x => x.PhoneNumber == item.PhoneNumber);

        if (chat == null)
        {
            Inbox.Insert(0, item);
        }
        else
        {
            chat.LastMessage = item.LastMessage;
            chat.LastMessageAt = item.LastMessageAt;
            chat.UnreadCount++;

            Inbox.Remove(chat);
            Inbox.Insert(0, chat);
        }

        InvokeAsync(StateHasChanged);
    }

    public async ValueTask DisposeAsync()
    {
        SignalR.OnInboxUpdated -= UpdateInbox;
    }
}