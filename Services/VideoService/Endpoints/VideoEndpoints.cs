using VideoService.Core.Interfaces;
using VideoService.Domain.Requests;

namespace VideoService.Endpoints
{
    public static class VideoEndpoints
    {
        public static WebApplication MapVideoEndpoints(this WebApplication app)
        {
            app.MapGet("/api/videos/{videoId}", async (string videoId, string userId, string roomId, IVideoServices videoServices, CancellationToken cancellationToken) =>
            {
                var result = await videoServices.GetVideo(videoId, userId, roomId, cancellationToken);
                
                if(result.IsSuccess)
                {
                    return Results.Ok(result);
                }
                else
                {
                    return Results.NotFound(result);
                }
            });

            app.MapPost("/api/videos/addVideo", async (AddVideo_Req req, IVideoServices videoServices, CancellationToken cancellationToken) =>
            {
                var result = await videoServices.AddVideo(req, cancellationToken);
                
                if(result.IsSuccess)
                {
                    return Results.Ok(result);
                }
                else
                {
                    return Results.NotFound(result);
                }
            });

            app.MapGet("/api/videos/user/{userId}", async (string userId, IVideoServices videoServices, CancellationToken cancellationToken) =>
            {
                var result = await videoServices.GetVideosByUserId(userId, cancellationToken);
               
                if(result.IsSuccess)
                {
                    return Results.Ok(result);
                }
                else
                {
                    return Results.NotFound(result);
                }
            });

            app.MapGet("/api/videos/room/{roomId}", async (string roomId, IVideoServices videoServices, CancellationToken cancellationToken) =>
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
           
            });

            app.MapDelete("/api/videos/deleteVideo/{videoId}", async (string videoId, IVideoServices videoServices) =>
            {
                var result = await videoServices.DeleteVideo(videoId);

                if (result.IsSuccess)
                {
                    return Results.Ok(result);
                }
                else
                {
                    return Results.BadRequest(result);
                }
            });

            app.MapGet("/api/videos/search", async (string query, IVideoServices videoServices, CancellationToken cancellationToken, int maxResults = 5) =>
            {
                var result = await videoServices.SearchVideos(query, maxResults, cancellationToken);
               
                if(result.IsSuccess)
                {
                    return Results.Ok(result);
                }
                else
                {
                    return Results.NotFound(result);
                }
            });

            return app;
        }
    }
}
