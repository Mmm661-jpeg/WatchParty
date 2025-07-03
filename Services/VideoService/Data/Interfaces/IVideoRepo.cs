using VideoService.Domain.Entities;

namespace VideoService.Data.Interfaces
{
    public interface IVideoRepo
    {
        Task<Video> GetVideo(string videoId,CancellationToken cancellationToken = default);

        Task<Video> AddVideo(Video video, CancellationToken cancellationToken = default);

        Task<IEnumerable<Video>> GetVideosByUserId(string userId, CancellationToken cancellationToken = default);

        Task<IEnumerable<Video>> GetVideosByRoomId(string roomId, CancellationToken cancellationToken = default);

        Task<bool> DeleteVideo(string videoId);


    }
}
