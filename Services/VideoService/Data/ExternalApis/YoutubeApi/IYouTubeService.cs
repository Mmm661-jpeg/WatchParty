using VideoService.Data.ExternalApis.YoutubeApi.YoutubeDto_s;
using VideoService.Domain.Entities;

namespace VideoService.Data.ExternalApis.YoutubeApi
{
    public interface IYouTubeService
    {
        Task<List<YouTubeSearchResult>> SearchVideosAsync(string query, int maxResults = 10);

        Task<Video> GetVideoMetadataAsync(string videoId);
    }
}
