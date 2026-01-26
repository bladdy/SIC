using Microsoft.AspNetCore.Components;
using SIC.Frontend.Repositories;
using SIC.Frontend.Services;
using SIC.Shared.DTOs;

namespace SIC.Frontend.Pages.Whatsapp;

public partial class WhatsappInbox : IAsyncDisposable
{
    [Inject] private IRepository Repository { get; set; } = default!;
    [Inject] private SignalRService SignalR { get; set; } = default!;
    [Inject] private NavigationManager Nav { get; set; } = default!;

    private List<WhatsappInboxItemDto>? Inbox;

    protected override async Task OnInitializedAsync()
    {
        Inbox = new List<WhatsappInboxItemDto>();

        SignalR.OnInboxUpdated += UpdateInbox;
        await SignalR.StartAsync("https://localhost:7141/hubs/whatsapp-chat");

        var response = await Repository.GetAsync<List<WhatsappInboxItemDto>>(
            "/api/whatsapp/webhook/whatsapp/inbox"
        );

        Inbox = response.Response ?? new();
    }

    private void UpdateInbox(WhatsappInboxItemDto item)
    {
        var chat = Inbox!.FirstOrDefault(x => x.PhoneNumber == item.PhoneNumber);

        if (chat == null)
        {
            Inbox.Insert(0, item);
        }
        else
        {
            chat.LastMessage = item.LastMessage;
            chat.LastDate = item.LastDate;
            chat.UnreadCount++;
            Inbox.Remove(chat);
            Inbox.Insert(0, chat);
        }

        InvokeAsync(StateHasChanged);
    }

    private void OpenChat(string phone)
        => Nav.NavigateTo($"/whatsapp/chat/{phone}");

    public async ValueTask DisposeAsync()
    {
        SignalR.OnInboxUpdated -= UpdateInbox;
    }
}