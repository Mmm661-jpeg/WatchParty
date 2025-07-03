using Grpc.Core;
using SharedProtos;
using SyncService.Core.Interfaces;
using SyncService.Domain.Dto_s;
using SyncService.Domain.Requests.VideoRequests;
using SyncService.Domain.UtilModels;
using System.Text.RegularExpressions;

namespace SyncService.Core.Services
{
    public class VideoSyncService : IVideoSyncService
    {
        private readonly ILogger<VideoSyncService> _logger;
        private readonly VideoService.VideoServiceClient _videoServiceClient;

        public VideoSyncService(ILogger<VideoSyncService> logger, VideoService.VideoServiceClient videoServiceClient)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _videoServiceClient = videoServiceClient ?? throw new ArgumentNullException(nameof(videoServiceClient));
        }

        public async Task<OperationResult<VideoDto>> AddVideo(AddVideo_Req req, CancellationToken cancellationToken = default)
        {
            if (req == null)
            {
                _logger.LogWarning("AddVideo request is null");
                return OperationResult<VideoDto>.Failure(null, "Request cannot be null");
            }

            try
            {
                var request = new AddVideoRequest
                {
                    UserId = req.AddedByUserId,
                    RoomId = req.RoomId,
                    ThubnailUrl = req.ThumbnailUrl,
                    Title = req.Title,
                    Duration = req.Duration,
                    VideoId = req.VideoId,
                    Platform = req.Platform,
                    ChannelName = req.ChannelName,
                    IsActive = req.IsActive,



                };

                var response = await _videoServiceClient.AddVideoAsync(request, cancellationToken: cancellationToken);

                if (response == null)
                {
                    _logger.LogWarning("AddVideo response is null");
                    return OperationResult<VideoDto>.Failure(null, "Failed to add video");
                }


                var videoResponse = response.Video;

                var videoDto = new VideoDto
                {

                    RoomId = videoResponse.RoomId,
                    Title = videoResponse.Title,
                    ThumbnailUrl = videoResponse.ThumbnailUrl,
                    Duration = videoResponse.Duration,
                    VideoId = videoResponse.VideoId,
                    AddedByUserId = videoResponse.UserId,
                    AddedAt = videoResponse.AddedAt.ToDateTime(),
                    Platform = videoResponse.Platform,
                    IsActive = videoResponse.IsActive,
                    ChannelName = videoResponse.ChannelName,

                };


                return OperationResult<VideoDto>.Success(videoDto, "Video added successfully");

            }
            catch (RpcException rpcEx)
            {
                var errorMessage = TranslateRpcErrorMessage(rpcEx.Message);
                _logger.LogWarning("Failed to add video. Message: {Message}",errorMessage);
                return OperationResult<VideoDto>.Failure(null, "Invalid request parameters");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding video");
                return OperationResult<VideoDto>.Error(ex, "An error occurred while adding the video");
            }
        }

        public async Task<OperationResult<bool>> DeleteVideo(string videoId)
        {
            if (string.IsNullOrEmpty(videoId))
            {
                _logger.LogWarning("DeleteVideo request is null or empty");
                return OperationResult<bool>.Failure(false, "Video ID cannot be null or empty");
            }

            try
            {
                var request = new DeleteVideoRequest
                {
                    VideoId = videoId
                };
                var response = await _videoServiceClient.DeleteVideoAsync(request);

                if (response == null)
                {
                    _logger.LogWarning("DeleteVideo response is null");
                    return OperationResult<bool>.Failure(false, "Failed to delete video");
                }
                if (response.Success)
                {
                    return OperationResult<bool>.Success(true, "Video deleted successfully");
                }
                else
                {
                    _logger.LogWarning("Failed to delete video");
                    return OperationResult<bool>.Failure(false, "Failed to delete video");
                }

            }
            catch (RpcException rpcEx)
            {
                var errorMessage = TranslateRpcErrorMessage(rpcEx.Message);
                _logger.LogWarning("Failed to delete video: {Message}",errorMessage);
                return OperationResult<bool>.Failure(false, "Invalid request parameters");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting video");
                return OperationResult<bool>.Error(ex, "An error occurred while deleting the video");
            }

        }

        public async Task<OperationResult<VideoDto>> GetVideo(string videoId, string userId, string roomId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(videoId))
            {
                _logger.LogWarning("GetVideo request is null or empty");
                return OperationResult<VideoDto>.Failure(null, "Video ID cannot be null or empty");
            }
            if (string.IsNullOrWhiteSpace(userId))
            {
                _logger.LogWarning("GetVideo request is null or empty");
                return OperationResult<VideoDto>.Failure(null, "User ID cannot be null or empty");
            }
            if (string.IsNullOrWhiteSpace(roomId))
            {
                _logger.LogWarning("GetVideo request is null or empty");
                return OperationResult<VideoDto>.Failure(null, "Room ID cannot be null or empty");
            }

            try
            {
                var request = new GetVideoRequest
                {
                    VideoId = videoId,
                    UserId = userId,
                    RoomId = roomId
                };
                var response = await _videoServiceClient.GetVideoAsync(request, cancellationToken: cancellationToken);
                if (response == null)
                {
                    _logger.LogWarning("GetVideo response is null");
                    return OperationResult<VideoDto>.Failure(null, "Failed to get video");
                }
                var videoResponse = response.Video;
                if (videoResponse == null)
                {
                    _logger.LogWarning("Video not found for ID: {VideoId}", videoId);
                    return OperationResult<VideoDto>.Failure(null, "Video not found");
                }
                var videoDto = new VideoDto
                {
                    RoomId = videoResponse.RoomId,
                    Title = videoResponse.Title,
                    ThumbnailUrl = videoResponse.ThumbnailUrl,
                    Duration = videoResponse.Duration,
                    VideoId = videoResponse.VideoId,
                    AddedByUserId = videoResponse.UserId,
                    AddedAt = videoResponse.AddedAt.ToDateTime(),
                    Platform = videoResponse.Platform,
                    IsActive = videoResponse.IsActive,
                    ChannelName = videoResponse.ChannelName
                };
                return OperationResult<VideoDto>.Success(videoDto, "Video retrieved successfully");

            }
            catch (RpcException rpcEx)
            {
                var errorMessage = TranslateRpcErrorMessage(rpcEx.Message);
                _logger.LogWarning("Failed to get video: {Message}", errorMessage);
                return OperationResult<VideoDto>.Failure(null, "Invalid request parameters");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting video");
                return OperationResult<VideoDto>.Error(ex, "An error occurred while getting the video");
            }
        }

        public async Task<OperationResult<IEnumerable<VideoDto>>> GetVideosByRoomId(string roomId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(roomId))
            {
                _logger.LogWarning("GetVideosByRoomId request is null or empty");

                return OperationResult<IEnumerable<VideoDto>>.Failure(null, "Room ID cannot be null or empty");
            }

            try
            {
                var request = new GetVideosByRoomIdRequest
                {
                    RoomId = roomId
                };

                var response = await _videoServiceClient.GetVideosByRoomIdAsync(request, cancellationToken: cancellationToken);

                if (response == null)
                {
                    _logger.LogWarning("GetVideosByRoomId response is null");
                    return OperationResult<IEnumerable<VideoDto>>.Failure(null, "Failed to get videos by room ID");
                }

                var responseData = response.Videos;

                if (responseData == null || !responseData.Any())
                {
                    _logger.LogWarning("No videos found for room ID: {RoomId}", roomId);
                    return OperationResult<IEnumerable<VideoDto>>.Success(Enumerable.Empty<VideoDto>(), "No videos found for the specified room ID");
                }

                var videoDtos = responseData.Select(video => new VideoDto
                {
                    RoomId = video.RoomId,
                    Title = video.Title,
                    ThumbnailUrl = video.ThumbnailUrl,
                    Duration = video.Duration,
                    VideoId = video.VideoId,
                    AddedByUserId = video.UserId,
                    AddedAt = video.AddedAt.ToDateTime(),
                    Platform = video.Platform,
                    IsActive = video.IsActive,
                    ChannelName = video.ChannelName
                });

                return OperationResult<IEnumerable<VideoDto>>.Success(videoDtos, "Videos retrieved successfully");

            }
            catch (RpcException rpcEx)
            {
                var errorMessage = TranslateRpcErrorMessage(rpcEx.Message);
                _logger.LogWarning("Failed to get videos by room ID: {Message}",errorMessage);
                return OperationResult<IEnumerable<VideoDto>>.Failure(null, "Invalid request parameters");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting videos by room ID");
                return OperationResult<IEnumerable<VideoDto>>.Error(ex, "An error occurred while getting videos by room ID");
            }
        }

        public async Task<OperationResult<IEnumerable<VideoDto>>> GetVideosByUserId(string userId, CancellationToken cancellationToken = default)
        {
            if(string.IsNullOrWhiteSpace(userId))
            {
                _logger.LogWarning("GetVideosByUserId request is null or empty");
                return OperationResult<IEnumerable<VideoDto>>.Failure(null, "User ID cannot be null or empty");
            }

            try
            {
                var request = new GetVideosByUserIdRequest
                {
                    UserId = userId
                };

                var response = await _videoServiceClient.GetVideosByUserIdAsync(request, cancellationToken: cancellationToken);

                if (response == null)
                {
                    _logger.LogWarning("GetVideosByUserId response is null");
                    return OperationResult<IEnumerable<VideoDto>>.Failure(null, "Failed to get videos by user ID");
                }

                var responseData = response.Videos;

                if (responseData == null || !responseData.Any())
                {
                    _logger.LogWarning("No videos found for user ID: {UserId}", userId);
                    return OperationResult<IEnumerable<VideoDto>>.Success(Enumerable.Empty<VideoDto>(), "No videos found for the specified user ID");
                }

                var videoDtos = responseData.Select(video => new VideoDto
                {
                    RoomId = video.RoomId,
                    Title = video.Title,
                    ThumbnailUrl = video.ThumbnailUrl,
                    Duration = video.Duration,
                    VideoId = video.VideoId,
                    AddedByUserId = video.UserId,
                    AddedAt = video.AddedAt.ToDateTime(),
                    Platform = video.Platform,
                    IsActive = video.IsActive,
                    ChannelName = video.ChannelName
                });

                return OperationResult<IEnumerable<VideoDto>>.Success(videoDtos, "Videos retrieved successfully");
            }
            catch (RpcException rpcEx)
            {
                var errorMessage = TranslateRpcErrorMessage(rpcEx.Message);
                _logger.LogWarning("Failed to get videos by user ID: {Message}", errorMessage);
                return OperationResult<IEnumerable<VideoDto>>.Failure(null, "Invalid request parameters");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting videos by user ID");
                return OperationResult<IEnumerable<VideoDto>>.Error(ex, "An error occurred while getting videos by user ID");
            }
        }

        public Task<OperationResult<List<VideoDto>>> SearchVideos(string query, int maxResults = 5, CancellationToken cancellationToken = default)
        {
            if(string.IsNullOrWhiteSpace(query))
            {
                _logger.LogWarning("SearchVideos request is null or empty");
                return Task.FromResult(OperationResult<List<VideoDto>>.Failure(null, "Query cannot be null or empty"));
            }

            if(maxResults <= 0)
            {
                _logger.LogWarning("SearchVideos maxResults is less than or equal to zero");
                return Task.FromResult(OperationResult<List<VideoDto>>.Failure(null, "Max results must be greater than zero"));
            }

            try
            {
                var request = new SearchVideosRequest
                {
                    Query = query,
                    MaxResults = maxResults
                };

                var response = _videoServiceClient.SearchVideos(request, cancellationToken: cancellationToken);

                if (response == null)
                {
                    _logger.LogWarning("SearchVideos response is null");
                    return Task.FromResult(OperationResult<List<VideoDto>>.Failure(null, "Failed to search videos"));
                }

                var responseData = response.Videos;

                if (responseData == null || !responseData.Any())
                {
                    _logger.LogWarning("No videos found for query: {Query}", query);
                    return Task.FromResult(OperationResult<List<VideoDto>>.Success(new List<VideoDto>(), "No videos found for the specified query"));
                }

                var videoDtos = responseData.Select(video => new VideoDto
                {
                    RoomId = video.RoomId,
                    Title = video.Title,
                    ThumbnailUrl = video.ThumbnailUrl,
                    Duration = video.Duration,
                    VideoId = video.VideoId,
                    AddedByUserId = video.UserId,
                    AddedAt = video.AddedAt.ToDateTime(),
                    Platform = video.Platform,
                    IsActive = video.IsActive,
                    ChannelName = video.ChannelName
                }).ToList();

                return Task.FromResult(OperationResult<List<VideoDto>>.Success(videoDtos, "Videos searched successfully"));
            }
            catch (RpcException rpcEx)
            {
                var errorMessage = TranslateRpcErrorMessage(rpcEx.Message);
                _logger.LogWarning("Failed to search videos: {Message}", errorMessage);
                return Task.FromResult(OperationResult<List<VideoDto>>.Failure(null, "Invalid request parameters"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching videos");
                return Task.FromResult(OperationResult<List<VideoDto>>.Error(ex, "An error occurred while searching for videos"));
            }
        }

        private static string TranslateRpcErrorMessage(string errorMessage)
        {
            var replacedString = errorMessage.Replace("RpcException: ", string.Empty)
                                     .Replace("Status(", string.Empty)
                                     .Replace(")", string.Empty)
                                     .Replace("Internal", "An internal server error occurred")
                                     .Replace("NotFound", "The requested resource was not found")
                                     .Replace("InvalidArgument", "Invalid argument provided")
                                     .Replace("AlreadyExists", "Resource already exists")
                                     .Replace("PermissionDenied", "Permission denied")
                                     .Replace("Unauthenticated", "User is not authenticated");



            replacedString = Regex.Replace(replacedString, @"\s+", " ");

            return replacedString.Trim();



        }
    }
}
