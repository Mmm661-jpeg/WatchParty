using UserService.Core.Interfaces;
using UserService.Domain.RequestModels;

namespace UserService.Endpoints
{
    public static class AnnonUserEndpoints
    {
        public static WebApplication MapAnnonUserEndpoints(this WebApplication app)
        {
            app.MapPost("api/user/annonUsers/generateToken", async (AnonUserRequest request, IAnonUsersServices anonUsersService) =>
            {
                var result = await anonUsersService.GenerateAnonUserToken(request.CurrentRoomId, request.UserName);
                if (!string.IsNullOrEmpty(result.Data))
                {
                    return Results.Ok(result);
                }
                return Results.BadRequest(result);
            });

            return app;
        }
    }
}
