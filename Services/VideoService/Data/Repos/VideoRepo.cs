using MongoDB.Driver;
using VideoService.Data.DataModels;
using VideoService.Data.Interfaces;
using VideoService.Domain.Entities;

namespace VideoService.Data.Repos
{
    public class VideoRepo:IVideoRepo
    {
        private readonly IDbContext _dbContext;
        private readonly ILogger<VideoRepo> _logger;

        public VideoRepo(IDbContext dbContext, ILogger<VideoRepo> logger)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Video> AddVideo(Video video, CancellationToken cancellationToken = default)
        {
            try
            {
                var collection = _dbContext.Videos;

                await collection.InsertOneAsync(video, cancellationToken: cancellationToken);

                return video;

            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error adding video: {VideoId}", video.Id);
                throw;
            }
        }

        public async Task<bool> DeleteVideo(string videoId)
        {
           try
            {
                var collection = _dbContext.Videos;
                var filter = Builders<Video>.Filter.Eq(v => v.VideoId, videoId);

                var result = await collection.DeleteOneAsync(filter);

                if (result.DeletedCount > 0)
                {
                    _logger.LogInformation("Video deleted successfully: {VideoId}", videoId);
                    return true;
                }
                else
                {
                    _logger.LogWarning("No video found to delete with ID: {VideoId}", videoId);
                    return false;
                }
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error deleting video: {VideoId}", videoId);
                throw;
            }
        }

        public async Task<Video> GetVideo(string videoId, CancellationToken cancellationToken = default)
        {
           try
            {
                var collection = _dbContext.Videos;

                var filter = Builders<Video>.Filter.Eq(v => v.VideoId, videoId);
                var video = await collection.Find(filter).FirstOrDefaultAsync(cancellationToken);

                return video;
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error retrieving video by ID: {VideoId}", videoId);
                throw;
            }
        }

        public async Task<IEnumerable<Video>> GetVideosByRoomId(string roomId, CancellationToken cancellationToken = default)
        {
            try
            {
                var collection = _dbContext.Videos;

                var filter = Builders<Video>.Filter.Eq(v => v.RoomId, roomId);
                var sort = Builders<Video>.Sort.Descending(v => v.AddedAt);

                var videos = await collection
                    .Find(filter)
                    .Sort(sort)
                    .ToListAsync(cancellationToken);

                return videos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving videos by room ID: {RoomId}", roomId);
                throw;
            }
        }

        public async Task<IEnumerable<Video>> GetVideosByUserId(string userId, CancellationToken cancellationToken = default)
        {
           try
            {
                var collection = _dbContext.Videos;

                var filter = Builders<Video>.Filter.Eq(v => v.AddedByUserId, userId);

                var sort = Builders<Video>.Sort.Descending(v => v.AddedAt);

                var videos = await collection
                    .Find(filter)
                    .Sort(sort)
                    .ToListAsync(cancellationToken);

                return videos;

            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error retrieving videos by user ID: {UserId}", userId);
                throw;
            }
        }
    }
}
