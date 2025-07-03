using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using SyncService.Core.Interfaces;
using SyncService.Domain.Requests.RoomRequests;
using SyncService.Hubs;
using System.Security.Claims;

namespace SyncService.Endpoints
{
    public static class RoomSyncEndpoints
    {
        public static IEndpointRouteBuilder MapRoomSyncEndpoints(this IEndpointRouteBuilder app)
        {


            app.MapGet("api/sync/rooms/getPublicRooms", async (int page, int pageSize, CancellationToken cancellation, IRoomSyncService repo) =>
            {
                var result = await repo.GetPublicRoomsAsync(page, cancellation, pageSize);

                if (result.IsSuccess)
                {
                    return Results.Ok(result);
                }

                return Results.BadRequest(result);

            }).RequireRateLimiting("basic");



            app.MapGet("api/sync/rooms/getRoomById/{roomId}", async (string roomId, CancellationToken cancellation, IRoomSyncService repo) =>
            {
                var result = await repo.GetRoomByIdsync(roomId, cancellation);

                if (result.IsSuccess)
                {
                    return Results.Ok(result);
                }

                return Results.NotFound(result);

            });

            app.MapPost("api/sync/rooms/createRoom", async (CreateRoom_Req req, CancellationToken cancellation, IRoomSyncService repo) =>
            {
                var result = await repo.CreateRoomAsync(req, cancellation);

                if (result.IsSuccess)
                {
                    return Results.Ok(result);
                }

                return Results.BadRequest(result);

            }).RequireAuthorization("NotAnon").RequireRateLimiting("basic");

            app.MapPut("api/sync/rooms/updateRoom", async (UpdateRoom_Req req, CancellationToken cancellation, IRoomSyncService repo) =>
            {
                var result = await repo.UpdateRoomAsync(req, cancellation);

                if (result.IsSuccess)
                {
                    return Results.Ok(result);
                }

                return Results.BadRequest(result);

            }).RequireAuthorization("NotAnon");

            app.MapDelete("api/sync/rooms/{roomId}", async (HttpContext ctx, string roomId, CancellationToken cancellation, IRoomSyncService repo) =>
            {
                var userId = ctx.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Results.Unauthorized();
                }

                var isAdmin = ctx.User.IsInRole("Admin");
                var result = await repo.DeleteRoomAsync(roomId, userId, isAdmin, cancellation);

                if (result.IsSuccess)
                {
                    return Results.Ok(result);
                }

                return Results.BadRequest(result);

            }).RequireAuthorization("NotAnon");

            app.MapPost("api/sync/rooms/generateNewRoomPass/{roomId}", async (HttpContext ctx, string roomId, CancellationToken cancellation, IRoomSyncService repo) =>
            {
                var userId = ctx.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Results.Unauthorized();
                }
                var result = await repo.GenerateNewRoomPassAsync(roomId, userId, cancellation);

                if (result.IsSuccess)
                {
                    return Results.Ok(result);
                }

                return Results.BadRequest(result);

            }).RequireAuthorization("NotAnon").RequireRateLimiting("basic");

            app.MapPost("api/sync/rooms/joinRoom/{roomId}/{userId}", async (string roomId, string userId, CancellationToken cancellation, IRoomSyncService repo) =>
            {
                var result = await repo.JoinRoomAsync(roomId, userId, cancellation);

                if (result.IsSuccess)
                {
                    return Results.Ok(result);
                }

                return Results.BadRequest(result);

            }).RequireRateLimiting("basic");

            app.MapPut("api/sync/rooms/leaveRoom/{roomId}/{userId}", async (string roomId, string userId, CancellationToken cancellation, IRoomSyncService repo) =>
            {
                var result = await repo.LeaveRoomAsync(roomId, userId, cancellation);

                if (result.IsSuccess)
                {
                    return Results.Ok(result);
                }

                return Results.BadRequest(result);

            }).RequireRateLimiting("basic");

            app.MapGet("api/sync/rooms/roomExists/{roomId}", async (string roomId, CancellationToken cancellation, IRoomSyncService repo) =>
            {
                var result = await repo.RoomExistsAsync(roomId, cancellation);

                if (result.IsSuccess)
                {
                    return Results.Ok(result);
                }

                return Results.BadRequest(result);

            });

            app.MapGet("api/sync/rooms/searchRooms/{searchTerm}", async (string searchTerm, CancellationToken cancellation, int page, int pageSize, IRoomSyncService repo) =>
            {
                var result = await repo.SearchRoomsAsync(searchTerm, cancellation, page, pageSize);

                if (result.IsSuccess)
                {
                    return Results.Ok(result);
                }

                return Results.BadRequest(result);

            });

            app.MapGet("api/sync/rooms/getRoomsByHost/{hostId}", async (string hostId, CancellationToken cancellation, IRoomSyncService repo) =>
            {
                var result = await repo.GetRoomsByHostAsync(hostId, cancellation);

                if (result.IsSuccess)
                {
                    return Results.Ok(result);
                }

                return Results.BadRequest(result);

            });

            app.MapPost("api/sync/rooms/updatePlaybackState", async (UpdatePlaybackStateInRoomRequest request, CancellationToken cancellation, IRoomSyncService repo, IHubContext<RoomSyncHub> hubContext) =>
            {
                var result = await repo.UpdatePlaybackStateAsync(request.RoomId, request.Position, request.IsPaused, request.VideoId, cancellation);

                if (!result.IsSuccess)
                {
                    return Results.BadRequest(result);
                }



                await hubContext.Clients.Group(request.RoomId).SendAsync("ReceivePlaybackState", new
                {
                    Position = request.Position,
                    IsPaused = request.IsPaused,
                    VideoId = request.VideoId
                });

                return Results.Ok(result);

            });

            app.MapGet("api/sync/rooms/getPlaybackState", async (string roomId, CancellationToken cancellation, IRoomSyncService repo) =>
            {
                var result = await repo.GetPlaybackStateForRoom(roomId, cancellation);

                if (result.IsSuccess)
                {
                    return Results.Ok(result);
                }

                return Results.BadRequest(result);

            });

            app.MapPost("api/sync/rooms/validateRoomPass/{roomId}", async (string roomId, AccessCodeRequest request, CancellationToken cancellation, IRoomSyncService repo) =>
            {
                var result = await repo.ValidateRoomPassAsync(roomId, request.AccessCode, cancellation);

                if (result.IsSuccess)
                {
                    return Results.Ok(result);
                }

                return Results.BadRequest(result);

            }).RequireRateLimiting("basic");

            app.MapGet("api/sync/rooms/isUserInRoom/{roomId}/{userId}", async (string roomId, string userId, CancellationToken cancellation, IRoomSyncService repo) =>
            {
                var result = await repo.IsUserInRoomAsync(roomId, userId, cancellation);

                if (result.IsSuccess)
                {
                    return Results.Ok(result);
                }

                return Results.BadRequest(result);

            });

            app.MapGet("api/sync/rooms/isRoomFull/{roomId}", async (string roomId, CancellationToken cancellation, IRoomSyncService repo) =>
            {
                var result = await repo.IsRoomFullAsync(roomId, cancellation);

                if (result.IsSuccess)
                {
                    return Results.Ok(result);
                }

                return Results.BadRequest(result);

            });

            app.MapPatch("api/sync/rooms/makeRoomPrivate/{roomId}", async (HttpContext ctx, string roomId, CancellationToken cancellation, IRoomSyncService repo) =>
            {
                var userId = ctx.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Results.Unauthorized();
                }
                var result = await repo.MakeRoomPrivateAsync(roomId, userId, cancellation);

                if (result.IsSuccess)
                {
                    return Results.Ok(result);
                }

                return Results.BadRequest(result);

            }).RequireAuthorization("NotAnon");

            app.MapPatch("api/sync/rooms/makeRoomPublic/{roomId}", async (HttpContext ctx, string roomId, CancellationToken cancellation, IRoomSyncService repo) =>
            {
                var userId = ctx.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Results.Unauthorized();
                }
                var result = await repo.MakeRoomPublicAsync(roomId, userId, cancellation);

                if (result.IsSuccess)
                {
                    return Results.Ok(result);
                }

                return Results.BadRequest(result);

            }).RequireAuthorization("NotAnon");

            app.MapGet("api/sync/rooms/getAccessCode/{roomId}/{userId}", async (string roomId, string userId, CancellationToken cancellation, IRoomSyncService repo) =>
            {
                var result = await repo.GetAccessCode(roomId, userId, cancellation);

                if (result.IsSuccess)
                {
                    return Results.Ok(result);
                }

                return Results.BadRequest(result);

            }).RequireAuthorization("NotAnon").RequireRateLimiting("basic");

            app.MapPost("api/sync/rooms/updateCurrentVideoId/{roomId}", async (HttpContext ctx, string roomId, [FromBody] UpdateVideoIdRequest request,
                                                                        CancellationToken cancellation, IRoomSyncService repo) =>
            {
                var userId = ctx.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Results.Unauthorized();
                }

                var result = await repo.UpdateCurrentVideoIdAsync(roomId, request.VideoId,userId, cancellation);

                if (result.IsSuccess)
                {
                    return Results.Ok(result);
                }

                return Results.BadRequest(result);

            }).RequireAuthorization("NotAnon");





            return app;
        }
    }
}
