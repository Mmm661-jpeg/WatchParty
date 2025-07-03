using SyncService.Core.Interfaces;
using SyncService.Domain.Requests.VideoRequests;

namespace SyncService.Endpoints
{
    public static class VideoSyncEndpoints
    {
        public static IEndpointRouteBuilder MapVideoSyncEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapGet("/api/sync/videos/{videoId}", async (string videoId, string userId, string roomId, IVideoSyncService videoServices, CancellationToken cancellationToken) =>
            {
                var result = await videoServices.GetVideo(videoId, userId, roomId, cancellationToken);

                if (result.IsSuccess)
                {
                    return Results.Ok(result);
                }
                else
                {
                    return Results.NotFound(new { Success = result.IsSuccess, Data = result.Data, Message = result.Message });
                }
            }).RequireAuthorization("NotAnon").RequireRateLimiting("basic");

            app.MapPost("/api/sync/videos/addVideo", async (AddVideo_Req req, IVideoSyncService videoServices, CancellationToken cancellationToken) =>
            {
                var result = await videoServices.AddVideo(req, cancellationToken);

                if (result.IsSuccess)
                {
                    return Results.Ok(result);
                }
                else
                {
                    return Results.NotFound(result);
                }
            }).RequireAuthorization("NotAnon").RequireRateLimiting("basic");

            app.MapGet("/api/sync/videos/user/{userId}", async (string userId, IVideoSyncService videoServices, CancellationToken cancellationToken) =>
            {
                var result = await videoServices.GetVideosByUserId(userId, cancellationToken);

                if (result.IsSuccess)
                {
                    return Results.Ok(result);
                }
                else
                {
                    return Results.NotFound(result);
                }
            }).RequireAuthorization("NotAnon").RequireRateLimiting("basic");

            app.MapGet("/api/sync/videos/room/{roomId}", async (string roomId, IVideoSyncService videoServices, CancellationToken cancellationToken) =>
            {
                var result = await videoServices.GetVideosByRoomId(roomId, cancellationToken);

                if (result.IsSuccess)
                {
                    return Results.Ok(result);
                }
                else
                {
                    return Results.NotFound(result);
                }

            }).RequireRateLimiting("basic");

            app.MapDelete("/api/sync/videos/deleteVideo/{videoId}", async (string videoId, IVideoSyncService videoServices) =>
            {
                var result = await videoServices.DeleteVideo(videoId);

                if (result.IsSuccess)
                {
                    return Results.Ok(new { Success = result.IsSuccess, Message = result.Message });
                }
                else
                {
                    return Results.BadRequest(new { Success = result.IsSuccess, Message = result.Message });
                }
            }).RequireAuthorization("NotAnon").RequireRateLimiting("basic");

            app.MapGet("/api/sync/videos/search", async (string query, IVideoSyncService videoServices, CancellationToken cancellationToken, int maxResults = 5) =>
            {
                var result = await videoServices.SearchVideos(query, maxResults, cancellationToken);

                if (result.IsSuccess)
                {
                    return Results.Ok(result);
                }
                else
                {
                    return Results.NotFound(result);
                }
            }).RequireAuthorization("NotAnon").RequireRateLimiting("basic");

            return app;
        }
    }
}
