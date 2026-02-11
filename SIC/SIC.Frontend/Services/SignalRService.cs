using Microsoft.AspNetCore.SignalR.Client;
using SIC.Shared.DTOs;

namespace SIC.Frontend.Services;

public class SignalRService
{
    private HubConnection? _connection;

    private string? _ownerPhone;
    private string? _eventCode;
    private string? _chatContact;

    public event Action<RealtimeChatMessageDto>? OnMessageReceived;

    public event Action<InboxConversationDto>? OnInboxUpdated;

    public async Task StartAsync(string hubUrl)
    {
        if (_connection != null &&
            _connection.State == HubConnectionState.Connected)
            return;

        _connection = new HubConnectionBuilder()
            .WithUrl(hubUrl)
            .WithAutomaticReconnect()
            .Build();

        _connection.On<RealtimeChatMessageDto>(
            "NewMessage",
            msg => OnMessageReceived?.Invoke(msg)
        );

        _connection.On<InboxConversationDto>(
            "InboxUpdated",
            inbox => OnInboxUpdated?.Invoke(inbox)
        );

        // 🔁 Rejoin automático después de reconectar
        _connection.Reconnected += async _ =>
        {
            if (_connection.State != HubConnectionState.Connected)
                return;

            if (!string.IsNullOrEmpty(_ownerPhone) &&
                !string.IsNullOrEmpty(_eventCode))
            {
                await _connection.InvokeAsync(
                    "JoinEventInbox",
                    _ownerPhone,
                    _eventCode
                );
            }

            if (!string.IsNullOrEmpty(_ownerPhone) &&
                !string.IsNullOrEmpty(_chatContact))
            {
                await _connection.InvokeAsync(
                    "JoinChat",
                    _ownerPhone,
                    _chatContact
                );
            }
        };

        await _connection.StartAsync();
    }

    // 📥 Inbox por evento
    public async Task JoinEventInbox(string ownerPhone, string eventCode)
    {
        _ownerPhone = ownerPhone;
        _eventCode = eventCode;

        if (_connection?.State == HubConnectionState.Connected)
        {
            await _connection.InvokeAsync(
                "JoinEventInbox",
                ownerPhone,
                eventCode
            );
        }
    }

    public async Task LeaveEventInbox(string ownerPhone, string eventCode)
    {
        if (_eventCode == eventCode)
            _eventCode = null;

        if (_connection?.State == HubConnectionState.Connected)
        {
            await _connection.InvokeAsync(
                "LeaveEventInbox",
                ownerPhone,
                eventCode
            );
        }
    }

    // 💬 Chat activo
    public async Task JoinChat(string ownerPhone, string contactPhone)
    {
        _ownerPhone = ownerPhone;
        _chatContact = contactPhone;

        if (_connection?.State == HubConnectionState.Connected)
        {
            await _connection.InvokeAsync(
                "JoinChat",
                ownerPhone,
                contactPhone
            );
        }
    }

    public async Task LeaveChat(string ownerPhone, string contactPhone)
    {
        if (_chatContact == contactPhone)
            _chatContact = null;

        if (_connection?.State == HubConnectionState.Connected)
        {
            await _connection.InvokeAsync(
                "LeaveChat",
                ownerPhone,
                contactPhone
            );
        }
    }

    public async Task StopAsync()
    {
        if (_connection != null)
        {
            await _connection.StopAsync();
            await _connection.DisposeAsync();
            _connection = null;
        }

        _ownerPhone = null;
        _eventCode = null;
        _chatContact = null;
    }
}