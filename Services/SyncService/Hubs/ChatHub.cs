using Microsoft.AspNetCore.SignalR;
using SyncService.Core.Interfaces;
using SyncService.Domain.Requests.ChatRequests;

namespace SyncService.Hubs;

public class ChatHub : Hub
{
    private readonly IChatSyncService _chatService;
    private readonly ILogger<ChatHub> _logger;

    public ChatHub(IChatSyncService chatService, ILogger<ChatHub> logger)
    {
        _chatService = chatService;
        _logger = logger;
    }

    public async Task SendMessage(string roomId, string userId, string username, string message)
    {
        try
        {
            var sendReq = new AddChat_Req()
            {
                RoomId = roomId,
                UserId = userId,
                Username = username,
                Message = message

            };

            var result = await _chatService.AddChatAsync(sendReq,CancellationToken.None);
            if (result.IsSuccess)
            {
                await Clients.Group(roomId).SendAsync("ReceiveMessage", username, message, DateTimeOffset.UtcNow);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending message in room {RoomId}", roomId);
            throw;
        }
    }

    public async Task JoinChatRoom(string roomId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
    }

    public async Task LeaveChatRoom(string roomId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId);
    }
}