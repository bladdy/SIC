using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using SIC.Frontend.Repositories;
using SIC.Frontend.Services;
using SIC.Shared.DTOs;
using SIC.Shared.Response;
using System.Net;
using System.Text.RegularExpressions;

namespace SIC.Frontend.Pages.Whatsapp.Components;

public partial class WhatsappChat : ComponentBase, IAsyncDisposable
{
    [Parameter, EditorRequired]
    public string OwnerPhone { get; set; } = null!; // 📌 número del inbox

    [Parameter, EditorRequired]
    public string PhoneNumber { get; set; } = null!; // 📌 contacto

    [Inject] private IJSRuntime JS { get; set; } = default!;
    [Inject] private IRepository Repository { get; set; } = default!;
    [Inject] private SignalRService SignalR { get; set; } = default!;
    [Inject] private SweetAlertService Swal { get; set; } = default!;

    protected List<RealtimeChatMessageDto> Messages { get; set; } = new();
    protected string NewMessage { get; set; } = string.Empty;
    protected ElementReference ChatBodyRef;

    private string? _currentContact;

    protected override async Task OnInitializedAsync()
    {
        SignalR.OnMessageReceived += OnMessage;

        await SignalR.StartAsync(
            "https://invboxv-app.com/hubs/whatsapp-chat"
        // "https://localhost:7141/hubs/whatsapp-chat"
        );
    }

    protected override async Task OnParametersSetAsync()
    {
        // 🔁 solo si cambió el contacto
        if (_currentContact == PhoneNumber)
            return;

        if (_currentContact != null)
        {
            await SignalR.LeaveChat(OwnerPhone, _currentContact);
        }

        _currentContact = PhoneNumber;
        Messages.Clear();

        // 🔥 Join correcto al grupo del chat
        await SignalR.JoinChat(OwnerPhone, PhoneNumber);

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

        var unseenIds = Messages
            .Where(m =>
                !string.IsNullOrEmpty(m.MessageId)
                && m.Direction == "IN"
                && m.Status != "seen"
            )
            .Select(m => m.MessageId!)
            .ToList();

        if (unseenIds.Count > 0)
        {
            MarkMessagesAsync(unseenIds);
        }

        await InvokeAsync(StateHasChanged);
    }

    private MarkupString FormatWhatsAppText(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return new MarkupString("");

        // Seguridad
        text = WebUtility.HtmlEncode(text);

        // Saltos de línea
        text = text.Replace("\\n", "<br>")
                   .Replace("\r\n", "<br>")
                   .Replace("\n", "<br>");

        // Negrita
        text = Regex.Replace(
            text,
            @"\*([^\*]+)\*",
            "<strong>$1</strong>");

        // Cursiva
        text = Regex.Replace(
            text,
            @"_([^_]+)_",
            "<em>$1</em>");

        // Tachado
        text = Regex.Replace(
            text,
            @"~([^~]+)~",
            "<del>$1</del>");

        // URLs al final
        text = Regex.Replace(
            text,
            @"(https?:\/\/[^\s<]+)",
            m =>
            {
                var url = m.Value;

                return $"<a href=\"{url}\" target=\"_blank\" rel=\"noopener noreferrer\" class=\"chat-link\">{url}</a>";
            });

        return new MarkupString(text);
    }

    private void MarkMessagesAsync(List<string> messageIds)
    {
        Repository.PutAsync(
           "/api/whatsapp/chat/mark-seen",
           new MarkMessagesAsSeenDto
           {
               Psid = messageIds
           }
       );
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (Messages.Count > 0)
        {
            await JS.InvokeVoidAsync("scrollToBottom", ChatBodyRef);
        }
    }

    private void OnMessage(RealtimeChatMessageDto msg)
    {
        // 🔐 solo mensajes del chat abierto
        if (msg.PhoneNumber != PhoneNumber)
            return;

        Messages.Add(msg);
        InvokeAsync(StateHasChanged);
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

        var messageText = NewMessage;

        // 🔥 1️⃣ Pintar inmediatamente el mensaje en el chat
        var tempMessage = new RealtimeChatMessageDto
        {
            PhoneNumber = PhoneNumber,
            Direction = "OUT",
            MessageType = "text",
            Content = messageText,
            Timestamp = DateTime.UtcNow,
            Status = "sending"
        };

        Messages.Add(tempMessage);
        await InvokeAsync(StateHasChanged);

        NewMessage = string.Empty;

        // 🔥 2️⃣ Enviar al backend
        await Repository.PostAsync(
            "/api/whatsapp/chat/send",
            new
            {
                PhoneNumber,
                Message = messageText
            }
        );
    }

    public async ValueTask DisposeAsync()
    {
        SignalR.OnMessageReceived -= OnMessage;

        if (_currentContact != null)
        {
            await SignalR.LeaveChat(OwnerPhone, _currentContact);
        }
    }
}