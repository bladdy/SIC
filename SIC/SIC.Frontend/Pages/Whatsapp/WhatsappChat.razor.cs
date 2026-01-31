using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using SIC.Frontend.Repositories;
using SIC.Frontend.Services;
using SIC.Shared.DTOs;
using SIC.Shared.Response;

namespace SIC.Frontend.Pages.Whatsapp;

public partial class WhatsappChat
    : ComponentBase, IAsyncDisposable
{
    [Parameter, EditorRequired]
    public string PhoneNumber { get; set; } = null!;

    [Inject] private IJSRuntime JS { get; set; } = default!;
    [Inject] private IRepository Repository { get; set; } = default!;
    [Inject] private SignalRService SignalR { get; set; } = default!;
    [Inject] private SweetAlertService Swal { get; set; } = default!;

    protected List<RealtimeChatMessageDto> Messages { get; set; } = new();
    protected string NewMessage { get; set; } = string.Empty;
    protected ElementReference ChatBodyRef;

    private string? _currentPhone;

    protected override async Task OnInitializedAsync()
    {
        SignalR.OnMessageReceived += OnMessage;
        await SignalR.StartAsync("https://localhost:7141/hubs/whatsapp-chat");
    }

    protected override async Task OnParametersSetAsync()
    {
        // ?? solo si cambió el chat
        if (_currentPhone == PhoneNumber)
            return;

        if (_currentPhone != null)
        {
            await SignalR.LeaveChat(_currentPhone);
        }

        _currentPhone = PhoneNumber;

        Messages.Clear();

        await SignalR.JoinChat(PhoneNumber);

        var response =
            await Repository.GetAsync<ActionResponse<List<RealtimeChatMessageDto>>>(
                $"/api/whatsapp/chat/{PhoneNumber}"
            );

        if (response.Error || response.Response?.Success != true)
        {
            await Swal.FireAsync(
                "Error",
                "No se pudo cargar el chat",
                SweetAlertIcon.Error
            );
            return;
        }

        Messages = response.Response.Result?.ToList() ?? new();
        await InvokeAsync(StateHasChanged);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (Messages.Count > 0)
        {
            await JS.InvokeVoidAsync("scrollToBottom", ChatBodyRef);
        }
    }

    protected void OnMessage(RealtimeChatMessageDto msg)
    {
        if (msg.PhoneNumber == PhoneNumber)
        {
            Messages.Add(msg);
            InvokeAsync(StateHasChanged);
        }
    }

    protected async Task HandleEnter(KeyboardEventArgs e)
    {
        if (e.Key == "Enter")
            await SendMessage();
    }

    protected async Task SendMessage()
    {
        if (string.IsNullOrWhiteSpace(NewMessage))
            return;

        await Repository.PostAsync("/api/whatsapp/chat/send", new
        {
            PhoneNumber,
            Message = NewMessage
        });

        NewMessage = string.Empty;
    }

    public async ValueTask DisposeAsync()
    {
        SignalR.OnMessageReceived -= OnMessage;

        if (_currentPhone != null)
            await SignalR.LeaveChat(_currentPhone);
    }
}