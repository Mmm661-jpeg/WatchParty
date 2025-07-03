using SyncService.Domain.Dto_s;
using SyncService.Domain.Requests.VideoRequests;
using SyncService.Domain.UtilModels;

namespace SyncService.Core.Interfaces
{
    public interface IVideoSyncService
    {
        Task<OperationResult<VideoDto>> GetVideo(string videoId, string userId, string roomId, CancellationToken cancellationToken = default);

        Task<OperationResult<VideoDto>> AddVideo(AddVideo_Req req, CancellationToken cancellationToken = default);

        Task<OperationResult<IEnumerable<VideoDto>>> GetVideosByUserId(string userId, CancellationToken cancellationToken = default);

        Task<OperationResult<IEnumerable<VideoDto>>> GetVideosByRoomId(string roomId, CancellationToken cancellationToken = default);

        Task<OperationResult<bool>> DeleteVideo(string videoId);

        Task<OperationResult<List<VideoDto>>> SearchVideos(string query, int maxResults = 5, CancellationToken cancellationToken = default);
    }
}
