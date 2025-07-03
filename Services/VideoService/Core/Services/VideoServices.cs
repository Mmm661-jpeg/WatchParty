using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using VideoService.Core.Interfaces;
using VideoService.Data.ExternalApis.YoutubeApi;
using VideoService.Data.ExternalApis.YoutubeApi.YoutubeDto_s;
using VideoService.Data.Interfaces;
using VideoService.Domain.Dto_s;
using VideoService.Domain.Entities;
using VideoService.Domain.Requests;
using VideoService.Domain.UtilModels;

namespace VideoService.Core.Services
{
    public class VideoServices : IVideoServices
    {
       

        private readonly IVideoRepo _videoRepo;
        private readonly ILogger<VideoServices> _logger;
        private readonly IMemoryCache _cache;
        private readonly IYouTubeService _youTubeService;

        public VideoServices(IVideoRepo videoRepo, ILogger<VideoServices> logger, IMemoryCache cache, IYouTubeService youTubeService)
        {
            _videoRepo = videoRepo;
            _logger = logger;
            _cache = cache;
            _youTubeService = youTubeService;
        }

        public async Task<OperationResult<VideoDto>> AddVideo(AddVideo_Req req, CancellationToken cancellationToken = default)
        {
            if (req == null)
            {
                _logger.LogWarning("Received null request to add video.");
                return OperationResult<VideoDto>.Failure(null, "Request cannot be null.");
            }

            if (string.IsNullOrWhiteSpace(req.VideoId) || string.IsNullOrWhiteSpace(req.AddedByUserId) || string.IsNullOrWhiteSpace(req.RoomId))
            {

                _logger.LogWarning("Invalid request data: {Request}", req);
                return OperationResult<VideoDto>.Failure(null, "Invalid request data.");
            }

            try
            {
               

                if (_cache.TryGetValue(req.VideoId, out Video cachedVideo))
                {
                    var mappedCachedVideo = MapToVideoDto(cachedVideo);
                   
                    _logger.LogInformation("Video found in cache: {VideoId}", req.VideoId);
                    return OperationResult<VideoDto>.Success(mappedCachedVideo,"Video found in cache");
                }

                _logger.LogInformation("Video not found in cache, checking database for VideoId: {VideoId}", req.VideoId);

                var existingVideo = await _videoRepo.GetVideo(req.VideoId, cancellationToken);

                if (existingVideo != null)
                {
                    var mappedExistingVideo = MapToVideoDto(existingVideo);
                   
                    _logger.LogInformation("Video found in database: {VideoId}", req.VideoId);
                    return OperationResult<VideoDto>.Success(mappedExistingVideo,"Video found in db");
                }

                _logger.LogInformation("Video not found in cache or database, adding new video: {VideoId}", req.VideoId);

                var videoDetails = await _youTubeService.GetVideoMetadataAsync(req.VideoId);

               

                if (videoDetails == null)
                {
                    _logger.LogWarning("Video details not found for VideoId: {VideoId}", req.VideoId);
                    return OperationResult<VideoDto>.Failure(null, "Video details not found.");
                }

               videoDetails = CompleteVideoModel(req, videoDetails);

                var addToDbResult = await _videoRepo.AddVideo(videoDetails);

                if(addToDbResult == null)
                {
                    _logger.LogError("Failed to add video to database for VideoId: {VideoId}", req.VideoId);
                    return OperationResult<VideoDto>.Failure(null, "Failed to add video to database.");
                }

                _cache.Set(addToDbResult.VideoId, addToDbResult, new MemoryCacheEntryOptions
                {
                    SlidingExpiration = TimeSpan.FromMinutes(60)
                });


                var mappedVideoDetails = MapToVideoDto(addToDbResult);

                return OperationResult<VideoDto>.Success(mappedVideoDetails,"Video added succesfully");

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding video with request: {Request}", req);
                return OperationResult<VideoDto>.Error(ex, "An error occurred while adding the video.");
            }
        }

        public async Task<OperationResult<bool>> DeleteVideo(string videoId)
        {
            if(string.IsNullOrWhiteSpace(videoId))
            {
                _logger.LogWarning("Received null or empty VideoId for deletion.");
                return OperationResult<bool>.Failure(false, "VideoId cannot be null or empty.");
            }

            try
            {
                if (_cache.TryGetValue(videoId, out Video cachedVideo))
                {
                    _cache.Remove(videoId);
                    _logger.LogInformation("Video removed from cache: {VideoId}", videoId);

                   
                }
                else
                {
                    _logger.LogInformation("Video not found in cache for VideoId: {VideoId}", videoId);
                }

                var deleteResult = await _videoRepo.DeleteVideo(videoId);

                if (!deleteResult)
                {
                    _logger.LogWarning("Failed to delete video from database for VideoId: {VideoId}", videoId);
                    return OperationResult<bool>.Failure(false, "Failed to delete video from database.");
                }

                _logger.LogInformation("Video successfully deleted from database: {VideoId}", videoId);
                return OperationResult<bool>.Success(true, "Video deleted successfully.");

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting video with VideoId: {VideoId}", videoId);
                return OperationResult<bool>.Error(ex, "An error occurred while deleting the video.");
            }
        }

        public async Task<OperationResult<VideoDto>> GetVideo(string videoId, string userId, string roomId, CancellationToken cancellationToken = default)
        {
            if(string.IsNullOrWhiteSpace(videoId))
            {
                _logger.LogWarning("Received null or empty VideoId for retrieval.");
                return OperationResult<VideoDto>.Failure(null, "VideoId cannot be null or empty.");
            }
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(roomId))
            {
                _logger.LogWarning("Received null or empty UserId or RoomId for retrieval.");
                return OperationResult<VideoDto>.Failure(null, "UserId and RoomId cannot be null or empty.");
            }
            try
            {
                var trimmedVideoId = videoId.Trim();

                if (_cache.TryGetValue(trimmedVideoId, out Video cachedVideo))
                {
                    var mappedCachedVideo = MapToVideoDto(cachedVideo);
                    _logger.LogInformation("Video found in cache: {VideoId}", trimmedVideoId);
                    return OperationResult<VideoDto>.Success(mappedCachedVideo, "Video found in cache");
                }

                _logger.LogInformation("Video not found in cache, checking database for VideoId: {VideoId}", videoId);

                var video = await _videoRepo.GetVideo(trimmedVideoId, cancellationToken);
                if(video != null)
                {
                    var mappedVideo = MapToVideoDto(video);
                    _logger.LogInformation("Video found in database: {VideoId}", trimmedVideoId);
                    return OperationResult<VideoDto>.Success(mappedVideo, "Video found in db");
                }

                _logger.LogInformation("Video not found in cache or database, retrieving from YouTube API for VideoId: {VideoId}", videoId);

                var videoDetails = await _youTubeService.GetVideoMetadataAsync(trimmedVideoId);

                if (videoDetails == null)
                {
                    _logger.LogWarning("Video details not found for VideoId: {VideoId}", trimmedVideoId);
                    return OperationResult<VideoDto>.Failure(null, "Video details not found.");
                }

                var normalizedRoomId = roomId.Trim();
                var normalizedUserId = userId.Trim();

                videoDetails.RoomId = normalizedRoomId;
                videoDetails.AddedByUserId = normalizedUserId;
                videoDetails.IsActive = false;

                _cache.Set(videoDetails.VideoId, videoDetails, new MemoryCacheEntryOptions
                {
                    SlidingExpiration = TimeSpan.FromMinutes(60)
                });

                var addToDbResult = await _videoRepo.AddVideo(videoDetails,cancellationToken);

                if (addToDbResult == null)
                {
                    _logger.LogError("Failed to add video to database for VideoId: {VideoId}", trimmedVideoId);
                    return OperationResult<VideoDto>.Failure(null, "Failed to add video to database.");
                }

                _logger.LogInformation("Video successfully added to database and cache: {VideoId}", trimmedVideoId);

                var mappedVideoDetails = MapToVideoDto(addToDbResult);

                return OperationResult<VideoDto>.Success(mappedVideoDetails, "Video retrieved successfully");


            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving video with VideoId: {VideoId}", videoId);
                return OperationResult<VideoDto>.Error(ex, "An error occurred while retrieving the video.");
            }
        }

        public async Task<OperationResult<IEnumerable<VideoDto>>> GetVideosByRoomId(string roomId, CancellationToken cancellationToken = default)
        {
            if(string.IsNullOrWhiteSpace(roomId))
            {
                _logger.LogWarning("Received null or empty RoomId for retrieval.");
                return OperationResult<IEnumerable<VideoDto>>.Failure(null, "RoomId cannot be null or empty.");
            }
            try
            {
                var videos = await _videoRepo.GetVideosByRoomId(roomId, cancellationToken);

                if (videos == null || !videos.Any())
                {
                    _logger.LogInformation("No videos found for RoomId: {RoomId}", roomId);
                    return OperationResult<IEnumerable<VideoDto>>.Success(new List<VideoDto>(), "No videos found for the specified room.");
                }

                var videoDtos = videos.Select(MapToVideoDto).ToList();

                _logger.LogInformation("Successfully retrieved {Count} videos for RoomId: {RoomId}", videoDtos.Count, roomId);
                return OperationResult<IEnumerable<VideoDto>>.Success(videoDtos, "Videos retrieved successfully for the specified room.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving videos for RoomId: {RoomId}", roomId);
                return OperationResult<IEnumerable<VideoDto>>.Error(ex, "An error occurred while retrieving videos for the room.");

            }
        }

        public async Task<OperationResult<IEnumerable<VideoDto>>> GetVideosByUserId(string userId, CancellationToken cancellationToken = default)
        {
            if(string.IsNullOrWhiteSpace(userId))
            {
                _logger.LogWarning("Received null or empty UserId for retrieval.");
                return OperationResult<IEnumerable<VideoDto>>.Failure(null, "UserId cannot be null or empty.");
            }
            try
            {
                var videos = await _videoRepo.GetVideosByUserId(userId, cancellationToken);

                if (videos == null || !videos.Any())
                {
                    _logger.LogInformation("No videos found for UserId: {UserId}", userId);
                    return OperationResult<IEnumerable<VideoDto>>.Success(new List<VideoDto>(), "No videos found for the specified user.");
                }

                var videoDtos = videos.Select(MapToVideoDto).ToList();
                _logger.LogInformation("Successfully retrieved {Count} videos for UserId: {UserId}", videoDtos.Count, userId);
                return OperationResult<IEnumerable<VideoDto>>.Success(videoDtos, "Videos retrieved successfully for the specified user.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving videos for UserId: {UserId}", userId);
                return OperationResult<IEnumerable<VideoDto>>.Error(ex, "An error occurred while retrieving videos for the user.");
            }
        }

        public async Task<OperationResult<List<VideoDto>>> SearchVideos(string query, int maxResults = 5, CancellationToken cancellationToken = default)
        {
            if(string.IsNullOrWhiteSpace(query))
            {
                _logger.LogWarning("Received null or empty search query.");
                return OperationResult<List<VideoDto>>.Failure(null, "Search query cannot be null or empty.");
            }

            try
            {
                var normalizedQuery = query.Trim().ToLower();
                var cacheKey = $"ytsearch:{normalizedQuery}:{maxResults}";

                if (_cache.TryGetValue(cacheKey, out List<VideoDto> cachedResults))
                {
                    _logger.LogInformation("Returning cached search results for query: {Query}", query);
                    return OperationResult<List<VideoDto>>.Success(cachedResults, "Search results retrieved from cache.");
                }

                _logger.LogInformation("Searching videos with query: {Query}", query);
                var searchResults = await _youTubeService.SearchVideosAsync(query, maxResults);

                if (searchResults == null || !searchResults.Any())
                {
                    _logger.LogInformation("No videos found for query: {Query}", query);
                    return OperationResult<List<VideoDto>>.Success(new List<VideoDto>(), "No videos found for the specified query.");
                }

                var videoDtos = MapFromYouTubeSearchResult(searchResults);

                _cache.Set(cacheKey, videoDtos, new MemoryCacheEntryOptions
                {
                    SlidingExpiration = TimeSpan.FromMinutes(30)
                });

                return OperationResult<List<VideoDto>>.Success(videoDtos, "Videos found successfully for the specified query.");


            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching videos with query: {Query}", query);
                return OperationResult<List<VideoDto>>.Error(ex, "An error occurred while searching for videos.");
            }
        }

        private VideoDto MapToVideoDto(Video video)
        {
            if (video == null) return null;

            try
            {
                return new VideoDto
                {

                    VideoId = video.VideoId,
                    Title = video.Title,
                    AddedByUserId = video.AddedByUserId,
                    RoomId = video.RoomId,
                    AddedAt = video.AddedAt,
                    ChannelName = video.ChannelName,
                    Duration = video.Duration,
                    ThumbnailUrl = video.ThumbnailUrl,
                    Platform = video.Platform,
                    FullUrl = video.FullUrl,
                    IsActive = video.IsActive,

                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error mapping Video to VideoDto for VideoId: {VideoId}", video.VideoId);
                throw;
            }
        }


        private Video CompleteVideoModel(AddVideo_Req req,Video videoTooComplete)
        {
            var addedByUserId = req.AddedByUserId.Trim();
            var roomId = req.RoomId.Trim();
            var IsActive = req.IsActive;

            videoTooComplete.AddedByUserId = addedByUserId;
            videoTooComplete.RoomId = roomId;
            videoTooComplete.IsActive = IsActive;

            return videoTooComplete;
        }

        

        private List<VideoDto> MapFromYouTubeSearchResult(List<YouTubeSearchResult> searchResults)
        {
            try
            {
                var videoDtos = searchResults.Select(result => new VideoDto
                {
                    VideoId = result.VideoId,
                    Title = result.Title,
                    ThumbnailUrl = result.ThumbnailUrl,
                    ChannelName = result.ChannelName,
                    Platform = Platform.YouTube,
                    IsActive = false,
                    

                }).ToList();

                return videoDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error mapping YouTube search results.");
                throw;
            }
        }

       
    }
}
