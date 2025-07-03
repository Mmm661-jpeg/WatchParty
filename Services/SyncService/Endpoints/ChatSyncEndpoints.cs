using SyncService.Core.Interfaces;
using SyncService.Domain.Requests.ChatRequests;

namespace SyncService.Endpoints
{
    public static class ChatSyncEndpoints
    {
        public static IEndpointRouteBuilder MapChatSyncEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapGet("/api/sync/chat/{chatId}", async (string chatId, CancellationToken cancellation, IChatSyncService chatServices) =>
            {
                var result = await chatServices.GetChatByIdAsync(chatId, cancellation);

                if (result.IsSuccess)
                {
                    return Results.Ok(new { Success = result.IsSuccess, Data = result.Data, Message = result.Message });
                }
                else
                {
                    return Results.NotFound(new { Success = result.IsSuccess, Message = result.Message });
                }

            });

            app.MapPost("/api/sync/chat/addChat", async (AddChat_Req req,CancellationToken cancellation ,IChatSyncService chatServices) =>
            {
                var result = await chatServices.AddChatAsync(req,cancellation);

                if (result.IsSuccess)
                {
                    return Results.Ok(new { Success = result.IsSuccess, Data = result.Data, Message = result.Message });
                }
                else
                {
                    return Results.BadRequest(new { Success = result.IsSuccess, Data = result.Data, Message = result.Message });
                }
            });

            app.MapGet("/api/sync/chat/user/{userId}", async (string userId, CancellationToken cancellation, IChatSyncService chatServices) =>
            {
                var result = await chatServices.GetChatsByUserIdAsync(userId, cancellation);

                if (result.IsSuccess)
                {
                    return Results.Ok(new { Success = result.IsSuccess, Data = result.Data, Message = result.Message });
                }
                else
                {
                    return Results.NotFound(new { Success = result.IsSuccess, Data = result.Data, Message = result.Message });
                }
            }).RequireAuthorization();

            app.MapGet("/api/sync/chat/room/{roomId}", async (string roomId, CancellationToken cancellation, IChatSyncService chatServices) =>
            {
                var result = await chatServices.GetChatsByRoomIdAsync(roomId, cancellation);

                if (result.IsSuccess)
                {
                    return Results.Ok(new { Success = result.IsSuccess, Data = result.Data, Message = result.Message });
                }
                else
                {
                    return Results.NotFound(new { Success = result.IsSuccess, Data = result.Data, Message = result.Message });
                }
            });

            app.MapGet("/api/sync/chat/getUserIdfromChat/{chatId}", async (string chatId, CancellationToken cancellation, IChatSyncService chatServices) =>
            {
                var result = await chatServices.GetUserIdFromChat(chatId, cancellation);

                if (result.IsSuccess)
                {
                    return Results.Ok(new { Success = result.IsSuccess, Data = result.Data, Message = result.Message });
                }
                else
                {
                    return Results.NotFound(new { Success = result.IsSuccess, Data = result.Data, Message = result.Message });
                }
            }).RequireAuthorization();


            return app;
        }
    }
}
