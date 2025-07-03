using UserService.Core.Interfaces;
using UserService.Core.Services;
using UserService.Domain.RequestModels;

namespace UserService.Endpoints
{
    public static class UserEndpoints
    {
        public static WebApplication MapUserEndpoints(this WebApplication app)
        {
            app.MapPost("/api/user/login" ,async (Login_Req loginReq, IUserServices userService) =>
            {
                var result = await userService.LoginAsync(loginReq);

                if (result.IsSuccess)
                    return Results.Ok(result); 
                else
                    return Results.BadRequest(result);

            });

            app.MapPost("/api/user/register", async (Register_Req registerReq, IUserServices userService) =>
            {
                var result = await userService.RegisterUserAsync(registerReq);

                if (result.IsSuccess)
                    return Results.Ok(result);
                else
                    return Results.BadRequest(result);
            });

            app.MapPut("/api/user/updateUser", async (Update_Req updateReq, IUserServices userService) =>
            {
                var result = await userService.UpdateUserAsync(updateReq);

                if (result.IsSuccess)
                    return Results.Ok(result);
                else
                    return Results.BadRequest(result);
            }).RequireAuthorization(); 

            app.MapPut("/api/user/updateLastActive", async (string userId, IUserServices userService) =>
            {
                var result = await userService.UpdateLastActiveAsync(userId);

                if (result.IsSuccess)
                    return Results.Ok(result);
                else
                    return Results.BadRequest(result);
            });

            app.MapPut("/api/user/changeUserRole", async (string userId, string newRole, IUserServices userService) =>
            {
                var result = await userService.ChangeUserRoleAsync(userId, newRole);

                if (result.IsSuccess)
                    return Results.Ok(result);
                else
                    return Results.BadRequest(result);
            }).RequireAuthorization(a => a.RequireRole("Admin"));

            app.MapDelete("/api/user/deleteUser/{userId}", async (string userId, IUserServices userService) =>
            {
                var result = await userService.DeleteUserAsync(userId);

                if (result.IsSuccess)
                    return Results.Ok(result);
                else
                    return Results.BadRequest(result);
            }).RequireAuthorization();

            app.MapGet("/api/user/getAllUsers", async (int page, IUserServices userService) =>
            {
                var result = await userService.GetAllUsersAsync(page);

                if (result.IsSuccess)
                    return Results.Ok(result);
                else
                    return Results.BadRequest(result);
            }).RequireAuthorization(a => a.RequireRole("Admin"));

            app.MapGet("/api/user/getUserByEmail/{email}", async (string email, IUserServices userService) =>
            {
                var result = await userService.GetUserByEmailAsync(email);

                if (result.IsSuccess)
                    return Results.Ok(result);
                else
                    return Results.BadRequest(result);
            }).RequireAuthorization();

            app.MapGet("/api/user/getUserByUsername", async (string username, IUserServices userService) =>
            {
                var result = await userService.GetUserByUsernameAsync(username);

                if (result.IsSuccess)
                    return Results.Ok(result);
                else
                    return Results.BadRequest(result);
            });

            app.MapGet("/api/user/getUserById/{userId}", async (string userId, IUserServices userService) =>
            {
                var result = await userService.GetUserByIdAsync(userId);

                if (result.IsSuccess)
                    return Results.Ok(result);
                else
                    return Results.BadRequest(result);
            });



            return app;
        }
    }
}
