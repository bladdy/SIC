using Microsoft.AspNetCore.Components;
using SIC.Frontend.Repositories;
using SIC.Frontend.Services;
using SIC.Shared.DTOs;

namespace SIC.Frontend.Pages.Whatsapp;

public partial class WhatsappInbox : IAsyncDisposable
{
    [Inject] private IRepository Repository { get; set; } = default!;
    [Inject] private SignalRService SignalR { get; set; } = default!;

    private List<InboxConversationDto>? Inbox;
    private string? SelectedPhone;

    protected override async Task OnInitializedAsync()
    {
        Inbox = new();

        SignalR.OnInboxUpdated += UpdateInbox;
        await SignalR.StartAsync("https://localhost:7141/hubs/whatsapp-chat");

        var response = await Repository.GetAsync<List<InboxConversationDto>>(
            "/api/whatsapp/webhook/whatsapp/inbox"
        );

        Inbox = response.Response ?? new();
    }

    private void SelectChat(string phone)
    {
        SelectedPhone = phone;

        var chat = Inbox?.FirstOrDefault(x => x.PhoneNumber == phone);
        if (chat != null)
            chat.UnreadCount = 0;
    }

    private void UpdateInbox(InboxConversationDto item)
    {
        var chat = Inbox!.FirstOrDefault(x => x.PhoneNumber == item.PhoneNumber);

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

    private void BackToInbox()
    {
        SelectedPhone = null;
    }
}