using Microsoft.AspNetCore.SignalR.Client;
using SIC.Shared.DTOs;

namespace SIC.Frontend.Services;

public class SignalRService
{
    private HubConnection? _connection;

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

        _connection.On<RealtimeChatMessageDto>("NewMessage",
            msg => OnMessageReceived?.Invoke(msg));

        _connection.On<InboxConversationDto>("InboxUpdated",
            inbox => OnInboxUpdated?.Invoke(inbox));

        await _connection.StartAsync();
    }

    public async Task JoinChat(string phone)
    {
        if (_connection?.State == HubConnectionState.Connected)
            await _connection.InvokeAsync("JoinChat", phone);
    }

    public async Task LeaveChat(string phone)
    {
        if (_connection?.State == HubConnectionState.Connected)
            await _connection.InvokeAsync("LeaveChat", phone);
    }
}