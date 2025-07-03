using Microsoft.AspNetCore.SignalR;
using SyncService.Core.Interfaces;
using System.Collections.Concurrent;
using System.Security.Claims;

namespace SyncService.Hubs;

public class RoomSyncHub : Hub
{
    private readonly IRoomSyncService _roomService;
    private readonly ILogger<RoomSyncHub> _logger;
    private static readonly ConcurrentDictionary<string, (string RoomId, string UserId, bool IsHost)> _connections = new();

    public RoomSyncHub(IRoomSyncService roomService, ILogger<RoomSyncHub> logger)
    {
        _roomService = roomService ?? throw new ArgumentNullException(nameof(roomService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task JoinRoom(string roomId, string userId, bool isHost)
    {
        try
        {
           
           
            _connections[Context.ConnectionId] = (roomId, userId, isHost);

       
            await Groups.AddToGroupAsync(Context.ConnectionId, roomId);

     
            await Clients.Group(roomId).SendAsync("UserJoined", userId);


            var result = await _roomService.JoinRoomAsync(roomId, userId, CancellationToken.None);
            if (!result.IsSuccess)
            {
                _logger.LogWarning("Failed to join room {RoomId} for user {UserId}: {Message}", roomId, userId, result.Message);
                throw new HubException(result.Message);
            }



            _logger.LogInformation("User {UserId} joined room {RoomId} as {Role}",
                userId, roomId, isHost ? "host" : "participant");

         
            var playbackState = await _roomService.GetPlaybackStateForRoom(roomId,CancellationToken.None);
            if (playbackState != null)
            {
                var data = playbackState.Data;
                await Clients.Caller.SendAsync("InitializePlayback",
                    data.VideoId,
                    data.Position,
                    data.IsPaused);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error joining room {RoomId}", roomId);
            throw;
        }
    }

    public async Task HostSetVideo(string roomId, string videoId,string userId)
    {
        if (!ValidateHost(roomId))
        {
            _logger.LogWarning("Non-host attempted to set video for room {RoomId}", roomId);
            throw new HubException("Only the host can change videos");
        }

        try
        {
         
            await _roomService.UpdateCurrentVideoIdAsync(roomId, videoId,userId,CancellationToken.None);

            
            await Clients.Group(roomId).SendAsync("ReceiveNewVideoId", videoId);

          
            await Clients.Group(roomId).SendAsync("ReceivePlaybackState", 0, true);

            _logger.LogInformation("Host changed video in room {RoomId} to {VideoId}", roomId, videoId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting video for room {RoomId}", roomId);
            throw;
        }
    }

    public async Task UpdatePlaybackState(string roomId, double position, bool isPaused, string videoId)
    {
        _logger.LogInformation("UpdatePlaybackState invoked for room {RoomId} at position {Position}, paused={IsPaused}, video={VideoId}",
    roomId, position, isPaused, videoId);

        Console.WriteLine($"UpdatePlaybackState invoked for room {roomId} at position {position}, paused={isPaused}, video={videoId}");

        if (!ValidateHost(roomId))
        {
            _logger.LogInformation("UpdatePlayBackstate: Isvalid = false");
            return;
        }

        try
        {
         
            await _roomService.UpdatePlaybackStateAsync(roomId, position, isPaused, videoId,CancellationToken.None);

       
            await Clients.OthersInGroup(roomId)
                .SendAsync("ReceivePlaybackState", position, isPaused);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Playback update failed");
            throw new HubException("Update failed");
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        try
        {
            if (_connections.TryRemove(Context.ConnectionId, out var data))
            {
                var (roomId, userId, isHost) = data;

               
                await _roomService.LeaveRoomAsync(roomId, userId,CancellationToken.None);

               
                await Clients.Group(roomId).SendAsync("UserLeft", userId);

             
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId);

                _logger.LogInformation("User {UserId} disconnected from room {RoomId}", userId, roomId);

             
                if (isHost)
                {
                    await Clients.Group(roomId).SendAsync("HostDisconnected");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during disconnection handling");
        }
        finally
        {
            await base.OnDisconnectedAsync(exception);
        }
    }

    private bool ValidateHost(string roomId)
    {
        if (_connections.TryGetValue(Context.ConnectionId, out var data))
        {
            return data.RoomId == roomId && data.IsHost;
        }
        return false;
    }
}