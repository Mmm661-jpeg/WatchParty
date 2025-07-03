using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using SharedProtos;
using VideoService.Core.Interfaces;
using VideoService.Domain.Entities;
using VideoService.Domain.Requests;

namespace VideoService.Core.Grpc
{
    public class VideoGrpcServices : SharedProtos.VideoService.VideoServiceBase
    {
        private readonly IVideoServices _videoService;
        private readonly ILogger<VideoGrpcServices> _logger;

        public VideoGrpcServices(IVideoServices videoService, ILogger<VideoGrpcServices> logger)
        {
            _videoService = videoService ?? throw new ArgumentNullException(nameof(videoService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public override async Task<AddVideoResponse> AddVideo(AddVideoRequest request, ServerCallContext context)
        {
            if (request == null)
            {
                _logger.LogWarning("AddVideo: request iss null");
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Request cannot be null"));
            }
            try
            {
                if (System.Enum.TryParse<Platform>(request.Platform, ignoreCase: true, out var platform) == false)
                {
                    _logger.LogWarning("AddVideo: Invalid platform value {Platform}", request.Platform);
                    throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid platform value"));
                }
                if (string.IsNullOrWhiteSpace(request.VideoId) || string.IsNullOrWhiteSpace(request.UserId) ||
                    string.IsNullOrWhiteSpace(request.RoomId) || string.IsNullOrWhiteSpace(request.Title) ||
                    string.IsNullOrWhiteSpace(request.ThubnailUrl) || string.IsNullOrWhiteSpace(request.ChannelName))
                {
                    _logger.LogWarning("AddVideo: Missing required fields in request");
                    throw new RpcException(new Status(StatusCode.InvalidArgument, "Missing required fields"));
                }

                var addVideoRequest = new AddVideo_Req
                {
                    VideoId = request.VideoId,
                    AddedByUserId = request.UserId,
                    RoomId = request.RoomId,
                    Title = request.Title,
                    ThumbnailUrl = request.ThubnailUrl,
                    Duration = request.Duration,
                    IsActive = request.IsActive,
                    ChannelName = request.ChannelName,
                    Platform = platform,

                };

                var result = await _videoService.AddVideo(addVideoRequest, context.CancellationToken);

                if (result.IsSuccess)
                {
                    _logger.LogInformation("Video added successfully: {VideoId}", result.Data?.VideoId);

                    var videoData = result.Data;

                    var addedAt = Timestamp.FromDateTime(videoData.AddedAt.ToUniversalTime());

                    var theGrpcVideo = new GrpcVideo
                    {
                        VideoId = videoData?.VideoId,
                        Platform = videoData?.Platform.ToString(),
                        Title = videoData?.Title,
                        ThumbnailUrl = videoData?.ThumbnailUrl,
                        Duration = videoData?.Duration ?? 0,
                        UserId = videoData?.AddedByUserId,
                        AddedAt = addedAt,
                        IsActive = videoData?.IsActive ?? false,
                        ChannelName = videoData?.ChannelName,
                        RoomId = videoData?.RoomId
                    };

                    return new AddVideoResponse
                    {
                        Video = theGrpcVideo,
                    };


                }
                else
                {
                    _logger.LogWarning("Failed to add video: {ErrorMessage}", result.Message);
                    throw new RpcException(new Status(StatusCode.Internal, result.Message));
                }


            }
            catch (RpcException)
            {

                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GrpcError adding room");
                throw new RpcException(new Status(StatusCode.Internal, "Internal server error"));
            }
        }

        public override async Task<DeleteVideoResponse> DeleteVideo(DeleteVideoRequest request, ServerCallContext context)
        {
            if (request == null)
            {
                _logger.LogWarning("DeleteVideo: request is null");
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Request cannot be null"));
            }

            if (string.IsNullOrWhiteSpace(request.VideoId))
            {
                _logger.LogWarning("DeleteVideo: VideoId is required");
                throw new RpcException(new Status(StatusCode.InvalidArgument, "VideoId is required"));
            }

            try
            {
                var result = await _videoService.DeleteVideo(request.VideoId);

                if (result.IsSuccess)
                {
                    _logger.LogInformation("Video deleted successfully: {VideoId}", request.VideoId);
                    return new DeleteVideoResponse
                    {
                        Success = true
                        
                    };
                }
                else
                {
                    _logger.LogWarning("Failed to delete video: {ErrorMessage}", result.Message);
                    throw new RpcException(new Status(StatusCode.Internal, result.Message));
                }
            }
            catch (RpcException)
            {

                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting video with ID: {VideoId}", request.VideoId);
                throw new RpcException(new Status(StatusCode.Internal, "Internal server error"));
            }
        }

        public override async Task<GetVideoResponse> GetVideo(GetVideoRequest request, ServerCallContext context)
        {
            if(request == null)
            {
                _logger.LogWarning("GetVideo: Request is null");
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Request cannot be null"));
            }

            if(string.IsNullOrWhiteSpace(request.VideoId))
            {
                _logger.LogWarning("GetVideo: VideoId is required");
                throw new RpcException(new Status(StatusCode.InvalidArgument, "VideoId is required"));
            }

            if(string.IsNullOrWhiteSpace(request.UserId))
            {
                _logger.LogWarning("GetVideo: UserId is required");
                throw new RpcException(new Status(StatusCode.InvalidArgument, "UserId is required"));
            }

            if(string.IsNullOrWhiteSpace(request.RoomId))
            {
                _logger.LogWarning("GetVideo: RoomId is required");
                throw new RpcException(new Status(StatusCode.InvalidArgument, "RoomId is required"));
            }

            try
            {
                var result = await _videoService.GetVideo(request.VideoId,request.UserId,request.RoomId,cancellationToken:context.CancellationToken);

                if (result.IsSuccess)
                {
                    var resultData = result.Data;

                    var addedAt = Timestamp.FromDateTime(resultData.AddedAt.ToUniversalTime());

                    var grpcVideo = new GrpcVideo
                    {
                        VideoId = resultData.VideoId,
                        Platform = resultData.Platform.ToString(),
                        Title = resultData.Title,
                        ThumbnailUrl = resultData.ThumbnailUrl,
                        Duration = resultData.Duration,
                        UserId = resultData.AddedByUserId,
                        AddedAt = addedAt,
                        IsActive = resultData.IsActive,
                        ChannelName = resultData.ChannelName,
                        RoomId = resultData.RoomId
                    };

                    var response = new GetVideoResponse
                    {
                        Video = grpcVideo
                    };

                    return response;

                }
                else
                {
                    _logger.LogWarning("Failed to retrieve video: {ErrorMessage}", result.Message);
                    throw new RpcException(new Status(StatusCode.NotFound, result.Message));
                }

            }
            catch (RpcException)
            {

                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving video with ID: {VideoId}", request.VideoId);

                throw new RpcException(new Status(StatusCode.Internal, "Internal server error"));
            }
        }

        public override async Task<GetVideosByRoomIdResponse> GetVideosByRoomId(GetVideosByRoomIdRequest request, ServerCallContext context)
        {
           if(request == null)
            {
                _logger.LogWarning("GetVideosByRoomId: Request is null");
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Request cannot be null"));
            }

           if(string.IsNullOrWhiteSpace(request.RoomId))
            {
                _logger.LogWarning("GetVideosByRoomId: RoomId required");
                throw new RpcException(new Status(StatusCode.InvalidArgument, "RoomId required"));
            }

            try
            {
                var result = await _videoService.GetVideosByRoomId(request.RoomId, context.CancellationToken);

                if (result.IsSuccess)
                {
                    var roomData = result.Data;

                    var grpcVideos = roomData.Select(room => new GrpcVideo()
                    {
                        RoomId = room.RoomId,
                        VideoId = room.VideoId,
                        Platform = room.Platform.ToString(),
                        Title = room.Title,
                        ThumbnailUrl = room.ThumbnailUrl,
                        Duration = room.Duration,
                        UserId = room.AddedByUserId,

                        AddedAt = Timestamp.FromDateTime(room.AddedAt.ToUniversalTime()),
                        IsActive = room.IsActive,
                        ChannelName = room.ChannelName
                    }).ToList();

                    var response = new GetVideosByRoomIdResponse
                    {
                        Videos = { grpcVideos }
                    };

                    return response;
                }
                else
                {
                    _logger.LogWarning("Failed to retrieve videos for RoomId: {RoomId}, Error: {ErrorMessage}", request.RoomId, result.Message);
                    throw new RpcException(new Status(StatusCode.NotFound, result.Message));
                }
            }
            catch (RpcException)
            {

                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting video by roomId");
                throw new RpcException(new Status(StatusCode.Internal, "Internal server error"));
            }
        }

        public override async Task<GetVideosByUserIdResponse> GetVideosByUserId(GetVideosByUserIdRequest request, ServerCallContext context)
        {
           if(request  == null)
            {
                _logger.LogWarning("GetVideosByUserId: Request is null");
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Request is null"));
            }

           if(string.IsNullOrWhiteSpace(request.UserId))
            {
                _logger.LogWarning("GetVideosByUserId: UserId is null");
                throw new RpcException(new Status(StatusCode.InvalidArgument, "UserId is required"));
            }

           try
            {
                var result = await _videoService.GetVideosByUserId(request.UserId, context.CancellationToken);

                if (result.IsSuccess)
                {
                    var roomData = result.Data;

                    var grpcVideos = roomData.Select(room => new GrpcVideo()
                    {
                        RoomId = room.RoomId,
                        VideoId = room.VideoId,
                        Platform = room.Platform.ToString(),
                        Title = room.Title,
                        ThumbnailUrl = room.ThumbnailUrl,
                        Duration = room.Duration,
                        UserId = room.AddedByUserId,

                        AddedAt = Timestamp.FromDateTime(room.AddedAt.ToUniversalTime()),
                        IsActive = room.IsActive,
                        ChannelName = room.ChannelName
                    }).ToList();

                    var response = new GetVideosByUserIdResponse
                    {
                        Videos = { grpcVideos }
                    };

                    return response;
                }
                else
                {
                    _logger.LogWarning("Failed to retrieve videos with UserId: {UserId}, Error: {ErrorMessage}", request.UserId, result.Message);
                    throw new RpcException(new Status(StatusCode.NotFound, result.Message));
                }
            }
            catch (RpcException)
            {

                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting videos by UserId: {UserId}", request.UserId);
                throw new RpcException(new Status(StatusCode.Internal, "Internal server error"));
            }
        }

        public override async Task<SearchVideosResponse> SearchVideos(SearchVideosRequest request, ServerCallContext context)
        {
           if(request == null)
            {
                _logger.LogWarning("SearchVideos: Request is null");
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Request cannot be null"));
            }

           if(string.IsNullOrWhiteSpace(request.Query))
            {
                _logger.LogWarning("SearchVideos: Query is required");
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Query is required"));
            }

           if(request.MaxResults < 1)
            {
                _logger.LogWarning("SearchVideos: Max result must have a value grater than 1");
                throw new RpcException(new Status(StatusCode.InvalidArgument, "MaxResults must be greater than 0"));
            }

            try
            {
                var result = await _videoService.SearchVideos(request.Query,request.MaxResults ,context.CancellationToken);

                if (result.IsSuccess)
                {
                    var roomData = result.Data;

                    

                    var grpcVideos = roomData.Select(room => new GrpcVideo
                    {
                        RoomId = room.RoomId ?? "",
                        VideoId = room.VideoId ?? "",
                        Platform = room.Platform.ToString() ?? "",
                        Title = room.Title ?? "",
                        ThumbnailUrl = room.ThumbnailUrl ?? "",
                        Duration = room.Duration,
                        UserId = room.AddedByUserId ?? "",
                        AddedAt = Timestamp.FromDateTime(room.AddedAt.ToUniversalTime()),
                        IsActive = room.IsActive,
                        ChannelName = room.ChannelName ?? ""
                    }).ToList();

                    var response = new SearchVideosResponse
                    {
                        Videos = { grpcVideos }
                    };

                    return response;
                }
                else
                {
                    _logger.LogWarning("SearchVideos: Video not found");
                    throw new RpcException(new Status(StatusCode.NotFound, result.Message));
                }

            }
            catch (RpcException)
            {

                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching videos with query: {Query}", request.Query);
                throw new RpcException(new Status(StatusCode.Internal, "Internal server error"));
            }
        }
    }
}
