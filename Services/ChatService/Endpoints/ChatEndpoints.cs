using ChatService.Core.Interfaces;
using ChatService.Domain.Requests;

namespace ChatService.Endpoints
{
    public static class ChatEndpoints
    {
        public static WebApplication MapChatEndpoints(this WebApplication app)
        {

            app.MapGet("/api/chat/{chatId}", async (string chatId,CancellationToken cancellation ,IChatServices chatServices) =>
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

            app.MapPost("/api/chat/addChat", async (AddChat_Req req ,IChatServices chatServices) =>
            {
                var result = await chatServices.AddChatAsync(req);

                if (result.IsSuccess)
                {
                    return Results.Ok(new { Success = result.IsSuccess, Data = result.Data, Message = result.Message });
                }
                else
                {
                    return Results.BadRequest(new { Success = result.IsSuccess, Data = result.Data, Message = result.Message });
                }
            });

            app.MapGet("/api/chat/user/{userId}", async (string userId,CancellationToken cancellation ,IChatServices chatServices) =>
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
            });

            app.MapGet("/api/chat/room/{roomId}", async (string roomId,CancellationToken cancellation ,IChatServices chatServices) =>
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

            app.MapGet("/api/chat/getUserIdfromChat/{chatId}", async (string chatId,CancellationToken cancellation ,IChatServices chatServices) =>
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
            });



            return app;
        }
    }
}
