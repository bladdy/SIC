using Microsoft.AspNetCore.SignalR.Client;
using SIC.Shared.DTOs;

namespace SIC.Frontend.Services;

public class SignalRService
{
    private HubConnection? _connection;

    private string? _phone;
    private string? _eventCode;
    private string? _chatContact;

    public event Action<RealtimeChatMessageDto>? OnMessageReceived;

    public event Action<InboxConversationDto>? OnInboxUpdated;

    public async Task StartAsync(string hubUrl)
    {
        if (_connection != null)
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
            if (_phone != null)
                await JoinPhoneInbox(_phone);

            if (_phone != null && _eventCode != null)
                await JoinEventInbox(_phone, _eventCode);

            if (_phone != null && _chatContact != null)
                await JoinChat(_phone, _chatContact);
        };

        await _connection.StartAsync();
    }

    // 📥 Inbox general del número
    public async Task JoinPhoneInbox(string phone)
    {
        _phone = phone;

        if (_connection?.State == HubConnectionState.Connected)
        {
            await _connection.InvokeAsync(
                "JoinPhoneInbox",
                phone
            );
        }
    }

    // 📥 Inbox por evento
    public async Task JoinEventInbox(string phone, string eventCode)
    {
        _phone = phone;
        _eventCode = eventCode;

        if (_connection?.State == HubConnectionState.Connected)
        {
            await _connection.InvokeAsync(
                "JoinEventInbox",
                phone,
                eventCode
            );
        }
    }

    public async Task LeaveEventInbox(string ownerPhone, string eventCode)
    {
        _phone = ownerPhone;
        _eventCode = eventCode;
        if (_connection?.State == HubConnectionState.Connected)
        {
            await _connection.SendAsync(
                "LeaveEventInbox",
                ownerPhone,
                eventCode
            );
        }
    }

    // 💬 Chat activo
    public async Task JoinChat(string phone, string contactPhone)
    {
        _phone = phone;
        _chatContact = contactPhone;

        if (_connection?.State == HubConnectionState.Connected)
        {
            await _connection.InvokeAsync(
                "JoinChat",
                phone,
                contactPhone
            );
        }
    }

    public async Task LeaveChat(string phone, string contactPhone)
    {
        _chatContact = null;

        if (_connection?.State == HubConnectionState.Connected)
        {
            await _connection.InvokeAsync(
                "LeaveChat",
                phone,
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
    }
}