using Microsoft.AspNetCore.Components;
using SIC.Frontend.Repositories;
using SIC.Frontend.Services;
using SIC.Shared.DTOs;

namespace SIC.Frontend.Pages.Whatsapp.Components;

public partial class WhatsappInbox : IAsyncDisposable
{
    [Inject] private IRepository Repository { get; set; } = default!;
    [Inject] private SignalRService SignalR { get; set; } = default!;

    [Parameter] public string? EventCode { get; set; }
    [Parameter] public string OwnerPhone { get; set; } = null!;
    [Parameter] public EventCallback OnBack { get; set; }

    private List<InboxConversationDto> Inbox = new();
    private string? SelectedPhone;

    protected override async Task OnInitializedAsync()
    {
        SignalR.OnInboxUpdated += UpdateInbox;
    }

    protected override async Task OnParametersSetAsync()
    {
        if (string.IsNullOrWhiteSpace(EventCode))
            return;

        var response = await Repository.GetAsync<List<InboxConversationDto>>(
            $"/api/whatsapp/webhook/whatsapp/inbox/{EventCode}"
        );

        Inbox = response.Response ?? new();
    }

    private void SelectChat(string phone)
    {
        SelectedPhone = phone;
    }

    private async Task Back()
    {
        if (SelectedPhone != null)
        {
            SelectedPhone = null;
        }
        else
        {
            await OnBack.InvokeAsync();
        }
    }

    private void UpdateInbox(InboxConversationDto item)
    {
        if (item.EventCode != EventCode)
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