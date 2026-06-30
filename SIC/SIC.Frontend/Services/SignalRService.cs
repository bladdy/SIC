using Microsoft.AspNetCore.SignalR.Client;
using SIC.Shared.DTOs;

namespace SIC.Frontend.Services;

public class SignalRService
{
    private HubConnection? _connection;
    private readonly string _hubUrl;

    private string? _ownerPhone;
    private string? _eventCode;
    private string? _chatContact;
    private string? _userId;

    public event Action<RealtimeChatMessageDto>? OnMessageReceived;
    public event Action<InboxConversationDto>? OnInboxUpdated;
    public event Action<string>? OnNotification;

    public SignalRService(string backendUrl)
    {
        _hubUrl = $"{backendUrl.TrimEnd('/')}/hubs/whatsapp-chat";
    }

    public async Task StartAsync()
    {
        if (_connection != null &&
            _connection.State == HubConnectionState.Connected)
            return;

        _connection = new HubConnectionBuilder()
            .WithUrl(_hubUrl)
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

        _connection.On<string>(
            "Notification",
            message =>
            {
                OnNotification?.Invoke(message);
            });

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

            if (!string.IsNullOrWhiteSpace(_userId))
            {
                await _connection.InvokeAsync(
                    "JoinNotifications",
                    _userId);
            }
        };

        await _connection.StartAsync();
    }

    private async Task EnsureConnectedAsync()
    {
        if (_connection == null || _connection.State != HubConnectionState.Connected)
        {
            await StartAsync();
        }
    }

    public async Task JoinEventInbox(string ownerPhone, string eventCode)
    {
        _ownerPhone = ownerPhone;
        _eventCode = eventCode;

        await EnsureConnectedAsync();

        if (_connection?.State == HubConnectionState.Connected)
        {
            await _connection.InvokeAsync(
                "JoinEventInbox",
                ownerPhone,
                eventCode
            );
        }
    }

    public async Task JoinNotifications(string userId)
    {
        _userId = userId;

        Console.WriteLine($"JoinNotifications: {userId}");

        await EnsureConnectedAsync();

        if (_connection != null)
        {
            await _connection.InvokeAsync(
                "JoinNotifications",
                userId);
        }
    }

    public async Task JoinChat(string ownerPhone, string contactPhone)
    {
        _ownerPhone = ownerPhone;
        _chatContact = contactPhone;

        await EnsureConnectedAsync();

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
        _userId = null;
    }
}
