using RoomService.Core.Interfaces;
using RoomService.Domain.Requests;

namespace RoomService.Endpoints
{
    public static class RoomEndpoints
    {
        public static WebApplication MapRoomEndpoints(this WebApplication app)
        {
            // Map the endpoints for room management
            app.MapGet("api/rooms/getAllRooms",async (CancellationToken cancellation,IRoomsServices repo) =>
            {
                var result = await repo.GetAllRoomsAsync(cancellation);

                if(result.IsSuccess)
                {
                    return Results.Ok(new {Success = result.IsSuccess,Data = result.Data,Message = result.Message});
                }

                return Results.BadRequest(new { Success = result.IsSuccess, Data = result.Data, Message = result.Message });

            });

            app.MapGet("api/rooms/getPublicRooms", async (int page,int pageSize,CancellationToken cancellation,IRoomsServices repo) =>
            {
                var result = await repo.GetPublicRoomsAsync(page, cancellation,pageSize);

                if (result.IsSuccess)
                {
                    return Results.Ok(new { Success = result.IsSuccess, Data = result.Data, Message = result.Message });
                }

                return Results.BadRequest(new { Success = result.IsSuccess, Data = result.Data, Message = result.Message });

            });

            app.MapGet("api/rooms/getPrivateRooms", async (CancellationToken cancellation,IRoomsServices repo) =>
            {
                var result = await repo.GetPrivateRoomsAsync(cancellation);

                if (result.IsSuccess)
                {
                    return Results.Ok(new { Success = result.IsSuccess, Data = result.Data, Message = result.Message });
                }

                return Results.BadRequest(new { Success = result.IsSuccess, Data = result.Data, Message = result.Message });

            });

            app.MapGet("api/rooms/getRoomById/{roomId}", async (string roomId,CancellationToken cancellation ,IRoomsServices repo) =>
            {
                var result = await repo.GetRoomByIdsync(roomId,cancellation);

                if (result.IsSuccess)
                {
                    return Results.Ok(new { Success = result.IsSuccess, Data = result.Data, Message = result.Message });
                }

                return Results.BadRequest(new { Success = result.IsSuccess, Data = result.Data, Message = result.Message });

            });

            app.MapPost("api/rooms/createRoom", async (CreateRoom_Req req,CancellationToken cancellation ,IRoomsServices repo) =>
            {
                var result = await repo.CreateRoomAsync(req,cancellation);

                if (result.IsSuccess)
                {
                    return Results.Ok(new { Success = result.IsSuccess, Data = result.Data, Message = result.Message });
                }

                return Results.BadRequest(new { Success = result.IsSuccess, Data = result.Data, Message = result.Message });

            });

            app.MapPut("api/rooms/updateRoom", async (UpdateRoom_Req req, CancellationToken cancellation, IRoomsServices repo) =>
            {
                var result = await repo.UpdateRoomAsync(req, cancellation);

                if (result.IsSuccess)
                {
                    return Results.Ok(new { Success = result.IsSuccess, Data = result.Data, Message = result.Message });
                }

                return Results.BadRequest(new { Success = result.IsSuccess, Data = result.Data, Message = result.Message });

            });

            app.MapDelete("api/rooms/{roomId}", async (string roomId, CancellationToken cancellation, IRoomsServices repo) =>
            {
                var result = await repo.DeleteRoomAsync(roomId, cancellation);

                if (result.IsSuccess)
                {
                    return Results.Ok(new { Success = result.IsSuccess, Data = result.Data, Message = result.Message });
                }

                return Results.BadRequest(new { Success = result.IsSuccess, Data = result.Data, Message = result.Message });

            });

            app.MapPost("api/rooms/generateNewRoomPass/{roomId}", async (string roomId, CancellationToken cancellation, IRoomsServices repo) =>
            {
                var result = await repo.GenerateNewRoomPassAsync(roomId, cancellation);

                if (result.IsSuccess)
                {
                    return Results.Ok(new { Success = result.IsSuccess, Data = result.Data, Message = result.Message });
                }

                return Results.BadRequest(new { Success = result.IsSuccess, Data = result.Data, Message = result.Message });

            });

            app.MapPost("api/rooms/joinRoom/{roomId}/{userId}", async (string roomId, string userId,CancellationToken cancellation ,IRoomsServices repo) =>
            {
                var result = await repo.JoinRoomAsync(roomId, userId,cancellation);

                if (result.IsSuccess)
                {
                    return Results.Ok(new { Success = result.IsSuccess, Data = result.Data, Message = result.Message });
                }

                return Results.BadRequest(new { Success = result.IsSuccess, Data = result.Data, Message = result.Message });

            });

            app.MapPut("api/rooms/leaveRoom/{roomId}/{userId}", async (string roomId, string userId, CancellationToken cancellation, IRoomsServices repo) =>
            {
                var result = await repo.LeaveRoomAsync(roomId, userId, cancellation);

                if (result.IsSuccess)
                {
                    return Results.Ok(new { Success = result.IsSuccess, Data = result.Data, Message = result.Message });
                }

                return Results.BadRequest(new { Success = result.IsSuccess, Data = result.Data, Message = result.Message });

            });

            app.MapGet("api/rooms/roomExists/{roomId}", async (string roomId,CancellationToken cancellation ,IRoomsServices repo) =>
            {
                var result = await repo.RoomExistsAsync(roomId, cancellation);

                if (result.IsSuccess)
                {
                    return Results.Ok(new { Success = result.IsSuccess, Data = result.Data, Message = result.Message });
                }

                return Results.BadRequest(new { Success = result.IsSuccess, Data = result.Data, Message = result.Message });

            });

            app.MapGet("api/rooms/searchRooms/{searchTerm}", async (string searchTerm, CancellationToken cancellation,int page,int pageSize ,IRoomsServices repo) =>
            {
                var result = await repo.SearchRoomsAsync(searchTerm,cancellation,page,pageSize);

                if (result.IsSuccess)
                {
                    return Results.Ok(new { Success = result.IsSuccess, Data = result.Data, Message = result.Message });
                }

                return Results.BadRequest(new { Success = result.IsSuccess,Data = result.Data ,Message = result.Message });

            });

            app.MapGet("api/rooms/getRoomsByHost/{hostId}", async (string hostId,CancellationToken cancellation ,IRoomsServices repo) =>
            {
                var result = await repo.GetRoomsByHostAsync(hostId, cancellation);

                if (result.IsSuccess)
                {
                    return Results.Ok(new { Success = result.IsSuccess, Data = result.Data, Message = result.Message });
                }

                return Results.BadRequest(new { Success = result.IsSuccess,Data = result.Data ,Message = result.Message });

            });

            app.MapPost("api/rooms/updatePlaybackState/{roomId}", async (string roomId, double position, bool isPaused,string videoId,CancellationToken cancellation ,IRoomsServices repo) =>
            {
                var result = await repo.UpdatePlaybackStateAsync(roomId, position, isPaused,videoId,cancellation);

                if (result.IsSuccess)
                {
                    return Results.Ok(new { Success = result.IsSuccess,Data = result.Data ,Message = result.Message });
                }

                return Results.BadRequest(new { Success = result.IsSuccess,Data = result.Data ,Message = result.Message });

            });

            app.MapPost("api/rooms/validateRoomPass/{roomId}", async (string roomId, string accessCode,CancellationToken cancellation,IRoomsServices repo) =>
            {
                var result = await repo.ValidateRoomPassAsync(roomId, accessCode,cancellation);

                if (result.IsSuccess)
                {
                    return Results.Ok(new { Success = result.IsSuccess,Data = result.Data ,Message = result.Message });
                }

                return Results.BadRequest(new { Success = result.IsSuccess,Data = result.Data ,Message = result.Message });

            });

            app.MapGet("api/rooms/isUserInRoom/{roomId}/{userId}", async (string roomId, string userId,CancellationToken cancellation ,IRoomsServices repo) =>
            {
                var result = await repo.IsUserInRoomAsync(roomId, userId,cancellation);

                if (result.IsSuccess)
                {
                    return Results.Ok(new { Success = result.IsSuccess, Data = result.Data, Message = result.Message });
                }

                return Results.BadRequest(new { Success = result.IsSuccess, Data = result.Data, Message = result.Message });

            });

            app.MapGet("api/rooms/isRoomFull/{roomId}", async (string roomId, CancellationToken cancellation, IRoomsServices repo) =>
            {
                var result = await repo.IsRoomFullAsync(roomId, cancellation);

                if (result.IsSuccess)
                {
                    return Results.Ok(new { Success = result.IsSuccess, Data = result.Data, Message = result.Message });
                }

                return Results.BadRequest(new { Success = result.IsSuccess, Data = result.Data, Message = result.Message });

            });

            app.MapPost("api/rooms/makeRoomPrivate/{roomId}", async (string roomId,CancellationToken cancellation ,IRoomsServices repo) =>
            {
                var result = await repo.MakeRoomPrivateAsync(roomId, cancellation);

                if (result.IsSuccess)
                {
                    return Results.Ok(new { Success = result.IsSuccess, Data = result.Data, Message = result.Message });
                }

                return Results.BadRequest(new { Success = result.IsSuccess, Data = result.Data, Message = result.Message });

            });

            app.MapPost("api/rooms/makeRoomPublic/{roomId}", async (string roomId, CancellationToken cancellation, IRoomsServices repo) =>
            {
                var result = await repo.MakeRoomPublicAsync(roomId, cancellation);

                if (result.IsSuccess)
                {
                    return Results.Ok(new { Success = result.IsSuccess, Data = result.Data, Message = result.Message });
                }

                return Results.BadRequest(new { Success = result.IsSuccess, Data = result.Data, Message = result.Message });

            });

            app.MapGet("api/rooms/getAccessCode/{roomId}/{userId}", async (string roomId, string userId,CancellationToken cancellation ,IRoomsServices repo) =>
            {
                var result = await repo.GetAccessCode(roomId, userId, cancellation);

                if (result.IsSuccess)
                {
                    return Results.Ok(new { Success = result.IsSuccess, Data = result.Data, Message = result.Message });
                }

                return Results.BadRequest(new { Success = result.IsSuccess, Data = result.Data, Message = result.Message });

            });

            app.MapPost("api/rooms/updateCurrentVideoId/{roomId}", async (string roomId, string videoId, CancellationToken cancellation, IRoomsServices repo) =>
            {
                var result = await repo.UpdateCurrentVideoIdAsync(roomId, videoId, cancellation);

                if (result.IsSuccess)
                {
                    return Results.Ok(new { Success = result.IsSuccess, Data = result.Data, Message = result.Message });
                }

                return Results.BadRequest(new { Success = result.IsSuccess, Data = result.Data, Message = result.Message });

            });



            return app;
        }
    }
}
