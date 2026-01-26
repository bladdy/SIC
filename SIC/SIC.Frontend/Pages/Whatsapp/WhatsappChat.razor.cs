using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using SIC.Frontend.Repositories;
using SIC.Frontend.Services;
using SIC.Shared.DTOs;
using SIC.Shared.Response;

namespace SIC.Frontend.Pages.Whatsapp;

public partial class WhatsappChat : IAsyncDisposable
{
    [Parameter] public string PhoneNumber { get; set; } = null!;

    [Inject] private IRepository Repository { get; set; } = default!;
    [Inject] private SignalRService SignalR { get; set; } = default!;
    [Inject] private SweetAlertService Swal { get; set; } = default!;

    private List<RealtimeChatMessageDto> Messages = new();
    private string NewMessage = "";

    protected override async Task OnInitializedAsync()
    {
        SignalR.OnMessageReceived += OnMessage;

        await SignalR.StartAsync("https://localhost:7141/hubs/whatsapp-chat");
        await SignalR.JoinChat(PhoneNumber);

        var response = await Repository.GetAsync<ActionResponse<List<RealtimeChatMessageDto>>>(
            $"/api/whatsapp/chat/{PhoneNumber}"
        );

        if (response.Error || response.Response == null || !response.Response.Success)
        {
            await Swal.FireAsync("Error", "No se pudo cargar el chat", SweetAlertIcon.Error);
            return;
        }

        Messages = response.Response.Result?.ToList() ?? new();
    }

    private void OnMessage(RealtimeChatMessageDto msg)
    {
        if (msg.PhoneNumber == PhoneNumber)
        {
            Messages.Add(msg);
            InvokeAsync(StateHasChanged);
        }
    }

    private async Task SendMessage()
    {
        if (string.IsNullOrWhiteSpace(NewMessage))
            return;

        await Repository.PostAsync("/api/whatsapp/chat/send", new
        {
            PhoneNumber,
            Message = NewMessage
        });

        NewMessage = "";
    }

    public async ValueTask DisposeAsync()
    {
        SignalR.OnMessageReceived -= OnMessage;
        await SignalR.LeaveChat(PhoneNumber);
    }
}